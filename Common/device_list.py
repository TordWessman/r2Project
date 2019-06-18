import scriptbase
import time
import clr
clr.AddReferenceToFileAndPath(r"Core.dll")
from System.Collections.Generic import List
from R2Core.Device import IDevice

class MainClass:

	def r2_init(self):
		self.l = self.device_manager.Get("log")

	def GetDevices(self,deviceIds):
		devices = List[object]()
		for deviceId in deviceIds:
			if (self.device_manager.Has(deviceId)):
				device = self.device_manager.Get(deviceId)
				devices.Add(device)
			else:
				self.l.error("Device: '" + deviceId + "' not found.")
		return devices

	def loop(self):
		return False


main_class = MainClass()
