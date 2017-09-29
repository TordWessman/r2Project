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
using TrackerPtr = System.IntPtr;
using Core.Device;
using System.Runtime.InteropServices;
using Core;
using Video.Camera;

namespace Video
{
	public class PointsTracker : IPointsTracker
	{
		
		private const string dllPath = "OpenCvModule.so";
	
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern TrackerPtr _ext_create_tracker_roi (System.IntPtr frame, CvRect roi);
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern TrackerPtr _ext_create_tracker (System.IntPtr frame);
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern CvPoint _ext_get_point (TrackerPtr tracker, int number);
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_release_tracker (TrackerPtr tracker);
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_update_tracker (System.IntPtr frame, TrackerPtr tracker);
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_count_points (TrackerPtr tracker);
		
		public PointsTracker ()
		{

			throw new NotImplementedException ("This will probably not work.");
		}

		#region IPointsTracker implementation
		public TrackerPtr CreateTracker (IFrameSource video, CvRect roi)
		{
			video.PauseFrameFetching ();
			TrackerPtr tracker = CreateTracker (video.CurrentFrame, roi);
			video.ResumeFrameFetching ();
			
			return tracker;
		}
		
		public TrackerPtr CreateTracker (IFrameSource video)
		{
			video.PauseFrameFetching ();
			TrackerPtr tracker = CreateTracker (video.CurrentFrame);
			video.ResumeFrameFetching ();
			
			return tracker;
		}
		
		public TrackerPtr CreateTracker (IplImage frame, CvRect roi)
		{
			return _ext_create_tracker_roi (frame.Ptr, roi);
		}

		public TrackerPtr CreateTracker (IplImage frame)
		{
			return _ext_create_tracker (frame.Ptr);
		}

		public CvPoint GetPoint (TrackerPtr tracker, int pointNumber)
		{
			return _ext_get_point (tracker, pointNumber);
		}

		public void ReleaseTracker (TrackerPtr tracker)
		{
			_ext_release_tracker (tracker);
		}

		public int UpdateTracker (IplImage frame, TrackerPtr tracker)
		{
			return _ext_update_tracker (frame.Ptr, tracker);
		}
		
		public int UpdateTracker (IFrameSource video, TrackerPtr tracker) {
			video.PauseFrameFetching ();
			int pointsFound = UpdateTracker(video.CurrentFrame,tracker);
			video.ResumeFrameFetching ();
			
			return pointsFound;
		}
		
		public int CountPoints (TrackerPtr tracker)
		{
			return _ext_count_points (tracker);
		}
		#endregion
	}
}

