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
using IniParser;
using IniParser.Model;
using IniParser.Parser;


namespace WolfyBot.Config
{
	public static class Configurator
	{
		public static void Configure ()
		{
			if (File.Exists ("config.ini")) {
				var parser = new FileIniDataParser ();
				IniData data = parser.ReadFile ("config.ini");
				Logging = Convert.ToBoolean (data ["ServerConfig"] ["Logging"]);
				IRCServerHostName = data ["ServerConfig"] ["HostName"];
				IRCServerPort = Convert.ToInt16 (data ["ServerConfig"] ["Port"]);
				IRCNick = data ["ServerConfig"] ["Nick"];
				IRCPassword = data ["ServerConfig"] ["Password"];
				IRCChannels = data ["ServerConfig"] ["Channels"];
			} else {
				WriteNewConfig ();
			}
			configured = true;
		}

		public static BotController BuildBotController ()
		{
			if (!configured) {
				Configure ();
			}
			var commands = new List<IBotCommand> ();
			commands.Add (new KeepAlive ());
			return new BotController (commands);
		}

		public static IRCServer BuildIRCServer ()
		{
			if (!configured) {
				Configure ();
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
			if (File.Exists ("config.ini")) {
				File.Delete ("config.ini");
			}
			Logging = true;
			IRCServerHostName = "irc.freenode.net";
			IRCServerPort = 6667;
			IRCNick = "wolfybot";
			IRCChannels = "#WolfyBot";
			IRCPassword = String.Empty;
			IniData data = new IniData ();
			data.Sections.AddSection ("ServerConfig");
			data ["ServerConfig"].AddKey ("Logging", Logging.ToString ());
			data ["ServerConfig"].AddKey ("HostName", IRCServerHostName);
			data ["ServerConfig"].AddKey ("Port", IRCServerPort.ToString ());
			data ["ServerConfig"].AddKey ("Nick", IRCNick);
			data ["ServerConfig"].AddKey ("Password", IRCPassword);
			data ["ServerConfig"].AddKey ("Channels", IRCChannels);
			var parser = new FileIniDataParser ();
			parser.WriteFile ("config.ini", data);
		}

		public static bool Logging {
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

		static bool configured = false;

	}
}

