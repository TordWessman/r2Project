#ifndef R2POWER_READING_H
#define R2POWER_READING_H

  // The analog treshold used to detect when the RPI:s TX pin (GPIO14) is "low enough" to be considered to be powered down.
  #define RPI_POWER_DETECTION_TRESHOLD 128

  // The number of readings below RPI_POWER_DETECTION_TRESHOLD before RPI_POWER_CONTROLLER_PORT is set to Low
  #define RPI_POWER_DETECTION_READINGS_COUNT 5 

  // The interval between power readings
  #define RPI_POWER_DETECTION_INTERVAL 200

  // With an Arduino running at 16 MHz, a `RPI_POWER_DETECTION_READINGS_COUNT`value of 5 and 
  // a `RPI_POWER_DETECTION_INTERVAL`of 200 implies that 5 consequent "power down readings" 
  // within 1 second is required for the power to be turned off.

  // Configure the ports for the power reading. Will set RPI_POWER_CONTROLLER_PORT to high.
  void powerReadingSetup();

  void loop_PowerReading();

  // If `enabled` is set to `true` the power controll logic will be activated.
  void enableRPiPowerControll(bool enabled);
#endif
