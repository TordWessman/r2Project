#include "r2RH24.h"
#include "r2Common.h"

#define RH24_DEBUG

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

// How often the slave will try to renew it's address 
#define RH24_NETWORK_RENEWAL_TIME 2000

// Used to keeep track of responses for each node
bool waitingForResponse[MAX_NODES];

// Contains the latest ResponsePackage received
ResponsePackage lastResponse;

// Message type sent over mesh considered to be device requests
#define RH24_MESSAGE 'M'

// -- Private method declarations:

#endif

// Returns true if this node is a master node
bool isMaster();

// This method should live in the run loop if the I'm a RH24 remote slave
void rh24Slave();

// This method should live in the run loop if the I'm a RH24 master.
void rh24Master();

// Use this to set errors if you're the master and encounter errors during communication with the slave.
void rh24MasterError(const char*msg, byte code);

// Will block until a response from <host> has arrived or when RH24_MAX_RESPONSE_TIMEOUT_COUNT * RH24_RESPONSE_DELAY ms. has been reached
void waitForResponseFrom(byte host);

#ifdef RH24_DEBUG

uint32_t displayTimer = 0;
RequestPackage payload;
bool initialize_sent = false;
bool initialize_ok = false;
int msgCount = 0;
int createdDeviceId = -1;

void rh_debug_host();

#endif

// -- Public method bodies

void rh24Setup() {

  //saveNodeId(0);
  delay(1000);
  byte id = getNodeId();
  
  Serial.println("Start rh24Setup with id:");
  R2_LOG(id);
 
  lastResponse.host = id;
  
  reservePort(RH24_PORT1);
  reservePort(RH24_PORT2);
  
  R2_LOG("droedhudhoudhuordhuondhonudh");
  for (int i = 0; i < MAX_NODES; i++) {
    waitingForResponse[i] = false;
  }
  R2_LOG("xdondhuondhuondhondho");
  if (!isMaster()) {  R2_LOG("Setting up as slave and "); }
  R2_LOG("guodtnduotnodutou");
  mesh.setNodeID(id);
  R2_LOG("aoundodtsodt");
  if (mesh.begin()) {
    R2_LOG("Did start mesh network");
  } else {
    return err("mesh.begin() timeod out", ERROR_RH24_TIMEOUT);  
  }
  
  
  if (!isMaster()) {  
    R2_LOG("Slave started"); 
  } else {
    R2_LOG("Master started "); 
  }
  
  //#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION
  //Serial.println(id);
  //#endif
  R2_LOG("Setup OK!"); 
  radio.printDetails();
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

  byte id = getNodeId();
  Serial.println("");
  
  if (id == DEVICE_HOST_LOCAL) {
    return true;
  }
  
  return false;
  
}

void rh24Communicate() {

  R2_LOG("Will update");
  mesh.update();
 //return delay (1000);
  
  if (isMaster()) {
      R2_LOG("Will DHCP");
      mesh.DHCP();
      //R2_LOG("DID DHCP");
#ifdef RH24_DEBUG
    //R2_LOG("HHAHAHA");
    //rh_debug_host();
#endif

    //rh24Master();
    
  } else {
    
    // The program won't run if this line is not here. Even if it's the master node...
     rh24Slave();
     
  }
  
}

void rh24Master() {
  
   if (network.available()) {
    
          R2_LOG("Master received data.");
          RF24NetworkHeader header;
          R2_LOG("message type:");
          R2_LOG(header.type);
          network.peek(header);
          
          switch (header.type) {
            
            case RH24_MESSAGE:
            
               // Try to read the response from the slave
              if (network.read(header, &lastResponse, sizeof(ResponsePackage)) != sizeof(ResponsePackage)) {
                
                rh24MasterError("Did not receive the expected data amount for the RH24 reply (void rh24Communicate()", ERROR_RH24_BAD_SIZE_READ);
                
              } else {
              
                R2_LOG("Read data from network.");
                
              }
              
              // Tell rh24Send that we have a reply.
              for (int i = 0; i < mesh.addrListTop; i++) {
              
                  // Try to match the header to a node id
                  if (mesh.addrList[i].address == header.from_node) {
    
                    waitingForResponse[mesh.addrList[i].nodeID] = false;
                
                  }
              
              }  
              
            default:
              network.read(header, 0, 0);
          }
       
         
    }
    
}

