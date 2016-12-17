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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Core;
using System.Linq;

namespace Audio
{

	public class Mp3Player : RemotlyAccessableDeviceBase, IAudioPlayer
	{
	
		private const string dllPath = "libr2mp3.so";

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_init_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_play_file_mp3(string fileName);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_pause_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_playing_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop_playback_mp3();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_is_initialized_mp3();

		private string m_basePath;
		private Task m_playTaks;

		public Mp3Player (string id, string basePath) :base(id)
		{
			m_basePath = basePath;
		}

		public void Play (string fileName)
		{

			if (_ext_is_initialized_mp3() != 1) {

				throw new DeviceException ("Unable to play: not initialized!");
			
			}

			fileName = m_basePath + Path.DirectorySeparatorChar + fileName;

			if (!File.Exists (fileName)) {

				throw new IOException ("FIle name " + fileName + " does not exist.");
			
			}

			if (m_playTaks != null && m_playTaks.Status == TaskStatus.Running) {
			
				_ext_stop_playback_mp3();

			}

			m_playTaks = Task.Factory.StartNew (() => {
			
				_ext_play_file_mp3(fileName);
			
			});

		}

		public bool IsPlaying {

			get {
			
				return _ext_is_playing_mp3 () == 1;
			
			}
		
		}


		public override void Start ()
		{
		
			if (_ext_is_initialized_mp3 () != 1) {

				if (_ext_init_mp3 () != 1) {
			
					throw new DeviceException("Unable to initialize mp3 player.");
				
				}

			}

		}

		public override void Stop ()
		{

			if (IsPlaying) {
			
				_ext_stop_playback_mp3();

			}

		}

		public override bool Ready {

			get {

				return _ext_is_playing_mp3() != 1 && 
					_ext_is_initialized_mp3() != 0;
			
			}
		
		}

		public string[] GetFileList {

			get { 

				string[] fileList = Directory.GetFiles (m_basePath, Settings.Consts.AudioFileExtension());
				return Array.ConvertAll (fileList, x => x.Split (Path.DirectorySeparatorChar).Last ()); 

			}

		}

		#region IRemoteDevice implementation
		public override byte[] RemoteRequest (string methodName, byte[] rawData, IRPCManager<System.Net.IPEndPoint> mgr)
		{

			if (IsBaseMethod (methodName)) {

				return ExecuteStandardDeviceMethod (methodName, rawData, mgr);
			
			}

			if (methodName ==  RemoteAudioPlayer.MethodNameGetIsPlaying) {
			
				return mgr.RPCReply<bool> (Guid, methodName, IsPlaying);
			
			} else  if (methodName ==  RemoteAudioPlayer.MethodNamePlay) {
			
				string file_name = mgr.ParsePackage<string> (rawData);
				Play (file_name);
			
			}  else if (methodName == RemoteAudioPlayer.MethodNameGetFilesList) {
			
				return mgr.RPCReply<string[]> (Guid, methodName, GetFileList);
			
			}

			return null;
		
		}

		public override RemoteDevices GetTypeId ()
		{

			return RemoteDevices.AudioPlayer;
		
		}

		#endregion

	}

}

