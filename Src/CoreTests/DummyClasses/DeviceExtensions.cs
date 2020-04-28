﻿// This file is part of r2Poject.
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
using R2Core.Device;
using System;
using System.Threading;

namespace R2Core.Tests {

	public static class DeviceExtensions {

        /// <summary>
        /// Wait for the device to have ´Ready´ set to ´true´. Will throw an exception if ´timeout´ milliseconds has passed.
        /// </summary>
        /// <param name="self">Self.</param>
		public static void WaitFor(this IDevice self, double timeout = 1000) {

            DateTime startTime = DateTime.Now;
			while (!self.Ready) {

                if(startTime.AddMilliseconds(timeout) < DateTime.Now) {

                    throw new TimeoutException($"WaitFor timed out after {timeout} milliseconds.");

                }

                Thread.Sleep(50); 
            
            }

        }

	}

}
