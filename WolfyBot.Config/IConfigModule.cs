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

namespace WolfyBot.Config
{
	/// <summary>
	/// <para>
	/// The configuration module interface for the WolfyBot configurator
	/// Please call ReadConfig or set values for the module before you call
	/// BuildCommand
	/// </para>
	/// </summary>
	public interface IConfigModule
	{
		/// <summary>
		/// <para>
		/// Reads the default config file located at either .config/wolfybot/ or 
		/// /username/appdata/wolfybot/
		/// </para>
		/// </summary>
		void ReadConfig ();

		/// <summary>
		/// Reads the config file located at the given string.
		/// </summary>
		/// <param name="configFileLocation">Config file location.</param>
		void ReadConfig (String configFileLocation);


		/// <summary>
		/// <para>
		/// Writes the current configuration of the module to the default config file
		///  located at either .config/wolfybot/ or /username/appdata/wolfybot/
		/// </para>
		/// </summary>
		void WriteConfig ();

		/// <summary>
		/// <para>
		/// Writes the current configuration of the module to the file
		/// at the specified location
		/// </para>
		/// </summary>
		void WriteConfig (String configFileLocation);

		/// <summary>
		/// Sets the Config Module to it's default values
		/// </summary>
		void ResetToDefaults ();
	}
}

