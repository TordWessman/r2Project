require 'output'
require 'scriptbase'

class MainClass < ScriptBase

	def setup

	end

	def get_content_type (file_name)

		extension = file_name.split('.').last.downcase
		if ["jpg", "jpeg"].include? extension
			return "image/jpg"
		elsif ["png"].include? extension
			return "image/png"
		elsif ["gif"].include? extension
			return "image/gif"
		elsif ["htm", "html"].include? extension
			return "text/html"
		else
			return "application/octet-stream"
		end

	end

	def on_receive (msg, outputObject, source)

		if msg.payload.has "FlName"

			name = msg.payload.FlName

			if File.file?(name)

				outputObject.add_metadata "Content-Type", get_content_type(name)
				outputObject.payload = open(name, "rb") {|io| io.read }

				cdp = 'attachment; filename="' + name + '"'
				outputObject.add_metadata "Content-Disposition",cdp

			else

				outputObject.add_metadata "Content-Type", "text/plain"
				outputObject.payload = "file: '" + name + "' not found."

			end
		else

			outputObject.add_metadata "Content-Type", "text/plain"
			outputObject.payload = "No file_name specified."
					
		end

		return outputObject

	end

end

self.main_class = MainClass.new()

