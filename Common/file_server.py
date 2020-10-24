import scriptbase
import time
import clr
import System.IO
from System.IO import Directory
from System.IO import Path
from System.IO import FileInfo
from System.IO import FileStream
from System.IO import FileMode
from System.IO import SeekOrigin
from System import Array
import mime_types

# Experimental file server implementation.
class MainClass:


	def list(self):
		files = []
		for filePath in Directory.GetFiles(self.path):
			files.append(Path.GetFileName(filePath))
		return files

	def get_content_type(self, filename):
		extension = filename.split(".")[-1]
		return mime_types.mime_types[extension]

	def r2_init(self):
		self.l = self.device_manager.Get("log")
		self.path = "_define_file_path_here_"
		self.stream = None

	def read(self, start, end):
		self.stream.Seek(start, SeekOrigin.Begin)
		data = Array.CreateInstance(System.Byte, end - start)
		self.stream.Read(data, 0, end - start)
		return data

	def openFile(self, path):
		try:
			self.stream = FileStream(path, FileMode.Open)
		except:
			self.stream.Close()
			self.stream = FileStream(path, FileMode.Open)

	def parse_file(self, filename, headers, outputObject):
		outputObject.AddMetadata("Content-Type", self.get_content_type(filename))
		path = Path.Combine(self.path,filename)
		fileInfo = FileInfo(path) 
		size = fileInfo.Length
		self.openFile(path)

		if "Range" in headers:
				rangeHeaderValue = headers["Range"].replace(" ", "")
				outputObject.Code = 206
				outputObject.AddMetadata("Accept-Rages", "bytes")
				ranges = rangeHeaderValue.strip("bytes=").split("-")
				start = int(ranges[0])
				end = size - 1
				if len(ranges) == 2 and len(ranges[1]) > 0:
					end = int(ranges[1])

				if end > size - 1:
					outputObject.Code = 416
					outputObject.Payload = "Bad range. File size: " + str(size)
				else:
					outputObject.AddMetadata("Content-Range", "bytes " + str(start) + "-" + str(end) + "/" + str(size))
					outputObject.Payload = self.read(start, end + 1)

		else:
			outputObject.Payload = self.read(0, size)

		return outputObject
	def stop(self):
		if self.stream != None:
			self.stream.Close()

	def on_receive(self, msg, outputObject, source):
		if msg.Payload.Has("name"):
			try:
				return self.parse_file(msg.Payload.name, msg.Headers, outputObject)
			finally:
				self.stream.Close()

		return outputObject

main_class = MainClass()
