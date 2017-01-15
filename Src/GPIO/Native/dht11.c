//http://www.uugear.com/portfolio/dht11-humidity-temperature-sensor-module/

// Install wiringpi in order for this to work..

#include <wiringPi.h>
 
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#define MAXTIMINGS	85

void read_dht11_dat();

int dht11_dat[5] = { 0, 0, 0, 0, 0 };

static int temp_h, temp_l, h_l, h_h, dht11_pin;
static bool dht11_should_run = true, dht11_is_running = false;

void _ext_dht11_init(int pin) { dht11_pin = pin; }

int _ext_dht11_get_temp_h() { return temp_h; }
int _ext_dht11_get_temp_l() { return temp_l; }

int _ext_dht11_get_h_h() { return h_h; }
int _ext_dht11_get_h_l() { return h_l; }

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
	float	f; /* fahrenheit */
 
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
		h_h = dht11_dat[0];
		h_l = dht11_dat[1];
		temp_h = dht11_dat[2];
		temp_l = dht11_dat[3];

		f = dht11_dat[2] * 9. / 5. + 32;
		printf( "Humidity = %d.%d %% Temperature = %d.%d *C (%.1f *F)\n",
			dht11_dat[0], dht11_dat[1], dht11_dat[2], dht11_dat[3], f );
	}else  {
		printf( "Data not good, skip\n" );
	}
}
 
int main( void )
{
	printf( "Raspberry Pi wiringPi DHT11 Temperature test program\n" );
 	 _ext_dht11_init (7);

	if ( wiringPiSetup() == -1 )
		exit( 1 );
 
	while ( 1 )
	{
		read_dht11_dat();
		delay( 1000 ); /* wait 1sec to refresh */
	}
 
	return(0);
}
