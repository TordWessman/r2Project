﻿// This file is part of r2Poject.
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
using R2Core.Network;
using System.Collections.Specialized;
using R2Core.Scripting;
using IronRuby.Builtins;
using System.Dynamic;
using System.Collections.Generic;
using System.Net;

namespace R2Core.Scripting.Network
{
	/// <summary>
	/// <para>Uses an IScript implementation to handle input. The receiver objects MainClass must implement the on_receive(dynamic inputObject, string path, IDictionary<string, object> metadata, IWebIntermediate outputObject).</para>
	/// <para>The type T must be an IWebIntermediate implementation used to transcibe data from script to sub system.</para>
	/// The outputObject is required for the script implementation to know how to return a data type compatible with a serializer.
	/// </summary>
	public class ScriptObjectReceiver<T>: IWebObjectReceiver where T: IWebIntermediate, new() {

		public static readonly string ON_RECEIVE_METHOD_NAME = "on_receive";

		private IScript m_script;
		private string m_path;

		/// <summary>
		/// Initializes a new instance of the <see cref="R2Core.Scripting.Network.ScriptObjectReceiver`1"/> class.
		/// ´script´ must conform to the requirements(i.e. have a ´ON_RECEIVE_METHOD_NAME´ method). ´path´ will be translated to an
		/// URI-path when determening if the containing IWebEndpoint should respond using this IWebObjectReceiver. 
		/// </summary>
		/// <param name="script">Script.</param>
		/// <param name="path">Path.</param>
		public ScriptObjectReceiver(IScript script, string path) {

			m_script = script;
			m_path = path;

		}

		public string DefaultPath { get { return m_path; } }

		public INetworkMessage OnReceive(INetworkMessage message, IPEndPoint source) {
			
			T response = m_script.Invoke(ON_RECEIVE_METHOD_NAME, message, new T(), source);

			response.CLRConvert();

			return response;

		}

	}

}