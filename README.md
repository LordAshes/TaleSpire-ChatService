# Chat Service Plugin

This unofficial TaleSpire plugin is a dependency plugin for creating local chat service functionality.
The plugin allows registering of key words with corresponding handlders to create functionality such
as whispers or dice rolling functionality.
 
## Change Log
```
2.4.5: Updated to fix compatibility issue with BR update
2.4.5: Updated to fix compatibility issue with deselected radial function
2.4.5: Minor bug fix with chat logging
2.4.2: Bug fix for modifier in log dice result
2.4.1: Bug fix for endless loopp when handler throws an exception
2.4.0: Added support for Aliases
2.4.0: Simplifed code
2.3.0: Added chat logging capabilities
2.3.0: Fixed deselect option
2.2.2: Mini does not say chat messages that are chat service handlers
2.2.1: Improved logs and handler processing
2.2.0: Improved speaker setting code to eliminate exception at start
2.1.1: Made method for checking if a chat key is registered
2.1.0: Added custom header messages
2.1.0: Improved patching to not replace code so that it works with updates like the chat ! roll
2.1.0: Removed legacy code and legacy access
2.0.2: Fix bug with diagnostic configuration setting
2.0.1: Added configurable diagnostic log level
2.0.0: Fix after BR HF Integration update
1.2.0: Added ChatMessageService class for proper subscription and sending handling and AssetData support.
1.1.2: Corrected documentation. No plugin change.
1.1.1: Fixed soft dependency check to avoid exception when soft dependency is not found
1.1.1: Corrected dependency list to include FileAccessPlugin
1.1.0: Added optional deselect action to the character radial menu to deselect the mini and returns
       the chat box input back to speaking a player (and not creature). Can be turned on or off in
	   the R2ModMan settings for plugin. If on, requires RadialUI plugin. If off, RadialUI plugin is
	   not needed.
1.0.1: Added source in the handler to distinguish messages from gm, player or creature.
1.0.1: Fixed bug when using multiple hanlders and one returns a null.
1.0.0: Initial release
```
## Install

Use R2ModMan or similar installer to install this plugin.

### Optional Dependency

Chat Service Plugin has a soft dependency on Radial UI plugin. If the deselect option is turned on in
the settings then Radial UI plugin is required. If the deselect option is turned off then Radial UI
plugin is not required. 
   
## Usage

Reference this dependency plugin in the parent plugin and then use the following syntax to add a chat
service:

```chatMessgeServiceHandlers.Add(serviceKey, handler)```

Where the service key is a string that must appear at the beginning of the chat message in order to trip
the corresponding handler.

Where hander is a function that takes in two string, the message content and the sender, and a source
which is a ChatSource enumeration indicating if the source if a GM message, player message or creature
message. The hanlder returns a string, the modified message or null. Returning null prevents the message
from being displayed.

An example of adding a inline handler for "/w" function would be:

```
chatMessgeServiceHandlers.Add("/w ", (chatMessage, sender, source)=> { Debug.Log(sender+" whispered "+chatMessage); });```

### Usage programatically

You can also use this function for sending messages to other clients without having the request triggered
by the user from the chat. This can be done by using the core TS function:

``ChatManager.SendChatMessage(message, sender)``

Where *message* is the content to be sent and should include the handling prefix.
Where *sender* is the NGuid of either a creature mini (CreatureId) or player (PlayerId).

For example, to use the above hander for "/w" assuming it is a whisper message to the GM:

``ChatManager.SendChatMessage("/w I pickpocket the person I am speaking to", LocalPlayer.Id.Value)``

### Custom Headers

Messages whose content contains text in square brackets is treated as custom header messages. The message
must start with an opening square bracket, have some text, have an ending square bracket, and finally have
some content. Such messages will replace the usual character or player name in the chat message header with
the contents between the square brackets and limit the message content to the contents after the ending
square. For example:

[Bilbo]Is it time to eat yet?

Would create a chat message whose header would be "Bilbo" and message content would be "Is it time to eat yet?"
regardless of who sent the message.  

### Aliases

Aliases are an optional feature supported for any ``/`` services. To use aliases, in a File Access Plugin legal
location, create a JSON file called ``Chat_Service_Aliases.json``. The contents of the file is a dictionary
of aliases and their value. For exmaple:
```
{
	"rps": "cr RPS",
	"draw": "cr DRAW"
}
```
In the above example, an alias of ``rps`` is mapped to ``cr RPS``. That means instead of having to type ``/cr RPS``
the user can just do ``/rps``. Similarly the user can just type ``/draw`` to execute ``/cr DRAW``. Note that the
alias configuration does not include the ``/`` character. Both the alias and the result assume a ``/`` character. 


### Chat Logging

When turned on, chat logging writes all chat messages to an time stamped HTML file. This creates a log or
record of the session's chat for later review. Unlike the Log Chat plugin which only logs what the player was
able to see in terms of chat messages (e.g. not showing messages that were wispered to other people) this
log feature logs even messages that were not displayed in the player's chat (e.g. messages whispered to other
players).

When a message was not seen by the player due to a chat service (such as Chat Whisper) the log indicates the
chat service that was used. This detection is not perfect but good enough. If a custom header was used the
detection will identify the service and parameters. If a custom header is not used then only the service is
identified and the parameters will show up as part of the message.

The log file can be configured in the R2ModMan settings for the plugin. This entry is the prefix of the file
and will be suffixed by the date and time of the session start and an HTML extension. This will generate one
log file per session.

The style of the log can also be modified using R2ModMan settings for the plugin.

Lastly, since this plugin logs all of the chat messages even if a chat service handler reduces the message
to nothing (e.g. Chat Whisper, AssetData, etc), it is necessary to add a filter that filters out chat service
messages which are undesirable to log. For example, Asset Data Plugin can use Chat Service to pass messages to
other players. Normally these types of messages are internal and not desired to be logged. As such the default
list of ignored services includes AssetData. If there are other chat services being used which you do not want
logged (e.g. Lookup Plugin) then just add the service to the ignore list.

If the log file prefix is empty, logging is turned off.
