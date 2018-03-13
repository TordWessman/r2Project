using System;
using Core.Device;
using System.Collections.Generic;
using Core;
using System.IO;

namespace GPIO
{

	public class SerialConnectionException : IOException {
	
		private SerialErrorType m_errorType;

		public SerialErrorType ErrorType { get { return m_errorType; } } 

		public SerialConnectionException(string message, SerialErrorType type) : base (message, (int) type) {
		
			m_errorType = type;
		
		}
	}

	public class SerialHost: DeviceBase, ISerialHost
	{
		private ISerialConnection m_connection;
		private ISerialPackageFactory m_packageFactory;

		private Action<byte> m_delegate;

		private readonly object m_lock = new object();

		/// <summary>
		/// Informs that a host has been reinitialized and need to be reconfigured (i.e. add devices). 
		/// </summary>
		public Action<byte> HostDidReset { 
			get { return m_delegate; }
			set { m_delegate = value; }
		}

		public SerialHost (string id, ISerialConnection connection, ISerialPackageFactory packageFactory) : base(id) {
			
			m_connection = connection;
			m_packageFactory = packageFactory;

		}

		public override void Start () {

			if (m_connection?.Ready == false) {

				m_connection.Start ();

			}

		}

		public override void Stop () {

			if (m_connection?.Ready == true) {

				m_connection.Stop ();

			}

		}

		public override bool Ready { get { return m_connection?.Ready == true; } }

		public DeviceData<T> GetValue<T>(byte slaveId, int nodeId) {

			DeviceResponsePackage<T> response = Send<T> (m_packageFactory.GetDevice (slaveId, (byte)nodeId));

			return new DeviceData<T>() { Id = response.Id, Value = response.Value };

		}

		public void Set(byte slaveId, int nodeId, int value) {

			DeviceResponsePackage<int> response = Send<int> (m_packageFactory.SetDevice (slaveId, (byte)nodeId, value));

		}

		public DeviceData<T> Create<T>(int nodeId, SerialDeviceType type, byte[] parameters) {

			DeviceRequestPackage request = m_packageFactory.CreateDevice ((byte)nodeId, type, parameters);

			DeviceResponsePackage<T> response = Send<T> (request);

			return new DeviceData<T>() { Id = response.Id, Value = response.Value };

		}

		public void Initialize(int host = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			ResetSlave ((byte)host);

		}

		public void Reset(int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {
		
			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.Reset, NodeId = (byte) nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public bool IsNodeAvailable(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.IsNodeAvailable, NodeId = (byte) nodeId };
			DeviceResponsePackage<bool> response = Send<bool> (request);

			return response.Value;
		
		}

		public bool IsNodeSleeping(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.CheckSleepState, NodeId = (byte) nodeId };
			DeviceResponsePackage<bool> response = Send<bool> (request);

			return response.Value;

		}

		public byte[] GetNodes() {
		
			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.GetNodes };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

			return response.Value;

		}

		public void Sleep(int nodeId, bool toggle, int cycles = ArduinoSerialPackageFactory.RH24_SLEEP_UNTIL_MESSAGE_RECEIVED) {

			DeviceRequestPackage request = m_packageFactory.Sleep ((byte) nodeId, toggle, (byte) cycles);
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public void PauseSleep(int nodeId, int seconds) {

			if (seconds > ArduinoSerialPackageFactory.RH24_MAXIMUM_PAUSE_SLEEP_INTERVAL) {
			
				throw new ArgumentException ($"`seconds` ({seconds}) exceeds the maximum pause sleep interval of { ArduinoSerialPackageFactory.RH24_MAXIMUM_PAUSE_SLEEP_INTERVAL}.");
			
			}

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.PauseSleep, NodeId = (byte) nodeId, Content = new byte[1] {(byte) seconds} };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public void SetNodeId(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.SetNodeId, NodeId = (byte) nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public byte[] GetChecksum(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = SerialActionType.GetChecksum, NodeId = (byte) nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

			return response.Value;

		}

		// For debugging...
		public void WaitFor(int nodeId) {
		
			while (!IsNodeAvailable (nodeId)) {
				Console.Write ("x");
				System.Threading.Thread.Sleep (1000);
			}
			System.Threading.Thread.Sleep (1000);
		}

		/// <summary>
		/// Internal send method containing retry upon timeout
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		private DeviceResponsePackage<T> _Send<T>(DeviceRequestPackage request) {
		
			byte[] requestData = m_packageFactory.SerializeRequest (request);
			byte[] responseData = m_connection.Send (requestData);

			DeviceResponsePackage<T> response = m_packageFactory.ParseResponse<T> (responseData);

			return response;

		}

		/// <summary>
		/// Sends the request to host, converts it to a DeviceResponsePackage, checks for errors and returns the package.
		/// </summary>
		/// <param name="request">Request.</param>
		private DeviceResponsePackage<T> Send<T>(DeviceRequestPackage request) {

			lock (m_lock) {

				if (!Ready) { throw new System.IO.IOException ("Communication busy/not started."); }

				//Log.t ($"Sending package: {request.Action} to node: {request.NodeId}.");

				DeviceResponsePackage<T> response = _Send<T> (request);

				//Log.t ($"Receiving: {response.Action} from: {response.NodeId}. MessageId: {response.MessageId}.");

				if (response.Action == SerialActionType.Initialization) {

					// The slave needs to be reset. Reset the slave and notify delegate, allowing it to re-create and/or re-send
					ResetSlave (response.NodeId);
					//Log.t ($"Resetting slave with id: {response.NodeId}.");

					if (HostDidReset != null) { HostDidReset (response.NodeId); }

					// Resend the current request
					response = _Send<T>(request);

				} else if (response.IsError) { 

					throw new SerialConnectionException ($"Response contained an error for action '{request.Action}': '{response.Error}'. Info: {response.ErrorInfo}. NodeId: {request.NodeId}.", response.Error);

				} else if (response.NodeId != request.NodeId) {

					throw new SerialConnectionException ($"Response node id missmatch: Requested '{request.NodeId}'. Got: '{response.NodeId}'. Request action: '{request.Action}'. Response action: '{response.Action}'.", SerialErrorType.ERROR_DATA_MISMATCH);

				} else if (!(request.Action == SerialActionType.Initialization && response.Action == SerialActionType.InitializationOk) &&
					response.Action != request.Action) {

					// The .InitializationOk is a response to a successfull .Initialization. Other successfull requests should return the requested Action.
					throw new SerialConnectionException ($"Response action missmatch: Expected '{request.Action}'. Got: '{response.Action}'. NodeId: {request.NodeId}.", SerialErrorType.ERROR_DATA_MISMATCH);

				}

				return response;

			}

		}

		/// <summary>
		/// Will send the Initialize request to the host and clear it's data.
		/// </summary>
		/// <param name="host">Host.</param>
		private void ResetSlave(byte nodeId) {

			Send<byte[]> (new DeviceRequestPackage() {Action = SerialActionType.Initialization, NodeId = nodeId});

		}

	}
}

