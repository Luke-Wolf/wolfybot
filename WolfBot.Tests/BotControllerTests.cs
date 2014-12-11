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
using NUnit.Framework;
using System;
using WolfyBot.Core;
using NUnit.Framework.SyntaxHelpers;
using System.Collections.Generic;
using PipeCleanerCommand;

namespace WolfBot.Tests
{
	[TestFixture ()]
	public class BotControllerTests
	{
		[Test ()]
		public void ListenForCommandTest ()
		{
			var called = false;
			var commands = new List<IBotCommand> ();
			var listenerwords = new List<String> ();
			listenerwords.Add ("PRIVMSG");
			commands.Add (new Pipecleaner (listenerwords, new List<String> (),
				new List<string> (), false, SecureLevelEnum.Bot));
			var controller = new BotController (commands, String.Empty);
			controller.MessageSent += (s, e) => called = true;
			controller.ReceiveMessageHandler (this, IRCMessageFactory.BuildSendChannelMessage ("#foo", "foobar"));
			Assert.AreEqual (true, called, "ERROR: Bot did not pick up on command");
		}

		[Test ()]
		public void ListenForCommandAndParametersTest ()
		{
			var called = false;
			var commands = new List<IBotCommand> ();
			var commandwords = new List<String> ();
			commandwords.Add ("PRIVMSG");
			var parameterwords = new List<String> ();
			parameterwords.Add ("Foo");
			commands.Add (new Pipecleaner (commandwords, parameterwords,
				new List<string> (), false, SecureLevelEnum.Bot));
			var controller = new BotController (commands, String.Empty);
			controller.MessageSent += (s, e) => called = true;
			controller.ReceiveMessageHandler (this, IRCMessageFactory.BuildSendChannelMessage ("Foo", "foobar"));
			Assert.AreEqual (true, called, "ERROR: Bot did not pick up on command");
		}

		[Test ()]
		public void ListenForCommandAndTrailingParametersTest ()
		{
			var called = false;
			var commands = new List<IBotCommand> ();
			var commandwords = new List<String> ();
			commandwords.Add ("PRIVMSG");
			var trailingparameterwords = new List<String> ();
			trailingparameterwords.Add ("Foo");
			commands.Add (new Pipecleaner (commandwords, new List<String> (),
				trailingparameterwords, false, SecureLevelEnum.Bot));
			var controller = new BotController (commands, String.Empty);
			controller.MessageSent += (s, e) => called = true;
			controller.ReceiveMessageHandler (this, IRCMessageFactory.BuildSendChannelMessage ("Foo", "Foo"));
			Assert.AreEqual (true, called, "ERROR: Bot did not pick up on command");
		}
	}
}

