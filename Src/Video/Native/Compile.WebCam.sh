#!/bin/bash

export GST_DEBUG=1
export GST_PLUGINS_PATH=/usr/lib/gstreamer-1.0
#-shared

library="WebCam.so"
lib_path="../../../Lib/"
full_path="$lib_path$library"

if [ -e "$full_path" ]
then
	rm  "$full_path"
fi

gcc `pkg-config gstreamer-1.0 --cflags`  WebCam.c WebCamGst.c -o $library `pkg-config gstreamer-1.0 --libs` -shared \
  -Wall -fPIC -I /usr/include -L /usr/lib -lgstapp-1.0  -lopencv_highgui  -lopencv_core -lopencv_ml -lopencv_imgproc -lopencv_objdetect `pkg-config glib-2.0 gobject-2.0 --libs --cflags`


if [ $? -eq 0 ];
then
	cp -f $library $lib_path
else
	exit 1;
fi

