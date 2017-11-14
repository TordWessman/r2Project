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
using System.Net;
using System.Threading.Tasks;
using Core;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;

namespace Core.Device
{
	/// <summary>
	/// Contains functionality for standard IDevice methods as well as typed funcitonaly that simplifies RPC requests
	/// </summary>
	public abstract class RemoteDeviceBase : IRemoteDevice
	{

		protected string m_id;
		protected IRPCManager<IPEndPoint> m_networkManager;
		protected IPEndPoint m_host;
		private Guid m_guid;
		private List<IDeviceObserver> m_observers;

		public IPEndPoint Host { get { return m_host; }}
		
		public RemoteDeviceBase (RemoteDeviceReference reference)
		{

			m_guid = reference.Guid;
			m_id = reference.Id;
			m_networkManager = reference.NetworkManager;
			m_host = reference.Host;
			m_observers = new List<IDeviceObserver> ();
			
		}
		
		protected void Execute (string methodName)
		{

			Task task = m_networkManager.RPCRequest (
				Guid,
				methodName,
				m_host);
			
			m_networkManager.TaskMonitor.AddTask ("RPC" + m_id + methodName, task);

			task.Wait ();

		}
		
		protected void Execute<T> (string methodName, T outputObject)
		{

			Task task = m_networkManager.RPCRequest<T> (
				Guid,
				methodName,
				m_host,
				outputObject);
			
			m_networkManager.TaskMonitor.AddTask ("RPC" + m_id + methodName, task);

			task.Wait ();

		}
		
		protected K Execute<K> (string methodName)
		{

			Task<K> task = m_networkManager.RPCRequest<K> (
				Guid,
				methodName,
				m_host);
			
			m_networkManager.TaskMonitor.AddTask ("RPC" + m_id + methodName, task);
			
			task.Wait ();
			
			return task.Result;

		}
		
		protected K Execute<K,T> (string methodName, T outputObject)
		{

			Task<K> task = m_networkManager.RPCRequest<K,T> (
				Guid,
				methodName,
				m_host,
				outputObject);
			
			m_networkManager.TaskMonitor.AddTask ("RPC" + m_id + methodName, task);
			
			task.Wait ();
			
			return task.Result;

		}

		/// <summary>
		/// Will notify observers of a value change
		/// </summary>
		protected void NotifyChange<T>(T value) {

			IDeviceNotification<object> deviceNotification = new DeviceNotification (m_id, GetCurrentMethod ().Name, value);
			m_observers.AsParallel ().ForAll (y => y.OnValueChanged (deviceNotification));

			//The notification to the local device should be triggered in their respeciva functions. m_networkManager.RPCRequest<IDeviceNotification<object>> (Guid, StandardRemoteDeviceMethods.NotifyChange.ToString (), m_host, deviceNotification);

		}

		public void AddObserver(IDeviceObserver observer) { m_observers.Add (observer); }

		[MethodImpl(MethodImplOptions.NoInlining)]
		private System.Reflection.MethodBase GetCurrentMethod ()
		{
			StackTrace st = new StackTrace ();
			StackFrame sf = st.GetFrame (2);

			return sf.GetMethod();
		}

		public void Start ()
		{

			Task fetch = m_networkManager.RPCRequest (Guid, StandardRemoteDeviceMethods.Start.ToString(), m_host);
			fetch.Wait ();

		}

		public void Stop ()
		{

			Task fetch = m_networkManager.RPCRequest (Guid, StandardRemoteDeviceMethods.Stop.ToString (), m_host);
			fetch.Wait ();
		
		}

		public string Identifier {

			get {

				return m_id;
			
			}
		
		}

		public Guid Guid {
		
			get {
			
				return m_guid;

			}

		}

		public bool Ready {

			get {
			
				if (!m_networkManager.HostAvailable (m_host)) {

					return false;
				
				}
				
				//FULKOD:
				//Sometimes the host-unregistering process does not fall through quick enough...
				const int CONNECTION_REFUSED_CODE = 10061;
				
				Task<bool> fetch = m_networkManager.RPCRequest<bool> (Guid, StandardRemoteDeviceMethods.Ready.ToString (), m_host);

				try {
				
					fetch.Wait ();
				
				} catch (AggregateException agx) {
				
					if (agx.InnerException is SocketException) {
						
						if (((SocketException)agx.InnerException).ErrorCode == CONNECTION_REFUSED_CODE) {

							fetch = m_networkManager.RPCRequest<bool> (Guid, StandardRemoteDeviceMethods.Ready.ToString (), m_host);
							fetch.Wait ();

							return fetch.Result;

						}
						
					}
					
					throw agx.InnerException;
					
				}
				
				return fetch.Result;
			
			}

		}

	}

}

