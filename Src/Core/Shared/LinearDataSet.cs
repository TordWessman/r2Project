using System;
using System.Collections.Generic;
using System.Linq;

namespace R2Core
{
	public class LinearDataSet: ILinearDataSet<double>
	{
		private IDictionary<double, double> m_points;

		public IDictionary<double, double> Points { get { return m_points; } }
			
		public LinearDataSet (IDictionary<double, double> points) {

			// Sort the data
			m_points = points.OrderBy(kvp => kvp.Key).ToDictionary((key) => key.Key, (value) => value.Value); 

		}

		public double Interpolate(double x) {

			if (m_points.Keys.Count == 0) { return 0; }
			else if (m_points.Keys.Count == 1 || x < m_points.Keys.First()) { return m_points.Values.First(); }	
			else if (x > m_points.Keys.Last()) { return m_points.Values.Last(); }	

			for (int i = 1; i < m_points.Keys.Count; i++) {

				if (x < m_points.Keys.ElementAt(i)) {
			
					return CalculateWeightedAverage (x, m_points.Keys.ElementAt(i - 1), m_points.Keys.ElementAt(i), m_points.Values.ElementAt(i - 1), m_points.Values.ElementAt (i));

				}
			}

			return 0;

		}

		// Simple weighted average
		private double CalculateWeightedAverage(double x, double x0, double x1, double y0, double y1) {
		
			return y0 + (x - x0) * ((y1 - y0) / (x1 - x0));

		}
	}
}

