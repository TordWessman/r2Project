#include "r2ESP8266.h"

#ifdef USE_ESP8266
  #if !defined(USE_ESP8266_WIFI_AP) && !defined(USE_ESP8266_WIFI)
    #error At least one of the configurations USE_ESP8266_WIFI_AP or USE_ESP8266_WIFI must be used.
  #endif
#include <ESP8266WiFi.h>
  #if defined(USE_ESP8266_WIFI_AP) && defined(USE_ESP8266_WIFI)
    #error Can't combine USE_ESP8266_WIFI_AP and USE_ESP8266_WIFI
  #endif

#include "r2I2C_config.h"
#include "r2Common.h"

WiFiServer server(TCP_PORT);
WiFiClient client;

#ifdef USE_ESP8266_WIFI_AP

void wifiSetup() {
  
  WiFi.mode(WIFI_AP);
  String hotspot = WIFI_SSID;
  char hotspot_char[hotspot.length() + 1];
  memset(hotspot_char, 0, hotspot.length() + 1);
  for (int i = 0; i < hotspot.length(); i++)
  hotspot_char[i] = hotspot.charAt(i);
  WiFi.softAP(hotspot_char,WIFI_PASSWORD);
  R2_LOG(WiFi.macAddress());
  server.begin();
  
}

#else

int wifiConnectionAttempts = 0;
uint32_t connectionAttemptCounter = 0;
uint32_t healthStatusCounter = 0;
uint32_t debugOutputCounter = 0;

void wifiSetup() {

  //R2_LOG(F("Disabling the WDT in order to facilitate poor network communication"));
  //ESP.wdtDisable();
  
  R2_LOG(F("Will initiate connection"));
  WiFi.mode(WIFI_STA);
  WiFi.hostname(WIFI_HOST_NAME);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD); //Connect to wifi
 
  // Wait for connection  
  R2_LOG(F("Connecting to Wifi"));
  while (WiFi.status() != WL_CONNECTED) {   
    
    delay(500);
    R2_PRINT(F("."));
    delay(500);
    wifiConnectionAttempts++;
    
    if (wifiConnectionAttempts == R2_TCP_MAX_WIFI_CONNECTION_ATTEMPTS) {
      arghhhh();
    }
  }

  wifiConnectionAttempts = 0;

  R2_LOG("");
  R2_LOG(WIFI_SSID);
  R2_LOG(WiFi.localIP());
  R2_LOG(WiFi.macAddress());
  server.begin();
  
}

#endif

// Contains data during serial read.
byte readBuffer[sizeof(RequestPackage) + 1];

void writeResponse(ResponsePackage out);

void terminate(const char* message, int code, int data) {
  
  R2_LOG(F("NETWORK ERROR"));
  err(message, code, data);
  writeResponse(createErrorPackage(0x0));
  
}
 
void loop_tcp() {

  byte packageSize;
  byte dx;

#ifdef USE_ESP8266_WIFI
  if (WiFi.status() != WL_CONNECTED) {
    if (client && client.connected()) {
      client.stop(); 
    }
    wifiSetup();
  }
#endif

  healthStatusCounter++;
  
  if (healthStatusCounter % 20000 == 0) { R2_PRINT(client ? (client.connected() ? F("*") : F("x")) : F(".")); healthStatusCounter = 0; debugOutputCounter++; }
  if (debugOutputCounter % 50 == 0) { R2_PRINT(F("\n")); debugOutputCounter++; }
  
  if (!client || !client.connected()) {
    
    client = server.available();
    
    if (client) { R2_LOG(F("Connected to:")); R2_LOG(client.remoteIP()); connectionAttemptCounter = 0; }

  }
  
  if (client && client.connected() && client.available()) {
    
    delay(10);
    R2_LOG(F("Incoming package."));
    
    uint32_t timeout = millis() + R2_TCP_READ_TIMEOUT;

    // Read package size
    if (!(client.read(&packageSize, 1) > 0)) { return terminate("TCP",ERROR_TCP_READ, 0); }

    // Read the message
    int i = 0;
    while (i < packageSize) {

      ESP.wdtFeed();
      
      if(millis() > timeout) { return terminate("TIMEOUT", ERROR_TCP_TIMEOUT, i); }
      
      if (client.read(&dx, 1) > 0) {
        
        readBuffer[i] = dx;
        i++;
        
      }
  
    } 
    
    R2_LOG(F("Package received."));

    writeResponse(execute((RequestPackage*)readBuffer));
    
    R2_LOG(F("Did reply."));
    
  }

}

void writeResponse(ResponsePackage out) {

  byte packageSize = RESPONSE_PACKAGE_SIZE(out);
  client.write(packageSize);
  client.write((const byte*)&out, packageSize);

}

#endif
