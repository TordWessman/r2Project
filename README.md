** r2Project ** is an experimental framework for building IOT stuff mainly targeting Raspberry Pi.

# WIP...

### Project structure

The project is organized in a three-tier structure, separating application logic (configuration & behaviour) and framework implementation (core libraries and their native dependencies).

 - **[Application Layer (Python/Ruby/Lua)]**

	Here be all application logic, setup etc.


 - **[Framework layer (C# Projects)]**

	This layer should be static through a projects lifecycle and only provide the modules the Application Layer requires.


 - **[Native Layer (Various native submodules required by the Framework layer)]**

	Here be the frameworks connection to various third part libraries, I/O-functionality etc.


### Direcotory structure

./Src - *contains r2Project source code.*

./Src/[XYZ]/Native - *contains native code. Each project has it's own native dependencies and thus its own native folder.*

./Lib - *Should contain the compiled native libraries mentioned above.*

./Common - *Shared files, such as shared scripts, templates, test data etc.*

./3rdParty - *Third party libraries.*
 
./Arduino - *Arduino related projects.*

# Basic installation

Download the latest version of Raspbian (https://www.raspberrypi.org/downloads/raspbian/) and install on sd-card.

If using ARMv6 (Raspberry Pi model A, B1, Zero etc.) download the older wheezy image in order for the installation to work (due to hard floating point conversion compatibility problems).
https://downloads.raspberrypi.org/raspbian/images/raspbian-2015-05-07/

## Base packages
Add the latest mono packages, python and more build tool support.
Mono build tools and python are the main dependencies.

In order to install mono on your operating system, please check out https://www.mono-project.com/download/stable/. 

### Example on how to install required runtimes on Raspbian 10
```bash
	$ sudo apt-get install apt-transport-https dirmngr gnupg ca-certificates
	$ sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
	$ echo "deb https://download.mono-project.com/repo/debian stable-raspbianbuster main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
	$ sudo apt-get update
```
### Install packages
Core packages for building and developing.
```bash
	$ sudo apt-get install mono-xbuild gstreamer-1.0 bison mono-complete mono-addins-utils mono-devel mono-dmcs python nunit dirmngr build-essential bc cmake autoconf libtool
```
### Install Nuget Package Manager
```bash
	$ sudo curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
```

### Install T4 template processor
```bash
	$ sudo mono /usr/local/bin/nuget.exe install Mono.TextTransform -OutputDirectory /usr/local/bin/
	$ sudo chmod +x /usr/local/bin/Mono.TextTransform.1.0.0/tools/TextTransform.exe
```

## Other packages
Many of these libraries are required by some common sub packages.

### GStreamer
GStreamer is used by various components in the project.

```bash
	$ sudo apt-get install libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev gstreamer1.0-plugins-good libespeak-dev gstreamer1.0-tools gstreamer1.0-plugins-ugly gstreamer1.0-plugins-bad portaudio19-dev libjack-dev libjack0 libgstreamer-plugins-bad1.0-dev gstreamer1.0-alsa gstreamer1.0-libav
```
### Native libraries
A few modules in the project requires native support of various native libraries. The sections below contains installation steps for each of them.

*Required process in order for any of the projects native libraries to work.*

- Add Lib (i.e. /home/[user]/workspace/r2/r2Project/Lib) path to ld library paths (This needs to be done in order for the system to locate the native libraries.)
```bash
	$ sudo nano /etc/ld.so.conf.d/r2.libs.conf
```

- Reload configuration: **THIS NEEDS TO BE DONE WHENEVER YOU COMPILE/ADD A NEW NATIVE LIBRARY TO r2Project/Lib**
```bash
	$ sudo ldconfig
```

# Raspberry pi I/O

Enable i2c/spi drivers through the advanced settings in raspi-config

```bash
	$ sudo raspi-config
```

Make sure the i2c and spi functionality is enabled in '/boot/config.txt' and '/etc/modules', but commented out (if present) in '/etc/modprobe.d/raspi-blacklist.conf'. 

Your '/etc/modules' should look something like this:
```bash
	i2c-bcm2708
	i2c-dev
	spi-dev
```
### Libraries required for shared I/O-functionality.

Download the from C library for Broadcom BCM 2835 http://www.open.com.au/mikem/bcm2835/index.html

```bash
	$ wget http://www.open.com.au/mikem/bcm2835/bcm2835-1.68.tar.gz
	$ tar xvf bcm2835-1.68.tar.gz
	$ cd bcm2835-1.68
	$ ./configure --prefix=/usr
	$ make
	$ sudo make install
	$ cd src
	$ cc -shared bcm2835.o -o libbcm2835.so
	$ sudo cp libbcm2835.so /usr/lib/
```

### PCA9685 
(Servo drivers)
```bash
	$ cd r2Project/3rdParty/PCA9685
	$ make
	$ sudo sh install_lib.sh
```

### r2I2C library
Used for custom i2c communications in general and arduino communication in particular
```bash
	$ sudo apt-get install libi2c-dev 
```

### Arduino RF24 libraries
Used by the r2I2CDeviceRouter networking functions. Setup assumes that your _Arduino Library Folder_ is located at _/usr/share/arduino/libraries_
```bash
	$ git clone https://github.com/nRF24/RF24
	$ sudo cp -r RF24 /usr/share/arduino/libraries
	$ git clone https://github.com/nRF24/RF24Network
	$ sudo cp -r RF24Network /usr/share/arduino/libraries
	$ git clone https://github.com/nRF24/RF24Mesh
	$ sudo cp -r RF24Mesh /usr/share/arduino/libraries
```
In the `RF24Network_config.h` (Found at /usr/share/arduino/libraries/RF24Network folder), make sure `ENABLE_SLEEP_MODE` is defined.


### Wiring Pi
Used for DHT11 support

```bash
	$ sudo apt-get install wiringpi
```

# Audio/Video
Installation for native support for various modules within the Audio/Video projects.

### OpenCV
Used by some of the experimental vision libraries.

```bash
$ sudo apt-get install libopencv-dev
```

### ESpeak
Simple speech synthesizer

Download and install the latest espeak plugin (http://download.sugarlabs.org/sources/honey/gst-plugins-espeak/)
```bash
	$ wget http://download.sugarlabs.org/sources/honey/gst-plugins-espeak/gst-plugins-espeak-0.5.0.tar.gz
	$ tar xvf gst-plugins-espeak-0.5.0.tar.gz
	$ cd gst-plugins-espeak-0.5.0
	$ ./configure --prefix=/usr --libdir=/usr/lib/arm-linux-gnueabihf/
	# if running on a 64-bit machine: $./configure --prefix=/usr --libdir=/usr/lib/x86_64-linux-gnu/
	$ make
	$ sudo make install
```
###### If 'make' fails, try to search for and remove this string (wherever it's found) from config.status: `$wl-version-script $wl$output_objdir/$libname.ver`

### GStreamer support for Raspberry Pi camera element
Used to create the GStreamer element required to fetch video from the raspberry pi camera, i.e. for live streaming.

```bash
	$ git clone https://github.com/thaytan/gst-rpicamsrc.git
	$ cd gst-rpicamsrc
	$ ./autogen.sh --prefix=/usr --libdir=/usr/lib/arm-linux-gnueabihf/
	# if running on a 64-bit machine: $ ./autogen.sh --prefix=/usr --libdir=/usr/lib/x86_64-linux-gnu/
	$ make
	$ sudo make install
```

### Speech recongnition
CMU Sphinx & GStreamer for speech recognition capabilities (using local computer or remote device (i.e. phone) as audio input).

```bash
	$ sudo apt-get install bison libpython-dev swig
```

Download the latest version from https://sourceforge.net/projects/cmusphinx/files/sphinxbase/5prealpha/
```bash
	$ wget https://sourceforge.net/projects/cmusphinx/files/sphinxbase/5prealpha/sphinxbase-5prealpha.tar.gz
	$ tar xvf sphinxbase-5prealpha.tar.gz
	$ cd sphinxbase-5prealpha
	$ touch configure.ac aclocal.m4 configure Makefile.am Makefile.in
	$ ./configure --prefix=/usr
	$ make
	$ sudo make install
```

Download the latest version from https://sourceforge.net/projects/cmusphinx/files/pocketsphinx/5prealpha/
```bash
	$ wget https://sourceforge.net/projects/cmusphinx/files/pocketsphinx/5prealpha/pocketsphinx-5prealpha.tar.gz
	$ tar xvf pocketsphinx-5prealpha.tar.gz
	$ cd pocketsphinx-5prealpha
	$ ./configure --prefix=/usr --libdir=/usr/lib/arm-linux-gnueabihf/
	# if running on a 64-bit machine: $./configure --prefix=/usr --libdir=/usr/lib/x86_64-linux-gnu/
	$ make
	$ sudo make install
```

## Other usefull packages
```bash
	$ sudo apt-get install alsa-tools alsa-utils git
```
