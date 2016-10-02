#!/bin/bash
export GST_DEBUG=5
export GST_DEBUG=*:5
export GST_PLUGINS_PATH=/usr/lib/gstreamer-1.0
export LD_LIBRARY_PATH=/usr/lib

library="VideoSource.so" 
lib_path="../../../Lib/"
full_path="$lib_path$library"

if [ -e "$full_path" ]
then
	rm  "$full_path"
fi


gcc `pkg-config gstreamer-1.0 --cflags`  VideoSource.c -o $library `pkg-config gstreamer-1.0 --libs` -shared -fPIC -I /usr/include -L /usr/lib `pkg-config glib-2.0 gobject-2.0 --libs --cflags`


if [ $? -eq 0 ]; 
then
	cp -f $library $lib_path
else
	exit 1;
fi


