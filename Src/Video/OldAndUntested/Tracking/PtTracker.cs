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
using Core.Device;
using TrackerPtr = System.IntPtr;
using System.Collections.Generic;
using Core;
using Video.Camera;

namespace Video
{
	public class PtTracker : DeviceBase
	{
		private IPointsTracker m_tracker;
		private TrackerPtr m_ptr;
		private CvSize m_roiSize;
		private CvPoint m_center;
		
		private int m_autoRefresh;
		private IFrameSource m_source;
		/*
		public PtTracker (string id, IPointsTracker tracker, IplImage frame) : base (id)
		{
			m_tracker = tracker;
			m_ptr = m_tracker.CreateTracker (frame);
		}*/
		
		public PtTracker (string id, IPointsTracker tracker, IFrameSource frame, CvRect roi) : base (id)
		{
			if (!frame.Ready) {
				throw new InvalidOperationException ("IFrameSource not ready!");
			}
			
			m_tracker = tracker;
			m_ptr = m_tracker.CreateTracker (frame, roi);
			m_roiSize = roi.Size;
			m_autoRefresh = 0;
			m_source = frame;
			UpdateCenter ();
		}
		
		public int AutoRefresh {
			get {
				return m_autoRefresh;
			}
			set {
				m_autoRefresh = value;
			}
		}
		
		public CvRect Container {
			get {
				
				if (Points.Count == 0) {
					//Log.w ("No points in PtTracker");
					return new CvRect (0, 0, 0, 0);
				}
				
				int x1 = int.MaxValue, y1 = int.MaxValue;
				int x2 = 0, y2 = 0;
				
				foreach (CvPoint point in Points) {
					
					if (point.x < x1) {
						x1 = point.x;
					}
					
					if (point.x > x2) {
						x2 = point.x;
					}
					
					if (point.y < y1) {
						y1 = point.y;
					}
					
					if (point.y > y2) {
						y2 = point.y;
					}
				}
				
				if (x1 < 0) {
					x1 = 0;
				}
				
				if (y1 < 0) {
					y1 = 0;
				}
				
				if (x2 > m_source.Width) {
					x2 = m_source.Width;
				}
				
				if (y2 > m_source.Height) {
					y2 = m_source.Height;
				}
				
				return new CvRect (x1, y1, x2 - x1, y2 - y1);
			}
		}
		
		public IList<CvPoint> Points {
			get {
				IList<CvPoint> points = new List<CvPoint> ();
				for (int i = 0; i < Size; i++) {
					points.Add (m_tracker.GetPoint (m_ptr, i));
				}
					           
				return points;
			}
		}
		
		public int Size { get {
				return m_tracker.CountPoints (m_ptr);
			}
		}
		
		public int Update ()
		{

			if (!m_source.Ready) {
				return 0;
			}
			
			int count = m_tracker.UpdateTracker (m_source, m_ptr);
		
			if (m_autoRefresh > 0 && count < m_autoRefresh) {
				return Refresh (m_source);
			} else {
				UpdateCenter ();
			
				return count;
			}
			
			
		}
		
		private void UpdateCenter ()
		{
			CvRect container = Container;
			
			if (!container.IsEmpty) {
				m_center = container.Center;
			}
		}
		
		public int Refresh (IFrameSource source)
		{
			
			if (!m_source.Ready) {
				return 0;
			}
			//m_tracker.ReleaseTracker (m_ptr);
			int x = m_center.x - m_roiSize.width / 2;
			int y = m_center.y = m_roiSize.height / 2;
			int width = m_roiSize.width;
			int height = m_roiSize.height;
			
			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (x + width > source.Width) 
				width = source.Width - x;
			if (y + height > source.Height)
				height = source.Height - y;
			
			CvRect roi = new CvRect (
				x,
				y,
				width,
				height);
			
			roi.BindTo (source.Width, source.Height);
				
			m_ptr = m_tracker.CreateTracker (source, roi);
			m_roiSize = roi.Size;
			
			UpdateCenter ();
			
			return m_tracker.CountPoints (m_ptr);
		}
		
		~PtTracker ()
		{
			//TODO:?
			//m_tracker.ReleaseTracker (m_ptr);
		}
	}
}

