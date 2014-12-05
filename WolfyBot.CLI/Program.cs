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
using System.Windows.Forms;
using WolfyBot.Core;
using WolfyBot.Config;
using System.Linq;
using System.ServiceProcess;

namespace WolfyBot.CLI
{
	class WolfyBotService : ServiceBase
	{
		public WolfyBotService ()
		{
			this.ServiceName = "Wolfybot";
			this.EventLog.Log = "Application";

			// These Flags set whether or not to handle that specific
			//  type of event. Set to true if you need it, false otherwise.
			/*this.CanHandlePowerEvent = false;
			this.CanHandleSessionChangeEvent = false;
			this.CanShutdown = false;*/
			this.CanPauseAndContinue = true;
			this.CanStop = true;
		}

		public static void Main (string[] args)
		{
			if (args.Contains ("--help") || args.Length == 0) {
				Console.WriteLine ("WolfyBot IRC Bot");
				Console.WriteLine ("Use mono-service to start the service under Linux or Mac, or InstallUtil under Windows");
				Console.WriteLine ("Arguments:");
				Console.WriteLine ("--help: Print this Help");
				Console.WriteLine ("--new-config: Generate a new configuration file");
				Application.Exit ();
			}
			if (args.Contains ("--new-config")) {
				Configurator.WriteNewConfig ();
				Console.WriteLine ("New Config File Written");
				Application.Exit ();
			}

			ServiceBase.Run (new WolfyBotService ());
		}

		protected override void OnStart (string[] args)
		{
			Configurator.Configure ();
			controller = Configurator.BuildBotController ();
			server = Configurator.BuildIRCServer ();
			Configurator.WireUp (controller, server);
			server.Connect ();
			base.OnStart (args);
		}

		protected override void OnStop ()
		{
			server.MessageReceived -= controller.ReceiveMessageHandler;
			controller.MessageSent -= server.SendMessageHandler;
			controller = null;
			server = null;
			base.OnStop ();
		}

		protected override void OnPause ()
		{
			server.MessageReceived -= controller.ReceiveMessageHandler;
			controller.MessageSent -= server.SendMessageHandler;
			controller = null;
			server = null;
			base.OnPause ();
		}

		protected override void OnContinue ()
		{
			controller = Configurator.BuildBotController ();
			server = Configurator.BuildIRCServer ();
			Configurator.WireUp (controller, server);
			server.Connect ();
			base.OnContinue ();
		}

		BotController controller;
		IRCServer server;
	}
}
