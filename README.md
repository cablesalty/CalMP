# Cats are Liquid - A Better Place | Steam Multiplayer Mod

# Installation
To use this mod (or to contribute to the project), you must own the game on Steam.

# Building and Development
These are the steps required if you want to build the project for yourself, or if you want to contribute. **This is NOT required if you just want to play**.

## Prerequsites
You must have **Visual Studio 2022** or **Visual Studio 2026** installed. TODO: Check if you need melonloader too

## Building
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