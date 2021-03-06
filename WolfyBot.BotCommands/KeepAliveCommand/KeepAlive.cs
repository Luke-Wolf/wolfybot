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
using WolfyBot.Core;
using System.Collections.Generic;

namespace KeepAliveCommand
{
	public class KeepAlive : SimpleBotCommandBase
	{
		public KeepAlive ()
		{
			CommandWords = new List<String> ();
			CommandWords.Add ("PING");
			ParameterWords = new List<string> ();
			TrailingParameterWords = new List<string> ();
			SecureLevel = SecureLevelEnum.Bot;
			Interactive = false;
		}

		public override void Execute (object sender, IRCMessage message)
		{
			OnScriptMessage (IRCMessageFactory.BuildPongMessage (message.TrailingParameters));
		}
	}
}

