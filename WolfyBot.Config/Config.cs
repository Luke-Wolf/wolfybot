﻿//
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
			#region IRCServer
			ServerConfig.ReadConfig ();
			#endregion
			if (File.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini")) {
				var parser = new FileIniDataParser ();
				IniData data = parser.ReadFile (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini");

				#region Bot
				BotPassword = data ["BotConfig"] ["Password"];
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

			return new BotController (commands, BotPassword);
		}

		public static IRCServer BuildIRCServer ()
		{
			return ServerConfig.BuildServer ();
		}

		public static void WireUp (BotController controller, IRCServer server)
		{
			server.MessageReceived += new EventHandler<IRCMessage> (controller.ReceiveMessageHandler);
			controller.MessageSent += new EventHandler<IRCMessage> (server.SendMessageHandler);
		}

		public static void WriteNewConfig ()
		{
			#region IrcServerConfig
			ServerConfig.ResetToDefaults ();
			ServerConfig.WriteConfig ();
			#endregion

			#region BotConfig
			BotPassword = "Foobar";
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

			#region Bot
			data.Sections.AddSection ("BotConfig");
			data ["BotConfig"].AddKey ("Password", BotPassword);

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

		public static IRCServerConfigModule ServerConfig {
			get;
			set;
		}


		#region BotConfiguration

		public static String BotPassword {
			get;
			set;
		}

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

