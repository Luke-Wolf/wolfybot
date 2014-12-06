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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.IO;

namespace WolfyBot.Core
{
	public class IRCChannel
	{
		#region ctor

		public IRCChannel (string channelName)
		{
			ChannelName = channelName;
			Topic = String.Empty;
			Muted = false;
			LoggingEnabled = true;
			joinQueue = new List<IRCMessage> ();
			Users = new List<IRCUser> ();
		}

		#endregion

		#region methods

		protected virtual void OnMessageReceived (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = MessageReceived;
			if (handler != null) {
				handler (this, e);
			}
		}

		public void HandleReceiveMessages (object sender, IRCMessage e)
		{
			if (e.Channel == String.Empty)
				return;
			//If it's not our channel don't care about it
			if (e.Channel != ChannelName)
				return;
			Log (e);

			var server = sender as IRCServer;

			if (e.Command == "353" || e.Command == "366") {
				HandleJoinMessage (e);
				return;
			} else if (e.Command == "332" || e.Command == "TOPIC") {
				Topic = e.TrailingParameters;
				return;
			} else if (e.Command == "JOIN") {
				ParsePermission (e.Sender);
				return;
			} else if (e.Command == "QUIT" | e.Command == "PART") {
				RemoveUser (e.Sender);
				return;
			} else if (e.Command == "MODE") {
				if (e.Parameters [1] == "+v") {
					var user = FindUser (e.Sender);
					user.Mode = SecureLevelEnum.Voice;
					return;
				} else if (e.Parameters [1] == "+h") {
					var user = FindUser (e.Sender);
					user.Mode = SecureLevelEnum.HalfOp;
					return;
				} else if (e.Parameters [1] == "+o") {
					var user = FindUser (e.Sender);
					user.Mode = SecureLevelEnum.Op;
					return;
				} else if (e.Parameters [1] == "+a") {
					var user = FindUser (e.Sender);
					user.Mode = SecureLevelEnum.SuperOp;
					return;
				} else if (e.Parameters [1] == "+q") {
					var user = FindUser (e.Sender);
					user.Mode = SecureLevelEnum.ChannelOwner;
					return;
				}
				return;
			} else if (e.Command == "NICK") {
				ChangeNick (e.Sender, e.TrailingParameters);
			} else if (e.Command == "PRIVMSG" && e.TrailingParameters.Contains ("!shutup") &&
			           e.TrailingParameters.Contains (server.Nick)) {
				Muted = true;
				return;
			} else if (e.Command == "PRIVMSG" && e.TrailingParameters.Contains ("!talk") &&
			           e.TrailingParameters.Contains (server.Nick)) {
				Muted = false;
				return;
			}


		}

		public IRCUser FindUser (String nick)
		{
			foreach (var user in Users) {
				if (user.Nick == nick)
					return user;
			}
			return null;
		}

		void ChangeNick (string oldName, string newName)
		{
			var user = FindUser (oldName);
			user.Nick = newName;
		}

		public void HandleSendMessages (Object sender, IRCMessage e)
		{
			//If it's not our channel don't care about it
			if (e.Channel != ChannelName)
				return;
			Log (e);

		}

		void HandleJoinMessage (IRCMessage e)
		{
			if (e.Command == "353") {
				joinQueue.Add (e);
			}
			if (e.Command == "366") {
				foreach (var item in joinQueue) {
					var joinstring = item.TrailingParameters;
					var joinarray = joinstring.Split (' ');
					foreach (var name in joinarray) {
						ParsePermission (name);
					}
				}
				joinQueue.Clear ();
			}
		}

		void ParsePermission (string name)
		{
			if (name.StartsWith ("&")) {
				Users.Add (new IRCUser (name, SecureLevelEnum.SuperOp));
				return;
			} else if (name.StartsWith ("~")) {
				Users.Add (new IRCUser (name, SecureLevelEnum.ChannelOwner));
				return;
			} else if (name.StartsWith ("@")) {
				Users.Add (new IRCUser (name, SecureLevelEnum.Op));
				return;
			} else if (name.StartsWith ("%")) {
				Users.Add (new IRCUser (name, SecureLevelEnum.HalfOp));
				return;
			} else if (name.StartsWith ("+")) {
				Users.Add (new IRCUser (name, SecureLevelEnum.Voice));
				return;
			} else {
				Users.Add (new IRCUser (name, SecureLevelEnum.User));
				return;
			}
		}

		void RemoveUser (string name)
		{
			var user = FindUser (name);
			Users.Remove (user);
		}

		void Log (IRCMessage e)
		{
			Console.WriteLine (e.ToLogString ());
			if (LoggingEnabled) {
				if (!Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot"))
					Directory.CreateDirectory (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot");
				if (!Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/logs"))
					Directory.CreateDirectory (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/logs");
				using (var stream = File.Open (String.Format (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + "/wolfybot/logs/{0}-log.txt", e.Channel), FileMode.Append)) {
					var filewriter = new StreamWriter (stream);
					filewriter.WriteLine (e.ToLogString ());
					filewriter.Flush ();
				}
			}
		}

		#endregion

		#region properties

		public String ChannelName {
			get;
			set;
		}

		public String Topic {
			get;
			set;
		}

		public bool Muted {
			get;
			set;
		}

		public bool LoggingEnabled {
			get;
			set;
		}

		public List<IRCUser> Users {
			get;
			set;
		}

		#endregion

		private List<IRCMessage> joinQueue;

		public event EventHandler<IRCMessage> MessageReceived;
	}
}

