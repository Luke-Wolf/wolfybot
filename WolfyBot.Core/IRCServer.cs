//
//  Copyright 2014  luke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace WolfyBot.Core
{
	public class IRCServer
	{
		#region constructors

		public IRCServer ()
		{
			Host = String.Empty;
			Port = 6667;
			Nick = String.Empty;
			Password = String.Empty;
			Channels = new List<String> ();
			Logging = false;
		}

		public IRCServer (String host, int port, String channels, String nick, String password = "")
		{
			Host = host;
			Port = port;
			Nick = nick;
			Password = password;
			Channels = channels.Split (' ').ToList ();
			Logging = false;
		}

		#endregion

		#region properties

		public String Host {
			get;
			set;
		}

		public int Port {
			get;
			set;
		}

		public String Nick {
			get;
			set;
		}

		public String Password {
			get;
			set;
		}

		public List<String> Channels {
			get;
			set;
		}

		public bool Logging {
			get;
			set;
		}

		#endregion

		public void Connect ()
		{
			using (var client = new TcpClient (Host, Port)) {			
				var stream = client.GetStream ();
				reader = new StreamReader (stream);
				writer = new StreamWriter (stream) { NewLine = "\r\n", AutoFlush = true };

				//Identify Bot to IRC Server
				if (Password != String.Empty) {
					SendMessageHandler (this, IRCMessageFactory.BuildSendPassMessage (Password));
				}
				SendMessageHandler (this, IRCMessageFactory.BuildSetNickMessage (Nick));
				SendMessageHandler (this, IRCMessageFactory.BuildUserMessage (Nick, Nick));

				String channelsString = String.Empty;
				foreach (var item in Channels) {
					channelsString = String.Concat (channelsString, item, " ");
				}
				SendMessageHandler (this, IRCMessageFactory.BuildJoinChannelMessage (channelsString));

				while (true) {
					var line = reader.ReadLine ();
					var msg = new IRCMessage (line);
					Log (msg);
					OnMessageReceived (msg);
				}
			}
		}

		void Log (IRCMessage item)
		{
			//TODO: Implement Logging
			Console.WriteLine (item.ToLogString ());
			if (item.Channel != String.Empty) {
				if (!Directory.Exists ("logs"))
					Directory.CreateDirectory ("logs");
				using (var stream = File.Open (String.Format ("logs/{0}-log.txt", item.Channel), FileMode.Append)) {
					var filewriter = new StreamWriter (stream);
					filewriter.WriteLine (item.ToLogString ());
					filewriter.Flush ();
				}
			}
		}

		protected virtual void OnMessageReceived (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = MessageReceived;
			if (handler != null) {
				handler (this, e);
			}
		}

		public void SendMessageHandler (Object sender, IRCMessage e)
		{
			Log (e);
			if (e.Command == "JOIN") {
				Channels.Add (e.TrailingParameters);
			} else if (e.Command == "PART") {
				Channels.Remove (e.TrailingParameters);
			}
			writer.WriteLine (e.ToIRCString ());
			writer.Flush ();
		}

		public event EventHandler<IRCMessage> MessageReceived;

		StreamReader reader;
		StreamWriter writer;
	}
}

