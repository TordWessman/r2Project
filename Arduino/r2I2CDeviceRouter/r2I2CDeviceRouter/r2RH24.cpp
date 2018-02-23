#include "r2I2C_config.h"
#include "r2RH24.h"
#include "r2Common.h"

//#define RH24_DEBUG

#ifdef USE_RH24

#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>
#include <avr/wdt.h>

#ifndef RH_24_CONFIGURED
#define RH_24_CONFIGURED

  RF24 radio(RH24_PORT1, RH24_PORT2);
  RF24Network network(radio);
  RF24Mesh mesh(radio, network);

// During sleep, this value defines a sleep process forcing the node to sleep until data is received
#define RH24_SLEEP_FOREVER 0xFF

// The default sleep cycles used by the WDT 
#define RH24_SLEEP_CYCLES WDTO_8S

// Used to determine if the node should sleep or not (done during the run loop).
bool shouldSleep = false;

// If sleep mode is enabled, the sleepCycles define the number of cycles the node should sleep. If it's set to RH24_SLEEP_FOREVER, the node will sleep until woken up by new data
byte sleepCycles = 0;

// Keeps track of the slave's address renewal timeout 
uint32_t renewalTimer = 0;

// Keeps track of the failures to renew address. 
uint32_t slaveRenewalFailures = 0;

// Message type sent over mesh considered to be device requests
#define RH24_MESSAGE 'M'

// Ping message from non-master nodes
#define RH24_PING 'P'

// -- Private method declarations:

#endif

// This method should live in the run loop if the I'm a RF24 remote slave nodes
void rh24Slave();

// The method being used in the run loop for master nodes.
void rh24Master();

// Blocks for RH24_READ_TIMEOUT ms and waits for a ResponsePackage from any node.
ResponsePackage tryReadMessage();

// Reads the latest message from any node. 
ResponsePackage readResponse();

// Blocks until data is available from a node or when timeout has been reached.
bool waitForResponse();

// --- Debug

#ifdef R2_PRINT_DEBUG
    int slaveDebugOutputCount = 0;
#endif

// Make sure debug output for sleep state is only printed once.
bool slaveSleepStarted = false;

// -- Public method bodies

// ----------------------------------- SETUP
void rh24Setup() {

  byte id = getNodeId();
  
  R2_LOG(F("Start rh24Setup with id:"));
  R2_LOG(id);
 
  reservePort(RH24_PORT1);
  reservePort(RH24_PORT2);
  
  if (!isMaster()) {  R2_LOG(F("Setting up as slave and ")); }

  mesh.setNodeID(id);
  
  if (mesh.begin()) { R2_LOG(F("Did start mesh network")); } 
  else { return err("E: mesh.begin()", ERROR_RH24_TIMEOUT); }
  
  network.setup_watchdog(RH24_SLEEP_CYCLES);
  
  if (!isMaster()) { R2_LOG(F("Slave started")); } 
  else { R2_LOG(F("Master started ")); }
  
  R2_LOG(F("Setup OK!"));
  
}

// ---------------------------------- LOOP --------------------
void loop_rh24() {

  if (isMaster()) { rh24Master(); } 
  else { rh24Slave(); }
  
}

bool nodeAvailable(HOST_ADDRESS nodeId) {

  for (int i = 0; i < mesh.addrListTop; i++) {
  
    if (nodeId == mesh.addrList[i].nodeID) { return true; }
  
  }
  
  return false;
    
}

int nodeCount() { return mesh.addrListTop; }

HOST_ADDRESS* getNodes() {

    HOST_ADDRESS* addresses = (HOST_ADDRESS*)malloc(mesh.addrListTop);
    
    for (int i = 0; i < mesh.addrListTop; i++) { addresses[i] = mesh.addrList[i].nodeID; }
    
    return addresses;
  
}

ResponsePackage rh24Send(RequestPackage* request) {

  ResponsePackage response = readResponse();
  
  // Remove any messages still in the pipe (and return an error if there are any)
  while (response.action != ACTION_RH24_NO_MESSAGE_READ) {
  
    // A message was read, which means that there was unread messages in the pipe. 
    err("E: Sync", ERROR_RH24_MESSAGE_SYNCHRONIZATION, request->action);
    response = readResponse();
  
  }
  
  // If synchronization issues occurred
  if (isError()) { return response; }
  
  for (int i = 0; i < mesh.addrListTop; i++) {
  
    if (request->host == mesh.addrList[i].nodeID) {
    
        RF24NetworkHeader header(mesh.addrList[i].address, RH24_MESSAGE);
        
        if (!network.write(header, request, sizeof(RequestPackage))) {
        
            err("E: slave write.", ERROR_RH24_WRITE_ERROR, request->host);
            return response;
            
        } else {
          
             response = tryReadMessage();
             
             // ignore ping messages:
             while (!isError() && response.action == ACTION_RH24_PING) { response = tryReadMessage(); }
             
             return response;
   
        }
        
     }
    
  }
  
  err("E: slave dead?", ERROR_RH24_NODE_NOT_AVAILABLE, (byte) mesh.addrListTop);
  
  return response;
  
}

