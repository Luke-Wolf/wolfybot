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
			Users = new List<string> ();
			Voiced = new List<string> ();
			HalfOps = new List<string> ();
			Ops = new List<string> ();
			SuperOps = new List<string> ();
			ChannelOwner = new List<string> ();

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
				switch (e.Parameters [1]) {
				case "+v":
					RemoveUser (e.Sender);
					Voiced.Add (e.Sender);
					return;
				case "+h":
					RemoveUser (e.Sender);
					HalfOps.Add (e.Sender);
					return;
				case "+o":
					RemoveUser (e.Sender);
					Ops.Add (e.Sender);
					return;
				case "+a":
					RemoveUser (e.Sender);
					SuperOps.Add (e.Sender);
					return;
				case "+q":
					RemoveUser (e.Sender);
					ChannelOwner.Add (e.Sender);
					return;
				}
				return;
			} else if (e.Command == "NICK") {
				ChangeNick (e.Sender, e.Parameters [0]);
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

		void ChangeNick (string oldName, string newName)
		{
			if (Users.Contains (oldName)) {
				Users.Remove (oldName);
				Users.Add (newName);
				return;
			} else if (Voiced.Contains (oldName)) {
				Voiced.Remove (oldName);
				Voiced.Add (newName);
				return;
			} else if (HalfOps.Contains (oldName)) {
				HalfOps.Remove (oldName);
				HalfOps.Add (newName);
				return;
			} else if (Ops.Contains (oldName)) {
				Ops.Remove (oldName);
				Ops.Add (newName);
				return;
			} else if (SuperOps.Contains (oldName)) {
				SuperOps.Remove (oldName);
				SuperOps.Add (newName);
				return;
			} else if (ChannelOwner.Contains (oldName)) {
				ChannelOwner.Remove (oldName);
				ChannelOwner.Add (newName);
				return;
			}
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
				SuperOps.Add (name);
				return;
			} else if (name.StartsWith ("~")) {
				ChannelOwner.Add (name);
				return;
			} else if (name.StartsWith ("@")) {
				Ops.Add (name);
				return;
			} else if (name.StartsWith ("%")) {
				HalfOps.Add (name);
				return;
			} else if (name.StartsWith ("+")) {
				Voiced.Add (name);
				return;
			} else {
				Users.Add (name);
				return;
			}
		}

		void RemoveUser (string name)
		{
			if (Users.Contains (name)) {
				Users.Remove (name);
				return;
			} else if (Voiced.Contains (name)) {
				Voiced.Remove (name);
				return;
			} else if (HalfOps.Contains (name)) {
				HalfOps.Remove (name);
				return;
			} else if (Ops.Contains (name)) {
				Ops.Remove (name);
				return;
			} else if (SuperOps.Contains (name)) {
				SuperOps.Remove (name);
				return;
			} else if (ChannelOwner.Contains (name)) {
				ChannelOwner.Remove (name);
				return;
			}
		}

		void Log (IRCMessage e)
		{
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

		public List<String> Users {
			get;
			set;
		}

		public List<String> Voiced {
			get;
			set;
		}

		public List<String> HalfOps {
			get;
			set;
		}

		public List<String> Ops {
			get;
			set;
		}

		public List<String> SuperOps {
			get;
			set;
		}

		public List<String> ChannelOwner {
			get;
			set;
		}


		#endregion

		private List<IRCMessage> joinQueue;

		public event EventHandler<IRCMessage> MessageReceived;
	}
}

