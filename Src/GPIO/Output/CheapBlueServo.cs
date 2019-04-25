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
using R2Core;


namespace R2Core.GPIO
{
	public class CheapBlueServo : ServoBase
	{
		//697 138

		//private float m_denominator;
		
		public CheapBlueServo(string id, int channel, IServoController controller) :
			base( id,channel, controller) {
		
		}
		
		protected override float Denomiator {
			get {
				return 3.10556f;
			}
		}
		
		protected override float LevelModifier {
			get {
				return 138;
			}
		}
		
		
	}
}

