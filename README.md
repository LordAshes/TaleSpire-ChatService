# Custom Stats Plugin

This unofficial TaleSpire adds mini or group specific stats replacing or adding to the common core stats.

This plugin, like all others, is free but if you want to donate, use: http://198.91.243.185/TalespireDonate/Donate.php

Video Link: https://youtu.be/TV8ruMDe1YE

!Preview(https://i.imgur.com/KXtvswm.png)

Donate: http://198.91.243.185/TalespireDonate/Donate.php

Lord Ashes' Talespire Plugins are always available for free. Lord Ashes does not have a Patreon but if you really want
to make a donation to the chocolate fund to keep the work going, you can do so using the Donate link above.

## Change Log

```
1.1.1: Fixed soft dependency check to avoid exception when soft dependency is not found
1.1.1: Corrected dependency list to include FileAccessPlugin
1.1.0: Bug fix
1.0.0: Initial release
```

## Install

Use R2ModMan or similar installer to install this plugin.

Any configurable settings can be set using R2ModMan config editor for the plugin.


## Usage

In order to add custom stats to a mini (creature or creature prop), first the desired model need to be applied to the
desired mini. A model is a simple list of stats that any mini using that model will have. To apply an existing model,
use:

```
Right CTRL + A = Prompts for the model name and then applies the corresponding custom stats to the selected mini.
```

To view and/or edit the custom stats, (right click to) open the raidal menu of the mini and select Custom Stats.
Please note, depending on the setting in the configuration for this plugin, the core option for Stats may or may
not appear in the radial menu, as such ensure that the raidal selection is Custom Stats and not Stats.

To edit custom stats, simply click on the desired text box and change the entry. Once all desired changes have been
made (or none changes have been made if only looking at the custom stats) press the red X button at the centre of the
custom stats menu to close the menu and save any changes. Any changes are not made permanenet until the menu is closed.

### Making Models

The Custom Stats Plugin uses a concept of model to apply custom stats to minis. A model is just a plain text file with
a list of custom stats. Depending on the intended use, a model can range from very specific (specific to a specific player)
to very broad (a group model use by many minis such as all props). By creating a model, it is easy to apply the exact
same custom stats to multiple minis.

The rules for making a model are as follows:

```
- A model file is a plain text file
- Only the first line of a model file is read. Successive lines can be used for comments.
- The first line has a list of one or more custom stat names
- Entries in the first line a separated by a comma
- Entries in the first line do not have spaces before or after the comma
```
For example:
```
AC,Move,Inspirations,SpellLV1,SpellLV2
```

### Stats Sync

To enable the Custom Stats plugin to be used with plugins that modify the core stats, there is a built in synchronization
between the core TS stats and custom stats that share the same name. When the custom stats are opened, any custiom stats
whose name matches the name of a core TS stat, will automatically be updated from the core TS stat. Similarly when the user
makes changes to custom stats, the if the custom stat has a corresponding core TS stat (a core TS stat that shares the same
name) the core TS stat will be updated with the custom value. Custom values with only a single number will update both the
core TS stat value and max value to custom value. Custom values which have two numbers separated by a forward slash, will
update the core TS stat value with the number before the slash and the core TS stat max value with the number after the
forward slash.
```
25/30 => Value = 25, Max Value = 30
```

This solution allows two ways in which the core and custom stats can be used:

#### Integrated Mode

In this mode, the option to show the core TS stats is turned off (thus hiding the old core TS menu option) and any core
stats are also listed in the custom stats. This provides the users with a single menu from which they can see both the
common stats and the custom specific stats but due to the stats sync function, the custom stats which match the names of
core stats are updated when core stats are updated. To use this mode:

```
1. Set the configruation to hide the core TS stats
2. Set the names of any core TS stats (as you would normally do when not using the plugin)
3. Create models which include the core stats names
```

#### Separated Mode

In this mode, the core stats are used as normal for any stats which are common to all minis and the custom stats plugin
is only used for additional custom stats. This option will produce two selection in the radial menu (Stats and Custom Stats)
and thus easily allow for looking up common stats or custom stats. In such a case, no synchronization between the core TS
stats and custom stats is necessary. To use this mode:

```
1. Set the configruation to show the core TS stats
2. Set the names of any core TS stats (as you would normally do when not using the plugin)
3. Create models which do not include the core stats names
```

## Limitations

```
1. Currently the synchronization between core stats and custom stats is done when the custom stats are opened. This means
   that any plugins that use character stats should continue to use the core TS stats to read such stats instead of reading
   the custom stats JSON from Stat Messaging. Only custom stats which are note synced with core stats should be read using
   the JSON acquired from Stat Messaging. This is necessary because if a plugin has changed the core stats, the corresponding
   JSON will not be changed until the radial menu has been opend and thus reading the JSON will provide the old value.
   
2. Currently the changed values are updated when the red "X" close button is used. Reading the JSON values of any changes
   stats when the "X" close button as not been used, results in displpay of the old values. As a result, after making any
   changes, do not keep the custom stats open. If it is desirable to have the custom stats open, close the custom stats
   using the red "X" button and then re-open them.
```
