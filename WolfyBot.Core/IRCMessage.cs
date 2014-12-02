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
using System.Runtime.Remoting;

namespace WolfyBot.Core
{
	public class IRCMessage
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WolfyBot.Core.IRCMessage"/> class.
		/// </summary>
		/// <param name="ircMessage">A Message formatted in the IRC Protocol</param>
		public IRCMessage (String ircMessage)
		{
			TimeStamp = DateTime.Now;

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
				//If Parameters contains a channel pull it out
				foreach (var item in Parameters) {
					if (item.Contains ("#")) {
						Channel = item;
						break;
					}
				}
			}
		}

		//Copy Constructor
		public IRCMessage (IRCMessage other)
		{
			TimeStamp = other.TimeStamp;
			Prefix = String.Copy (other.Prefix);
			Command = String.Copy (other.Command);
			Parameters = new List<String> (other.Parameters);
			Channel = String.Copy (other.Channel);
		}

		#endregion

		#region Methods

		public String ToIRCString ()
		{
			return String.Format (":{0} {1} {2} :{3}", Prefix,
				Command, Parameters.ToString ().Replace (",", " "), TrailingParameters);
		}

		public String ToLogString ()
		{
			return String.Format ("{0} {1} {2} {3} {4}", TimeStamp, Prefix,
				Command, Parameters.ToString ().Replace (",", " "), TrailingParameters);
		}

		public String ToString ()
		{
			return String.Format ("TimeStamp: {0}, IRC Message: ':{1} {2} {3} :{4}'",
				TimeStamp, Prefix, Command, Parameters.ToString ().Replace (",", " "), TrailingParameters);
		}

		#endregion

		#region Properties

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

		public DateTime TimeStamp {
			get;
			set;
		}

		public String Channel {
			get;
			set;
		}

		#endregion
	}
}

