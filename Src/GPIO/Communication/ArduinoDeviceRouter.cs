// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
// 
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using R2Core.Device;

namespace R2Core.GPIO
{

	/// <summary>
	/// SerialHost handles communication to a r2I2C device(see the r2I2CDeviceRouter Arduino project).
	/// It's heavily coupled to the r2I2CDeviceRouter implementation. Any changes there should be reflected
	/// in changes in this class.
	/// </summary>
	public class ArduinoDeviceRouter : DeviceBase, IArduinoDeviceRouter {
		
		private ISerialConnection m_connection;
		private ISerialPackageFactory m_packageFactory;

		/// <summary>
		/// Delay after an error response or a bad response checksum before executing a new retry.
		/// </summary>
		private const int RetryDelay = 250;

		private readonly object m_lock = new object();

		/// <summary>
		/// Informs that a host has been reinitialized and need to be reconfigured(i.e. add devices). 
		/// </summary>
		public Action<byte> HostDidReset { get; set; }

        public int RetryCount { get; set; }

        public ArduinoDeviceRouter(string id, ISerialConnection connection, ISerialPackageFactory packageFactory) : base(id) {
			
			m_connection = connection;
			m_packageFactory = packageFactory;
            RetryCount = 7;

		}

		public override void Start() {

			if (m_connection?.Ready == false) {

				m_connection.Start();

			}

		}

		public override void Stop() {

			if (m_connection?.Ready == true) {

				m_connection.Stop();

			}

		}

		public override bool Ready { get { return m_connection.ShouldRun && m_connection?.Ready == true; } }

		public DeviceData<T>GetValue<T>(byte deviceId, int nodeId) {

            DeviceResponsePackage<T> response = Send<T>(m_packageFactory.GetDevice(deviceId, (byte)nodeId));

            return new DeviceData<T> { Id = response.Id, Value = response.Value };

        }

		public void Set(byte deviceId, int nodeId, int value) {
    
            DeviceResponsePackage<int> response = Send<int>(m_packageFactory.SetDevice(deviceId, (byte)nodeId, value));

        }

		public DeviceData<T> Create<T>(int nodeId, SerialDeviceType type, byte[] parameters) {

			DeviceRequestPackage request = m_packageFactory.CreateDevice((byte)nodeId, type, parameters);

			DeviceResponsePackage<T> response = Send<T>(request);

			return new DeviceData<T> { Id = response.Id, Value = response.Value };

		}

		public void Initialize(int host = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {

			ResetNode((byte)host);

		}

		public void Reset(int nodeId = ArduinoSerialPackageFactory.DEVICE_NODE_LOCAL) {
		
			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.Reset, NodeId = (byte)nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public bool IsNodeAvailable(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.IsNodeAvailable, NodeId = (byte)nodeId };
			DeviceResponsePackage<bool> response = Send<bool> (request);

			return response.Value;
		
		}

		public bool IsNodeSleeping(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.CheckSleepState, NodeId = (byte)nodeId };
			DeviceResponsePackage<bool> response = Send<bool> (request);
			return response.Value;

		}

		public byte[] GetNodes() {
		
			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.GetNodes };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

			return response.Value;

		}

		public void Sleep(int nodeId, bool toggle, int cycles = ArduinoSerialPackageFactory.RH24_SLEEP_UNTIL_MESSAGE_RECEIVED) {

			DeviceRequestPackage request = m_packageFactory.Sleep((byte)nodeId, toggle, (byte)cycles);
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public void PauseSleep(int nodeId, int seconds) {

			if (seconds > ArduinoSerialPackageFactory.RH24_MAXIMUM_PAUSE_SLEEP_INTERVAL) {
			
				throw new ArgumentException($"`seconds` ({seconds}) exceeds the maximum pause sleep interval of { ArduinoSerialPackageFactory.RH24_MAXIMUM_PAUSE_SLEEP_INTERVAL}.");
			
			}

			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.PauseSleep, NodeId = (byte)nodeId, Content = new byte[1] {(byte)seconds} };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

		}

		public void SetNodeId(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.SetNodeId, Id = (byte)nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

			if (!response.IsError && response.NodeId != 0) {
			
				Log.w($"Node has changed id to {response.NodeId}. I2C connection might fail.", Identifier);

			}

		}

		public byte[] GetChecksum(int nodeId) {

			DeviceRequestPackage request = new DeviceRequestPackage() { Action = SerialActionType.GetChecksum, NodeId = (byte)nodeId };
			DeviceResponsePackage<byte[]> response = Send<byte[]> (request);

			return response.Value;

		}

