base_path="../../.."

# AD / MCP3008:
rm $base_path/Lib/libbcm2835.so
cd $base_path/Src/bcm2835-1.26
./configure
cd src
make libbcm2835.a
cc -shared bcm2835.o -o libbcm2835.so
cp libbcm2835.so ../../../Lib/

# SERVO CONTROLLER:
pca_library="libPCA9685.*"
cd ../../PCA9685
make clean && make
cp $pca_library ../Lib/

echo "Make sure to install (i e by symlinking) libraries into /usr/lib."
