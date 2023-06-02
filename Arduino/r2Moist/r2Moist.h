#ifndef r2Moist_h
#define r2Moist_h

#include <Arduino.h>
#include <math.h>

#define DEVICE_PORT uint8_t

// Two "rods" per sensor. Each rod is activated using a corresponding "control port".
#define SENSOR_ROD_COUNT 2

class R2Multiplexer {

	public:

		/// Select the channel of the multiplexer to open (for a multiplexer with 3 channel selection ports (8 channels) it should be a value between 0 and 7).
		void open(uint8_t channel);

		/// `channelSelectionPorts` is the output pins connected to the multiplexers channel selection ports.
		/// The order of the `channelSelectionPorts` should correspond to the significance index of the multiplexer's channel selection ports (LSB -> MSB).
		R2Multiplexer(DEVICE_PORT* channelSelectionPorts, uint8_t channelSelectionPortCount);

		/// Number of available, controllable ports for the multiplexer.
		inline uint8_t channelCount() { return pow(2, _channelSelectionPortCount); }

		~R2Multiplexer();

	private:

		uint8_t _channelSelectionPortCount;
		DEVICE_PORT* _channelSelectionPorts;

};

class R2Moist {

	public:

		/// `multiplexer`: The `R2Multiplexer` used for routing signals to the analog port.
		/// `analogPort`: The input analog port.
		/// `controlPorts`: An array output ports of size `SENSOR_ROD_COUNT` which is used to provide voltage to a humidity sensor rod.
		/// `sensorChannels`: A flattened array of the channels for the multiplexer.
		/// `sensorPairCount`: The number of sensors being used. It should be equal to the length of `sensorChannels` / `SENSOR_ROD_COUNT`.
		///
		/// It's important that the sensor pairs are wired correctly: `controllPort[k]` should activate the reading of `sensorChannels[n + k]` where `k` < SENSOR_ROD_COUNT.
		///
		R2Moist(R2Multiplexer* multiplexer, 
				 DEVICE_PORT analogPort, 
				 const uint8_t (&controlPorts)[SENSOR_ROD_COUNT],
				 uint8_t* sensorChannels,
				 uint8_t sensorPairCount);

		/// Return the average value read by each of the rods of a sensor.
		/// `sensorPairIndex` is the index of a sensor pair defined in `sensorPairs`.
		int read(int sensorPairIndex);

		/// Returns the number of registered sensor pairs. Will match using `read(sensorPairIndex)`.
		inline uint8_t sensorPairCount() { return _sensorPairCount; }

		/// Return the underlying multiplexer.
		R2Multiplexer* getMultiplexer();

		~R2Moist();

	private:

		void activateControlPort(int activeControlPort);

		R2Multiplexer* _multiplexer;
		uint8_t _controlPorts[SENSOR_ROD_COUNT];
		DEVICE_PORT _analogPort;
		uint8_t** _sensorPairs;
		uint8_t _sensorPairCount;

};

#endif
