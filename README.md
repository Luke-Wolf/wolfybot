wolfybot
========

An Object Oriented, Extensible IRC Bot Written in C#

Bot Currently provides the following modules:

* KeepAlive (Maintains connection to server; you cannot remove or disable this)
* NickServIdentify (Identifies the bot to the configured nickserv)
* ProxyDetection (Detects proxies by checking the configured ports on a user who  
 joins, and kickbans them if those ports are open).

Before you run the bot you should generate a default configuration file using  
 WolfyBot.CLI.exe --new-config and configure it to your liking

 In order to extend behavior implement the IBotCommand interface, and then extend  
 the configurator 

 This bot implements security based on a securelevel concept. An invokable module,  
 which is to say one that can be invoked by a user should offer the option to configure  
 a level to determine who can invoke a command (where 0 is all users, and 6 is only the owner of the bot)  
 Noninvokable modules should set their securelevel to Bot internally.

 if you wish to mute the bot use !shutup botnick, where botnick is the name of the bot  
 similarly to unmute the bot use !talk botnick. Muting the bot will only stop interactive  
 modules, which is to say those that message the channel, non interactive modules will still  
 run.
