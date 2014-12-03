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

namespace WolfBot.Tests
{
	[TestFixture ()]
	public class IRCMessageTests
	{
		[Test ()]
		public void IRCMessageParsing ()
		{
			string ircMsg = ":foo!bar@baz PRIVMSG #foo :Hello World";
			var message = new IRCMessage (ircMsg);
			//Test IRC Parsing
			Assert.AreEqual ("foo!bar@baz", message.Prefix);
			Assert.AreEqual ("PRIVMSG", message.Command);
			Assert.AreEqual ("Hello World", message.TrailingParameters);

			//Test Convenience Properties
			Assert.AreEqual ("foo", message.Sender);
			Assert.AreEqual ("#foo", message.Channel);

		}

		[Test ()]
		public void IRCMessageToLogFormat ()
		{
			string ircMsg = ":foo!bar@baz PRIVMSG #foo :Hello World";
			var testTime = DateTime.Now;
			var message = new IRCMessage (ircMsg);

			var testString = String.Format ("{0} :foo!bar@baz PRIVMSG #foo :Hello World", testTime);
			Assert.AreEqual (testString, message.ToLogString ());
		}

		[Test ()]
		public void IRCMessageBackToIRCFormat ()
		{
			string ircMsg = ":foo!bar@baz PRIVMSG #foo :Hello World";
			var message = new IRCMessage (ircMsg);
			Assert.AreEqual (ircMsg, message.ToIRCString ());
		}

		[Test ()]
		public void IRCMessageToString ()
		{
			string ircMsg = ":foo!bar@baz PRIVMSG #foo :Hello World";
			var testTime = DateTime.Now;
			var message = new IRCMessage (ircMsg);

			var testString = String.Format ("TimeStamp: {0}, IRC Message: ':foo!bar@baz PRIVMSG #foo :Hello World'", testTime);
			Assert.AreEqual (testString, message.ToString ());
		}

		[Test ()]
		public void IRCMessageFactoryTests ()
		{
			IRCMessage message = IRCMessageFactory.BuildActionMessage ("#foo", "bar");
			Assert.AreEqual ("ACTION #foo :bar", message.ToIRCString ());
			message = IRCMessageFactory.BuildJoinChannelMessage ("#foo");
			Assert.AreEqual ("JOIN #foo", message.ToIRCString ());
			message = IRCMessageFactory.BuildSetNickMessage ("foo");
			Assert.AreEqual ("NICK foo", message.ToIRCString ());
			message = IRCMessageFactory.BuildSendPassMessage ("foo");
			Assert.AreEqual ("PASS foo", message.ToIRCString ());
			message = IRCMessageFactory.BuildPingMessage ("foo.bar");
			Assert.AreEqual ("PING foo.bar", message.ToIRCString ());
			message = IRCMessageFactory.BuildPongMessage ("foo.bar");
			Assert.AreEqual ("PONG foo.bar", message.ToIRCString ());
			message = IRCMessageFactory.BuildSendChannelMessage ("#foo", "bar");
			Assert.AreEqual ("PRIVMSG #foo :bar", message.ToIRCString ());
			message = IRCMessageFactory.BuildQuitMessage ();
			Assert.AreEqual ("QUIT", message.ToIRCString ());
			message = IRCMessageFactory.BuildLeaveChannelMessage ("#foo");
			Assert.AreEqual ("PART #foo", message.ToIRCString ());
			message = IRCMessageFactory.BuildUserMessage ("foo", "foobar");
			Assert.AreEqual ("USER foo 0 * :foobar", message.ToIRCString ());
		}
	}
}

