using System;
using System.IO;

namespace R2Core.GPIO
{
	/// <summary>
	/// An exeption related to serial communication
	/// </summary>
	public class SerialConnectionException : IOException {

		private SerialErrorType m_errorType;

		public SerialErrorType ErrorType { get { return m_errorType; } } 

		public SerialConnectionException(string message, SerialErrorType type) : base(message, (int)type) {

			m_errorType = type;

		}

	}

}

