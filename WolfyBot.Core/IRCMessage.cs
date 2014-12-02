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
			//TODO: write IRC Parser
		}

		//Copy Constructor
		public IRCMessage (IRCMessage other)
		{
			TimeStamp = other.TimeStamp;
			Prefix = new String (other.Prefix);
			Command = new String (other.Command);
			Parameters = new String (other.Parameters);
			TrailingParameters = new String (other.TrailingParameters);
		}

		#endregion

		#region Methods

		public String ToIRCString ()
		{
			//TODO: Implement Serialization to IRC Protocol
		}

		public String ToLogString ()
		{
			//TODO: Implement Serialization to logging format
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

		public String Parameters {
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

		#endregion
	}
}

