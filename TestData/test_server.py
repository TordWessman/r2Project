
class MainClass:

	def r2_init(self):
		self.additional_string = None

	def on_receive(self, msg, outputObject, source):
		outputObject.AddMetadata("TestHeader", "baz")

		if msg.Payload.Has("text"):

			outputObject.Payload = msg.Payload.text

			if self.additional_string is not None:
				outputObject.Payload = outputObject.Payload + self.additional_string

		elif msg.Payload.Has("ob") and msg.Payload.ob.Has("bar"):
			outputObject.Payload = {}
			outputObject.Payload["foo"] = msg.Payload.ob.bar * 10
			outputObject.Payload["bar"] = "baz"
		else:
			outputObject.Payload = "ARGH!!!"

		return outputObject

main_class = MainClass()