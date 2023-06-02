#include "r2Moist.h"

#define CONTROL_PORTS_OFF -1
#define MEASURE_TIME_MS 50

/// Convenience method. Create a 2-dimensional sensor pair struct using a 1-dimensional array.
uint8_t** generateSensorPairs(uint8_t *sensorPairs, uint8_t sensorPairCount) {

	uint8_t** _sensorPairs = (uint8_t**)malloc(sensorPairCount * sizeof(uint8_t*));

	for (int i = 0; i < sensorPairCount; i++) {
    	
    	_sensorPairs[i] = (uint8_t*)malloc(SENSOR_ROD_COUNT * sizeof(uint8_t));
	    
	    for(int j = 0; j < SENSOR_ROD_COUNT; j++) {

	    	_sensorPairs[i][j] = sensorPairs[i * sensorPairCount + j];

	    }

     }

     return _sensorPairs;

}

R2Multiplexer::R2Multiplexer(DEVICE_PORT* channelSelectionPorts, uint8_t channelSelectionPortCount) {

	_channelSelectionPortCount = channelSelectionPortCount;

	_channelSelectionPorts = (DEVICE_PORT *) malloc(sizeof(DEVICE_PORT) * channelSelectionPortCount);

	for(int i = 0; i < _channelSelectionPortCount; i++) {

		_channelSelectionPorts[i] = channelSelectionPorts[i];
		pinMode(_channelSelectionPorts[i], OUTPUT);

	}

}

void R2Multiplexer::open(uint8_t channel) {

	uint8_t portConfig = channel;

	for(uint8_t i = 0; i < _channelSelectionPortCount; i++) {

		digitalWrite(_channelSelectionPorts[i], portConfig & 1 ? HIGH : LOW);
		portConfig = portConfig >> 1;

	}

}

R2Multiplexer::~R2Multiplexer() {

	free(_channelSelectionPorts);

}


R2Moist::~R2Moist() {

	delete _multiplexer;
	
	for (int i = 0; i < _sensorPairCount; i++) { free(_sensorPairs[i]); }

	free(_sensorPairs);

}

R2Moist::R2Moist(R2Multiplexer* multiplexer, 
				 DEVICE_PORT analogPort, 
				 const uint8_t (&controlPorts)[SENSOR_ROD_COUNT],
				 uint8_t* sensorChannels,
				 uint8_t sensorPairCount) {

	_multiplexer = multiplexer;
	_analogPort = analogPort;
	_sensorPairCount = sensorPairCount;

	for(int i = 0; i < SENSOR_ROD_COUNT; i++) { 

		_controlPorts[i] = controlPorts[i];
		pinMode(_controlPorts[i], OUTPUT);

	}
	
	_sensorPairs = generateSensorPairs(sensorChannels, _sensorPairCount);


}


void R2Moist::activateControlPort(int activeControlPort) {

	for(int i = 0; i < SENSOR_ROD_COUNT; i++) {
	
		digitalWrite(_controlPorts[i], activeControlPort == i ? HIGH : LOW);

	}

}


int R2Moist::read(int sensorPairIndex) {

	int readings[SENSOR_ROD_COUNT];

	for(int i = 0; i < SENSOR_ROD_COUNT; i++) {

		_multiplexer->open(_sensorPairs[sensorPairIndex][i]);
		activateControlPort(i);
		delay(MEASURE_TIME_MS);
		readings[i] = analogRead(_analogPort);

	}

	activateControlPort(CONTROL_PORTS_OFF);

	int value = 0;
	
	for(int i = 0; i < SENSOR_ROD_COUNT; i++) { value += readings[i]; }

	return value / SENSOR_ROD_COUNT;

}

R2Multiplexer* R2Moist::getMultiplexer() {

	return _multiplexer;

}
