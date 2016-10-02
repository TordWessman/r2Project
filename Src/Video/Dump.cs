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
namespace Video
{
	public class Dump
	{
		public Dump ()
		{
		}
	}
}



/*
 * 
 * 
 * 			
					
				if (server.Server.Ready) {
					server.Server.PauseFrameFetching ();
					server.Vision.testHaarToDisk (server.Server);
					server.Server.ResumeFrameFetching ();
				}
				//viss.SaveFrameToDisk ("hej.jpg", vs); 
		public void testHaarToDisk(VideoServer server) {
			Console.WriteLine("test haar to disk started");
			string filenameOut = "delme";
			string fileName = "haarcascade_frontalface_alt.xml";
			HaarCascade haar = _ext_create_haar_cascade(fileName, 42);
			
			IplImage imgPointer = server.getFrame();
			
			CaptureObjectArray capture = _ext_haar_capture(imgPointer, haar);
			
			
			for (int i =0 ;i < capture.size; i++) {
				CaptureObject obj = _ext_get_capture_object (capture,i);
				
				Console.WriteLine("Saving for Y: " + obj.capture_bounds.y + " " );
				_ext_create_dump (filenameOut + i + ".jpg" , obj.captured_image);	
			} 
			
			if (capture.size == 0) {
				Log.d("No shit found!");
			}
			
			_ext_release_haar_cascade(ref haar);
			//_ext_release_ipl_image (imgPointer);
			_ext_release_capture_object_array(ref capture);
		}
 */


		
		/*
		unsafe public void testHaar(string filenameIn) {
			
			string filenameOut = "delme";
			string fileName = "haarcascade_frontalface_alt.xml";
			HaarCascade haar = _ext_create_haar_cascade(fileName, 42);
			
			IplImage* imgPointer = _ext_load_image(filenameIn);
			
			CaptureObjectArray capture = _ext_haar_capture(imgPointer, haar);
			
			
			for (int i =0 ;i < capture.size; i++) {
				CaptureObject obj = _ext_get_capture_object (capture,i);
				
				Console.WriteLine("THIS IS MUM: " + obj.capture_bounds.y + " " );
				_ext_create_dump (filenameOut + i + ".jpg" , obj.captured_image);	
			} 
			
			
			_ext_release_haar_cascade(ref haar);
			_ext_release_ipl_image (imgPointer);
			
			//_ext_testish (haar);
			
		}*/
 
