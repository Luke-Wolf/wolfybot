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
using System.Configuration.Install;
using System.ServiceProcess;

namespace WolfyBot.CLI
{
	public class WindowsServiceInstaller:Installer
	{
		public WindowsServiceInstaller ()
		{
			ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller ();
			ServiceInstaller serviceInstaller = new ServiceInstaller ();


			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
			serviceProcessInstaller.Username = null;
			serviceProcessInstaller.Password = null;


			serviceInstaller.DisplayName = "Wolfybot";
			serviceInstaller.StartType = ServiceStartMode.Automatic;


			serviceInstaller.ServiceName = "Wolfybot";

			this.Installers.Add (serviceProcessInstaller);
			this.Installers.Add (serviceInstaller);
		}
	}
}

