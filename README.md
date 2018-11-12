# PathfinderKingmakerMods
My Collection of Pathfinder Kingmaker Mods

# FolderSaverMod:

When a savegame is loaded the Areas folder is cleared (AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Areas).
Also all files from the save zip file, including 200 areas are extracted and copied to the area folder. (up to 200MB?)
This obviously scales horribly as you progress in the game or keep loading a Game in the same area.

This mod changes this behavior to use a plain FolderSaver instead (no zipping, unzipping)
Also we only copy changed files to the areas folder, no need to copy 200 areas when we're only in one area.
This reduces loading times probably by about 2-5 seconds, depending on your CPU and HardDrive.

Disadvantages: Steam Upload probably wont work anymore, or uploads of saves to Owlcat.
Your Savefiles will be larger, because no zip.
If you remove this mod: Loading of old Folder Saves wont work anymore because of a bug in the original code of FolderSaver.CopyFromStash.
To fix you would probably need to convert your FolderSave back to a ZipSave.
(Just zip the folder content and copy the zip to AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games\)

If anyone likes to copy this code to their Unity Mod Manager / Kingmaker Mod Loader Mod feel free to do so.
This mod uses patchwork  https://github.com/GregRos/Patchwork, i dont have time to convert it to a Unity Mod Manager Mod currently.

If anyone from Owlcat finds this mod, feel free to copy my code into the game or change how you like, maybe add an option to use a FolderSaver or ZipSaver
