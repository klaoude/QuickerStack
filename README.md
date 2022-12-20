# QuickStackStore

QuickStackStore is a BepInEx mod for the game Valheim that allows players to
- quickly stack their items into the current or nearby chests
- quickly restock the items they want (like food and ammo) from the current or nearby chests
- store all items into the current chest (complementary to the 'take all' button)
- sort the player inventory or the current chest by configurable criteria
- trash the currently held item or quick trash all previously trash flagged items in the player inventory

All these features are controlled by the option to favorite items or slots similar to games like Terraria.

For further information visit its [Nexus page](https://www.nexusmods.com/valheim/mods/2094).

## Credits

- QuickStackStore is based on the [Quick Stack mod](https://www.nexusmods.com/valheim/mods/29) by damnsneaker, who gave me permission to upload this (and has since changed the nexus permission settings)
- QuickStackStore is actually forked off of the [Quicker Stack mod](https://github.com/klaoude/QuickerStack) by klaoude, which is a decompiled version with a few bug fixes of the Quick Stack mod and is released under MIT licence, but the threading that is special to Quicker Stack was removed from QuickStackStore
- The sorting is based on the [Inventory Sorting mod](https://github.com/end360/Valheim-Inventory-Sorting) by end360, which is under MIT licence
- The trashing is based on the [Trash Items mod](https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems) by virtuaCode which has permissive settings on its [nexus page](https://www.nexusmods.com/valheim/mods/441)
- various good Valheim coding practises like Keybind checking from Aedenthorn's mods (https://github.com/aedenthorn/ValheimMods), public domain