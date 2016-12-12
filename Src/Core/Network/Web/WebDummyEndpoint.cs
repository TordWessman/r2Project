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
using Core.Device;
using System.Collections.Specialized;

namespace Core.Network.Web
{
	/// <summary>
	/// An interpreter which never returns any data.
	/// </summary>
	public class WebDummyEndpoint: DeviceBase, IWebEndpoint
	{
		string m_path;

		public WebDummyEndpoint (string id, string path) : base(id)
		{
			m_path = path;
		}

		public byte[] Interpret(byte[] inputData, Uri uri = null, string httpMethod = null, NameValueCollection headers = null) { return new byte[0]; }
		public string UriPath { get { return m_path; } }
		public NameValueCollection ExtraHeaders { get { return new NameValueCollection(); } }
	
	}

}