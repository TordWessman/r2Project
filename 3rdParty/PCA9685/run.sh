make clean && make
if [ $? -eq 0 ]; 
then
	rm servo.exe
	g++ test_servo.cpp -o servo.exe  -L /usr/lib -I /usr/include -Wall -fPIC -lPCA9685
	./servo.exe
fi
