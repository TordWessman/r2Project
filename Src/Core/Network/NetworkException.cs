using System;
using R2Core.Network;

namespace R2Core
{

	/// <summary>
	/// Represents an exception occuring in a ´R2Core.Network´ class.
	/// </summary>
	public class NetworkException : ApplicationException {

		/// <summary>
		/// Constructor where description is retrieved from an ´INetworkMessage´
		/// </summary>
		/// <param name="message">Message.</param>
		public NetworkException(INetworkMessage message) : base(message.ErrorDescription()) { }

		/// <summary>
		/// Use ´description´ as error message.
		/// </summary>
		/// <param name="description">Error message.</param>
		public NetworkException(string description) : base(description) {}

	}

}

