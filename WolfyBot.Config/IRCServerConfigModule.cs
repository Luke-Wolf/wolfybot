//
//  Copyright 2015  Luke
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
using WolfyBot.Core;
using System.IO;
using IniParser;
using IniParser.Model;

namespace WolfyBot.Config
{
	public sealed class IRCServerConfigModule : ConfigModuleBase, IServerConfigModule
	{
		public IRCServerConfigModule ()
		{
			ResetToDefaults ();
		}

		public override void ReadConfig (string configFileLocation)
		{
			if (File.Exists (configFileLocation)) {
				var parser = new FileIniDataParser ();
				IniData data = parser.ReadFile (configFileLocation);
				LoggingEnabled = Convert.ToBoolean (data ["ServerConfig"] ["Logging"]);
				HostName = data ["ServerConfig"] ["HostName"];
				Port = Convert.ToInt16 (data ["ServerConfig"] ["Port"]);
				SSLEnabled = Convert.ToBoolean (data ["ServerConfig"] ["SSL_Enabled"]);
				Nick = data ["ServerConfig"] ["Nick"];
				Password = data ["ServerConfig"] ["Password"];
				Channels = data ["ServerConfig"] ["Channels"].Split (' ');
				DontLogChannels = data ["ServerConfig"] ["DontLog"].Split (' ');
			}
		}

		public override void WriteConfig (string configFileLocation)
		{

			var data = new IniData ();


			data.Sections.AddSection ("ServerConfig");
			data ["ServerConfig"].AddKey ("Logging", LoggingEnabled.ToString ());
			data ["ServerConfig"].AddKey ("HostName", HostName);
			data ["ServerConfig"].AddKey ("Port", Port.ToString ());
			data ["ServerConfig"].AddKey ("SSL_Enabled", SSLEnabled.ToString ());
			data ["ServerConfig"].AddKey ("Nick", Nick);
			data ["ServerConfig"].AddKey ("Password", Password);
			var channelsString = String.Empty;
			foreach (var item in Channels) {
				String.Concat (item, " ", channelsString);
			}
			data ["ServerConfig"].AddKey ("Channels", channelsString);

			var dontLogString = String.Empty;
			foreach (var item in DontLogChannels) {
				String.Concat (item, " ", dontLogString);
			}
			data ["ServerConfig"].AddKey ("DontLog", dontLogString);

			var parser = new FileIniDataParser ();
			parser.WriteFile (configFileLocation, data);
		}

		public IRCServer BuildServer ()
		{
			var server = new IRCServer (HostName, Port, Channels, Nick, SSLEnabled, Password);
			server.Logging = LoggingEnabled;
			return server;
		}

		public override void ResetToDefaults ()
		{
			LoggingEnabled = true;
			HostName = "irc.freenode.net";
			Port = 6667;
			SSLEnabled = false;
			Nick = "wolfybot";
			Channels = new [] { "#WolfyBot" };
			Password = String.Empty;
			DontLogChannels = new String[0];
		}

		public bool LoggingEnabled {
			get;
			set;
		}

		public String HostName {
			get;
			set;
		}

		public int Port {
			get;
			set;
		}

		public bool SSLEnabled {
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

		public String[] Channels {
			get;
			set;
		}

		public String[] DontLogChannels {
			get;
			set;
		}



	}
}

