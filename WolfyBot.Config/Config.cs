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
using System.IO;
using System.Configuration;
using WolfyBot.Core;
using System.Collections.Generic;
using System.Linq;
using KeepAliveCommand;
using NickServIdentifyCommand;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using ProxyDetectionCommand;


namespace WolfyBot.Config
{
	public static class Configurator
	{
		#region PublicMethods

		public static void ReadConfig ()
		{
			if (File.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini")) {
				var parser = new FileIniDataParser ();
				IniData data = parser.ReadFile (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini");

				#region IRCServer
				Logging = Convert.ToBoolean (data ["ServerConfig"] ["Logging"]);
				IRCServerHostName = data ["ServerConfig"] ["HostName"];
				IRCServerPort = Convert.ToInt16 (data ["ServerConfig"] ["Port"]);
				IRCNick = data ["ServerConfig"] ["Nick"];
				IRCPassword = data ["ServerConfig"] ["Password"];
				IRCChannels = data ["ServerConfig"] ["Channels"];
				DontLogChannels = data ["ServerConfig"] ["DontLog"].Split (' ');
				#endregion

				#region Bot

				#region NickServ
				NickServEnabled = Convert.ToBoolean (data ["NickServ"] ["Enabled"]);
				NickServName = data ["NickServ"] ["Name"];
				NickServPassword = data ["NickServ"] ["Password"];
				#endregion

				#region DetectProxy
				DetectProxyEnabled = Convert.ToBoolean (data ["DetectProxy"] ["Enabled"]);
				DetectProxyPorts = data ["DetectProxy"] ["Ports"].Split (' ').Select (n => Convert.ToInt32 (n)).ToArray ();
				DetectProxyBanMessage = data ["DetectProxy"] ["BanMessage"];
				#endregion

				#endregion
			} else {
				WriteNewConfig ();
			}
			configured = true;
		}

		public static BotController BuildBotController ()
		{
			if (!configured) {
				ReadConfig ();
			}
			var commands = new List<IBotCommand> ();
			commands.Add (new KeepAlive ());
			if (NickServEnabled == true) {
				commands.Add (new IdentifyWithNickServ (NickServName, NickServPassword));
			}
			if (DetectProxyEnabled == true) {
				commands.Add (new ProxyDetector (DetectProxyPorts, DetectProxyBanMessage));
			}

			return new BotController (commands);
		}

		public static IRCServer BuildIRCServer ()
		{
			if (!configured) {
				ReadConfig ();
			}
			var server = new IRCServer (IRCServerHostName, IRCServerPort, IRCChannels, IRCNick, IRCPassword);
			server.Logging = Logging;
			return server;
		}

		public static void WireUp (BotController controller, IRCServer server)
		{
			server.MessageReceived += new EventHandler<IRCMessage> (controller.ReceiveMessageHandler);
			controller.MessageSent += new EventHandler<IRCMessage> (server.SendMessageHandler);
		}

		public static void WriteNewConfig ()
		{
			#region IrcServerConfig
			Logging = true;
			IRCServerHostName = "irc.freenode.net";
			IRCServerPort = 6667;
			IRCNick = "wolfybot";
			IRCChannels = "#WolfyBot";
			IRCPassword = String.Empty;
			DontLogChannels = new String[0];
			#endregion

			#region BotConfig
			#region NickServ
			NickServEnabled = false;
			NickServName = "NickServ";
			NickServPassword = String.Empty;
			#endregion
			#region DetectProxy
			DetectProxyEnabled = false;
			DetectProxyPorts = new [] { 80, 8080, 443 };
			DetectProxyBanMessage = "Don't Use a Proxy to Connect to IRC";
			#endregion
			#endregion

			WriteConfig ();
		}

		public static void WriteConfig ()
		{		
			if (!Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot"))
				Directory.CreateDirectory (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot");
			if (File.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini")) {
				File.Delete (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini");
			}

			var data = new IniData ();

			#region IRCServer
			data.Sections.AddSection ("ServerConfig");
			data ["ServerConfig"].AddKey ("Logging", Logging.ToString ());
			data ["ServerConfig"].AddKey ("HostName", IRCServerHostName);
			data ["ServerConfig"].AddKey ("Port", IRCServerPort.ToString ());
			data ["ServerConfig"].AddKey ("Nick", IRCNick);
			data ["ServerConfig"].AddKey ("Password", IRCPassword);
			data ["ServerConfig"].AddKey ("Channels", IRCChannels);
			var dontLogString = String.Empty;
			foreach (var item in DontLogChannels) {
				String.Concat (item, " ", dontLogString);
			}
			data ["ServerConfig"].AddKey ("DontLog", dontLogString);
			#endregion

			#region Bot
			data.Sections.AddSection ("BotConfig");

			#region NickServ
			data.Sections.AddSection ("NickServ");
			data ["NickServ"].AddKey ("Enabled", NickServEnabled.ToString ());
			data ["NickServ"].AddKey ("Name", NickServName);
			data ["NickServ"].AddKey ("Password", NickServPassword);
			#endregion

			#region DetectProxy
			data.Sections.AddSection ("DetectProxy");
			data ["DetectProxy"].AddKey ("Enabled", DetectProxyEnabled.ToString ());
			var portString = String.Empty;
			foreach (var item in DetectProxyPorts) {
				portString = String.Concat (item, " ", portString);
			}
			data ["DetectProxy"].AddKey ("Ports", portString);
			data ["DetectProxy"].AddKey ("BanMessage", DetectProxyBanMessage);
			#endregion

			#endregion

			var parser = new FileIniDataParser ();
			parser.WriteFile (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini", data);
		}

		#endregion

		#region IRCServerConfiguration

		public static bool Logging {
			get;
			set;
		}

		public static String[] DontLogChannels {
			get;
			set;
		}

		public static String IRCServerHostName {
			get;
			set;
		}

		public static int IRCServerPort {
			get;
			set;
		}

		public static String IRCNick {
			get;
			set;
		}

		public static String IRCChannels {
			get;
			set;
		}

		public static String IRCPassword {
			get;
			set;
		}

		#endregion

		#region BotConfiguration

		public static bool DetectProxyEnabled {
			get;
			set;
		}

		public static int[] DetectProxyPorts {
			get;
			set;
		}

		public static String DetectProxyBanMessage {
			get;
			set;
		}

		public static bool NickServEnabled {
			get;
			set;
		}

		public static String NickServName {
			get;
			set;
		}

		public static String NickServPassword {
			get;
			set;
		}

		#endregion

		static bool configured = false;

	}
}

