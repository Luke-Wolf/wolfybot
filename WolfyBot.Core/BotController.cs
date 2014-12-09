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
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace WolfyBot.Core
{
	public class BotController
	{
		public BotController (List<IBotCommand> commands, String botPassword)
		{
			password = botPassword;
			ownerNick = String.Empty;
			if (commands.Count == 0) {
				throw new NotSupportedException ();
			}
			_commands = commands;
			foreach (var item in _commands) {
				item.ScriptMessage += new EventHandler<IRCMessage> (ScriptMessageHandler);
			}
		}


		protected virtual void OnMessageSent (IRCMessage e)
		{
			EventHandler<IRCMessage> handler = MessageSent;
			if (handler != null) {
				handler (this, e);
			}
		}

		public void ReceiveMessageHandler (Object sender, IRCMessage e)
		{
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

            var commandItems = (from c in _commands                              
                               
                                where c.CommandWords .Contains(e.Command) && 
                                c.ParameterWords.Count == 0 &&
                                c.TrailingParameterWords.Count == 0
                                select c).Union(
                                from c in _commands
                                from p in c.ParameterWords
                                where c.CommandWords.Contains(e.Command) &&
                                e.Parameters.Contains(p)
                                select c
                                ).Union(
                                from c in _commands
                                from p in c.TrailingParameterWords
                                where e.TrailingParameters.Contains(p)
                                select c
                                    );
            foreach (var item in commandItems)
            {
                try
                {
                    if (CheckPermissions (item, e, server))
								item.Execute (sender, e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            /*
			foreach (var item in commandItems) {
				if (item.CommandWords.Contains (e.Command)) {
					//If a command script is listening for a command as opposed to parameters
					//or trailing parameters invoke it
					if (item.ParameterWords.Count == 0 && item.TrailingParameterWords.Count == 0) {
						try {
							if (CheckPermissions (item, e, server))
								item.Execute (sender, e);
						} catch (Exception ex) {
							Console.WriteLine (ex.Message);
						}

						//if a command script is looking for parameters but not trailing parameters
						//invoke it
					} else if (item.ParameterWords.Count > 0 && item.TrailingParameterWords.Count == 0) {
						foreach (var item2 in item.ParameterWords) {
							if (e.Parameters.Contains (item2)) {
								try {
									if (CheckPermissions (item, e, server))
										item.Execute (sender, e);
								} catch (Exception ex) {
									Console.WriteLine (ex.Message);
								}
							}
						}
						//if a command script is looking for words in a trailing parameter
						//invoke it.
					} else if (item.TrailingParameterWords.Count > 0) {
						foreach (var item3 in item.TrailingParameterWords) {
							if (e.TrailingParameters.Contains (item3)) {
								try {
									if (CheckPermissions (item, e, server))
										item.Execute (sender, e);
								} catch (Exception ex) {
									Console.WriteLine (ex.Message);
								}
							}
						}
					}
				}
			}*/
		}

		public void ScriptMessageHandler (Object sender, IRCMessage e)
		{
			OnMessageSent (e);
		}

		static bool CheckPermissions (IBotCommand command, IRCMessage message, IRCServer server)
		{
			if (command.SecureLevel == SecureLevelEnum.Bot)
				return true;
			if (message.Channel == String.Empty)
				return true;
			IRCChannel _channel = null;
			foreach (var channel in server.Channels) {
				if (channel.ChannelName == message.Channel)
					_channel = channel;
				break;
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
