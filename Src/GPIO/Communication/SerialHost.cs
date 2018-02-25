﻿using System;
using Core.Device;
using System.Collections.Generic;
using Core;

namespace GPIO
{
	public class SerialHost: DeviceBase, ISerialHost
	{
		private ISerialConnection m_connection;
		private ISerialPackageFactory m_packageFactory;

		private Action<byte> m_delegate;

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

		public bool IsNodeAvailable(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = (byte)ActionType.IsNodeAvailable, NodeId = (byte) nodeId };
			DeviceResponsePackage<bool> response = Send<bool> (request);

			return response.Value;
		
		}

		public bool IsNodeSleeping(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = (byte)ActionType.CheckSleepState, NodeId = (byte) nodeId };
			DeviceResponsePackage<bool> response = Send<bool> (request);

			return response.Value;

		}

		public byte[] GetNodes() {
		
			DeviceRequestPackage request = new DeviceRequestPackage () { Action = (byte)ActionType.GetNodes };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

			return response.Value;

		}

		public void Sleep(int nodeId, bool toggle, int cycles = ArduinoSerialPackageFactory.RH24_SLEEP_UNTIL_MESSAGE_RECEIVED) {

			DeviceRequestPackage request = m_packageFactory.Sleep ((byte) nodeId, toggle, (byte) cycles);
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public void SetNodeId(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage () { Action = (byte)ActionType.SetNodeId, NodeId = (byte) nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

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
		
			byte[] responseData = m_connection.Send (request.ToBytes ());
			DeviceResponsePackage<T> response = m_packageFactory.ParseResponse<T> (responseData);

			// TODO: make this ok..
			for (int i = 0; i < 3; i++) {

				// Never re-send create packages
				if (response.Error != ErrorType.RH24_TIMEOUT || (ActionType)request.Action == ActionType.Create) { break; }
				Log.d ($"SerialHost got timeout from node. Will retry action '{(ActionType)request.Action}'...");
				response = m_packageFactory.ParseResponse<T> (m_connection.Send (request.ToBytes ()));

			}

			return response;

		}

		/// <summary>
		/// Sends the request to host, converts it to a DeviceResponsePackage, checks for errors and returns the package.
		/// </summary>
		/// <param name="request">Request.</param>
		public DeviceResponsePackage<T> Send<T>(DeviceRequestPackage request) {

			if (!Ready) { throw new System.IO.IOException ("Communication not started."); }

			Log.t ($"Sending package: {(ActionType) request.Action} to node: {request.NodeId}.");

			DeviceResponsePackage<T> response = _Send<T> (request);

			Log.t ($"Receiving: {response.Action} from: {response.NodeId}. MessageId: {response.MessageId}.");

			if (response.Action == ActionType.Initialization) {

				// The slave needs to be reset. Reset the slave and notify delegate, allowing it to re-create and/or re-send
				ResetSlave (response.NodeId);
				Log.t ($"Resetting slave with id: {response.NodeId}.");

				if (HostDidReset != null) { HostDidReset (response.NodeId); }

				// Resend the current request
				response = _Send<T>(request);

			} else if (response.IsError) { 
				
				throw new System.IO.IOException ($"Response contained an error for action '{(ActionType) request.Action}': '{response.Error}'. Info: {response.ErrorInfo}. NodeId: {request.NodeId}.");
			
			} else if (!((ActionType) request.Action == ActionType.Initialization && response.Action == ActionType.InitializationOk) &&
				response.Action != (ActionType) request.Action) {

				// The .InitializationOk is a response to a successfull .Initialization. Other successfull requests should return the requested Action.
				throw new System.IO.IOException ($"Response action missmatch: Expected '{(ActionType) request.Action}'. Got: '{response.Action}'. NodeId: {request.NodeId}.");

			}

			return response;

		}

		/// <summary>
		/// Will send the Initialize request to the host and clear it's data.
		/// </summary>
		/// <param name="host">Host.</param>
		private void ResetSlave(byte nodeId) {

			Send<byte[]> (new DeviceRequestPackage() {Action = (byte) ActionType.Initialization, NodeId = nodeId});

		}

	}
}

