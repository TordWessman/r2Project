import scriptbase
import time
import clr
import System.IO
from System.IO import Directory
from System.IO import Path

class MainClass:

	def list(self, extension):
		files = []
		for filePath in Directory.GetFiles(self.path):
			if extension != None and filePath.endswith(extension):
				files.append(Path.GetFileName(filePath))
			elif extension == None:
				files.append(Path.GetFileName(filePath))
		return files

	def r2_init(self):
		self.l = self.device_manager.Get("log")

	# Will be called on a server request
	def on_receive(self, msg, outputObject, source):

		if msg.Payload.Has("extension"):
			outputObject.Payload = self.list(msg.Payload.extension)
		else:
			outputObject.Payload = self.list(None)

		outputObject.AddMetadata("Content-Type", "application/json")
		return outputObject

main_class = MainClass()
