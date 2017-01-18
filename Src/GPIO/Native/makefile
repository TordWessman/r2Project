CC=g++
CFLAGS=-Wall -fPIC -shared -I /usr/include -L /usr/lib -lPCA9685
R2_LIB_DIR=../../../Lib/
R2_SRC_DIR=../../../Src/
SERVO=r2servo
SERVO_LIB=lib$(SERVO).so
DHT11=dht11
DHT11_LIB=lib$(DHT11).so

all: servo dht11

servo:
	$(CC) $(SERVO).cpp -o $(SERVO_LIB) $(CFLAGS)
	cp $(SERVO_LIB) $(R2_LIB_DIR)
	echo "Make sure to install (i e by symlinking) libraries into /usr/lib."
	
dht11:
	gcc $(DHT11).c -o $(DHT11_LIB) -lwiringPi -shared -Wall -fPIC -I /usr/include -L /usr/lib
	cp $(DHT11_LIB) $(R2_LIB_DIR)

clean:
	rm *.so
	rm $(R2_LIB_DIR)$(SERVO_LIB)
	rm $(R2_LIB_DIR)$(DHT11_LIB)


