#export GST_DEBUG=1
#export GST_PLUGINS_PATH=/usr/lib/gstreamer-1.0

# -shared
#CC=gcc
#CPREFIX=`pkg-config gstreamer-1.0 --cflags`
#CFLAGS=`pkg-config gstreamer-1.0 --libs` -shared -Wall -fPIC -I /usr/include -L /usr/lib -lgstapp-1.0 `pkg-config glib-2.0 gobject-2.0 --libs --cflags`

CC=gcc
ifdef test
SHARED_FLAG=
else
SHARED_FLAG=-shared
endif
PREFIX=`pkg-config gstreamer-1.0 --cflags`
CFLAGS=-Wall -fPIC $(SHARED_FLAG) `pkg-config gstreamer-1.0 gio-2.0 --libs --cflags`

R2_LIB_DIR=../../../Lib/
CAMERA=r2picam
CAMERA_LIB=lib$(CAMERA).so

all:
	$(CC) $(PREFIX) RPiCamera.c -o $(CAMERA_LIB) $(CFLAGS)
	cp $(CAMERA_LIB) $(R2_LIB_DIR)

