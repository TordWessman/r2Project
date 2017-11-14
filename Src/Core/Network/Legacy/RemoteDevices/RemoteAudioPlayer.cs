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
using Core.Device;
using System.Net;

namespace Core
{
	public class RemoteAudioPlayer : RemoteDeviceBase, IAudioPlayer
	{

		public static readonly string MethodNamePlay = "play";
		public static readonly string MethodNameStop = "stop";
		public static readonly string MethodNameGetIsPlaying = "getIsPlaying";
		public static readonly string MethodNameGetFilesList = "getFiles";

		public RemoteAudioPlayer (RemoteDeviceReference reference) : base (reference) {}

		public bool IsPlaying {
			get {
				return Execute<bool> (RemoteAudioPlayer.MethodNameGetIsPlaying);
			}
		}
			
		public void Play (string fileName)
		{
			Execute<string>(RemoteAudioPlayer.MethodNamePlay, fileName);
		}

		public string[] GetFileList {
			get { 

				return Execute<string[]>(RemoteAudioPlayer.MethodNameGetFilesList);


			}
		}

	}
}

