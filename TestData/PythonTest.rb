import sys
import time
import scriptbase

class MainClass(scriptbase.ScriptBase):

	def hund(self, siffra):
		print(siffra)
		self.e(siffra)
		return siffra + 42

	def loop(self):
		self.d("A")
		time.sleep(0.5)
		return True


main_class = MainClass()

