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
using System.Net.Security;

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
			Channels = new List<IRCChannel> ();
			Logging = false;
			SSL = false;
		}

		public IRCServer (String host, int port, String channels, String nick, bool ssl, String password = "")
		{
			Host = host;
			Port = port;
			Nick = nick;
			SSL = ssl;
			Password = password;
			var channelStringList = channels.Split (' ').ToList ();
			Channels = new List<IRCChannel> ();
			foreach (var item in channelStringList) {
				var _channel = new IRCChannel (item);
				Channels.Add (_channel);
				MessageReceived += new EventHandler<IRCMessage> (_channel.HandleReceiveMessages);
				MessageSent += new EventHandler<IRCMessage> (_channel.HandleSendMessages);
			}
			Logging = false;
		}

		#endregion

		#region Methods

		public async void Connect ()
		{
			using (var client = new TcpClient (Host, Port)) {
				if (SSL) {
					//need to disable the validation callback otherwise it errors due to the fact
					//that IRC certificates will cause SslStream to error off otherwise hence the
					//lamda function
					var stream = new SslStream (client.GetStream (), false, (a, b, c, d) => true);

					stream.AuthenticateAsClient (Host, null, 
						System.Security.Authentication.SslProtocols.Default, false);
					reader = new StreamReader (stream);
					writer = new StreamWriter (stream){ NewLine = "\r\n", AutoFlush = true };
				} else {
					var stream = client.GetStream ();
					reader = new StreamReader (stream);
					writer = new StreamWriter (stream) { NewLine = "\r\n", AutoFlush = true };
				}
				//Identify Bot to IRC Server
				if (Password != String.Empty) {
					SendMessageHandler (this, IRCMessageFactory.BuildSendPassMessage (Password));
				}
				SendMessageHandler (this, IRCMessageFactory.BuildSetNickMessage (Nick));
				SendMessageHandler (this, IRCMessageFactory.BuildUserMessage (Nick, Nick));

				String channelsString = String.Empty;
				if (Channels.Count == 1) {
					channelsString = Channels [0].ChannelName;
				} else {
					foreach (var item in Channels) {
						channelsString = String.Concat (item.ChannelName, ",", channelsString);
					}
				}
				SendMessageHandler (this, IRCMessageFactory.BuildJoinChannelMessage (channelsString));

				while (client.Connected) {
					var line = await reader.ReadLineAsync ();
					var msg = new IRCMessage (line);
					if (msg.Channel == String.Empty)
						Log (msg);
					OnMessageReceived (msg);
				}
			}
		}

		static void Log (IRCMessage item)
		{
			if (item.Channel != String.Empty)
				return;
			Console.WriteLine (item.ToLogString ());
			if (!Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot"))
				Directory.CreateDirectory (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot");
			if (!Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/logs"))
				Directory.CreateDirectory (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/logs");
			using (var stream = File.Open (String.Format (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/logs/server-log.txt"), FileMode.Append)) {
				var filewriter = new StreamWriter (stream);
				filewriter.WriteLine (item.ToLogString ());
				filewriter.Flush ();
			}

		}

		protected virtual void OnMessageReceived (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = MessageReceived;
			if (handler != null) {
				handler (this, e);
			}
		}

		protected virtual void OnMessageSent (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = MessageSent;
			if (handler != null) {
				handler (this, e);
			}
		}

		public void SendMessageHandler (Object sender, IRCMessage e)
		{
			if (e.Channel == String.Empty)
				Log (e);
			if (e.Command == "JOIN") {
				var channels = e.Parameters [0].Split (',');
				foreach (var item in channels) {
					bool alreadyExists = false;
					foreach (var channel in Channels) {
						if (channel.ChannelName == item) {
							alreadyExists = true;
							break;
						}
					}
					if (!alreadyExists) {
						var _channel = new IRCChannel (item);
						Channels.Add (_channel);
						MessageReceived += new EventHandler<IRCMessage> (_channel.HandleReceiveMessages);
						MessageSent += new EventHandler<IRCMessage> (_channel.HandleSendMessages);
					}
				}
			} else if (e.Command == "PART") {
				var channels = e.Parameters [0].Split (',');
				foreach (var item in channels) {
					var channelsToRemove = new  List<IRCChannel> ();
					foreach (var item2 in Channels) {
						if (item2.ChannelName == item) {
							channelsToRemove.Add (item2);
						}
					}
					foreach (var item2 in channelsToRemove) {
						Channels.Remove (item2);
						MessageReceived -= item2.HandleReceiveMessages;
						MessageSent -= item2.HandleSendMessages;
					}
					channelsToRemove.Clear ();
				}
			}
			writer.WriteLine (e.ToIRCString ());
			writer.Flush ();
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

		public List<IRCChannel> Channels {
			get;
			set;
		}

		public bool Logging {
			get;
			set;
		}

		public bool SSL {
			get;
			set;
		}

		#endregion

		public event EventHandler<IRCMessage> MessageReceived;
		public event EventHandler<IRCMessage> MessageSent;

		StreamReader reader;
		StreamWriter writer;
	}
}

