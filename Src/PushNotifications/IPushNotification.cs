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
using System.Collections.Generic;

namespace PushNotifications
{

	/// <summary>
	/// Push notification message representation
	/// </summary>
	public interface IPushNotification
	{
		/// <summary>
		/// Which PushNotificationClientTypes are targeted
		/// </summary>
		/// <value>The client type mask.</value>
		int ClientTypeMask {get;}

		/// <summary>
		/// Message. Available for all types
		/// </summary>
		/// <value>The message.</value>
		string Message {get;}

		/// <summary>
		/// Custom metadata.
		/// </summary>
		/// <value>The values.</value>
		Dictionary<string,object> Metadata { get; }

		/// <summary>
		/// Add payload data to the message.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void AddMetadata(string key, object value);

	}

}

