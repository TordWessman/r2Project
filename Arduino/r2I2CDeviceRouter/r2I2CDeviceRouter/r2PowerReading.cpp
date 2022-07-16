#include "r2PowerReading.h"
#include "r2I2C_config.h"
#include "r2I2CDeviceRouter.h"

#ifdef USE_SERIAL
     #error USE_SERIAL is not compatible with the RPI_POWER_DETECTION_PORT, since establishing a serial connection will reset the Arduino. Check r2I2C_config.h.
#endif
int powerCheckCount = 0;
bool powerCheckActive = false;
unsigned long powerCheckTimer = 0;

void powerReadingSetup() {
  reservePort(RPI_POWER_DETECTION_PORT);
  reservePort(RPI_POWER_CONTROLLER_PORT);
  pinMode(RPI_POWER_CONTROLLER_PORT, OUTPUT);
  digitalWrite(RPI_POWER_CONTROLLER_PORT, true);
}

void loop_PowerReading() {

  if (powerCheckActive && 
      analogRead(RPI_POWER_DETECTION_PORT) < RPI_POWER_DETECTION_TRESHOLD) {

      if (powerCheckCount > RPI_POWER_DETECTION_READINGS_COUNT) {

        digitalWrite(RPI_POWER_CONTROLLER_PORT, false);
        powerCheckActive = false;
        powerCheckCount = 0;
        powerCheckTimer  = 0;
        
      
      } else if(millis() - powerCheckTimer  >= RPI_POWER_DETECTION_INTERVAL) {
        
        powerCheckTimer  = millis();
        powerCheckCount++;

      }
        
    } else {

      powerCheckCount = 0;
      powerCheckTimer  = 0;
      
    }
    
}

void enableRPiPowerControll(bool enabled) {

    powerCheckActive = enabled;

}
