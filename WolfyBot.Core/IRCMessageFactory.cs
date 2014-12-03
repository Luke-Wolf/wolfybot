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
using System.Security.Authentication.ExtendedProtection;

namespace WolfyBot.Core
{
	public static class IRCMessageFactory
	{
		public static IRCMessage BuildActionMessage (String channel, String message)
		{
			return new IRCMessage (IRCCommand.ACTION, channel, message);
		}

		public static IRCMessage BuildJoinChannelMessage (String channel)
		{
			return new IRCMessage (IRCCommand.JOIN, channel);
		}

		public static IRCMessage BuildSetNickMessage (String nick)
		{
			return new IRCMessage (IRCCommand.NICK, nick);
		}

		public static IRCMessage BuildQuitMessage ()
		{
			return new IRCMessage (IRCCommand.QUIT);
		}

		public static IRCMessage BuildSendPassMessage (String password)
		{
			return new IRCMessage (IRCCommand.PASS, password);
		}

		public static IRCMessage BuildPingMessage (String target)
		{
			return new IRCMessage (IRCCommand.PING, trailingParameters: target);
		}

		public static IRCMessage BuildPongMessage (String server)
		{
			return new IRCMessage (IRCCommand.PONG, trailingParameters: server);
		}

		public static IRCMessage BuildSendChannelMessage (String channel, String message)
		{
			return new IRCMessage (IRCCommand.PRIVMSG, channel, message);
		}

		public static IRCMessage BuildLeaveChannelMessage (String channel)
		{
			return new IRCMessage (IRCCommand.PART, channel);
		}

		public static IRCMessage BuildUserMessage (String name, String realName)
		{
			return new IRCMessage (IRCCommand.USER, String.Format ("{0} 0 *", name), realName);
		}

	}
}

