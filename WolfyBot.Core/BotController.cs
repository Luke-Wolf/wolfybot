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
	/// <summary>
	/// <para>The Controller Class for the IRC Bot, Selects the commands that need to be executed based upon one
	/// of three possibilities:</para>
	/// <para>1). A command is just listening for an IRC command</para>
	/// <para>2). A command is listening for a command and a parameter</para>
	/// <para>3). A command is listening for a command and a trailing parameter (Most common)</para>
	/// 
	/// </summary>
	public class BotController
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WolfyBot.Core.BotController"/> class.
		/// </summary>
		/// <param name="commands">A collection of bot commands for the controller to execute</param>
		/// <param name="botPassword"> A Password for the bot to recognize the owner of the bot by, 
		/// authenticate via a PRIVMSG directly to the bot</param>
		public BotController (IEnumerable<IBotCommand> commands, String botPassword)
		{
			password = botPassword;
			ownerNick = String.Empty;
			if (commands.Count () == 0) {
				throw new NotSupportedException ();
			}

			_commands = new List<IBotCommand> (commands);
			foreach (var item in _commands) {
				item.ScriptMessage += new EventHandler<IRCMessage> (ScriptMessageHandler);
			}
		}

		/// <summary>
		/// Raises the message sent event. Subscribe to this to get messages from the IBotCommands
		/// </summary>
		protected virtual void OnMessageSent (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = MessageSent;
			if (handler != null) {
				handler (this, e);
			}
		}

		/// <summary>
		/// Handler for receiving messages from an IRC Server connection, selects and passes IRCMessages down to
		/// IBotCommands
		/// </summary>
		public void ReceiveMessageHandler (Object sender, IRCMessage e)
		{
			try {
				var server = sender as IRCServer;
				//handle Setting ownership on bot
				if (e.Command == "PRIVMSG" && e.Parameters [0] == server.Nick && e.TrailingParameters == password) {
					Console.WriteLine ("USER:" + e.Sender + " Now has control of the bot");
					ownerNick = e.Sender;
				} //handle nick change by owner
				if (e.Sender == ownerNick && e.Command == "NICK") {
					Console.WriteLine ("Owner Changed Nick to: " + e.TrailingParameters);
					ownerNick = e.TrailingParameters;
				}//handle owner leaving the server
				if (e.Sender == ownerNick && e.Command == "QUIT") {
					Console.WriteLine ("Owner has left the server");
					ownerNick = String.Empty;
				}
			} catch (NullReferenceException ex) {
				Console.WriteLine ("Caller is not an IRC server connection");
			}
			var commandItems = (
			                       from c in _commands //Get Modules that are just listening for a command
	                               where c.CommandWords.Contains (e.Command) &&
			                           c.ParameterWords.Count == 0 &&
			                           c.TrailingParameterWords.Count == 0
			                       select c
			                   ).Union (//Get modules that are listening for a command and a parameter
				                   from c in _commands
				                   from p in c.ParameterWords
				                   where c.CommandWords.Contains (e.Command) &&
				                       e.Parameters.Contains (p)
				                   select c
			                   ).Union (// get modules that are listening for a command and a trailing parameter
				                   from c in _commands
				                   from p in c.TrailingParameterWords
				                   where c.CommandWords.Contains (e.Command) &&
				                       e.TrailingParameters.Contains (p)
				                   select c
			                   );
			foreach (var item in commandItems) {
				try {
					var server = sender as IRCServer;
					if (CheckPermissions (item, e, server))
						item.Execute (sender, e);
				} catch (Exception ex) {
					Console.WriteLine (ex.Message);
				}
			}
		}

		/// <summary>
		/// Handler for Scripts to Attatch to, Hooks up to the OnMessageSent Handler
		/// For Internal Usage Only
		/// </summary>
		public void ScriptMessageHandler (Object sender, IRCMessage e)
		{
			OnMessageSent (e);
		}

		static bool CheckPermissions (IBotCommand command, IRCMessage message, IRCServer server)
		{
			//If SecureLevel is the Bot, then it's an "internal" command for the bot, such as
			//ponging a server
			if (command.SecureLevel == SecureLevelEnum.Bot)
				return true;
			//If the "Channel" is empty then it is a message from the server
			if (message.Channel == String.Empty)
				return true;

			IRCChannel _channel = null;
			foreach (var channel in server.Channels) {
				if (channel.ChannelName == message.Channel) {
					_channel = channel;
					break;
				}
			}

			//should not happen
			if (_channel == null)
				return false;
			//Check if the channel is muted and the command is interactive
			if (_channel.Muted && command.Interactive)
				return false;
			var user = _channel.FindUser (message.Sender);
			//if the user's nick is the owner nick of the bot set the mode
			//for the channel to be Owner
			if (user.Nick == ownerNick) {
				user.Mode = SecureLevelEnum.Owner;
			}
			return user.Mode >= command.SecureLevel;
		}

		public event EventHandler<IRCMessage> MessageSent;

		static String password;
		static String ownerNick;
		List<IBotCommand> _commands;
	}
}
