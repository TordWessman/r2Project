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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Core.Device;
using Core.Shared;


namespace Core
{
	public class Serializer : DeviceBase
	{
		public Serializer (string id) : base (id)
		{
		}
		
		public object ClientDeserialize (string input)
		{
			string[] vals = input.Split (',');
			
			if (vals [0] == "rect") {
				return new CvRect (vals);
			} else if (vals [0] == "point") {
				return new CvPoint (vals);
			}
			
			throw new InvalidDataException ("Unable to parse: " + input);
		}
		
		public byte []Serialize (object obj)
		{
			byte [] data = null;
			
			using (MemoryStream stream = new MemoryStream ()) {
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (stream, obj);
				
				data = stream.ToArray ();
			}
			
			
			if (data == null) {
				throw new InvalidDataException ("Unable to serialize value: " + obj.GetType ().ToString ());
			}
			
			return data;
		}
	}
}

