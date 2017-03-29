using System;

namespace Core.Data
{
	public class LinearDataSet: ILinearDataSet<double>
	{
		private double[] m_x;
		private double[] m_y;

		public LinearDataSet (double[] xValues, double[] yValues) {

			m_x = xValues;
			m_y = yValues;

		}

		public double Interpolate(double x) {
		
			return 0;

		}
	}
}

