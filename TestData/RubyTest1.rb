require 'scriptbase'

class MainClass < ScriptBase

	attr_accessor :foo
	attr_accessor :bar
	
	def setup

		puts "TEMPLATE"

	end

	def set_up_foo
		@foo = "bar"
		return "baz"
	end

	def loop
		sleep (1);
		print "."
		
		#Return true if loop should continue. Otherwise it will stop.
		return true
	end


end

self.main_class = MainClass.new()
