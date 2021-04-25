#include "r2ESP8266.h"

#ifdef USE_ESP8266_WIFI_AP

#include <ESP8266WiFi.h>
#include "r2I2C_config.h"

WiFiServer server(80);

void wifiApSetup() {
  WiFi.mode(WIFI_AP);
  String WIFIHOTSPOT = WIFI_SSID;
  char WIFIHOTSPOT_CHAR[WIFIHOTSPOT.length() + 1];
  memset(WIFIHOTSPOT_CHAR, 0, WIFIHOTSPOT.length() + 1);
  for (int i=0; i<WIFIHOTSPOT.length(); i++)
  WIFIHOTSPOT_CHAR[i] = WIFIHOTSPOT.charAt(i);
  WiFi.softAP(WIFIHOTSPOT_CHAR,WIFI_PASSWORD);
}


void loop_wifi() {

  WiFiClient client = server.available();
  if (client) {
    
  }
}
#endif
