# Chat Service Plugin

This unofficial TaleSpire plugin is a dependency plugin for creating local chat service functionality.
The plugin allows registering of key words with corresponding handlders to create functionality such
as whispers or dice rolling functionality.
 
## Change Log

1.0.1: Added source in the handler to distinguish messages from gm, player or creature.

1.0.1: Fixed bug when using multiple hanlders and one returns a null.

1.0.0: Initial release

## Install

Use R2ModMan or similar installer to install this plugin.
   
## Usage

Reference this dependency plugin in the parent plugin and then use the following syntax to add a chat
service:

```ChatServicePlugin.handlers.Add(serviceKey, handler)```

Where the service key is a string that must appear at the beginning of the chat message in order to trip
the corresponding handler.

Where hander is a function that takes in two string, the message content and the sender, and a source
which is a ChatSource enumeration indicating if the source if a GM message, player message or creature
message. The hanlder returns a string, the modified message or null. Returning null prevents the message
from being displayed.

An example of adding a inline handler for "/w" function would be:

```
handlers.Add("/w ", (chatMessage, sender, source)=> { Debug.Log(sender+" whispered "+chatMessage); });```


