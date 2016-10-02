#export GST_DEBUG=1
export GST_PLUGINS_PATH=/usr/lib/gstreamer-0.10

#export LM="../../../Development/language/4734.lm"
#export DICT="../../../Development/language/4734.dic"
export LM="../../../Development/language/language.arpa"
export DICT="../../../Development/language/language.dict"
#export HMM="/usr/share/pocketsphinx/model/hmm/en_US/hub4wsj_sc_8k"

export HMM="../../../Development/hub4wsj_sc_8k"
#caps = audio/x-raw-int, rate=(int)8000, channels=(int)1, endianness=(int)1234, width=(int)16, depth=(int)16, signed=(boolean)true


#gst-launch-0.10 -v filesrc location="../../../Development/te2st.m4a" ! decodebin ! audioconvert ! audioresample ! vader ! pocketsphinx configured=true hmm=$HMM dict=$DICT lm=$LM ! filesink location="hej.txt"
#exit 0;


#-shared
#gcc `pkg-config gstreamer-1.0 --cflags`  VideoServer.c VideoServerGst.c -o VideoServer.so `pkg-config gstreamer-1.0 --libs` -shared \
#  -Wall -fPIC -I /usr/include -L /usr/lib -lgstapp-1.0  -lopencv_highgui  -lopencv_core -lopencv_ml -lopencv_imgproc -lopencv_objdetect `pkg-config glib-2.0 gobject-2.0 --libs --cflags`
gcc `pkg-config gstreamer-0.10 --cflags`  -o sphinx_stream.so sphinx_stream.c `pkg-config gstreamer-0.10 --libs` -fPIC -shared

#cp -f sphinx_stream.so ../MainFrame/bin/Debug/
#cp -f sphinx_stream.so ../Video/bin/Debug/
if [ $? -eq 0 ]; 
then
	cp -f sphinx_stream.so ../../../Lib/
else
	exit 1;
fi


