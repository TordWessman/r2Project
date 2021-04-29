#include "r2ESP8266.h"

#ifdef USE_ESP8266

#if defined(USE_ESP8266_WIFI_AP) && defined(USE_ESP8266_WIFI)
  #error Can't combine USE_ESP8266_WIFI_AP and USE_ESP8266_WIFI
#endif

#include <ESP8266WiFi.h>
#include "r2I2C_config.h"
#include "r2Common.h"

WiFiServer server(80);
#ifdef USE_ESP8266_WIFI_AP

void wifiSetup() {
  WiFi.mode(WIFI_AP);
  String hotspot = WIFI_SSID;
  char hotspot_char[hotspot.length() + 1];
  memset(hotspot_char, 0, hotspot.length() + 1);
  for (int i = 0; i < hotspot.length(); i++)
  hotspot_char[i] = hotspot.charAt(i);
  WiFi.softAP(hotspot_char,WIFI_PASSWORD);
}

#else

void wifiSetup() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD); //Connect to wifi
 
  // Wait for connection  
  R2_LOG(F("Connecting to Wifi"));
  while (WiFi.status() != WL_CONNECTED) {   
    delay(500);
    Serial.print(F("."));
    delay(500);
  }

  Serial.println("");
  Serial.println(WIFI_SSID);
  Serial.println(WiFi.localIP());
  
}

#endif

//WiFiClient::setDefaultNoDelay(true);
WiFiClient client;

// Contains data during serial read.
byte readBuffer[sizeof(RequestPackage) + 1];
#define R2_TCP_READ_TIMEOUT 1000

void writeResponse(ResponsePackage out);

void terminate(const char* message, int code, int data) {
  err(message, code, data);
  writeResponse(createErrorPackage(0x0));
  client.stop();
}

void loop_tcp() {

  byte packageSize;
  byte dx;
  int pos = 0;
  
  if(!client || !client.connected()) {
  
    client = server.available();
    if(client && client.connected()) { client.keepAlive(); }

  } else if (client.available()) {

    uint32_t timeout = millis() + R2_TCP_READ_TIMEOUT;

    // Read package size
    if (!(client.read(&packageSize, 1) > 0)) { terminate("TCP",ERROR_TCP_READ,0); }

    // Read the message
    for (int i = 0; i < packageSize; i++) {
      
      if((millis() > timeout)) { terminate("Timeout", ERROR_TCP_TIMEOUT , pos); }
      
      if (client.read(&dx, 1) > 0) { readBuffer[i] = dx; }
      else { terminate("TCPx", ERROR_TCP_READ, i); }
  
    } 
       
    writeResponse(execute((RequestPackage*)readBuffer));

  }

}

void writeResponse(ResponsePackage out) {

  byte packageSize = RESPONSE_PACKAGE_SIZE(out);
  client.write(packageSize);
  client.write((const byte*)&out, packageSize);

}

#endif
