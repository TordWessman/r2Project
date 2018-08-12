using System;
using R2Core.Device;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace R2Core.GPIO
{
	public class DHT11: DeviceBase, IDHT11
	{
		private const string dllPath = "libdht11.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_dht11_init (int pin);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_get_temp();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_get_humidity();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_start();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_stop();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_dht11_is_running();

		public DHT11 (string id, int pin): base (id)
		{

			if (!_ext_dht11_init (pin)) {
			
				throw new ApplicationException ("Unable to start DHT11. Initialization failed. Is wiringPi installed?");
			
			}

		}

		public int Temperature {  get { return _ext_dht11_get_temp(); } }

		public int Humidity  {  get { return _ext_dht11_get_humidity(); } }

		public override bool Ready { get { return _ext_dht11_is_running (); } }

		public override void Start () {
			
			Task.Factory.StartNew (() => {

				_ext_dht11_start ();

			});

		}

		public override void Stop () {
			
			_ext_dht11_stop ();
		
		}

		public IInputMeter<int> GetHumiditySensor(string id) {
		
			return new DHT11Sensor (id, this, DHT11Sensor.DHT11ValueType.Humidity);
		
		}

		public IInputMeter<int> GetTemperatureSensor(string id) {
		
			return new DHT11Sensor (id, this, DHT11Sensor.DHT11ValueType.Temperature);

		}

	}

}