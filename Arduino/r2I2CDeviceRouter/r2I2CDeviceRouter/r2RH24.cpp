#include "r2I2C_config.h"
#include "r2RH24.h"
#include "r2Common.h"

#ifdef USE_RH24

#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>
#include <avr/wdt.h>
#include <EEPROM.h>

#ifndef RH_24_CONFIGURED
#define RH_24_CONFIGURED

  RF24 radio(RH24_PORT1, RH24_PORT2);
  RF24Network network(radio);
  RF24Mesh mesh(radio, network);

// During sleep, this value defines a sleep process forcing the node to sleep until data is received
#define RH24_SLEEP_FOREVER 0xFF

// The default sleep cycles used by the WDT 
#define RH24_SLEEP_CYCLES WDTO_2S

// Maximum number of seconds for the paus sleep interval.
#define MAX_PAUSE_SLEEP_SECONDS 60

// Message definition forcing the node to sleep until the next message is available.
#define RH24_SLEEP_UNTIL_MESSAGE_RECEIVED 0xFF

// Used to determine if the node should sleep or not (done during the run loop).
bool shouldSleep = false;

// If sleep mode is enabled, the sleepCycles define the number of cycles the node should sleep. If it's set to RH24_SLEEP_FOREVER, the node will sleep until woken up by new data
byte sleepCycles = 0;

// Used to pause sleep cycles.
unsigned long pauseSleepTimer = 0;

// The interval for which the sleep state of this node will stay awake if the sleep state has been paused.
uint32_t pauseSleepInterval = 0;

// Keeps track of the slave's address renewal timeout 
uint32_t renewalTimer = 0;

// Keeps track of the failures to renew address. 
uint32_t slaveRenewalFailures = 0;

// Message type sent over mesh considered to be device requests
#define RH24_MESSAGE 'M'

// Ping message from non-master nodes to master node
#define RH24_MESSAGE_PING 'P'
          
// Keeps track of the ping intervals.
unsigned long pingTimer = 0;

// -- Private method declarations --

#endif

// Blocks for RH24_READ_TIMEOUT ms and waits for a ResponsePackage from any node.
ResponsePackage master_tryReadMessage();

// Reads the latest message from any node. 
ResponsePackage master_readResponse();

// Will clean the buffer and hopefully return a "ACTION_RH24_NO_MESSAGE_READ" if the buffer was empty. Run this method prior to any transmission.
ResponsePackage master_readClean(RequestPackage* request);

// Slave run loop: check network status, send ping and renew address
void slave_networkCheck();

// Slave run loop: handle sleeping
void slave_handleSleep();

// Reads a ping from the network 
void slave_readPing(RF24NetworkHeader header);

// Reads a message from the network
void slave_readMessage(RF24NetworkHeader header);

// This method should live in the run loop if the I'm a RF24 remote slave nodes
void rh24SlaveLoop();

// The method being used in the run loop for master nodes.
void rh24MasterLoop();

// -- Debug --

#ifdef R2_PRINT_DEBUG
    int slaveDebugOutputCount = 0;
#endif

// Make sure debug output for sleep state is only printed once.
bool slaveSleepStarted = false;

// -- Public method bodies

// ---------------------------------- LOOP --------------------
void loop_rh24() {

  if (isMaster()) { rh24MasterLoop(); } 
  else { rh24SlaveLoop(); }

}
// -- SETUP --

void rh24Setup() {
  
  //saveNodeId(0);
  //EEPROM.write(SLEEP_MODE_EEPROM_ADDRESS, 0x00);
  byte id = getNodeId();
  
  byte savedCycles = EEPROM.read(SLEEP_MODE_EEPROM_ADDRESS);
  
  if (savedCycles != 0x00) {
  
    sleepCycles = savedCycles;
    shouldSleep = true;
    
  }
  
  R2_LOG(F("Start rh24Setup with id:"));
  R2_LOG(id);
 
  reservePort(RH24_PORT1);
  reservePort(RH24_PORT2);
  
  if (!isMaster()) {  R2_LOG(F("Setting up as slave and ")); }

  mesh.setNodeID(id);
  
  //radio.setPALevel(RF24_PA_HIGH);
  
  if (mesh.begin()) { R2_LOG(F("Did start mesh network")); } 
  else { return err("E: mesh.begin()", ERROR_RH24_TIMEOUT); }
  
  network.setup_watchdog(RH24_SLEEP_CYCLES);
    
  if (!isMaster()) { R2_LOG(F("Slave started")); } 
  else { R2_LOG(F("Master started ")); }
  
  R2_LOG(F("RH24 Setup OK!"));
  
}

bool nodeAvailable(HOST_ADDRESS nodeId) {

  if (nodeId == DEVICE_HOST_LOCAL) { return true; }
  
  for (int i = 0; i < mesh.addrListTop; i++) {
  
    if (nodeId == mesh.addrList[i].nodeID) { 
    
      return true;
      
      // TODO: 
      RequestPackage *request = (RequestPackage*)malloc(MIN_REQUEST_SIZE);
      
      request->host = nodeId;
      request->action = ACTION_RH24_PING_SLAVE;
      request->id = 0;
      request->argSize = 0;
      request->checksum = createRequestChecksum(request);
      
      ResponsePackage response = rh24Send(request);
      free(request);
      return response.action == ACTION_RH24_PING_SLAVE;
      
    }
  
  }
  
  return false;
    
}

