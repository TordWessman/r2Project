require 'output'
require 'scriptbase'

class MainClass < ScriptBase

	attr_accessor :additional_string

	def on_receive (msg, path, headers, outputObject)

		outputObject.add_metadata "TestHeader", "baz"

		OP::msg "GINININININ"
		if msg.has "text"

			OP::msg "GINININININ"
			outputObject.data = msg.text

			if @additional_string != nil

				outputObject.data = outputObject.data + @additional_string

			end
		
		elsif (msg.has("ob") && msg.ob.has("bar"))
			
			outputObject.data = { "foo" => (msg.ob.bar * 10), "bar" => "baz" } 
		
		else

			outputObject.data = "ARGH!½!!"
 
		end

		OP::msg "ARGH"
		return outputObject

	end

end

self.main_class = MainClass.new()

