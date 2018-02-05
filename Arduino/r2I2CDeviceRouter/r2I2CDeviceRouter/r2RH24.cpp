#include "r2RH24.h"
#include "RF24.h"
#include "RF24Network.h"
#include "RF24Mesh.h"
#include <SPI.h>
#include "r2Common.h"

RF24 radio(RF24_PORT1, RF24_PORT2);
RF24Network network(radio);
RF24Mesh mesh(radio, network);

bool waitingForResponse[255];

// Contains the latest ResponsePackage received
ResponsePackage lastResponse;

#define MESSAGE_TYPE_MESSAGE 'M'
// -- Private method declarations:

// Returns true if this node is a master node
bool isMaster();

// This method should live in the run loop if the I'm a RH24 remote slave
void rh24Slave();

// This method should live in the run loop if the I'm a RH24 master.
void rh24Master();

// Use this to set errors if you're the master and encounter errors during communication with the slave.
void rh24MasterError(const char*msg, byte code);

// -- Public method bodies

void rh24Setup() {

  lastResponse.host = getNodeId();
  
  reservePort(RF24_PORT1);
  reservePort(RF24_PORT2);
  
  for (int i = 0; i < 255; i++) {
    waitingForResponse[i] = false;
  }
  
  mesh.setNodeID(getNodeId());
  mesh.begin();

}

void waitForResponseFrom(byte host) {

  int responseTimer = 0;
  
  while (waitingForResponse[host] && (responseTimer++) < RH24_MAX_RESPONSE_TIMEOUT_COUNT) { delay(RH24_RESPONSE_DELAY); }
  
  if (responseTimer == RH24_MAX_RESPONSE_TIMEOUT_COUNT) {
  
      rh24MasterError("Response timed out.", ERROR_RH24_TIMEOUT);
      waitingForResponse[host] = false;
      
  }
  
}

// Returns true if the package was sent.
ResponsePackage rh24Send(RequestPackage* request) {

  for (int i = 0; i < mesh.addrListTop; i++) {
  
    if (request->host == mesh.addrList[i].nodeID) {
    
        RF24NetworkHeader header(mesh.addrList[i].address, OCT);
        
        waitingForResponse[mesh.addrList[i].nodeID] = true;
        if (!network.write(header, request, sizeof(request))) {
        
            rh24MasterError("Unable to write to slave", ERROR_RH24_WRITE_ERROR);
        
        } else {
          
            waitForResponseFrom(mesh.addrList[i].nodeID);
        
        }
       
        return lastResponse;
        
    }
    
  }
  
  rh24MasterError("Node not available", ERROR_RH24_NODE_NOT_AVAILABLE);
  
  return lastResponse;
  
}

// -- Private method bodies

bool isMaster() {

  return getNodeId() == DEVICE_HOST_LOCAL;
  
}

void rh24Communicate() {

  mesh.update();

  if (isMaster()) {
    
    rh24Master();
  
  } else {
    
     rh24Slave();
     
  }
  
}

void rh24Master() {
  
   mesh.DHCP();
  
   if (network.available()) {
    
          RF24NetworkHeader header;
          
          network.peek(header);
          
          // Try to read the response from the slave
          if (network.read(header, &lastResponse, sizeof(ResponsePackage)) != sizeof(ResponsePackage)) {
            
            rh24MasterError("Did not receive the expected data amount for the RH24 reply (void rh24Communicate()", ERROR_RH24_BAD_SIZE_READ);
            
          }
          
          // Tell rh24Send that we have a reply.
          for (int i = 0; i < mesh.addrListTop; i++) {
          
              // Try to match the header to a node id
              if (mesh.addrList[i].address == header.from_node) {

                waitingForResponse[mesh.addrList[i].nodeID] = false;
            
              }
          
          }  
         
    }
}

void rh24Slave() {

  if ( !mesh.checkConnection() ) {  mesh.renewAddress(RH24_SLAVE_RENEWAL_TIMEOUT); }
   
  if (network.available()) {
  
      RF24NetworkHeader header;
      
      network.peek(header);
      
      RequestPackage request;
      
      // Try to read the response from the slave
      if (network.read(header, &request, sizeof(RequestPackage)) != sizeof(RequestPackage)) {
        
        err("Did not receive the expected data amount for the RH24 reply (rh24Slave()", ERROR_RH24_BAD_SIZE_READ);
        
      } else {
      
          ResponsePackage response = execute(&request);
        
          if (!mesh.write(&response, MESSAGE_TYPE_MESSAGE, sizeof(ResponsePackage))) {
      
            err("Did not receive the expected data amount for the RH24 reply (rh24Slave()", ERROR_RH24_WRITE_ERROR);
          
          }
      
      }
  
}
   
   
}

void rh24Error(const char*msg, byte code) {

  lastResponse.action = ACTION_ERROR;
  lastResponse.id = code;
  lastResponse.contentSize = strlen(msg);
    
  for (int i = 0; i < lastResponse.contentSize; i++) { lastResponse.content[i] = msg[i]; }
  
}
