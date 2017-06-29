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
using Core.Device;
using Core.Data;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Network.Web
{
	public class HttpClient: DeviceBase, IHttpClient
	{
		public static string DefaultHttpMethod = "POST";

		private ISerialization m_serializer;

		public HttpClient (string id, ISerialization serializer) : base (id) {

			m_serializer = serializer;
		}

		public HttpMessage Send(HttpMessage message) {
		
			return _Send (message);

		}

		public void SendAsync(HttpMessage message, Action<HttpMessage> responseDelegate) {
		
			Task.Factory.StartNew ( () => {
			
				try {
				
					HttpMessage response = _Send(message);

					responseDelegate(response); //.DynamicInvoke(response);

				} catch (Exception ex) {
				
					responseDelegate(new HttpMessage( new HttpError() { Message = ex.Message }, (int) WebStatusCode.NetworkError));

				}

			});

		}

		private HttpMessage _Send (HttpMessage message) {
		
			HttpMessage responseObject = new HttpMessage () { Headers = new Dictionary<string, object> ()};

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(message.Destination ?? "");

			request.Method = message.Method ?? DefaultHttpMethod;

			byte[] requestData = message.Payload?.GetType().IsValueType == true || message.Payload != null ? m_serializer.Serialize(message.Payload): new byte[0];

			request.ContentLength = requestData.Length;
			request.ContentType = message.ContentType;
			request.ReadWriteTimeout = 30000;
			request.Timeout = 30000;

			if (requestData.Length > 0) {
			
				using (Stream dataStream = request.GetRequestStream()) {

					dataStream.Write(requestData, 0, requestData.Length);
					dataStream.Close ();

				}

			}

			HttpWebResponse response = null;
			byte[] responseData = null;

			try {
				
				response = (HttpWebResponse) request.GetResponse();
			
				using (Stream responseStream = response.GetResponseStream ()) {
					
					MemoryStream ms = new MemoryStream ();
					responseStream.CopyTo (ms);
					responseData = ms.ToArray ();

				}

			} catch (System.Net.WebException ex) {
				
				Log.w ( $"Connection failed: {request.RequestUri.ToString()} exception: '{ex.Message}'");
					
				responseObject.Payload = new HttpError() { Message = ex.Message };
				responseObject.Code = (int) WebStatusCode.NetworkError; 

				return responseObject;
				 
			} finally {
			
				response?.Close ();

			}

			//responseObject.Code = response?.StatusCode;

			response?.Headers?.AllKeys.ToList().ForEach( key => responseObject.Headers[key] = response.Headers[key]);

			if (response.StatusCode == HttpStatusCode.OK) {

				responseObject.Code = (int) WebStatusCode.Ok;

				if (response.ContentType.ToLower().Contains ("text")) {
					
					responseObject.Payload = m_serializer.Encoding.GetString (responseData);

				} if (response.ContentType.ToLower().Contains ("json")) {
					
					responseObject.Payload = m_serializer.Deserialize (responseData);

				} else {
					
					responseObject.Payload = responseData;

				}

			} else {
			
				string error = $"HttpRequest got bad http status code: {response.StatusCode}.";

				responseObject.Payload = new HttpError () {Message = error};
				responseObject.Code = (int) response.StatusCode;

				Log.w (error);

			}

			response.Close ();

			return responseObject;

		}

		// http://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
		private byte[] ReadToEnd(System.IO.Stream stream)
		{
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

				while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
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
	}
}

