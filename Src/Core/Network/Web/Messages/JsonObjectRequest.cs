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
using System.Dynamic;

namespace Core.Network.Web {

	/// <summary>
	/// Response for object requests.
	/// </summary>
	public class JsonObjectResponse {

		/// <summary>
		/// Containing the object to be invoked.
		/// </summary>
		public dynamic Object;

		/// <summary>
		/// If a value or property changed, the result of the method will be stored here (ignored if ActionType is Get).
		/// </summary>
		public dynamic ActionResponse;

		/// <summary>
		/// The name of the method or property wich was invoked (or null if no changes was made).
		/// </summary>
		public string Action;

	}

	/// <summary>
	/// Message used to invoke and retrieve data from objects.
	/// </summary>
	internal class JsonObjectRequest
	{

		/// <summary>
		/// Actions to perform on a requested object.
		/// </summary>
		public enum ActionType 
		{
			/// <summary>
			/// Will only request the object data to be returned
			/// </summary>
			Get = 0,

			/// <summary>
			/// Sets a specific property to the first param
			/// </summary>
			Set = 1,

			/// <summary>
			/// Invokes a method using params (if any).
			/// </summary>
			Invoke = 2,

		}

		/// <summary>
		/// Containing an identifier for the object.
		/// </summary>
		public dynamic Id;

		/// <summary>
		/// Type of action to be performed (ignored if ActionType is Get).
		/// </summary>
		public ActionType Type;

		/// <summary>
		/// Name of the method or property which will be invoked (ignored if ActionType is Get).
		/// </summary>
		public string Action;

		/// <summary>
		/// Containing a list of parameters used while setting values and invoking methods.
		/// </summary>
		public object[] Params;

		/// <summary>
		/// Optional use for puny security.
		/// </summary>
		public string Token;

	}



}

