// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
// 
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
// 

#include "../../../Src/PCA9685/PCA9685.h"
#include <iostream>
#include <stdio.h>
#include <unistd.h>

using namespace std;


extern "C" {
	// problably 1 (or 0 for older boards) and 0x40. use i2cdetect -y 0 && i2cdetect -y 1
	// good, testad freqency = 61
	void _ext_init_pca9685(int i2c_buss, int i2c_address, int frequency);

	void _ext_set_pwm_pca9685 (int channel, int value);
}

static PCA9685 pwm;

void _ext_init_pca9685(int i2c_buss, int i2c_address, int frequency) {
	pwm.init (i2c_buss, i2c_address);
	usleep(100 * 1000);
	pwm.setPWMFreq (frequency);
	
}

void _ext_set_pwm_pca9685 (int channel, int value) {
	pwm.setPWM(channel,0,value);	
}

int main () {

	int servoNo = 4;
	printf ("Testing testing\n");
	PCA9685 pwm2;
	pwm2.init(1,0x40);
	usleep(100 * 100);
	printf ("Setting frequency..");
	pwm2.setPWMFreq (61);
	usleep(500 * 1000);
	printf ("start run 3*2 times\n");
	

	int count = 0;
	while (count++<3) {
		printf ("Servo min: \n");
		pwm2.setPWM(servoNo,0,150);	
		
		usleep(1000 * 1000);
		printf ("Servo Max: \n");
		pwm2.setPWM(servoNo,0,600);
		
		usleep(1000 * 1000);
	}

	printf ("\n");
	//cout << "HAHA" << endl;
	return 0;
} 
