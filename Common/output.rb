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
		
		@@logger.message line.to_clr_string
	end

	def self.err (line)

		if !(line.is_a? String)
			line = line.to_s
		end

		@@logger.error line.to_clr_string
	end

	def self.warn (line)

		if !(line.is_a? String)
			line = line.to_s
		end

		@@logger.warning line.to_clr_string

	end

end
