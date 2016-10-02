#!/bin/bash
library="tracker.exe" 
lib_path="../../../Lib/"
full_path="$lib_path$library"

if [ -e "$full_path" ]
then
	rm  "$full_path"
fi

gcc OpenCvTracking.cpp -o $library  -Wall -fPIC -I /usr/include -L /usr/lib -lopencv_highgui  -lopencv_core -lopencv_ml -lopencv_imgproc -lopencv_objdetect -lopencv_contrib -lopencv_video  `pkg-config glib-2.0 gobject-2.0 --libs --cflags`


if [ $? -eq 0 ]; 
then
	#echo "LALALAL"
	./$library
else
	exit 1;
fi


