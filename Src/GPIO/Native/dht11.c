//http://www.uugear.com/portfolio/dht11-humidity-temperature-sensor-module/

// Install wiringpi in order for this to work..

#include "dht11.h"

#include <wiringPi.h>

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#define MAXTIMINGS	85

void read_dht11_dat();

int dht11_dat[5] = { 0, 0, 0, 0, 0 };

static int dht11_temp, dht11_humidity, dht11_pin;
static bool dht11_should_run = true, dht11_is_running = false;

bool _ext_dht11_init(int pin) { 

	if ( wiringPiSetup() == -1) { return false; }

	dht11_pin = pin; 
	return true;

}

int _ext_dht11_get_temp() { return dht11_temp; }
int _ext_dht11_get_humidity() { return dht11_humidity; }

void _ext_dht11_start() {

	dht11_should_run = true;
	dht11_is_running = true;

	while ( dht11_should_run )
	{
		read_dht11_dat();
		delay( 5000 );
	}

	dht11_is_running = false;
}

void _ext_dht11_stop() {

	dht11_should_run = false;
}

bool _ext_dht11_is_running() { return dht11_is_running; }


void read_dht11_dat()
{
	uint8_t laststate	= HIGH;
	uint8_t counter		= 0;
	uint8_t j		= 0, i;
 
	dht11_dat[0] = dht11_dat[1] = dht11_dat[2] = dht11_dat[3] = dht11_dat[4] = 0;
 
	/* pull pin down for 18 milliseconds */
	pinMode( dht11_pin, OUTPUT );
	digitalWrite( dht11_pin, LOW );
	delay( 18 );
	/* then pull it up for 40 microseconds */
	digitalWrite( dht11_pin, HIGH );
	delayMicroseconds( 40 );
	/* prepare to read the pin */
	pinMode( dht11_pin, INPUT );
 
	/* detect change and read data */
	for ( i = 0; i < MAXTIMINGS; i++ )
	{
		counter = 0;
		while ( digitalRead( dht11_pin ) == laststate )
		{
			counter++;
			delayMicroseconds( 1 );
			if ( counter == 255 )
			{
				break;
			}
		}
		laststate = digitalRead( dht11_pin );
 
		if ( counter == 255 )
			break;
 
		/* ignore first 3 transitions */
		if ( (i >= 4) && (i % 2 == 0) )
		{
			/* shove each bit into the storage bytes */
			dht11_dat[j / 8] <<= 1;
			if ( counter > 16 )
				dht11_dat[j / 8] |= 1;
			j++;
		}
	}
 
	/*
	 * check we read 40 bits (8bit x 5 ) + verify checksum in the last byte
	 * print it out if data is good
	 */
	if ( (j >= 40) &&
	     (dht11_dat[4] == ( (dht11_dat[0] + dht11_dat[1] + dht11_dat[2] + dht11_dat[3]) & 0xFF) ) )
	{
		dht11_humidity = dht11_dat[0];
		dht11_temp = dht11_dat[2];

//		printf( "- Values: temp: %d  humid: %d \n", dht11_dat[2], dht11_dat[0] );

	} else  {
//		printf( "TEMP HUMID: Data not good, skip. temp: %d  humid: %d \n", dht11_dat[2], dht11_dat[0] );
	}

}
 
int main( void )
{
	printf( "Raspberry Pi wiringPi DHT11 Temperature test program\n" );
 	

	if (!_ext_dht11_init (7))
		exit( 1 );
 
	while ( 1 )
	{
		read_dht11_dat();
		delay( 1000 ); /* wait 1sec to refresh */
	}
 
	return(0);
}
