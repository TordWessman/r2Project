using System;

namespace R2Core.Data
{
	public interface ILinearDataSet<T>
	{
		/// <summary>
		/// Will make an interpolation of the coresponding y value using the contained points information.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		T Interpolate(T x);

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>The values.</value>
		System.Collections.Generic.IDictionary<double, double> Points { get; }

	}
}