		/// <summary>
		/// Internal send method containing retry upon timeout
		/// </summary>
		/// <param name="request">Request.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		private DeviceResponsePackage<T> _Send<T>(DeviceRequestPackage request, int retryCount = 0) {
		
			byte[] requestData = m_packageFactory.SerializeRequest(request);

			try {
			
				byte[] responseData = m_connection.Send(requestData);
				DeviceResponsePackage<T> response = m_packageFactory.ParseResponse<T>(responseData);

				if (response.CanRetry() && retryCount < RetryCount) {
					
					if (retryCount == 3) {
						
						Log.i($"Arduino Device Router failed for '{request.Description()}'. Error: {response.Error}. Retrying...", Identifier);
					
					}

					System.Threading.Tasks.Task.Delay(RetryDelay * (retryCount * 2 + 1)).Wait();
					return _Send<T>(request, retryCount + 1);

				}

                return response;

			} catch (SerialConnectionException ex) {

				// IO exceptions can occur from time to time. Give it a few more shots, mamma.
				if (retryCount < RetryCount && 
					
					// The connection has been closed. Probably manually:
					ex.ErrorType != SerialErrorType.ERROR_SERIAL_CONNECTION_CLOSED) {
			
					Log.i($"Retry: {retryCount}. Exception: {ex.Message}. {request.Description()}", Identifier);
					System.Threading.Tasks.Task.Delay(RetryDelay * (retryCount * 2 + 1)).Wait();
					return _Send<T>(request, retryCount + 2);

				}

				throw ex;

			}

		}

		/// <summary>
		/// Sends the request to host, converts it to a DeviceResponsePackage, checks for errors and returns the package.
		/// </summary>
		/// <param name="request">Request.</param>
		private DeviceResponsePackage<T> Send<T>(DeviceRequestPackage request) {

			lock(m_lock) {

                if (m_connection?.ShouldRun != true) {

                    Log.w($"'{m_connection.Identifier}' has been Stopped! Unable to send request: '{request}'", Identifier);
                    return default(DeviceResponsePackage<T>);

                }

                if (!Ready) { throw new SerialConnectionException("Communication busy/not started.", SerialErrorType.ERROR_SERIAL_CONNECTION_FAILURE); }

				DeviceResponsePackage<T> response = _Send<T>(request);

				if (!response.IsChecksumValid) {

					throw new SerialConnectionException($"Response checksum is bad({response.Checksum}): Node: '{request.NodeId}'. Action: '{request.Action}'. ", SerialErrorType.ERROR_BAD_CHECKSUM);

				} if (response.Action == SerialActionType.Initialization) {

					// The node needs to be reset. Reset the node and notify delegate, allowing it to re-create and/or re-send
					ResetNode(response.NodeId);

                    HostDidReset?.Invoke(response.NodeId);

                    // Resend the current request
                    response = _Send<T>(request);

				} else if (response.IsError) { 

					throw new SerialConnectionException($"Response contained an error for action '{request.Action}': '{response.Error}'. Info: {response.ErrorInfo}. NodeId: {request.NodeId}.", response.Error);

				} else if (request.Action != SerialActionType.SetNodeId && response.NodeId != request.NodeId) {

					throw new SerialConnectionException($"Response node id missmatch: Requested '{request.NodeId}'. Got: '{response.NodeId}'. Request action: '{request.Action}'. Response action: '{response.Action}'.", SerialErrorType.ERROR_DATA_MISMATCH);

				} else if (!(request.Action == SerialActionType.Initialization && response.Action == SerialActionType.InitializationOk) &&
					response.Action != request.Action) {

                    Log.w("Closing connection", this.Identifier);

                    // Until there's a way to flush the stream - close the connection.
                    m_connection.Stop();

					// The .InitializationOk is a response to a successfull .Initialization. Other successfull requests should return the requested Action.
					throw new SerialConnectionException($"Response action missmatch: Expected '{request.Action}'. Got: '{response.Action}'. NodeId: {request.NodeId}.", SerialErrorType.ERROR_DATA_MISMATCH);

				}

                return response;

			}

		}

		/// <summary>
		/// Will send the Initialize request to the node and clear it's data.
		/// </summary>
		private void ResetNode(byte nodeId) {

			Send<byte[]> (new DeviceRequestPackage() {Action = SerialActionType.Initialization, NodeId = nodeId});

		}

	}
}