int nodeCount() { return mesh.addrListTop; }

HOST_ADDRESS* getNodes() {

    HOST_ADDRESS* addresses = (HOST_ADDRESS*)malloc(mesh.addrListTop);
    
    for (int i = 0; i < mesh.addrListTop; i++) { addresses[i] = mesh.addrList[i].nodeID; }
    
    return addresses;
  
}

ResponsePackage master_readClean(RequestPackage* request) {

  ResponsePackage response = master_readResponse();
  
  // Remove any messages still in the pipe (and return an error if there are any non-ping messages)
  while (response.action != ACTION_RH24_NO_MESSAGE_READ) {
  
    if (response.action != ACTION_RH24_NO_MESSAGE_READ && 
        response.action != ACTION_RH24_PING) {
    
          // A message was read, which means that there was unread messages in the pipe. 
      err("E: Sync", ERROR_RH24_MESSAGE_SYNCHRONIZATION, request->action);
      break;
      
    }
    
    response = master_readResponse();
    
  }
  
  return response;
  
}

ResponsePackage rh24Send(RequestPackage* request) {

  // Make sure there are nothing unread in the input buffer 
  ResponsePackage response = master_readClean(request);
  
  // If synchronization issues occurred
  if (isError()) { return response; }
  
  for (int i = 0; i < mesh.addrListTop; i++) {
  
    if (request->host == mesh.addrList[i].nodeID) {
    
        RF24NetworkHeader header(mesh.addrList[i].address, RH24_MESSAGE);
        
        if (!network.write(header, request, requestPackageSize(request))) {
        
            err("E: slave write.", ERROR_RH24_WRITE_ERROR, request->host);
            return response;
            
        } else {
          
             response = master_tryReadMessage();
             
             // ignore ping messages:
             while (!isError() && response.action == ACTION_RH24_PING) { response = master_tryReadMessage(); }
             
             return response;
   
        }
        
     }
    
  }
  
  err("E: slave dead?", ERROR_RH24_NODE_NOT_AVAILABLE, (byte) mesh.addrListTop);
  
  return response;
  
}

void sleep(bool on) { sleep(on, RH24_SLEEP_UNTIL_MESSAGE_RECEIVED); }

void pauseSleep() { pauseSleep(PAUSE_SLEEP_DEFAULT_INTERVAL); }

void pauseSleep(byte seconds) { 

  if (seconds > 0) {
  
    pauseSleepTimer = millis();
    pauseSleepInterval = 1000 * (seconds > MAX_PAUSE_SLEEP_SECONDS ? MAX_PAUSE_SLEEP_SECONDS : seconds);
    R2_LOG(F("Sleep paused for milliseconds:"));
    R2_LOG(pauseSleepInterval);
 
  } else {
  
    pauseSleepTimer = 0;
    pauseSleepInterval = 0;
  
  }
}

void sleep(bool on, byte cycles) {

  sleepCycles = cycles;
  
  // Avoid retundant EEPROM writes:
  if (on != shouldSleep) {
  
    shouldSleep = on;
    if (!on) { slaveSleepStarted = false; }

    EEPROM.write(SLEEP_MODE_EEPROM_ADDRESS, on ? sleepCycles : 0x00);

  }
    
}


bool isMaster() { return DEVICE_HOST_LOCAL == getNodeId(); }

bool isSleeping() { return shouldSleep; }

// -- Private method bodies

ResponsePackage master_readResponse() {

   ResponsePackage response;
   response.id = 0;
   response.action = ACTION_RH24_NO_MESSAGE_READ;
   
  if (network.available()) {
  
        RF24NetworkHeader header;
        network.peek(header);

        switch (header.type) {
          
          case RH24_MESSAGE: {
        
             // Try to read the response from the slave
            byte bytesRead = network.read(header, &response, sizeof(ResponsePackage));
            
            if (bytesRead < MIN_REQUEST_SIZE) {  err("E: read size", ERROR_RH24_BAD_SIZE_READ, bytesRead); } 
            else { 
                
              R2_LOG(F("Read m/a/ui:"));
              R2_LOG(response.messageId); 
              R2_LOG(response.action);
              R2_LOG(response.host);
              
            }
            
          } break; 
          
          case RH24_MESSAGE_PING: {
            
            R2_LOG(F("Ping!"));
            
            byte ping = 0;
            network.read(header, &ping, 1);
            
            RF24NetworkHeader responseHeader(header.from_node, RH24_MESSAGE_PING);
            response.action = ACTION_RH24_PING;
            
            if (!network.write(responseHeader, &ping, 1)) {
                R2_LOG(F("E: Ping reply failed"));
                //TODO: ping failed
            }
            
          } break;
          
          default:
          
            if (header.type != 0) { err("E: header.type", ERROR_RH24_UNKNOWN_MESSAGE_TYPE_ERROR, header.type); }
            //else {R2_LOG(F("Unknown message type (0). Should this be ignored?"));}
             
            network.read(header, 0, 0);
            
        }
       
    }
    
    return response;

}

