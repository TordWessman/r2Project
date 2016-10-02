CC=gcc
PREFIX=`pkg-config gstreamer-1.0 --cflags`
CFLAGS=-Wall -fPIC -shared `pkg-config gstreamer-1.0 --libs` -I /usr/include -L /usr/lib -lgstapp-1.0  `pkg-config glib-2.0 gobject-2.0 --libs --cflags`
PARSE_LINE_LIB=libr2gstparseline.so
R2_LIB_DIR=../../../Lib/

all:
	$(CC) $(PREFIX) gstparseline.c -o $(PARSE_LINE_LIB) $(CFLAGS)
	cp $(PARSE_LINE_LIB) $(R2_LIB_DIR)

clean:
	rm *.so
	rm $(R2_LIB_DIR)$(PARSE_LINE_LIB)
