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

// How often the slave will try to renew it's address 
#define RH24_NETWORK_RENEWAL_TIME 2000

// Message type sent over mesh considered to be device requests
#define RH24_MESSAGE 'M'

// -- Private method declarations:

#endif

// Returns true if this node is a master node
bool isMaster();

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

bool waitForResponse() {

  int responseTimer = 0;
  
  while (!network.available() && (responseTimer++) < RH24_MAX_RESPONSE_TIMEOUT_COUNT) { delay(RH24_RESPONSE_DELAY); }
  
  if (responseTimer == RH24_MAX_RESPONSE_TIMEOUT_COUNT) { return false; }
  
  return true;
  
}


void rh24Communicate() {

  mesh.update();
  
  if (isMaster()) { mesh.DHCP();
  } else { rh24Slave(); }
  
}

ResponsePackage rh24Send(RequestPackage* request) {

  ResponsePackage response;
 
  for (int i = 0; i < mesh.addrListTop; i++) {
  
    if (request->host == mesh.addrList[i].nodeID) {
    
        RF24NetworkHeader header(mesh.addrList[i].address, OCT);
        
        if (!network.write(header, request, sizeof(RequestPackage))) {
        
            err("Unable to write to slave", ERROR_RH24_WRITE_ERROR);
        
        } else {
          
            if (!waitForResponse()) {
              
              err("Response timed out.", ERROR_RH24_TIMEOUT);
            
          }
        
        }
       
        return rh24Read();
        
    }
    
  }
  
  err("Unavailable", ERROR_RH24_NODE_NOT_AVAILABLE);
  
  response.action = ACTION_ERROR;
  return response;
  
}

// -- Private method bodies

ResponsePackage rh24Read() {
  
   ResponsePackage response;
  
   if (network.available()) {
    
          R2_LOG(F("Received data of type:"));
          RF24NetworkHeader header;
          network.peek(header);
          R2_LOG(header.type);
          
          switch (header.type) {
            
            case RH24_MESSAGE:
            
               // Try to read the response from the slave
              if (network.read(header, &response, sizeof(ResponsePackage)) != sizeof(ResponsePackage)) {

                err("Bad read size", ERROR_RH24_BAD_SIZE_READ);
                
              } else {
              
                R2_LOG(F("Read data from network."));
                
              }
              
            break; 
            
            default:
            
              err("Unknown message.", ERROR_RH24_UNKNOWN_MESSAGE_TYPE_ERROR);
              network.read(header, 0, 0);
              
          }
       
    }
    
    response.action = ACTION_ERROR;
    return response;
    
}

void rh24Slave() {

  if (millis() - renewalTimer >= RH24_NETWORK_RENEWAL_TIME) {
  
    renewalTimer = millis();
    setStatus(true);
#ifdef R2_PRINT_DEBUG
    Serial.print("x");
#endif

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
  
    setStatus(false);;
}
   
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
        R2_LOG(F("Writing response with action:"));
        R2_LOG(response.action);
        
          if (!mesh.write(&response, RH24_MESSAGE, sizeof(ResponsePackage))) {
      
            err("E: Slave write", ERROR_RH24_WRITE_ERROR);
          
          }
      
      }
  
  } 
   
}


bool isMaster() { return DEVICE_HOST_LOCAL == getNodeId(); }

#endif