ResponsePackage master_tryReadMessage() {
  
   ResponsePackage response;
   response.id = 0;
   response.action = ACTION_RH24_NO_MESSAGE_READ;

   unsigned long responseTimer = millis();
  
   // Try to fetch a response. The response will have the ACTION_RH24_NO_MESSAGE_READ action until it contains data.
   while (responseTimer + RH24_READ_TIMEOUT > millis()) { 
    
     response = master_readResponse();
     mesh.update();
     mesh.DHCP();
     
     // Return the message if it was read.
     if (response.action != ACTION_RH24_NO_MESSAGE_READ) { return response; }
     
   }
   
   err("Read timeout", ERROR_RH24_TIMEOUT);
   
   return response;
    
}

#ifdef R2_PRINT_DEBUG
unsigned long masterDebugTimer = 0;
#endif

void rh24MasterLoop() {
  
  #ifdef R2_PRINT_DEBUG
  
  // Check if i'm alive.
  if (millis() - masterDebugTimer >= 5000) {
  
    masterDebugTimer = millis();
    Serial.print(F("x"));
  }
  
  #endif
    mesh.update();
    mesh.DHCP();
  
  ResponsePackage response = master_readResponse();
  
  if (response.action != ACTION_RH24_NO_MESSAGE_READ && response.action != ACTION_RH24_PING) {
    R2_LOG(F("WTF!!"));
    R2_LOG(response.action);
  }
  
  if (mesh.addrListTop > 0) {
    setStatus(true);
  } else {
    setStatus(false);
  }
  
}

void rh24SlaveLoop() {

  mesh.update();
  
  slave_networkCheck();
  
  while (network.available()) {

      if (slaveSleepStarted) { R2_LOG(F("Waking up from sleep")); }
      
      slaveSleepStarted = false;
      
      RF24NetworkHeader header;
      network.peek(header);
      
      switch(header.type) {
        
        case RH24_MESSAGE: {
           
          slave_readMessage(header);
          
        } break;
        
        case RH24_MESSAGE_PING: {
          
          slave_readPing(header);
          
        } break;
    
      }
      
  } 
   
  slave_handleSleep(); 

}

void slave_networkCheck() {

#ifdef RH24_PING_ENABLED
  if (millis() - pingTimer >= RH24_PING_INTERVAL) {
    
      pingTimer = millis();
      byte slaveId = getNodeId();
      
      if (!mesh.write(&slaveId, RH24_PING, 1)) {
        
        R2_LOG(F("Ping failed. Renewing address."));
        mesh.renewAddress(RH24_SLAVE_RENEWAL_TIMEOUT);
        
      }
  }
#endif

  if (millis() - renewalTimer >= RH24_NETWORK_RENEWAL_TIME) {
  
    renewalTimer = millis();
    
    #ifdef R2_PRINT_DEBUG
    Serial.print("x");
    if (slaveDebugOutputCount++ > 30) { Serial.println(""); slaveDebugOutputCount = 0; }
    #endif
    
    if ( !mesh.checkConnection() ) {
      
        R2_LOG(F("No connection. Renewing address."));
        mesh.renewAddress(RH24_SLAVE_RENEWAL_TIMEOUT); 
        
    }
   
  }
  
}

void slave_handleSleep() {
   
  // Handle sleep pausing:
   if (pauseSleepInterval > 0 && pauseSleepTimer + pauseSleepInterval > millis()) {
   
     return;
     
   } else {
   
     pauseSleepTimer = 0;
     pauseSleepInterval = 0;
     
   }
   
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
  
}

void slave_readPing(RF24NetworkHeader header) {

  byte ping = 0;
  
  if (network.read(header, &ping, 1) != 1) { R2_LOG(F("Unable to read ping!")); } 
  else {
  
    // TODO: Remove this...
    R2_LOG(F("Got ping!"));
    R2_LOG(ping);
    
  }
  
}

void slave_readMessage(RF24NetworkHeader header) {

   R2_LOG(F("Got message")); 
  
   RequestPackage request;
   
   const uint16_t bytesRead = network.read(header, &request, MAX_REQUEST_SIZE);
   
    // Try to read the response from the slave
    if (bytesRead < MIN_REQUEST_SIZE) {
      
      err("E:Slave Bad read size", ERROR_RH24_BAD_SIZE_READ);
      
    } else {
      
      ResponsePackage response = execute(&request);
      const uint16_t responseSize = responsePackageSize(&response);
      R2_LOG(F("Slave: Response with action:"));
      R2_LOG(response.action);
      
      if (!mesh.write(&response, RH24_MESSAGE, responseSize)) {
  
        delay(RH24_SLAVE_WRITE_RETRY);
        
        if (!mesh.write(&response, RH24_MESSAGE, responseSize)) {
  
          err("E: Slave write", ERROR_RH24_WRITE_ERROR);
      
        }
        
      }
    
    } 
    
}

#endif
