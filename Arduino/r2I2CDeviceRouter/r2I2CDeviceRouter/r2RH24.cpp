#include "r2I2C_config.h"
#include "r2RH24.h"
#include "r2Common.h"

//#define RH24_DEBUG

#ifdef USE_RH24

#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>

#ifndef RH_24_CONFIGURED
#define RH_24_CONFIGURED

  RF24 radio(RH24_PORT1, RH24_PORT2);
  RF24Network network(radio);
  RF24Mesh mesh(radio, network);

// Maximum number of nodes in the slave network
#define MAX_NODES 20

// Keeps track of the slave's address renewal timeout 
uint32_t renewalTimer = 0;

// Keeps track of the failures to renew address. 
uint32_t slaveRenewalFailures = 0;

// If this amount is reached, the mest.begin() should be called on the slave
#define MAX_RENEWAL_FAILURE_COUNT 10

// How often the slave will try to renew it's address (in ms)
#define RH24_NETWORK_RENEWAL_TIME 4000

// Timeout in ms before a network read operation fails
#define RH24_READ_TIMEOUT 2000

// Message type sent over mesh considered to be device requests
#define RH24_MESSAGE 'M'

// Ping message from non-master nodes
#define RH24_PING 'P'

// -- Private method declarations:

#endif

// This method should live in the run loop if the I'm a RH24 remote slave
void rh24Slave();

// Blocks and waits for a ResponsePackage from any node..
ResponsePackage rh24Read();

// Blocks until data is available from a node or when timeout has been reached.
bool waitForResponse();

// -- Public method bodies

void rh24Setup() {

  //saveNodeId(0);
  delay(1000);
  byte id = getNodeId();
  
  R2_LOG(F("Start rh24Setup with id:"));
  R2_LOG(id);
 
  reservePort(RH24_PORT1);
  reservePort(RH24_PORT2);
  
  if (!isMaster()) {  R2_LOG(F("Setting up as slave and ")); }

  mesh.setNodeID(id);
  
  if (mesh.begin()) {
    R2_LOG(F("Did start mesh network"));
  } else {
    return err("mesh.begin() timeod out", ERROR_RH24_TIMEOUT);  
  }
  
  
  if (!isMaster()) {  
    R2_LOG(F("Slave started")); 
  } else {
    R2_LOG(F("Master started ")); 
  }
  
  R2_LOG(F("Setup OK!")); 
  radio.printDetails();
}

void rh24Communicate() {

  mesh.update();
  
  if (isMaster()) { mesh.DHCP();
  } else { rh24Slave(); }
  
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
    
    for (int i = 0; i < mesh.addrListTop; i++) {
       addresses[i] = mesh.addrList[i].nodeID;
    }
    
    return addresses;
  
}

ResponsePackage rh24Send(RequestPackage* request) {

  ResponsePackage response;
 
  byte foundNodeId = 0;
  
  for (int i = 0; i < mesh.addrListTop; i++) {
  
    foundNodeId = mesh.addrList[i].nodeID;
    
    if (request->host == mesh.addrList[i].nodeID) {
    
        RF24NetworkHeader header(mesh.addrList[i].address, OCT);
        
        if (!network.write(header, request, sizeof(RequestPackage))) {
        
            err("Unable to write to slave", ERROR_RH24_WRITE_ERROR, request->host);
            return response;
            
        } else {
          
             response = rh24Read();
             
             // ignore ping messages:
             while (!isError() && response.action == ACTION_RH24_PING) { response = rh24Read();   }
             
             return response;
   
        }
        
     }
    
  }
  
  err("Unavailable", ERROR_RH24_NODE_NOT_AVAILABLE, foundNodeId);{
          
             response = rh24Read();
             
             // ignore ping messages:
             while (!isError() && response.action == ACTION_RH24_PING) { response = rh24Read();   }
             
             return response;
   
        }
  
  return response;
  
}

// -- Private method bodies

uint16_t previousReceivedId = 0;

ResponsePackage rh24Read() {
  
   ResponsePackage response;
   response.id = 0;
   response.action = 0;

   unsigned long responseTimer = millis();
  
   while (responseTimer + RH24_READ_TIMEOUT > millis() ) { 
    
     mesh.update();
     mesh.DHCP();
     
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
              
              if (readSize != sizeof(ResponsePackage)) {

                err("Bad read size", ERROR_RH24_BAD_SIZE_READ, readSize);
                
              } else {
              
                R2_LOG(F("Read data from network."));
                
              }
              
              // DEBUG ME:
              if (response.action == ACTION_CREATE_DEVICE) {
                 response.action == response.id;
              }
              
              return response;
              
            } break; 
            
            case RH24_PING: {
              
                R2_LOG(F("Got ping from node:"));
                R2_LOG(header.from_node);
                HOST_ADDRESS addr;
                network.read(header, &addr, sizeof(HOST_ADDRESS));
                R2_LOG(addr);
                response.action = ACTION_RH24_PING;
                
                return response;
                
            } break;
        
            default:
            
              err("Unknown message.", ERROR_RH24_UNKNOWN_MESSAGE_TYPE_ERROR, header.type);
              network.read(header, 0, 0);
              return response;
              
          }
         
      } 
    
   }
   
   err("Read timeout.", ERROR_RH24_TIMEOUT);
   
   return response;
    
}

#ifdef R2_PRINT_DEBUG
    int slaveDebugOutputCount = 0;
#endif

void rh24Slave() {

  
  // Begin check connection
  if (millis() - renewalTimer >= RH24_NETWORK_RENEWAL_TIME) {
  
    renewalTimer = millis();
    setStatus(true);
#ifdef R2_PRINT_DEBUG
    Serial.print("x");
    if (slaveDebugOutputCount++ > 30) { Serial.println(""); slaveDebugOutputCount = 0; }
#endif

    HOST_ADDRESS addr = getNodeId();
    
    //if (!mesh.write(&addr, RH24_PING, sizeof(HOST_ADDRESS))) {
      
      if ( !mesh.checkConnection() ) {
    
        if (slaveRenewalFailures++ > MAX_RENEWAL_FAILURE_COUNT) {
        
            R2_LOG(F("Reconnectinging to mesh."));
            mesh.begin();
            mesh.update();
            slaveRenewalFailures = 0;
            
        }
        
        R2_LOG(F("Renewing address."));
        mesh.renewAddress(RH24_SLAVE_RENEWAL_TIMEOUT); 
        
        }
  
      setStatus(false);
      
     //}
   
  }
    
   //End check connection
   
   //Begin try read
 
  while (network.available()) {

      R2_LOG(F("Got message"));  
      
      RF24NetworkHeader header;
      
      network.peek(header);
      
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
   
   // End tryread
   
}


bool isMaster() { return DEVICE_HOST_LOCAL == getNodeId(); }

#endif
