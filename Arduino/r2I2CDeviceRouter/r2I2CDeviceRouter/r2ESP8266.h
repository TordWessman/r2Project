#ifndef R2_ESP8266L_H
#define R2_ESP8266L_H
#include "r2I2CDeviceRouter.h"

// Represents the id of this device.
typedef struct UnitIdentifiers {
  byte *id;
  byte size;
} UnitIdentifier;

// Represents a raw TCPPackage which should be in the format that the r2Projects' TCPPackageFactory can parse
typedef struct TCPPackages {
  int size;
  byte *data;
} TCPPackage;

//TODO: ...
//UnitIdentifier createUnitIdentifier();


// Configure and initialize the WiFi (connect or access point).
void wifiSetup();

// Handle LAN traffic.
void loop_tcp();

//TODO: Remove from header:
void broadcast();

#endif
