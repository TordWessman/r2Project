require 'mscorlib'
require 'colors'
require 'output'
require 'scriptbase'

class MainClass < ScriptBase

	def setup
		
		@line = ""
		@line_pos = 0
		@history = Array.new
		@key_pos = 0
		@should_run = true
	end

	def loop
		

		#System::Console.set_cursor_position @key_pos, 0
		#System::Console.read_line

		key_info = System::Console.read_key true
		key = key_info.key
		if key == System::ConsoleKey.escape
			disable
		elsif key == System::ConsoleKey.up_arrow
			if (@line_pos > 0)
				@line_pos = @line_pos - 1
				@line = @history[@line_pos]
				OP::clr_cmd	
				OP::print_cmd @line + "_"
			end
		elsif key == System::ConsoleKey.down_arrow
			if (@line_pos < @history.length - 1)
				@line_pos = @line_pos + 1
				@line = @history[@line_pos]
				OP::clr_cmd	
				OP::print_cmd @line + "_"
			end
		elsif key == System::ConsoleKey.backspace
			if (@line.length > 0)
				@line = @line [0, @line.length - 1]
				OP::clr_cmd				
				OP::print_cmd @line	+ "_" 	
			end
		elsif key == System::ConsoleKey.enter

			@line = " #{@line}".strip

			if @line.length > 0
				line_end @line
			end
			
		else
			@key_pos = @key_pos + 1
			@line = @line + key_info.key_char
			OP::print_cmd @line + "_" 
			
		end
		
	end

	def line_end (line)
 
		@key_pos = 0
		@history.push @line
		@line_pos = @line_pos + 1
		OP::clr_cmd
		print "\n"
		if (interpret line) == false
			OP::msg " -- Unknown command: " + line
		end
	
		@line = ""

	end

	def interpret (line)
		
		if (line == "exit")
			disable
			return true
		elsif (line == "devices")
			@go.robot.print_devices
			return true
		elsif (line == "tasks")
			@go.taskmonitor.print_tasks
			return true
		elsif (command line)
			return true
		elsif (exec_assign line)
			return true
		elsif (exec_device line)
			return true
		elsif @did_exec
			@did_exec = false
			return true
		end		
		return false
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
			device = @go.robot.get(device_name)
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

			if @go.robot.has(script_name)

				script =  @go.robot.get(script_name)

				if script.ready
					script.stop
				end

				@go.robot.get("core").remove_script script
			end

			load_script script_name, args

			return true
		end

		return false
	end
	
	def load_script (script_name, args)
			factory = @go.robot.get("script_factory")
			script = factory.create_process script_name, script_name + ".rb", args
			@go.robot.get("core").add_script script
	end

	def exec_assign (line)
		@did_exec = false
		if line["="]
			vname = line.split("=").first
			vname = " #{vname} ".strip
			line_exp = line[line.index("=") + 1, line.length - line.index("=")  -1]
			line_exp = " #{line_exp} ".strip
			dev_result = eval ("exec_device (line_exp)")
			if @did_exec
				OP::msg dev_result
				eval ("#{vname} = dev_result") 
			else
				#puts "neej"
				eval ("#{vname} = line_exp") 
			end

			return true
		elsif line[0,1] == "@"

			OP::msg eval (line)
			return true
		end
		
		return false
	end

	# Tries to fetch a device from the robot and execute a method specified
	def exec_device (line)

		device_name = line.split(".").first
		@did_exec = false

		if !@go.robot.has(device_name)

			return nil

		end

		device = @go.robot.get(device_name)
		
		begin
			
			if (device != nil)
				@did_exec = true

				command = line[device_name.length, line.length - device_name.length]
				command_output = eval("device" + command)
				
				if (command_output != nil) 
					OP::msg command_output
					return command_output
				end
				
				return nil
			end

			#rescue System::MissingMethodException
			#	message = " -- Missing method " + command + " on device:" + device_name
			#	puts message.cyan
			rescue System::Exception => ex
				message =  " -- unable to interpret: " + line
				OP::warn message
				OP::err ex.message
				ex.backtrace.each do |ex_line|
					OP::err ex_line
				end
		end
		return nil
	end

end

self.main_class = MainClass.new(self)

