// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
//
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using R2Core.Device;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R2Core.Network
{
    public class HttpClient : DeviceBase, IMessageClient {

        public static string DefaultHttpMethod = "POST";
        public int Timeout = 30000;

        private ISerialization m_serializer;
        private int m_lastPort;
        private string m_lastHost;

        public bool Busy { get; private set; }
        public string Address => m_lastHost;
        public int Port => m_lastPort;
        public IDictionary<string, object> Headers { get; set; }

        public string LocalAddress => throw new NotImplementedException();

        public HttpClient(string id, ISerialization serializer) : base(id) {

			m_serializer = serializer;

		}

		public INetworkMessage Send(INetworkMessage message) {
		
			return _Send(message);

		}

		public Task SendAsync(INetworkMessage message, Action<INetworkMessage> responseDelegate) {
		
			return Task.Factory.StartNew( () => {
			
				INetworkMessage response;
				Exception exception = null;

				try {
				
					response = _Send(message);

				} catch (Exception ex) {

					response = new NetworkErrorMessage(ex, message);
					exception = ex;

				}

				responseDelegate(response);

				if (exception != null) { throw exception; }

			});

		}

		private HttpMessage _Send(INetworkMessage request) {

            try {

                Busy = true;
                HttpMessage message = new HttpMessage(request);

                HttpMessage responseObject = new HttpMessage() { Headers = new Dictionary<string, object>() };

                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(message.Destination ?? "");
                m_lastPort = httpRequest.RequestUri.Port;
                m_lastHost = httpRequest.RequestUri.Host;

                httpRequest.Method = message.Method ?? DefaultHttpMethod;

                message.Headers = message.OverrideHeaders(Headers);

                byte[] requestData = message.Payload?.GetType().IsValueType == true || message.Payload != null ? m_serializer.Serialize(message.Payload) : new byte[0];

                httpRequest.ContentLength = requestData.Length;
                httpRequest.ContentType = message.ContentType;

                httpRequest.ReadWriteTimeout = Timeout;
                httpRequest.Timeout = Timeout;

                if (message.Headers != null) {

                    message.Headers.ToList().ForEach(kvp => httpRequest.Headers[kvp.Key] = kvp.Value?.ToString() ?? "");

                }

                if (requestData.Length > 0) {

                    using (Stream dataStream = httpRequest.GetRequestStream()) {

                        dataStream.Write(requestData, 0, requestData.Length);
                        dataStream.Close();

                    }

                }

                HttpWebResponse response = null;
                byte[] responseData = null;

                try {

                    response = (HttpWebResponse)httpRequest.GetResponse();

                    using (Stream responseStream = response.GetResponseStream()) {

                        MemoryStream ms = new MemoryStream();
                        responseStream.CopyTo(ms);
                        responseData = ms.ToArray();

                    }

                } catch (WebException ex) {

                    Log.w($"Connection failed: {httpRequest.RequestUri.ToString()} exception: '{ex.Message}'");

                    responseObject.Payload = new NetworkErrorDescription() { Message = ex.Message };

                    if (ex.Status == System.Net.WebExceptionStatus.ProtocolError) {

                        HttpStatusCode? status = (ex.Response as HttpWebResponse)?.StatusCode;

                        if (status.HasValue) {

                            responseObject.Code = (int)status;

                        } else {

                            responseObject.Code = NetworkStatusCode.ServerError.Raw();

                        }

                    }else {

                        responseObject.Code = NetworkStatusCode.NetworkError.Raw();

                    }

                    return responseObject;

                }
                finally
                {

                    response?.Close();

                }

                response?.Headers?.AllKeys.ToList().ForEach(key => responseObject.Headers[key] = response.Headers[key]);

                responseObject.Code = (int)response.StatusCode;

                if (response.ContentType.ToLower().Contains("text"))  {

                    responseObject.Payload = m_serializer.Encoding.GetString(responseData);

                } else if (response.ContentType.ToLower().Contains("json")) {

                    responseObject.Payload = m_serializer.Deserialize(responseData);

                } else if (response.ContentType.ToLower().Contains("xml")) {

                    responseObject.Payload = System.Text.Encoding.UTF8.GetString(responseData);

                } else {

                    responseObject.Payload = responseData;

                }

                response.Close();

                return responseObject;

            } finally {
            
                Busy = false;
            
            }

		}

		// http://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
		private byte[] ReadToEnd(System.IO.Stream stream) {
			long originalPosition = 0;

			if(stream.CanSeek)
			{
				originalPosition = stream.Position;
				stream.Position = 0;
			}

			try
			{
				byte[] readBuffer = new byte[4096];

				int totalBytesRead = 0;
				int bytesRead;

				while((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
				{
					totalBytesRead += bytesRead;

					if (totalBytesRead == readBuffer.Length)
					{
						int nextByte = stream.ReadByte();
						if (nextByte != -1)
						{
							byte[] temp = new byte[readBuffer.Length * 2];
							Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
							Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
							readBuffer = temp;
							totalBytesRead++;
						}
					}
				}

				byte[] buffer = readBuffer;
				if (readBuffer.Length != totalBytesRead)
				{
					buffer = new byte[totalBytesRead];
					Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
				}
				return buffer;
			}
			finally
			{
				if(stream.CanSeek)
				{
					stream.Position = originalPosition; 
				}
			}
		}

		public void AddClientObserver(IMessageClientObserver observer) {
		
			throw new NotImplementedException("AddClientObserver not implemented for HttpClient.");

		}

		public void StopListening() {
		
			throw new NotImplementedException("StopListening not implemented for HttpClient.");

		}

	}

}