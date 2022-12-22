# QuickStackStore

QuickStackStore is a BepInEx mod for the game Valheim that allows players to
- quickly stack their items into the current or nearby chests
- quickly restock the items they want (like food and ammo) from the current or nearby chests
- store all items into the current chest (complementary to the 'take all' button)
- sort the player inventory or the current chest by configurable criteria
- trash the currently held item or quick trash all previously trash flagged items in the player inventory

All these features are controlled by the option to favorite items or slots similar to games like Terraria.

For further information visit its [Nexus page](https://www.nexusmods.com/valheim/mods/2094).

## Building the project

#### Game Directory
- If your Valheim game directory is not coincidentally at the same location as mine (`D:\Steam\steamapps\common\Valheim`), edit [GameDir.targets](/QuickStackStore/GameDir.targets) accordingly. The standard windows location is `C:\Program Files (x86)\Steam\steamapps\common\Valheim`.
- Remember to not stage and commit any changes to `GameDir.targets`
#### BepInEx
 - Download and install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) *(this is an Valheim specific pack)*
 - Follow the information under manual install
#### BepInEx Publicizer
 - To get access to private members, you need the publicized assembly files
 - Install the assembly publicizer for BepInEx from: https://github.com/elliotttate/Bepinex-Tools/releases
 - The `Bepinex-Publicizer` folder from the `.zip` should be placed under `<ValheimGameDirectory>\BepInEx\plugins`
 - Run the game once, BepInEx console should pop-up. At the background, BepInEx Publicizer will create assemblies  
 under `<ValheimGameDirectory>\valheim_Data\Managed\publicized_assemblies`
 
 You should now successfully build this project 🎉

## Credits

- QuickStackStore is based on the [Quick Stack mod](https://www.nexusmods.com/valheim/mods/29) by damnsneaker, who gave me permission to upload this (and has since changed the nexus permission settings)
- QuickStackStore is actually forked off of the [Quicker Stack mod](https://github.com/klaoude/QuickerStack) by klaoude, which is a decompiled version with a few bug fixes of the Quick Stack mod and is released under MIT licence, but the threading that is special to Quicker Stack was removed from QuickStackStore
- The sorting was based on the [Inventory Sorting mod](https://github.com/end360/Valheim-Inventory-Sorting) by end360, which is under MIT licence, even if most of it has been rewritten by now
- The trashing is based on the [Trash Items mod](https://github.com/virtuaCode/valheim-mods/tree/main/TrashItems) by virtuaCode which has permissive settings on its [nexus page](https://www.nexusmods.com/valheim/mods/441)
- various good Valheim coding practises like Keybind checking from Aedenthorn's mods (https://github.com/aedenthorn/ValheimMods), public domain
