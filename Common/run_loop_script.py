import sys
import io
import scriptbase
import time
from System import MemberAccessException
import System
from System.Collections.Generic import List
import os
import clr
clr.AddReferenceToFileAndPath(r"R2Core.Scripting.dll")
import R2Core.Scripting
clr.ImportExtensions(R2Core.Scripting)

class MainClass:

	def r2_init(self):
		self.settings = self.device_manager.Get("settings")
		self.l = self.device_manager.Get(self.settings.I.Logger())
		self.task_monitor = self.device_manager.Get(self.settings.I.TaskMonitor())
		self.object_invoker = self.device_manager.Get(self.settings.I.ObjectInvoker())	

	def setup(self):
		a = 5
		# Nothing to configure

	def interpret(self,line):

		if (line == "exit"):
			self.device_manager.Get(self.settings.I.RunLoop()).Stop()
			return True
		elif (line == "devices"):
			self.device_manager.PrintDevices()
			return True
		elif (line == "tasks"):
			self.task_monitor.PrintTasks()
			return True
		elif (self.command(line)):
			return True
		elif (self.exec_device(line)):
			return True
		elif (self.exec_assign(line)):
			return True
		
		return False
	
	def load_script(self,script_name, args):

			#args not yet implemented

			script = self.device_manager.Get(self.settings.I.PythonScriptFactory()).CreateScript(script_name)
			
			self.task_monitor.AddMonitorable(script)
			self.device_manager.Add(script)
			
	
	# Execptues a custom, static command:
	def command(self,line):

		command_name = line.split(" ")[0]

		# loadp = load script process, loads = load script
		if (command_name == "load"):

			line_split = line.split(" ")

			if (len(line_split) < 2):
				self.l.error("No script name specified")
				return True
			

			script_name = line_split[1]

			args = None

			# TODO: Arghz!
			#if line_split.length > 2
			#	args = System::Array[System::Object].new (line_split [2..line_split.length-1].map { |s| s.to_s.to_clr_string })
			#end

			if (self.device_manager.Has(script_name)):
				
				script = self.device_manager.Get(script_name)
				
				if (script.Ready):

					self.task_monitor.RemoveMonitorable(script)
					script.Stop

				self.device_manager.Remove(script_name)	

			self.load_script(script_name, args)

			return True
		
		return False

	def exec_assign(self,line):
		# TODO: implement assignment
		return False

	# Tries to fetch a device from the robot and execute a method specified
	def exec_device(self,line):

		if (self.object_invoker == None):
			raise Exception("self.object_invoker not set")

		device_name = line.split(".")[0]

		if (self.device_manager.Has(device_name) != True):

			self.l.warning("ARGH! Device not found: " + device_name)
			return False

		device = self.device_manager.Get(device_name)
		
		if (len(line.split(".")) == 1):

			#just print the device type
			self.l.message(device)
			return True
		
		try:
			
			if (device != None):

				command = line[len(device_name):]
				command_output = None

				setAttrSplit = command.split("=")
				attrSplit = command.split(".")
				if (len(setAttrSplit) == 2): # Set attribute
					attrName = setAttrSplit[0][1:].strip()
					command = attrName
					self.object_invoker.Set(device, attrName, setAttrSplit[1])
				elif (len(attrSplit) == 2 and ("(" not in attrSplit[1])): # Get attribute
					attrName = attrSplit[1].strip()
					command_output = self.object_invoker.Get(device, attrName)
				else: # Invoke (currently incomplete).
					methodName = attrSplit[1].split("(")[0].strip()
					params = command.split('(')[-1].replace(')','').split(',')
					paramsArray = List[object]()
					command = methodName # improve debug output...

					for param in params:
						if (len(param.strip()) > 0):
							stringCheckSplit = param.split("\"")
							if (len(stringCheckSplit) == 3):
								paramsArray.Add(stringCheckSplit[1])	#string
							elif (len(stringCheckSplit) == 1):
								paramsArray.Add(int(param))	#other
							else:
								raise Exception("Invokation of '" + device_name + "' failed. Unable to determine parameter type : '" + param + "'")
	
					command_output = self.object_invoker.Invoke(device, methodName, paramsArray)

				if (command_output != None):
					self.l.message(command_output)
				
				return True

		except MemberAccessException as ex:
			self.l.error("Missing method/member in '" + command + "' on device '" + device_name + "'. Error message: \"" + ex.Message + "\".")

		return False
	
	def loop(self):
		#time.sleep(0.5)
		return False


main_class = MainClass()

