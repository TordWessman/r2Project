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
using R2Core.Device;
using RaspberryPiDotNet;

namespace R2Core.GPIO
{
	public class AnalogInput : DeviceBase, IInputMeter<double> {
		
		private MCP3008 m_ad;
		private ILinearDataSet<double> m_dataSet;

		public AnalogInput(string id, MCP3008 ad) : base(id) {
			
			m_ad = ad;

		}
		
		public double Value {
			
			get {
				
				return  m_dataSet?.Interpolate(AnalogValue) ?? (double) AnalogValue;

			}

		}
			
		/// <summary>
		/// Used by overriding classes to resolve the value
		/// </summary>
		/// <value>The analog value.</value>
		public int AnalogValue { get { return m_ad.AnalogToDigital; } }

		public void SetData(ILinearDataSet<double> dataSet) {
		
			m_dataSet = dataSet;

		}

	}

}

