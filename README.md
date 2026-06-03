# Cats are Liquid - A Better Place | Steam Multiplayer Mod

# Installation
**To use this mod (or to contribute to the project), you must own the game on Steam, and you must use a computer.** This mod uses steam networking to make and host lobbies, therefore you cannot use it on mobile or with the Itch version.

## Installation video

## Installation steps
1. Install MelonLoader [from their site](https://melon-loader.com/) or [GitHub (releases)](https://github.com/LavaGang/MelonLoader/releases/), and mod the game
2. Download the mod from [the releases tab](https://github.com/LavaGang/MelonLoader/releases/)
3. Copy the mod into the game's `mods` folder (game's folder can be opened by locating the game in your steam library, then clicking `Settings > Manage > Browse local files`)
4. Launch the game. If you succeeded, there should be a notification telling you the mod loaded correctly.

# Building and Contributing
These are the steps required if you want to build the project for yourself, or if you want to contribute. **This is NOT required if you just want to play**.

## Prerequsites
You must have **Visual Studio 2022** or **Visual Studio 2026** installed, and you must mod the game using MelonLoader. (Follow the [installation steps or the video](#installation) to mod the game).

## Building
After you modded the game using MelonLoader, follow these steps if you want to build the project.

1. Clone the repository and open the solution (`CalMP_Melon.slnx`)
2. Create the following file
    ```
    (Repository)\CalMP_Melon\Directory.Build.props
    ```
    This file will tell MSBuild where your local game installation is located, allowing the linker to reference the game's assemblies correctly.

3. Edit the following file to include your game path like the example below. To get this path, locate the game in your steam library, then click `Settings > Manage > Browse local files`
    ```xml
    <Project>
      <PropertyGroup>
        <!-- Replace with your own installation path -->
        <GamePath>C:\Program Files (x86)\Steam\steamapps\common\Cats are Liquid - A Better Place</GamePath>
      </PropertyGroup>
    </Project>
    ```
    (The path must be the location of your game install (where the .exe is located), not the mods directory.)
4. Build the project by pressing `CTRL + B` or by going to `Build > Build CalMP_Melon` (or `Build > Build Solution`)
5. Check if MSBuild success copied the mod into the game's `mods` directory, then start the game to test it.

## Contributing
Before making any contributions, read the [contribution guidelines](CONTRIBUTING.md).

# Notes