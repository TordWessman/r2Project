require 'output'

#All scripts must inherit from this base file!

class ScriptBase
	
	def initialize (global_object)
		@go = global_object
		@should_run = true
		@settings = @go.robot.get("settings")
		@core = @go.robot.get(@settings.i.core)
		@log = @go.robot.get(@settings.i.logger)
		OP::set_logger @log
	end
	
	def enable
		@should_run = true
	end

	def disable
		@should_run = false
	end

	def should_run
		return @should_run
	end
end
