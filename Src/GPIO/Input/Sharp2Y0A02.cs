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
using RaspberryPiDotNet;
using System.Collections.Generic;
using Core;

namespace GPIO
{
	public class Sharp2Y0A02 : InputMeterBase
	{
		
		private IDictionary<int,int> m_refs;
		public Sharp2Y0A02 (string id, MCP3008 ad) : base (id, ad)
		{
			m_refs = new Dictionary<int, int> ();
			m_refs.Add (120, 150);
			m_refs.Add (200, 90);
			m_refs.Add (240, 70);
			m_refs.Add (333, 50);
			m_refs.Add (406, 40);
			m_refs.Add (533, 30);
			m_refs.Add (694, 20);
			
			
		}

/*
- 0.45 => 140, 0.5 => 130, .55 => 120, .65 => 110 0.7 => 100, 
0.45 = x/110 => 150 ,

0.75 => 90   
0.9 => 70
1.25 => 50
1.52 => 40
2.0 => 30
2.6 => 20
			//int a = 600;
			//(65.0 * Math.Pow ((float)a / 267.0, -1.1))
			// return (int) (65.0 * Math.Pow ((float)MCPOutput / 267.0, -1.1));
 */

		
		protected override int ResolveValue ()
		{
			int analog = AnalogValue;
			float output = (float)analog;

			if (output == 0) {
				Log.w ("Input was zero for Sharp2Y0A02. Device is not running");
			}

			float prev = 0;
			float p_val = 0;

			foreach (int r in m_refs.Keys) {
				
				if (r == output) {
					return m_refs [r];
				} else if (output < r) {
					if (prev == 0) {
						return 151;
					}
					float val = m_refs [r];
					float rval = r;
					return (int) (p_val - (output - prev) * ((p_val - val) / (rval - prev)));
	
				}
			
				prev = r;
				p_val = m_refs [r];

			}
			
			return 19;
		}
		
	}
}

