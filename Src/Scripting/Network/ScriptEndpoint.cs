using System;
using R2Core.Network;
using System.Net;

namespace R2Core.Scripting
{
	/// <summary>
	/// Wraps an ´IScript´ as IWebEndpoint
	/// </summary>
	public class ScriptEndpoint: IWebEndpoint {
		
		public static readonly string ON_RECEIVE_METHOD_NAME = "on_receive";

		private IScript m_script;
		private string m_path;

		/// <summary>
		/// ´script´ must conform to the requirements(i.e. have a ´ON_RECEIVE_METHOD_NAME´ method). ´path´ will be translated to an
		/// URI-path when determening if the containing IWebEndpoint should respond using this IWebObjectReceiver. 
		/// </summary>
		/// <param name="script">Script.</param>
		/// <param name="path">Path.</param>
		public ScriptEndpoint(IScript script, string path) {

			m_script = script;
			m_path = path;

		}

		public string UriPath { get { return m_path; } }

		public INetworkMessage Interpret(INetworkMessage message, IPEndPoint source) {

			ScriptNetworkMessage response = m_script.Invoke(ON_RECEIVE_METHOD_NAME, message, new ScriptNetworkMessage(message.Destination), source);

			return response;

		}

	}
}

