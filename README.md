# Enhanced Inventory

Enhances the inventory experience. Adds features such as: a search bar to all inventory screens, additional filtering and sorting options, customizable filter and sort lists, highlight important loot in the loot window. Options are highly configurable through the UMM mod options screen.

# How to install

0. Download the latest published zip file.
1. Install [UnityModManager](https://www.nexusmods.com/site/mods/21)
2. Install the zip with UnityModManager.

# How to Compile

0. Install all required development pre-requisites:
	- [Visual Studio 2019 Community Edition](https://visualstudio.microsoft.com/downloads/)
	- [.NET "Current" x86 SDK](https://dotnet.microsoft.com/download/visual-studio-sdks)
1. Download and install [Unity Mod Manager (UMM)](https://www.nexusmods.com/site/mods/21)
2. Execute UMM, Select Pathfinder: WoTR, and Install
3. Create the environment variable *WrathInstallDir* and point it to your Pathfinder: WoTR game home folder
	- tip: search for "edit the system environment variables" on windows search bar45. Use "Install Release" or "Install Debug" to have the Mod installed directly to your Game Mods folder
4. Run [AssemblyPublicizer](https://github.com/CabbageCrow/AssemblyPublicizer) on the WotR Assembly-CSharp.dll inside the WotR folder you set earlier.
5. Build! If you get assembly reference errors, check the project file and make sure your publicized assembly is in the correct location.

# Links

Source code: https://github.com/cstamford/WOTR_EnhancedInventory
Nexus: https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/137
