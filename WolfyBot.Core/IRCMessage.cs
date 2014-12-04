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
using System.Linq;

namespace WolfyBot.Core
{
	public class IRCMessage : EventArgs
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WolfyBot.Core.IRCMessage"/> class.
		/// </summary>
		/// <param name="ircMessage">A Message formatted in the IRC Protocol</param>
		public IRCMessage (String ircMessage)
		{
			TimeStamp = DateTime.Now;
			Prefix = String.Empty;
			Command = String.Empty;
			Parameters = new List<String> ();
			TrailingParameters = String.Empty;

			//If an IRC Message starts with a colon the first string before
			//the space is the prefix
			int prefixEnd = -1;
			if (ircMessage.StartsWith (":", StringComparison.Ordinal)) {
				prefixEnd = ircMessage.IndexOf (" ", StringComparison.Ordinal);
				Prefix = ircMessage.Substring (1, prefixEnd - 1);
			}

			//Get the Trailing Parameters if they exist as a string literal
			//Trailing Parameters are demarkated by a space and a colon at the
			//end of the normal parameter set
			int trailingStart = ircMessage.IndexOf (" :", StringComparison.Ordinal);
			if (trailingStart >= 0) {
				TrailingParameters = ircMessage.Substring (trailingStart + 2);
			} else {
				trailingStart = ircMessage.Length;
			}

			//pull the command and parameters out of the IRC Message by pulling everything
			//between the prefix and the trailing parameters, The Command is the first item,
			//and everything else if it exists is the parameter to that command
			String[] commandAndParameters = ircMessage.Substring (prefixEnd + 1, trailingStart - prefixEnd - 1).Split (' ');
			Command = commandAndParameters [0];
			if (commandAndParameters.Length > 1) {
				Parameters = commandAndParameters.Skip (1).ToList ();

			}
			msgtype = IRCMessageType.RECEIVE;
		}

		public IRCMessage (IRCCommand command, String parameters = "", String trailingParameters = "")
		{
			TimeStamp = DateTime.Now;
			msgtype = (int)IRCMessageType.SEND;
			Command = command.ToString ();
			Parameters = parameters != String.Empty ? parameters.Split (' ').ToList () : new List<String> ();
			TrailingParameters = trailingParameters;

		}

		//Copy Constructor
		public IRCMessage (IRCMessage other)
		{
			TimeStamp = other.TimeStamp;
			Prefix = String.Copy (other.Prefix);
			Command = String.Copy (other.Command);
			Parameters = new List<String> (other.Parameters);
		}

		#endregion

		#region Methods

		public String ToIRCString ()
		{
			String param = BuildParamString ();
			if (msgtype == IRCMessageType.RECEIVE) {
				return String.Format (":{0} {1} {2} :{3}", Prefix,
					Command, param, TrailingParameters);
			} else {
				if (TrailingParameters.Length > 0 && param.Length > 0)
					return String.Format ("{0} {1} :{2}", Command, param, TrailingParameters);
				else if (TrailingParameters.Length > 0 && param.Length == 0)
					return String.Format ("{0} :{1}", Command, TrailingParameters);
				else if (TrailingParameters.Length == 0 && param.Length > 0)
					return String.Format ("{0} {1}", Command, param);
				else
					return Command;

			}
		}

		public String ToLogString ()
		{
			String param = BuildParamString ();
			return String.Format ("{0} :{1} {2} {3} :{4}", TimeStamp, Prefix,
				Command, param, TrailingParameters);
		}

		public override String ToString ()
		{
			String param = BuildParamString ();
			return String.Format ("TimeStamp: {0}, IRC Message: ':{1} {2} {3} :{4}'",
				TimeStamp, Prefix, Command, param, TrailingParameters);
		}

		String BuildParamString ()
		{
			if (Parameters.Count == 1) {
				return Parameters [0];
			} else if (Parameters.Count == 0) {
				return String.Empty;
			}

			var param = Parameters [0];
			for (int i = 1; i < Parameters.Count; i++) {
				param = String.Concat (new []{ param, " ", Parameters [i] });
			}
			return param;
		}

		#endregion

		#region Properties

		//IRC Message Properties
		public String Prefix {
			get;
			set;
		}

		public String Command {
			get;
			set;
		}

		public List<String> Parameters {
			get;
			set;
		}

		public String TrailingParameters {
			get;
			set;
		}

		//Utility Properties
		public String Channel {
			get {
				if (Parameters.Count > 0) {
					foreach (var item in Parameters) {
						if (item [0] == '#') {
							return item;
						}
					}
				}
				return String.Empty;
			}
		}

		public String Sender {
			get {
				if (msgtype == IRCMessageType.RECEIVE) {
					return Prefix.Length > 0 ? Prefix.Split ('!') [0] : String.Empty;
				} else {
					return "LOCALHOST";
				}
			}
		}

		public String Host {
			get {
				if (msgtype == IRCMessageType.RECEIVE) {
					return Prefix.Length > 0 ? Prefix.Split ('!') [1] : String.Empty;
				}
				return "LOCALHOST";
			}
		}

		//Logging Properties
		public DateTime TimeStamp {
			get;
			set;
		}

		#endregion

		#region private variables

		readonly IRCMessageType msgtype;

		#endregion
	}

	public enum IRCMessageType:int
	{
		SEND,
		RECEIVE
	}

	public enum IRCCommand:int
	{
		ACTION,
		JOIN,
		MODE,
		KICK,
		NICK,
		OPER,
		PART,
		PASS,
		PING,
		PONG,
		PRIVMSG,
		TOPIC,
		USER,
		QUIT
	}
}

