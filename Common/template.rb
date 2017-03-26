require 'scriptbase'

class MainClass < ScriptBase

	def setup
		
		OP::msg "TEMPLATE"
		# set up code for ruby processes goes here

	end

	def loop
		
		# will continue to run until stopped or if returning false
		sleep (1);
		print "."

		# return true if loop should continue. Otherwise it will stop.
		return true
	end


end

self.main_class = MainClass.new()

