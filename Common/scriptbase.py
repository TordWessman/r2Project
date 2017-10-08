
class ScriptBase:

	def d(self, message):
		self.logger.message(message)

	def e(self, message):
		self.logger.error(message)

	def w(self, message):
		self.logger.warning(message)

	def r2_init(self):
		self.logger = self.device_manager.Get("log")


