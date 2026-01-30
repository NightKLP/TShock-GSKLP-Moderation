# TShock-GSKLP-Moderation
Moderation Plugin from GSKLP Server

In order to install the plugin download `MKLP.dll` in releases after that put that file in `ServerPlugins` folder

⚠️ Warning! you need `Discord.Net.Rest.dll`, `Discord.Net.Core.dll`, `Discord.Net.WebSocket.dll` in `bin` folder!
- MKLP uses Discord.Net v3.13.0

## Features
- Disable Feature ( MKLP )
- BossManager ( Modified ) { from Ozz5581 }
- Discord Bot ( GSKLP )
- Illegal Progression ( MKLP )
- Warning Suspicion Activity `dupe` ( MKLP )
- InventoryViewer ( Modified ) { from Nightklp }

# Guide
**On Config File** `MKLP.json`
- **Replace_Ban_TShockCommand** if set to true `/qban` from mklp will replace `/ban` from tshock
- **Replace_Mute_TShockCommand** if set to true `/qmute` from mklp will replace `/mute` from tshock

- All permissions are in `MKLP.json` in `tshock` folder which they can be modified ( require's restart when modifying it )

# Disable Code

**Main code1** : High Stack Value

**Main code2** : null item boss spawn

**Survival code1** : Illegal Item Progression

**Survival code2** : Illegal Projectile Progression

**Survival code3** : Illegal Tile Progression

**Survival code4** : Illegal Wall Progression

**Default code1** : Tile Kill Threshold

**Default code2** : Tile Place Threshold

**Default code3** : Tile Paint Threshold

**Default code4** : Liquid Threshold

**Default code5** : Projectile Threshold

**Default code6** : HealOther Threshold

# Commands

### Default
**/ping** : Display's players latency

**/progression** : Displays defeated boss

**/report** : Report any suspicious activity

### Staff
**/staffchat** : sends your message who has **Staff** permission

### Admin
**/clearmessage** : Clears the whole message chat

**/lockdown** : Prevents Players from joining the server

**lockdownregister** : Prevents Players to register their account

**/tpmap** : toggle tp map ping

**/clearlag** : Deletes low value npc/items and projectile

**/manageboss** : Manage it by enable/disable boss or schedule it

**/vanish** : allows you to become completely invisible to players.

### Moderator

**/managereport** : View/Delete any reports

**/baninfo** : Displays ban information using ban ticket number

**/disable** : Acts as Ban but prevents players from doing anything

**/enable** : enable's a player that got disabled

**/unban** : Removes ban tickets
**/qban** :  Bans a player

**/unmute** : Unmutes a player
**/mute** : Mutes a player

### Inspect
**/inventoryview** : "View's inventory of a player

**/spy** : spy a player

### Manage
**/mklpdiscord** : manage account link players which can be used when `UsingDB` and `UsingMKLPDatabase` is true on config
