using System;
using R2Core.Network;

namespace R2Core.Scripting
{
	/// <summary>
	/// Intermediate ´INetworkMessage´ implementation used by script endpoidnts.
	/// </summary>
	public struct ScriptNetworkMessage: INetworkMessage {

		public int Code { get; set; }
		public string Destination { get; set; }
		public System.Collections.Generic.IDictionary<string, object> Headers { get; set; }
		public dynamic Payload { get; set; }

		public ScriptNetworkMessage(string destination = null) {

			Code = 0;
			Headers = new System.Collections.Generic.Dictionary<string, object>();
			Destination = destination;
			Payload = null;

		}

		public ScriptNetworkMessage(INetworkMessage message) {

			Code = message.Code != 0 ? message.Code : NetworkStatusCode.Ok.Raw();
			Destination = message.Destination;
			Headers = message.Headers ?? new System.Collections.Generic.Dictionary<string, object>();
			Payload = message.Payload;

		}

		public void AddMetadata(string key, object value) {

			if (Headers == null) {

				Headers = new System.Collections.Generic.Dictionary<string, object>();

			}

			Headers[key] = value;

		}

		public override string ToString() {

			return string.Format("[ScriptNetworkMessage: Code={0}, Destination={1}, Payload={2}]", Code, Destination, Payload);

		}

	}

}

