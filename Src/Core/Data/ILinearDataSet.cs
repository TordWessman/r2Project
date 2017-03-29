using System;

namespace Core.Data
{
	public interface ILinearDataSet<T>
	{
		/// <summary>
		/// Will make an interpolation of the coresponding y value using the contained points information.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		T Interpolate(T x);

	}
}

