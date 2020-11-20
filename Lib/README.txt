This folder contains compiled native binaries.
In order to load the librarie into the project during runtime, the path to this folder must be included in the LD_LIBRARY_PATH.

Linux (Ubuntu) example:

$ sudo nano /etc/ld.so.conf.d/r2.libs.conf
Add the path to your Lib folder (i.e. /home/tord/workspace/r2/r2Project/Lib)

Reload configuration:
$ sudo ldconfig
