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
using Core.Network.Http;
using System.Collections.Specialized;
using Core.Scripting;
using IronRuby.Builtins;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Dynamic;

namespace Core.Network.Http
{
	/// <summary>
	/// Uses an IScript implementation to handle input. The receiver objects MainClass must implement the on_receive(dynamic inputObject, string httpMethod, NameValueCollection headers, IHttpIntermediate outputObject).
	/// The outputObject is required for the script implementation to know how to return a data type compatible with a serializer.
	/// </summary>
	public class ScriptObjectReceiver: IHttpObjectReceiver
	{

		IScript m_script;

		public ScriptObjectReceiver (IScript script)
		{

			m_script = script;
		
		}

		public IHttpIntermediate onReceive (dynamic input, string httpMethod, NameValueCollection headers = null) {

			return m_script.MainClass.@on_receive (input, httpMethod, headers, new HttpIntermediateObject());

		}

	}

}