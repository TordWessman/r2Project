require 'output'
require 'scriptbase'

class MainClass < ScriptBase

	attr_accessor :additional_string

	def on_receive (msg, outputObject, source)

		outputObject.add_metadata "TestHeader", "baz"

		OP::msg "GINININININ"
		if msg.payload.has "text"

			OP::msg "GINININININ"
			outputObject.payload = msg.payload.text

			if @additional_string != nil

				outputObject.payload = outputObject.payload + @additional_string

			end
		
		elsif (msg.payload.has("ob") && msg.payload.ob.has("bar"))
			
			outputObject.payload = { "foo" => (msg.payload.ob.bar * 10), "bar" => "baz" } 
		
		else

			outputObject.payload = "ARGH!½!!"
 
		end

		OP::msg "ARGH"
		return outputObject

	end

end

self.main_class = MainClass.new()

