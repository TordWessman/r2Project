using System;
using System.Collections.Specialized;

namespace Core.Network.Http
{
	/// <summary>
	/// Represents an implementation capable of handdling objects through HTTP requests
	/// </summary>
	public interface IHttpObjectReceiver
	{
		/// <summary>
		/// Handles the receival of the input object using httpMethod and including the optional header fields
		/// </summary>
		/// <param name="input">Input.</param>
		/// <param name="httpMethod">Http method.</param>
		/// <param name="headers">Headers.</param>
		dynamic onReceive (dynamic input, string httpMethod, NameValueCollection headers = null);

	}
}