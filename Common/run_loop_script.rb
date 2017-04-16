require 'mscorlib'
require 'colors'
require 'output'
require 'scriptbase'

class MainClass < ScriptBase

	def setup

		@vars = Array.new
		@task_monitor = @device_manager.get(@settings.i.task_monitor)

	end

	def interpret (line)

		if (line == "exit")
			return false
		elsif (line == "devices")
			@device_manager.print_devices
			return true
		elsif (line == "tasks")
			@taskmonitor.print_tasks
			return true
		elsif (command line)
			return true
		elsif (exec_device line)
			return true
		elsif (exec_assign line)
			return true
		end	

		OP::warn "Unable to interpret: " + line	

		return true
	end
	
	def set_variable (line) 
		#equals	
	end

	# Execptues a custom, static command:
	def command (line)

		command_name = line.split(" ").first

		if (command_name == "restart" || 
		    command_name == "start" || 
			command_name == "stop")
			device_name = line.split(" ").last
			device = @device_manager.get(device_name)
			if (device != nil)
				if (command_name == "restart")
					OP::msg " -- restarting: #{device_name}" 
					device.stop
					device.start
				elsif (command_name == "start")
					OP::msg " -- starting: #{device_name}"
					device.start
				elsif (command_name == "stop")
					OP::msg " -- stopping: #{device_name}"
					device.stop
				end
			else
				OP::msg " -- No device named: #{device_name}"
			end

			return true

		elsif (command_name == "load")

			line_split = line.split(" ")

			if line_split.length < 2
				OP::err "No script name specified"
				return true			
			end

			script_name = line_split[1]

			args = nil

			if line_split.length > 2
				args = System::Array[System::Object].new (line_split [2..line_split.length-1].map { |s| s.to_s.to_clr_string })
			end

			if @device_manager.has(script_name)

				script =  @device_manager.get(script_name)

				if script.ready
					script.stop
				end

				@device_manager.remove script
				@task_monitor.remove_monitorable script

			end

			load_script script_name, args

			return true
		end

		return false
	end
	
	def load_script (script_name, args)

			#args not yet implemented
			factory = @device_manager.get("script_factory")
			script = factory.create_process script_name
			@device_manager.add script
			@task_monitor.add_monitorable script
	end

	#TODO: make assign work...
	def exec_assign (line)
		
		if line.split("=").length > 1

			vname = line.split("=").first
			if vname == nil || vname.length == 0
				return false
			end

			vname = " #{vname} ".strip
			line_exp = line[line.index("=") + 1, line.length - line.index("=")  -1]
			line_exp = " #{line_exp} ".strip
			dev_result = exec_device (line_exp)

			if dev_result
				OP::msg dev_result
				eval ("@#{vname} = dev_result") 
			else
				eval ("@#{vname} = line_exp") 
			end

			return true

		
		#elsif line[0,1] == "@"
		#	OP::msg eval (line)
		#	return true
		end
		
		return false

	end

	# Tries to fetch a device from the robot and execute a method specified
	def exec_device (line)

		device_name = line.split(".").first

		if !@device_manager.has(device_name)

			return false

		end

		device = @device_manager.get(device_name)
		
		begin
			
			if (device != nil)

				command = line[device_name.length, line.length - device_name.length]
				
				command_output = eval("device" + command)
				
				if (command_output != nil) 
					OP::msg command_output
				end
				
				return true

			end

			rescue System::MissingMethodException => ex
				message = " -- Missing method " + command + " on device: " + device_name
				OP::warn message
			#rescue System::Exception => ex
			#	message =  " -- unable to interpret: " + line
			#	OP::warn message
			#	OP::err ex.message
			#	ex.backtrace.each do |ex_line|
			#		OP::err ex_line
			#	end
		end

		return false
	
	end

end

self.main_class = MainClass.new()

