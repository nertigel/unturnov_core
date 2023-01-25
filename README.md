## unturnov_core
### An Unturned OpenMod plugin that used to "manage" my localhost.

## commands:
* /skills - set skills to max level (A: [/maxskills]
* /pm - send a message to a player (A: [/dm, /tell, /msg])
* /reply - reply to previous message (A: [/r])
* /heal - heal yourself/diff player

## events:
* UnturnedPlayerDeathEvent: removes all items from player except items in Hands aka secure container and drop a dogtag(STG) on player's death location.
* UnturnedPlayerConnectedEvent: modify container size.

## secure container size permissions:
* Nertigel.EfUCore:leftbehind = 4x3
* Nertigel.EfUCore:prepareforescape = 4x4
* Nertigel.EfUCore:edgeofdarkness = 5x4
