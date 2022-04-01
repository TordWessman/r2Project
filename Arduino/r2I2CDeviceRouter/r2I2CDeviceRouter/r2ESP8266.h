#ifndef R2_ESP8266L_H
#define R2_ESP8266L_H
#include "r2I2CDeviceRouter.h"


// Configure and initialize the WiFi (connect or access point).
void wifiSetup();

// Handle LAN traffic.
void loop_tcp();

#endif
