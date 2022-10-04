#ifndef R2_ESP8266L_H
#define R2_ESP8266L_H

#include "r2I2CDeviceRouter.h"

#define R2_TCP_READ_TIMEOUT 5000
#define R2_TCP_MAX_WIFI_CONNECTION_ATTEMPTS 10

// Configure and initialize the WiFi (connect or access point).
void wifiSetup();

// Handle LAN traffic.
void loop_tcp();

#endif
