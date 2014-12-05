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
using WolfyBot.Core;
using System.Threading;
using System.Collections.Generic;
using System.Net;

namespace ProxyDetectionCommand
{
	public class ProxyDetector:IBotCommand
	{
		public ProxyDetector (int[] ports, string kickmessage = "")
		{
			portsToScan = ports;
			CommandWords = new List<String> ();
			CommandWords.Add ("JOIN");
			ParameterWords = new List<string> ();
			TrailingParameterWords = new List<string> ();
			kickMessage = kickmessage;
		}

		#region IBotCommand implementation

		public event EventHandler<IRCMessage> ScriptMessage;

		public void Execute (object sender, IRCMessage message)
		{
			var hostname = message.Host.Split ('@') [1];
			var entry = Dns.GetHostEntry (hostname);
			if (IsBehindProxy (entry.AddressList)) {
				OnScriptMessage (IRCMessageFactory.BuildBanMessage (message.Channel, message.Prefix));
				OnScriptMessage (IRCMessageFactory.BuildKickCommandMessage (message.Channel, message.Prefix.Split ('!') [0], kickMessage));
			}
		}

		static bool IsBehindProxy (IPAddress[] addresses)
		{
			var client = new TcpClient ();
			foreach (var address in addresses) {
				foreach (var port in portsToScan) {
					try {
						client.Connect (address, port);
						client.Close ();
						return true;

					} catch (Exception ex) {
						//intentionally empty, if the ports aren't open they're
						//not behind a proxy
					}
				}
			}
			return false;
		}

		public void OnScriptMessage (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = ScriptMessage;
			if (handler != null) {
				handler (this, e);
			}
		}

		public List<string> CommandWords {
			get;
			set;
		}

		public List<string> ParameterWords {
			get;
			set;
		}

		public List<string> TrailingParameterWords {
			get;
			set;
		}

		static int[] portsToScan;
		static String kickMessage;

		#endregion
	}
}

