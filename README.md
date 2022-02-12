# Chat Service Plugin

This unofficial TaleSpire plugin is a dependency plugin for creating local chat service functionality.
The plugin allows registering of key words with corresponding handlders to create functionality such
as whispers or dice rolling functionality.
 
## Change Log

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

## Install

Use R2ModMan or similar installer to install this plugin.

### Optional Dependency

Chat Service Plugin has a soft dependency on Radial UI plugin. If the deselect option is turned on in
the settings then Radial UI plugin is required. If the deselect option is turned off then Radial UI
plugin is not required. 
   
## Usage

Reference this dependency plugin in the parent plugin and then use the following syntax to add a chat
service:

```handlers.Add(serviceKey, handler)```

Where the service key is a string that must appear at the beginning of the chat message in order to trip
the corresponding handler.

Where hander is a function that takes in two string, the message content and the sender, and a source
which is a ChatSource enumeration indicating if the source if a GM message, player message or creature
message. The hanlder returns a string, the modified message or null. Returning null prevents the message
from being displayed.

An example of adding a inline handler for "/w" function would be:

```
handlers.Add("/w ", (chatMessage, sender, source)=> { Debug.Log(sender+" whispered "+chatMessage); });```

### Usage programatically

You can also use this function for sending messages to other clients without having the request triggered
by the user from the chat. This can be done by using the core TS function:

``ChatManager.SendChatMessage(message, sender)``

Where *message* is the content to be sent and should include the handling prefix.
Where *sender* is the NGuid of either a creature mini (CreatureId) or player (PlayerId).

For example, to use the above hander for "/w" assuming it is a whisper message to the GM:

``ChatManager.SendChatMessage("/w I pickpocket the person I am speaking to", LocalPlayer.Id.Value)``

