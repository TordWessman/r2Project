require 'mscorlib'
require 'colors'

class OP

	@@logger = nil

	def self.set_logger (logger)

	@@logger = logger
		
	end

	def self.msg (line)
		if !(line.is_a? String)
			line = line.to_s
		end
		
		@@logger.debug line
	end

	def self.print_cmd (line) 
		left = System::Console.cursor_left
		top = System::Console.cursor_top
		#System::Console.set_cursor_position 0, top	
		System::Console.set_cursor_position 0, (System::Console.window_height - 1)
		print line.reverse_color #+ top.to_s()
		System::Console.set_cursor_position left, top
	end
	def self.clr_cmd
		OP::print_cmd "                                                            "
	end

	def self.err (line)
		@@logger.error line
	end

	def self.warn (line)
		@@logger.warn line
	end

	def self.success (line)
		@@logger.ok line
	end
end
