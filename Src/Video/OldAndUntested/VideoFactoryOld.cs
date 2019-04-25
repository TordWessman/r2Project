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

namespace R2Core.Video
{
	public class VideoFactoryOld : DeviceBase, IDevice
	{
		private IImageStorage m_storage;
		private HaarOperations m_haar;
		
		public VideoFactoryOld(string id, IImageStorage storage, HaarOperations haar) : base(id) {
			m_storage = storage;
			m_haar = haar;
		}

		/*
		public IFaceRecognizer CreateFaceRecognizer (string id) {

			throw new NotImplementedException("This will probably not work");
			return new FaceRecognizer (	id, 
			                           	m_storage,
			                          	m_haar.CreateHaar ("left_eye.xml"),
			                           	m_haar.CreateHaar ("right_eye.xml"),
			                            m_haar.CreateHaar ("mouth.xml"));
			
		}
		
		public ITrackerFactory CreateTrackerFactory(string id) {
			
			throw new NotImplementedException("This will probably not work");
			return new TrackerFactory(id);
		}


		public IFrameSource CreateWebCam(string id, int width, int height) {

			throw new NotImplementedException("This will probably not work");
			return new WebCam(id, width, height);
		}


		public IFrameSource CreateVideoRouter(string id, string remoteIp, int remotePort, string localIp ) {

			throw new NotImplementedException("This will probably not work");
			return new VideoRouter (id, remoteIp, remotePort, localIp);
		}
		*/
		public CvRect CreateRect(int x, int y, int width, int height) {

			return new CvRect() { X = x, Y = y, Width = width, Height = height };

		}

		public HaarOperations CreateHaarOperator(string id, string basePath = null) {
	
			return new HaarOperations(id, basePath);

		}

	}

}

