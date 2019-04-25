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
//
using System;
using R2Core.Device;

namespace R2Core.Video
{
	public class VideoFactory : DeviceBase {
		
		public VideoFactory(string id) : base(id) {
		}

		public CvRect CreateRect(int x, int y, int width, int height) {

			return new CvRect() { X = x, Y = y, Width = width, Height = height };

		}
		public CvSize CreateSize(int width, int height) {

			return new CvSize() {Width = width, Height = height };

		}

		public HaarOperations CreateHaarOperator(string id, string basePath = null) {

			return new HaarOperations(id, basePath);

		}

		public CvCamera CreateCamera(string id,int width = 0, int height = 0, int cameraId = 0, int skipFrames = CvCamera.SkipFrames) {
		
			return new CvCamera(id, width, height, cameraId, skipFrames);

		}

	}

}