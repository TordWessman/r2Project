using R2Core;
using R2Core.Scripting;
using R2Core.Common;

namespace r2Boilerplate {
    class MainClass {

        public BaseContainer R2Core { get; }
        private IScript m_setupScript;

        public MainClass(string dbName, string setupFileName) {

            // Use 'BaseContainer' which is a pre-configured object containing basic functionality.
            R2Core = new BaseContainer(dbName);

            // Retrieve the script factory

            IScriptFactory<IronScript> scriptFactory = R2Core.GetDeviceManager().Get<IScriptFactory<IronScript>>(Settings.Identifiers.PythonScriptFactory());

            // Add cconfiguratin paths
            scriptFactory.AddSourcePath(Settings.Paths.PythonSetup());
            scriptFactory.AddSourcePath(Settings.Paths.PythonImport());

            // Add GPIO support
            var ioFactory = new R2Core.GPIO.GPIOFactoryFactory(Settings.Identifiers.IoFactory());
            R2Core.GetDeviceManager().Add(ioFactory);

            // Create start script
            m_setupScript = scriptFactory.CreateScript(setupFileName);
            R2Core.TaskMonitor.AddMonitorable(m_setupScript);
            
            // Add start script to device manager
            R2Core.GetDeviceManager().Add(m_setupScript);

        }

        public void Start() {

            // Start background services
            R2Core.Start();

            // Start the setup script
            m_setupScript.Start();

            // Start the main runloop script console
            R2Core.RunLoop();

        }

        static MainClass main;

        public static void Main(string[] args) {

            // Create main application. "app.db" is the main database and "simple_setup" refers to Resources/Scripts/Python/PythonSetup/simple_setup.py
            main = new MainClass("app.db", "simple_setup");

            // Start (and block) the application
            main.Start();
        }
    }
}
