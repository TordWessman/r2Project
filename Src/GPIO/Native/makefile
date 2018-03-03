CC=g++
CFLAGS=-Wall -fPIC -shared -I /usr/include -L /usr/lib -lPCA9685
R2_LIB_DIR=../../../Lib/
R2_SRC_DIR=../../../Src/
SERVO=r2servo
SERVO_LIB=lib$(SERVO).so
DHT11=dht11
DHT11_LIB=lib$(DHT11).so
r2I2C:=r2I2C
r2I2C_LIB=lib$(r2I2C).so

all: servo dht11 r2I2C

servo:
	$(CC) $(SERVO).cpp -o $(SERVO_LIB) $(CFLAGS)
	cp $(SERVO_LIB) $(R2_LIB_DIR)
	
dht11:
	gcc $(DHT11).c -o $(DHT11_LIB) -lwiringPi -Wall -fPIC -I /usr/include -L /usr/lib -shared
	cp $(DHT11_LIB) $(R2_LIB_DIR)


r2I2C::
	gcc $(r2I2C).c -o $(r2I2C_LIB) -Wall -fPIC -I /usr/include -L /usr/lib -lm -shared
	cp $(r2I2C_LIB) $(R2_LIB_DIR)

clean:
	rm *.so
	rm $(R2_LIB_DIR)$(SERVO_LIB)
	rm $(R2_LIB_DIR)$(DHT11_LIB)
	rm $(R2_LIB_DIR)$(r2I2C_LIB)


