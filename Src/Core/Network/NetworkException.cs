using System;
using R2Core.Network;

namespace R2Core
{
	public static class IMessageLoggerExtensions {
	
		public static bool IsError(this INetworkMessage self) {

			return self.Code != (int)WebStatusCode.Ok;

		}

		public static string ErrorDescription(this INetworkMessage self) {
		
			if (!self.IsError()) { return ""; }

			string description = $"Error: {self.Code}. Description: ";
			if (self.Payload is string) { return description + (string)self.Payload; }

			return description + Convert.ToString (self.Payload);

		}

	}

	public class NetworkException : ApplicationException
	{
		
		public NetworkException (INetworkMessage message) : base(message.ErrorDescription()) {

		}

	}

}

