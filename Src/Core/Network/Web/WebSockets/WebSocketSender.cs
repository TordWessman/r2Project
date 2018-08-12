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
//
using System;
using R2Core.Device;
using R2Core.Scripting;
using Newtonsoft.Json;
using R2Core.Data;

namespace R2Core.Network
{
	public class WebSocketSender: DeviceBase, IWebSocketSender
	{
		private string m_uriPath;
		private IWebSocketSenderDelegate m_delegate;
		private ISerialization m_serialization;

		public WebSocketSender (string id, string uriPath, ISerialization serialization) : base (id)
		{
			m_uriPath = uriPath;
			m_serialization = serialization;
		}

		public IWebSocketSenderDelegate Delegate {
			get { return m_delegate;  }
			set { m_delegate = value; }
		}

		public void Send(dynamic outputObject) {
		
			string outputString = Convert.ToString (JsonConvert.SerializeObject (outputObject)) ?? "";
			byte[] outputData = m_serialization.Encoding.GetBytes (outputString);

			if (outputData.Length > 0) {
			
				Delegate.OnSend (outputData);

			}
		
		}

		public string UriPath { get { return m_uriPath; } }

	}

}

