﻿//
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
using System.IO;
using WolfyBot.Core;

namespace WolfyBot.Config
{
	public abstract class ConfigModuleBase :IConfigModule
	{
		public abstract IBotCommand BuildCommand ();

		public void ReadConfig ()
		{
			ReadConfig (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini");
		}

		public abstract void ReadConfig (string configFileLocation);

		public void WriteConfig ()
		{
			WriteConfig (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/config.ini");
		}

		public abstract void WriteConfig (string configFileLocation);

		public abstract void ResetToDefaults ();
	}
}
