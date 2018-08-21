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
using R2Core.Device;
using R2Core.Network;
using MemoryType = System.String;
using System.Linq;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.IO;
using R2Core.Data;
using System.Collections.Generic;

namespace R2Core.Common
{
	/// <summary>
	/// Device factory for the creation of uncategorized shared devices. Most of the factory methods should be moved to a more domain specific factory.
	/// </summary>
	public class DeviceFactory : DeviceBase
	{
		private IDeviceManager m_deviceManager;

		public DeviceFactory (string id, IDeviceManager deviceManager) : base (id)
		{
			m_deviceManager = deviceManager;
		}

		public IDeviceManager DeviceManager { get { return m_deviceManager; } }

		/// <summary>
		/// Creates a simple gstreamer pipeline. Requires libr2gstparseline.so to be compiled and installed into your system library directory.
		/// </summary>
		/// <returns>The gstream.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="pipeline">Pipeline.</param>
		public IGstream CreateGstream(string id, string pipeline) {
		
			return new Gstream (id, pipeline);
		
		}

		public WebFactory CreateWebFactory(string id, ISerialization serializer) {
		
			return new WebFactory (id, serializer);

		}

		public JsonMessageFactory CreateJsonMessageFactory(string id) {
		
			return new JsonMessageFactory (id);
		
		}

		public DataFactory CreateDataFactory(string id, IEnumerable<string> searchPaths) {
		
			return new DataFactory(id, searchPaths);

		}

	}

}

