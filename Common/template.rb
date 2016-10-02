class MainClass

	def initialize (global_object)
		@go = global_object
		@should_run = true

	end
	def setup
		
		puts "TEMPLATE"

	end

	def loop
		sleep (1);
		print "."
	end

	def stop
		@should_run = false
	end

	def should_run
		return @should_run
	end


end

self.main_class = MainClass.new(self)

