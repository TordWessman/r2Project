//http://www.uugear.com/portfolio/dht11-humidity-temperature-sensor-module/

// Install wiringpi in order for this to work..

void _ext_dht11_init(int pin);

int _ext_dht11_get_temp_h();
int _ext_dht11_get_temp_l();

int _ext_dht11_get_h_h();
int _ext_dht11_get_h_l();

void _ext_dht11_start();

void _ext_dht11_stop();

bool _ext_dht11_is_running();
