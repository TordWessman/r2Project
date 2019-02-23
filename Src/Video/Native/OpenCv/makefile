CC=gcc
#not working, but required for tracking etc. missing contrib
#CFLAGS=-shared  -Wall -fPIC -I /usr/include -L /usr/lib  -lopencv_highgui  -lopencv_core -lopencv_ml -lopencv_imgproc -lopencv_objdetect -lopencv_contrib -lopencv_video `pkg-config glib-2.0 gobject-2.0 --libs --cflags`
CFLAGS=-shared  -Wall -fPIC -I /usr/include/opencv2 -L /usr/lib -L /usr/lib/x86_64-linux-gnu -lopencv_highgui -lopencv_videoio -lopencv_core -lopencv_ml -lopencv_imgproc -lopencv_objdetect -lopencv_video `pkg-config glib-2.0 gobject-2.0 --libs --cflags`
R2_LIB_DIR=../../../../Lib/
OPENCV=r2opencv
OPENCV_LIB=lib$(OPENCV).so

all:
	$(CC) OpenCvBase.cpp OpenCvObjDetect.cpp OpenCvFaceRecognize.cpp OpenCvFaceRecAddons.cpp OpenCvFaceRecognizeMethods.cpp OpenCvTracking.cpp -o $(OPENCV_LIB) $(CFLAGS)
	cp $(OPENCV_LIB) $(R2_LIB_DIR)

base:
	$(CC) OpenCvBase.cpp OpenCvCamera.cpp OpenCvObjDetect.cpp -o $(OPENCV_LIB) $(CFLAGS)
	cp $(OPENCV_LIB) $(R2_LIB_DIR)
