require 'output'

#All scripts must inherit from this base file!

class ScriptBase
	
	attr_accessor :should_run
	attr_accessor :device_manager

	def r2_init

		@should_run = true
		@settings = @device_manager.get("settings")
		@log = @device_manager.get(@settings.i.logger)
		OP::set_logger @log
	end
	
	def enable
		@should_run = true
	end

	def disable
		@should_run = false
	end

end
