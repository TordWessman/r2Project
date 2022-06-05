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
#ifdef USE_ESP8266_WIFI_AP

void wifiSetup() {
  
  WiFi.mode(WIFI_AP);
  String hotspot = WIFI_SSID;
  char hotspot_char[hotspot.length() + 1];
  memset(hotspot_char, 0, hotspot.length() + 1);
  for (int i = 0; i < hotspot.length(); i++)
  hotspot_char[i] = hotspot.charAt(i);
  WiFi.softAP(hotspot_char,WIFI_PASSWORD);
  Serial.println(WiFi.macAddress());
  server.begin();
  
}

#else

void wifiSetup() {

  R2_LOG(F("Will initiate connection"));
  WiFi.mode(WIFI_STA);
  WiFi.hostname(WIFI_HOST_NAME);
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
  Serial.println(WiFi.macAddress());
  server.begin();
  
}

#endif

//WiFiClient::setDefaultNoDelay(true);
WiFiClient client;

// Contains data during serial read.
byte readBuffer[sizeof(RequestPackage) + 1];

unsigned long broadcastTimer = 0;

#define R2_TCP_BROADCAST_TIME 2000 // UDP broadcast every 2 second

#define R2_TCP_READ_TIMEOUT 1000

void writeResponse(ResponsePackage out);
void broadcast();

void terminate(const char* message, int code, int data) {
  
  Serial.println(R2_LOG(F("NETWORK ERROR")));
  Serial.println(message);
  err(message, code, data);
  writeResponse(createErrorPackage(0x0));
  client.stop();
  
}


void broadcastTimerCheck() {
  
  if(millis() - broadcastTimer >= R2_TCP_BROADCAST_TIME) {
    
    broadcastTimer = millis();
    //broadcast();
    
  }
  
}

void loop_tcp() {

  byte packageSize;
  byte dx;

  if (!client) { client = server.available(); }
  
  if (client && client.connected() && client.available()) {
    
    delay(10);
    R2_LOG(F("Incoming package data."));
    
    uint32_t timeout = millis() + R2_TCP_READ_TIMEOUT;

    // Read package size
    if (!(client.read(&packageSize, 1) > 0)) { return terminate("TCP",ERROR_TCP_READ, 0); }

    // Read the message
    for (int i = 0; i < packageSize; i++) {
      
      if((millis() > timeout)) { return terminate("Timeout", ERROR_TCP_TIMEOUT, i); }
      
      if (client.read(&dx, 1) > 0) { readBuffer[i] = dx; }
      else { return terminate("TCPx", ERROR_TCP_READ, i); }
  
    } 
    
    R2_LOG(F("Got a package! Will reply."));
    writeResponse(execute((RequestPackage*)readBuffer));
    R2_LOG(F("Did reply."));
    
  } else {
    
    broadcastTimerCheck();
    
  }

}

void writeResponse(ResponsePackage out) {

  byte packageSize = RESPONSE_PACKAGE_SIZE(out);
  client.write(packageSize);
  client.write((const byte*)&out, packageSize);

}


#define PACKAGE_SIGNATURE 42,0,42,0 // Defined by TCPPackageFactory.cs
#define PACKAGE_CODE 200,0 // Not really used, but HTTP-OK seems good
#define PACKAGE_PATH 47,101,115,112,56,50 // "/esp82"
#define PACKAGE_PATH_SIZE sizeof(PACKAGE_PATH),0,0,0
#define PACKAGE_HEADERS_SIZE 0,0,0,0 // No headers
#define PACKAGE_PAYLOAD_DATA_TYPE 3,0 // Payload type: "Bytes"
#define PACKAGE_PAYLOAD_SIZE(size) size,0,0,0
#define IPV4_SIZE 4 // 4 bytes for an IPv4 address

// Create a byte-formatted TCPPackage.
TCPPackage createTCPPackage(byte* payload, byte payloadSize) {
  
  byte components[] = { PACKAGE_SIGNATURE, PACKAGE_CODE,
                        PACKAGE_PATH_SIZE, PACKAGE_HEADERS_SIZE, PACKAGE_PAYLOAD_SIZE(payloadSize),
                        PACKAGE_PATH, PACKAGE_PAYLOAD_DATA_TYPE };
                        
  int packageSize = sizeof(components) + payloadSize;
  
  TCPPackage package;
  package.size = packageSize;
  package.data = (byte *)malloc(packageSize);
  int offset = 0;

  for(int i = 0; i < sizeof(components); i++) {
    
    package.data[i] = components[i];
    offset++;
  
  }

  for(int i = 0; i < payloadSize; i++) {
    
    package.data[offset + i] = payload[i];
  
  }

  return package;
  
}

//TODO:
UnitIdentifier createUnitIdentifier() {
  UnitIdentifier id;
  id.size = 4;
  id.id = (byte *)malloc(4);
  for(int i = 0; i < 4; i++) { id.id[i] = i; }
  return id;
}

UnitIdentifier unitIdentifier = createUnitIdentifier();

// Broadcast the existance of self with local ip and "unit identifier" over UDP
void broadcast() {

  IPAddress address = WiFi.localIP();
  int size = IPV4_SIZE + unitIdentifier.size;
  byte payload[size];
  
  for(int i = 0; i < IPV4_SIZE; i++) { payload[i] = address[i]; }
  for(int i = 0; i < unitIdentifier.size; i++) { payload[i + IPV4_SIZE] = unitIdentifier.id[i]; }

  TCPPackage package = createTCPPackage(payload, size);

  for(int i = 0; i < package.size; i++) {
    Serial.print((int)package.data[i]); Serial.print("|");
  }

  Serial.println();

  free(package.data);

}

#endif
