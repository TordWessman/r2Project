using System;

namespace R2Core
{
	/// <summary>
	/// Implementations allows member invocations using method/property handles as strings.
	/// </summary>
	public interface IInvokable {

		/// <summary>
		/// Set the variable identified by handle to value.
		/// </summary>
		/// <param name="handle">Handle.</param>
		/// <param name="value">Value.</param>
		void Set(string handle, dynamic value);

		/// <summary>
		/// Returns the value of variable identified by handle
		/// </summary>
		/// <param name="handle">Handle.</param>
		dynamic Get(string handle);

		/// <summary>
		/// Invokes a method using provided arguments(args) and returning result(or null if a void function invoced)
		/// </summary>
		/// <param name="args">Arguments.</param>
		dynamic Invoke(string handle, params dynamic[] args);

	}
}