// -- Private method bodies


bool isMaster() { return DEVICE_HOST_LOCAL == getNodeId(); }

void sleep(bool on, byte cycles) {

  shouldSleep = on;
  sleepCycles = cycles;
  
  if (!on) { slaveSleepStarted = false; }

}

uint16_t previousReceivedId = 0;

ResponsePackage readResponse() {

   ResponsePackage response;
   response.id = 0;
   response.action = ACTION_RH24_NO_MESSAGE_READ;
   
  if (network.available()) {
  
        RF24NetworkHeader header;
        network.peek(header);
        
        if (previousReceivedId != 0 && header.id == previousReceivedId) {
        
           err("Duplicate msg", ERROR_RH24_DUPLICATE_MESSAGES);
           return response;
           
        }
        
        previousReceivedId = header.id;
        
        R2_LOG(F("Received data of type:"));
        R2_LOG(header.type);
        
        switch (header.type) {
          
          case RH24_MESSAGE: {
          
             // Try to read the response from the slave
            byte readSize = network.read(header, &response, sizeof(ResponsePackage));
            
            if (readSize != sizeof(ResponsePackage)) {  err("Bad read size", ERROR_RH24_BAD_SIZE_READ, readSize); } 
            else { R2_LOG(F("Read data from network.")); }
            
            return response;
            
          } break; 
      
          default:
          
            err("Unknown msg", ERROR_RH24_UNKNOWN_MESSAGE_TYPE_ERROR, header.type);
            network.read(header, 0, 0);
            return response;
            
        }
       
    }
    
    return response;

}

ResponsePackage tryReadMessage() {
  
   ResponsePackage response;
   response.id = 0;
   response.action = ACTION_RH24_NO_MESSAGE_READ;

   unsigned long responseTimer = millis();
  
   // Try to fetch a response. The response will have the ACTION_RH24_NO_MESSAGE_READ action until it contains data.
   while (responseTimer + RH24_READ_TIMEOUT > millis()) { 
    
     response = readResponse();
     mesh.update();
     mesh.DHCP();
     
     // Return the message if it was read.
     if (response.action != ACTION_RH24_NO_MESSAGE_READ) { return response; }
     
   }
   
   err("Read timeout", ERROR_RH24_TIMEOUT);
   
   return response;
    
}

bool blinkx = false;

void rh24Master() {
  
    mesh.update();
    mesh.DHCP();
  
  if (mesh.addrListTop > 0) {
    setStatus(blinkx);
    blinkx = true;
  } else {
    setStatus(false);
  }
  
}

void rh24Slave() {

  mesh.update();
  
  // --- BEGIN CHECK CONNECTION
  
  if (millis() - renewalTimer >= RH24_NETWORK_RENEWAL_TIME) {
  
    renewalTimer = millis();
    
    #ifdef R2_PRINT_DEBUG
    Serial.print("x");
    if (slaveDebugOutputCount++ > 30) { Serial.println(""); slaveDebugOutputCount = 0; }
    #endif

    if ( !mesh.checkConnection() ) {
        
        R2_LOG(F("Renewing address."));
        mesh.renewAddress(RH24_SLAVE_RENEWAL_TIMEOUT); 
        
    }
   
  }

   // --- END CHECK CONNECTION
  
   // -- BEGIN TRY READ
 
  while (network.available()) {

      if (slaveSleepStarted) { R2_LOG(F("Waking up from sleep")); }
      
      slaveSleepStarted = false;
      
      R2_LOG(F("Got message"));  
      
      RF24NetworkHeader header;
      RequestPackage request;
            
      // Try to read the response from the slave
      if (network.read(header, &request, sizeof(RequestPackage)) != sizeof(RequestPackage)) {
        
        err("E: Bad read size", ERROR_RH24_BAD_SIZE_READ);
        
      } else {
        
        ResponsePackage response = execute(&request);
        R2_LOG(F("Writing response with action & id:"));
        R2_LOG(response.action);
        R2_LOG(response.id);
        
        if (!mesh.write(&response, RH24_MESSAGE, sizeof(ResponsePackage))) {
    
          err("E: Slave write", ERROR_RH24_WRITE_ERROR);
        
        }
      
      } 
    
  } 
   
   // --- END TRY READ
   
    // --- BEGIN CHECK SLEEP
    
   if (shouldSleep == true) {

     if (!slaveSleepStarted) { 
       
       R2_LOG(F("Sending node to sleep.")); 
       slaveSleepStarted = true;
    
       // Allows me to finish stuff (i.e. device configiratons, debug output etc.) 
       delay(1000); 
    
     }
    
    if (!network.sleepNode(1,255)) {
    
      R2_LOG(F("Failed to sleep node."));
      err("E: sleep", ERROR_FAILED_TO_SLEEP);
    
    }
    
  }
  
  // --- END CHECK SLEEP
   
}

#endif
