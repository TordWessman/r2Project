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

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace Core.Network
{
	
public class SynchronousSocketClient {

    public static void StartClient (int id)
		{
			// Data buffer for incoming data.
			byte[] bytes = new byte[1024];

			// Connect to a remote device.
			try {
				// Establish the remote endpoint for the socket.
				// This example uses port 11000 on the local computer.
				IPHostEntry ipHostInfo = Dns.Resolve (Dns.GetHostName ());
				IPAddress ipAddress = ipHostInfo.AddressList [0];
				IPEndPoint remoteEP = new IPEndPoint (ipAddress, 11000);
				
				

				// Create a TCP/IP  socket.
				Socket sender = new Socket (AddressFamily.InterNetwork, 
                SocketType.Stream, ProtocolType.Tcp);

				// Connect the socket to the remote endpoint. Catch any errors.
				try {
					sender.Connect (remoteEP);

					Console.WriteLine ("Socket connected to {0} for client {1}",
                    sender.RemoteEndPoint.ToString (), id);

					// Encode the data string into a byte array.
					byte[] msg = Encoding.ASCII.GetBytes ("This is a test<EOF>");

					// Send the data through the socket.
					int bytesSent = sender.Send (msg);

					// Receive the response from the remote device.
					int bytesRec = sender.Receive (bytes);
					Console.WriteLine ("Echoed test = {0}",
                    Encoding.ASCII.GetString (bytes, 0, bytesRec));

					// Release the socket.
					sender.Shutdown (SocketShutdown.Both);
					sender.Close ();
                
				} catch (ArgumentNullException ane) {
					Console.WriteLine ("ArgumentNullException : {0}", ane.ToString ());
				} catch (SocketException se) {
					Console.WriteLine ("SocketException : {0} {1}",se.ToString(), id);
            } catch (Exception e) {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        } catch (Exception e) {
            Console.WriteLine( e.ToString());
        }
    }
    

}

}