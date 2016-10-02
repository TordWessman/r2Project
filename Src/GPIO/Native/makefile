CC=g++
CFLAGS=-Wall -fPIC -shared -I /usr/include -L /usr/lib -lPCA9685
R2_LIB_DIR=../../../Lib/
R2_SRC_DIR=../../../Src/
SERVO=r2servo
SERVO_LIB=lib$(SERVO).so

all: servo

servo:
	$(CC) $(SERVO).cpp -o $(SERVO_LIB) $(CFLAGS)
	cp $(SERVO_LIB) $(R2_LIB_DIR)
	echo "Make sure to install (i e by symlinking) libraries into /usr/lib."
	
clean:
	rm *.so
	rm $(R2_LIB_DIR)$(SERVO_LIB)

