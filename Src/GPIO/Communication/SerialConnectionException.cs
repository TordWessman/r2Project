using System.IO;

namespace R2Core.GPIO
{
	/// <summary>
	/// An exeption related to serial communication
	/// </summary>
	public class SerialConnectionException : IOException {

		public SerialErrorType ErrorType { get; private set; } 

		public SerialConnectionException(string message, SerialErrorType type) : base(message, (int)type) {

			ErrorType = type;

		}

	}

}

