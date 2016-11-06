#include "PCA9685.h"
#include <iostream>
#include <stdio.h>
//#include <ncurses.h>
//#include <thread>
#include <unistd.h>

using namespace std;
int main () {

	printf ("Testing testing\n");
	//make sure you use the right address values.
	PCA9685 pwm;
	pwm.init(1,0x40);
	usleep(1000 * 100);
	printf ("Setting frequency..");
	pwm.setPWMFreq (61);
	usleep(1000 * 1000);

	int count = 0;
	while (count++<10) {
		;
		pwm.setPWM(0,0,150);	
		usleep(1000 * 1000);
		
		pwm.setPWM(0,0,600);
		
		usleep(1000 * 1000);
	}
	printf ("\n");
	
	return 0;
} 
