using System;
using Core.Device;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GPIO
{
	public class DHT11: DeviceBase, IDHT11
	{
		private const string dllPath = "libdht11.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_dht11_init (int pin);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_get_temp_h();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_get_temp_l();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_get_h_h();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_get_h_l();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_start();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_dht11_stop();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern bool _ext_dht11_is_running();

		public DHT11 (string id, int pin): base (id)
		{

			_ext_dht11_init (pin);

		}

		public float Temperature {  get {
				
				return Parse (_ext_dht11_get_temp_h(), _ext_dht11_get_temp_l());
			
			}
		
		}

		public float Humidity  {  get {
				
				return Parse (_ext_dht11_get_h_h(), _ext_dht11_get_h_l());
			}
		}

		public override void Start ()
		{
			Task.Factory.StartNew (() => {

				_ext_dht11_start ();

			});

		}

		public override void Stop ()
		{
			_ext_dht11_stop ();
		}

		public override bool Ready {
			get {
				return _ext_dht11_is_running ();
			}
		}

		private float Parse(int integer, int dec) {
	
			return float.Parse( integer + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + dec);

		}
	}
}

