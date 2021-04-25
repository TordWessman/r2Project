#ifdef USE_ESP8266_WIFI_AP

#ifndef ESP8266

#error WIFI only available for ESP8266. Check your "USE_" definitions in r2I2C_config.h

#endif

// Configure and initialize the WiFi Access Point.
void wifiApSetup();

// Handle LAN traffic.
void loop_wifi();

#endif
