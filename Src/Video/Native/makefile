CC=gcc
ifdef test
SHARED_FLAG=
echo "- MAKING TEST BUILDS (non libraries) -"
else
SHARED_FLAG=-shared
endif
PREFIX=`pkg-config gstreamer-1.0 --cflags`
CFLAGS=-Wall -fPIC $(SHARED_FLAG) `pkg-config gstreamer-1.0 gio-2.0 --libs --cflags`

R2_LIB_DIR=../../../Lib/

CAMERA=r2picam
CAMERA_LIB=lib$(CAMERA).so
RECORDER=r2picamrecorder
RECORDER_LIB=lib$(RECORDER).so

all: camera recorder

camera:
	$(CC) $(PREFIX) RPiCamera.c -o $(CAMERA_LIB) $(CFLAGS)
	cp $(CAMERA_LIB) $(R2_LIB_DIR)

recorder:
	$(CC) $(PREFIX) RPiCameraRecorder.c -o $(RECORDER_LIB) $(CFLAGS)
	cp $(RECORDER_LIB) $(R2_LIB_DIR)

clean:
	rm *.so