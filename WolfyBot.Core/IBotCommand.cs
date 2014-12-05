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
using System.Collections.Generic;

namespace WolfyBot.Core
{
	public interface IBotCommand
	{
		/// <summary>
		/// Called by the Bot on the command.
		/// The full IRC message is passed in. If
		/// you desire to return a message to the server
		/// raise an event
		/// </summary>
		void Execute (object sender, IRCMessage message);

		/// <summary>
		/// The list of command words that the controller is
		/// listening for the execute the command. For complex
		/// multi-step scripts, change the backing variable of the
		/// property to point to a different list of strings to listen
		/// for. It is recommended to set up a state machine to handle this.
		/// </summary>
		/// <value>The command words.</value>
		List<String> CommandWords {
			get;
		}

		List<String> ParameterWords {
			get;
		}

		List<String> TrailingParameterWords {
			get;
		}

		bool Interactive {
			get;
		}

		SecureLevelEnum SecureLevel {
			get;
		}

		void OnScriptMessage (IRCMessage e);

		event EventHandler<IRCMessage> ScriptMessage;
	}

	public enum SecureLevelEnum : int
	{
		User,
		Voice,
		HalfOp,
		Op,
		Owner,
		Bot
	}
}

