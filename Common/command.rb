require 'scriptbase'

class MainClass < ScriptBase

	def execute command
		
		@go.result = false
		parts = command.split(" ")
		

		if (command.start_with?("add name") ||
			command.start_with?("create name") || 
			command.start_with?("add the name") ||
			command.start_with?("create the name") || 
			command.start_with?("remember name") || 
			command.start_with?("remember the name") 
		)

			@name = @go.robot.get("memory").create "name", parts.last.downcase
			puts @name.id
			@go.robot.get("memory").delete @name
		elsif (command.start_with?("record"))
			face_script = load_script "mainframe_face_remember"
			face_script.set "name", parts.last.downcase
			face_script.set "quick", true
			face_script.start
		elsif (command.start_with?("remember the face of"))
			face_script = load_script "mainframe_face_remember"
			face_script.set "name", parts.last.downcase
			face_script.set "quick", false
			face_script.start
		elsif (command.start_with?("who are you looking at"))
			face_script = load_script "mainframe_face_identify"
			face_script.start
		elsif (parts.first == "lights")

			lamp =  @go.robot.get("lamp")

			if (lamp != nil)
				if (parts.last == "on")
					lamp.value = true
				elsif (parts.last == "off")
					lamp.value = false
				else
					puts "Unknown lamp command: " + command
				end
			else
				puts "NO LAMP".red
			end
		elsif (command.start_with?("stopp talking") ||
				command.start_with?("shut up") ||
			command.start_with?("be quiet"))
			@go.robot.get("interpreter").set_reply_mode false
			say "i love you!"
		elsif (command.start_with?("start talking") ||
			command.start_with?("start answering"))
			@go.robot.get("interpreter").set_reply_mode true
			say "yay!"
		elsif (command.start_with?("what do you know about") || 
				command.start_with?("tell me what you know about"))
			name = parts.last.downcase
			ms =  @go.robot.get("memory")
			name_model = ms.get("name",name)
			
			if (name_model)
				attribute_models = name_model.get_associations("attribute")
				if (attribute_models.length > 0)
					response = ""
					attribute_models.each do |attribute_model|
						response += name_model.name + " " + attribute_model.name + ". . "						
					end
					say response
					@go.result = true
				else
					@go.result = true
					say "nothing"
				end
			end
		else
			ms =  @go.robot.get("memory")
			name_models = ms.get_memories_of_type("name")
			first_part = parts[0].downcase
			name_models.each do |name_model|
				if (first_part == name_model.name)
					attribute = ""
					
					parts[1..parts.size()].each do |part|
						attribute += part.downcase + " "
					end
					attribute_models = name_model.get_associations("attribute")
					attribute_models.each do |attribute_model|
						if (attribute_model.name == attribute)
							puts "attribute already added: #{name_model.name} #{attribute}"
							return true
						end
					end
					puts "new attribute: #{name_model.name} #{attribute}"
					attribute_model = ms.create("attribute",attribute)
					name_model.associate attribute_model
					@go.result = true
					return true
				end
			end
		end
	end
	def get_infor_about (name_model)
	end

	#loads a script if not present. will remove a script if present
	def load_script script_name

		if @go.robot.has(script_name)

			script =  @go.robot.get(script_name)

			if script.ready
				script.stop
			end

			@go.robot.get("core").remove_script script
		end

		factory = @go.robot.get("ruby_script_factory")
		script = factory.create_script script_name, script_name + ".rb"
		@go.robot.get("core").add_script script

		return script

	end


end

self.main_class = MainClass.new(self)