void rh24Slave() {

  if (millis() - renewalTimer >= RH24_NETWORK_RENEWAL_TIME) {
  
    renewalTimer = millis();
    setStatus(true);
    R2_LOG("R2: Will check connection on R2");
  
  if ( !mesh.checkConnection() ) {  
    
    R2_LOG("Renewing address.");
    mesh.renewAddress(RH24_SLAVE_RENEWAL_TIMEOUT); 
  }
  
    setStatus(false);;
}
   
  while (network.available()) {

      R2_LOG("got message");  
      RF24NetworkHeader header;
      
      network.peek(header);
      
      RequestPackage request;
      
      // Try to read the response from the slave
      if (network.read(header, &request, sizeof(RequestPackage)) != sizeof(RequestPackage)) {
        
        err("Did not receive the expected data amount for the RH24 reply (rh24Slave()", ERROR_RH24_BAD_SIZE_READ);
        
      } else {
      
        R2_LOG("parsed response");
          ResponsePackage response = execute(&request);
        
          if (!mesh.write(&response, RH24_MESSAGE, sizeof(ResponsePackage))) {
      
            err("Did not receive the expected data amount for the RH24 reply (rh24Slave()", ERROR_RH24_WRITE_ERROR);
          
          }
      
      }
  
}
   
   
}

void rh24MasterError(const char*msg, byte code) {

#ifdef PRINT_ERRORS_AND_FUCK_UP_SERIAL_COMMUNICATION
    if (Serial && msg) { R2_LOG(msg); }
#endif

  lastResponse.action = ACTION_ERROR;
  lastResponse.id = code;
  lastResponse.contentSize = strlen(msg);
    
  for (int i = 0; i < lastResponse.contentSize; i++) { lastResponse.content[i] = msg[i]; }
  
}

// -------------------- DEBUG -------------------

#ifdef RH24_DEBUG

void rh_debug_host() {

   if (millis() - displayTimer > 5000) {

    displayTimer = millis();
    
    R2_LOG("Will send when ready.");
    
    for (int i = 0; i < mesh.addrListTop; i++) {

      Serial.println(mesh.addrList[i].nodeID);
      RF24NetworkHeader header(mesh.addrList[i].address, OCT); //Constructing a header
      
      waitingForResponse[mesh.addrList[i].nodeID] = true;
      
      payload.host = mesh.addrList[i].nodeID;
      
      if (!initialize_sent) {
        
        initialize_sent = true;
        payload.action = ACTION_INITIALIZE;
        
      } else if (msgCount == 1) {
        
        R2_LOG("Creating create device package.");
        payload.action = ACTION_CREATE_DEVICE;
        payload.args[REQUEST_ARG_CREATE_TYPE_POSITION] = DEVICE_TYPE_DIGITAL_INPUT;
        payload.args[REQUEST_ARG_CREATE_PORT_POSITION] = 0x2;
        
      } else {
      
          R2_LOG("Creating get device package");
          payload.action = ACTION_GET_DEVICE;
          payload.id = createdDeviceId;
          
      }
      
      network.write(header, &payload, sizeof(payload)) == 1 ? R2_LOG("Send OK") : R2_LOG("Send Fail");

      waitForResponseFrom(mesh.addrList[i].nodeID);
      
      if (initialize_sent && !initialize_ok) {
      
        if (lastResponse.action != ACTION_INITIALIZATION_OK) { err("Initialize failed.", 0); }
        else { initialize_ok = true; R2_LOG("Initialize ok."); }
      
      } else if (msgCount == 1 && lastResponse.action == ACTION_CREATE_DEVICE) {
          
         createdDeviceId = lastResponse.id;
         R2_LOG("Created device width id:");
         Serial.println(createdDeviceId);
        
      } else if (lastResponse.action == ACTION_GET_DEVICE) {
      
        R2_LOG("Got new value: ");
        Serial.println(lastResponse.content[0] + (lastResponse.content[1] << 8));
        
      } else {
      
        err("Got bad response with action:", 0);
        Serial.println(lastResponse.action);
        
      }
      
      msgCount++;
   
    }
      
  }
  
}

#endif

#endif
