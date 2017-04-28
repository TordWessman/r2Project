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
	public class HttpClient: DeviceBase
	{
		private ISerialization m_serializer;

		public HttpClient (string id, ISerialization serializer) : base (id) {

			m_serializer = serializer;
		}

		public HttpResponse Send(HttpRequest message) {
		
			return _Send (message);

		}

		public void SendAsync(HttpRequest message, Action responseDelegate) {
		
			Task.Factory.StartNew ( () => {
			
				try {
				
					HttpResponse response = _Send(message);

				} catch (Exception ex) {
				
					responseDelegate.DynamicInvoke(new HttpResponse() { Error = new HttpError() { Message = ex.Message } });

				}

			});

		}

		private HttpResponse _Send (HttpRequest message) {
		
			HttpResponse responseObject = new HttpResponse () { Headers = new Dictionary<string, object> ()};

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(message.Url);

			request.Method = message.Method;

			byte[] requestData = message.Body != null ? m_serializer.Serialize(message.Body): new byte[0];

			request.ContentLength = requestData.Length;
			request.ContentType = message.ContentType;
			request.ReadWriteTimeout = 30000;
			request.Timeout = 30000;

			using (Stream dataStream = request.GetRequestStream()) {
				
				dataStream.Write(requestData, 0, requestData.Length);
				dataStream.Close ();

			}

			HttpWebResponse response = null;

			try {
				
				response = (HttpWebResponse) request.GetResponse();
			
			} catch (System.Net.WebException ex) {
				
				Log.w ( $"Connection failed: {request.RequestUri.ToString()} exception: '{ex.Message}'");
					
				using (Stream responseStream = ex.Response.GetResponseStream ()) {
					
					StreamReader reader = new StreamReader (responseStream, m_serializer.Encoding);
					responseObject.Error = new HttpError() { Message = reader.ReadToEnd () };
				}

				responseObject.Code = (ex.Response as HttpWebResponse).StatusCode;

				return responseObject;
				 
			} finally {
			
				response?.Close ();

			}

			responseObject.Code = response?.StatusCode;

			response?.Headers?.AllKeys.ToList().ForEach( key => responseObject.Headers[key] = response.Headers[key]);

			if (response.StatusCode == HttpStatusCode.OK) {
				
				using (Stream responseStream = response.GetResponseStream ()) {

					byte[] responseData = ReadToEnd (responseStream);

					if (response.ContentType.ToLower().Contains ("text")) {
					
						responseObject.Body = m_serializer.Encoding.GetString (responseData);

					} if (response.ContentType.ToLower().Contains ("json")) {
					
						responseObject.Body = m_serializer.Deserialize (responseData);

					} else {
					
						responseObject.Body = responseData;

					}

				}

			} else {
			
				string error = $"HttpRequest got bad http status code {response.StatusCode}";

				responseObject.Error = new HttpError () {Message = error};
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

