require 'output'
require 'scriptbase'
#include System::Dynamic

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

	def on_receive (msg, path, headers, outputObject)
# ex	{System.IO.FileNotFoundException: No such file or directory - /home/olga/workspace/r2/./r2Project/Tes…}	System.IO.FileNotFoundException
		#outputObject.add_metadata "Content-Type", "text/html"
		#	outputObject.data = "hello"
		#	return outputObject
		if msg.file_name != nil
			name = msg.file_name

			outputObject.add_metadata "Content-Type", get_content_type(name)
			#outputObject.data = "hello"
			#return outputObject
			outputObject.data = open(name, "rb") {|io| io.read }
			

			cdp = 'attachment; filename="' + name + '"'
			outputObject.add_metadata "Content-Disposition",cdp
			#'Content-Disposition: inline; filename="July Report.pdf"'
		end
		

		return outputObject

	end

end

self.main_class = MainClass.new()

