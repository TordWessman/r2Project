import scriptbase

# This script is used by test cases. Do not change!

class MainClass(scriptbase.ScriptBase):

	def add_42(self, siffra):
		self.d("PythonScript gets a number:")
		self.d(siffra)
		return siffra + 42

	def return_katt_times_10(self):
		return self.katt * 10

	def dog_becomes_value(self, value):
		self.dog = value

	def loop(self):
		self.d("A")
		return True


main_class = MainClass()

