CC=gcc
PREFIX=`pkg-config gstreamer-1.0 --cflags`
CFLAGS=-Wall -fPIC -shared `pkg-config gstreamer-1.0 --libs` -I /usr/include -L /usr/lib -lgstapp-1.0  `pkg-config glib-2.0 gobject-2.0 --libs --cflags`
R2_LIB_DIR=../../../Lib/
ESPEAK=r2espeak
MP3=r2mp3
SPHINX=r2sphinx
ESPEAK_LIB=lib$(ESPEAK).so
MP3_LIB=lib$(MP3).so
SPHINX_LIB=lib$(SPHINX).so


all: espeak mp3 sphinx

espeak:
	$(CC) $(PREFIX) $(ESPEAK).c -o $(ESPEAK_LIB)  $(CFLAGS)
	cp $(ESPEAK_LIB) $(R2_LIB_DIR)

mp3:
	$(CC) $(PREFIX) $(MP3).c -o $(MP3_LIB) $(CFLAGS)
	cp $(MP3_LIB) $(R2_LIB_DIR)

sphinx:
	$(CC) $(PREFIX) $(SPHINX).c -o $(SPHINX_LIB) $(CFLAGS)
	cp $(SPHINX_LIB) $(R2_LIB_DIR)

clean:
	rm *.so
	rm $(R2_LIB_DIR)$(MP3_LIB)
	rm $(R2_LIB_DIR)$(ESPEAK_LIB)
	rm $(R2_LIB_DIR)$(SPHINX_LIB)

