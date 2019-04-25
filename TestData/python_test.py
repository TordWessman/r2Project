import scriptbase
import time

class MainClass:

	def r2_init(self):
		self.settings = self.device_manager.Get("settings")
		self.l = self.device_manager.Get("log")

	def add_42(self, aNumber):
		#self.l.message(aNumber)
		return aNumber + 42

	def wait_and_return_value_plus_value(self,aValue):
		time.sleep(0.25)
		return aValue + aValue

	def return_katt_times_10(self):
		return self.katt * 10

	def dog_becomes_value(self, value):
		self.dog = value

	def loop(self):
		self.l.message("A")
		return False

main_class = MainClass()