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

﻿using System;
using Core.Memory;
using Core.Device;
using System.Linq;
using Core.Scripting;
using System.Dynamic;

namespace Core.Network.Http
{
	/// <summary>
	/// Creates various http components
	/// </summary>
	public class HttpFactory : DeviceBase
	{

		public HttpFactory (string id) : base (id) {
		
		}

		public IHttpServer CreateHttpServer (string id, int port) {

			return new HttpServer (id, port);

		}

		public IJsonClient CreateJsonClient(string id, string serverUrl) {

			return new JsonClient (id, serverUrl);

		}

		/// <summary>
		/// Creates an instance of IHttpObjectReceiver capable of handling input through an IScript.
		/// </summary>
		/// <returns>The script object receiver.</returns>
		public IHttpObjectReceiver CreateRubyScriptObjectReceiver(IScript script) {
		
			return new ScriptObjectReceiver<HttpRubyIntermediate> (script);

		}

		/// <summary>
		/// Creates an endpoit that serves files. localPath is the path where files are stored, uriPath is the uri path (i.e. /images) and contentType is the contentType returned to the client (if contentType="images", the content-type header will be image/[file extension of the file being served.])
		/// </summary>
		/// <returns>The file endpoint.</returns>
		/// <param name="localPath">Local path.</param>
		/// <param name="uriPath">URI path.</param>
		/// <param name="contentType">Content type.</param>
		public IHttpEndpoint CreateFileEndpoint (string localPath, string uriPath, string contentType = "image") {

			return new HttpFileEndpoint (localPath, uriPath, contentType);

		}

		/// <summary>
		/// Creates an endpoint capable of receiving and returning Json-data..
		/// </summary>
		/// <returns>The json endpoint.</returns>
		/// <param name="deviceListenerPath">Device listener path.</param>
		/// <param name="receiver">Receiver.</param>
		public IHttpEndpoint CreateJsonEndpoint(string uriPath, IHttpObjectReceiver receiver) {

			return new HttpJsonEndpoint (uriPath, receiver);

		}

	}

}