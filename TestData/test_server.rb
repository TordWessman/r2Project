require 'output'
require 'scriptbase'

class MainClass < ScriptBase

	attr_accessor :additional_string

	def on_receive (msg, path, headers, outputObject)

		outputObject.add_metadata "TestHeader", "baz"

		if msg.Text != nil

			outputObject.data = msg.Text

			if @additional_string != nil

				outputObject.data = outputObject.data + @additional_string

			end 
					
		end



		return outputObject

	end

end

self.main_class = MainClass.new()

