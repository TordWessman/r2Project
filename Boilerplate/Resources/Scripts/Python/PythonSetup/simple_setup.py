import time

class MainClass:

	def r2_init(self):
		self.settings = self.device_manager.Get("settings")
		self.l = self.device_manager.Get(self.settings.I.Logger())
		self.l.LogLevel = LogLevel.Info
		self.device_manager.Get(self.settings.I.FileLogger()).LogLevel = LogLevel.Info
		self.script_factory = self.device_manager.Get(self.settings.I.PythonScriptFactory())
		self.web_factory = self.device_manager.Get(self.settings.I.WebFactory())
		common_factory = self.device_manager.Get(self.settings.I.DeviceFactory())
		self.identity = common_factory.CreateIdentity("router")
		self.device_manager.Add(self.identity)
		self.setup_vars()
		self.set_up_servers()		

	def setup_vars(self):
		self.tcp_port = self.settings.C.DefaultTcpPort()
		self.http_port = self.settings.C.DefaultHttpPort()
		self.udp_port = self.settings.C.DefaultBroadcastPort()

	def set_up_servers(self):
		self.http_server = self.web_factory.CreateHttpServer(self.settings.I.HttpServer(), self.http_port)
		self.tcp_server = self.web_factory.CreateTcpServer(self.settings.I.TcpServer(), self.tcp_port)
		self.device_manager.Add(self.http_server)
		self.device_manager.Add(self.tcp_server)
		self.http_server.Start()
		self.tcp_server.Start()

		tcp_router_endpoint = self.web_factory.CreateTcpRouterEndpoint(self.tcp_server)
		self.device_manager.Add(tcp_router_endpoint)
		self.http_server.AddEndpoint(tcp_router_endpoint)
		self.tcp_server.AddEndpoint(tcp_router_endpoint)

	def setup(self):
		self.l.message("setup")

	def stop(self):
		self.should_run = False

	def loop(self):
		return False


main_class = MainClass()
