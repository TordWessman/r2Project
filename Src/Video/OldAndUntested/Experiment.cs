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
using R2Core.Device;
using System.Runtime.InteropServices;
using R2Core.DataManagement.Memory;
using System.Collections.Generic;
using R2Core;
using System.Linq;

namespace R2Core.Video
{
	public class Experiment : DeviceBase, IDevice
	{
		private const string dllPath = "libr2opencv.so";
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		private static extern short  _ext_sharpnes(System.IntPtr image);
		
		private IImagePointerManager m_imgStorage;

		public Experiment(string id,
		       
		                  IImagePointerManager imgStorage) : base(id) {

			m_imgStorage = imgStorage;
		}
		
		
		public void Sharpnes(IMemory nameMemory) {
			IEnumerable<IMemory> faces = (from m in nameMemory.Associations
				where m.Type == "face" select m);
			
			foreach (IMemory face in faces) {
				Log.t("Sharpnes: " + face.Id + " : " + _ext_sharpnes(m_imgStorage.LoadImage(face).Ptr));
			}
		}
		
		
		
	}
}

