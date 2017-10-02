#export GST_DEBUG=1
#export GST_PLUGINS_PATH=/usr/lib/gstreamer-1.0

CC=gcc
CPREFIX=`pkg-config gstreamer-1.0 --cflags`
CFLAGS=`pkg-config gstreamer-1.0 --libs` -shared -Wall -fPIC -I /usr/include -L /usr/lib -lgstapp-1.0  -lopencv_highgui  -lopencv_core -lopencv_ml -lopencv_imgproc -lopencv_objdetect `pkg-config glib-2.0 gobject-2.0 --libs --cflags`

R2_LIB_DIR=../../../../Lib/
WEBCAM=r2webcam
WEBCAM_LIB=lib$(WEBCAM).so

all:
	$(CC) $(CPREFIX) WebCam.c WebCamGst.c -o $(WEBCAM_LIB) $(CFLAGS)
	cp $(WEBCAM_LIB) $(R2_LIB_DIR)

