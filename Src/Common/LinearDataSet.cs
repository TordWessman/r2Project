using System.Collections.Generic;
using System.Linq;

namespace R2Core.Common
{
	
	public class LinearDataSet : ILinearDataSet<double> {
		
		public IDictionary<double,double> Points { get; private set; }
			
		public LinearDataSet(IDictionary<double, double> points) {

			// Sort the data
			Points = points.OrderBy(kvp => kvp.Key).ToDictionary((key) => key.Key, (value) => value.Value); 

		}

		public double Interpolate(double x) {

			if (Points.Keys.Count == 0) { return 0; }
			else if (Points.Keys.Count == 1 || x < Points.Keys.First()) { return Points.Values.First(); }	
			else if (x > Points.Keys.Last()) { return Points.Values.Last(); }	

			for (int i = 1; i < Points.Keys.Count; i++) {

				if (x < Points.Keys.ElementAt(i)) {
			
					return CalculateWeightedAverage(x, Points.Keys.ElementAt(i - 1), Points.Keys.ElementAt(i), Points.Values.ElementAt(i - 1), Points.Values.ElementAt(i));

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
