# Cats are Liquid - A Better Place | Steam Multiplayer Mod

## Contributing

To contribute to the project, you must own the game on Steam.

After cloning the repository, create the following file:

```text
Repo/CalMP_Melon/Directory.Build.props
```

This file tells MSBuild where your local game installation is located, allowing the linker to reference the game's assemblies correctly.

### Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <!-- Replace with your own installation path -->
    <GamePath>D:\SteamLibrary\steamapps\common\Cats are Liquid - A Better Place</GamePath>
  </PropertyGroup>
</Project>
```
(The path must be the location of your game install (where the .exe is located), not the mods directory.
You can open that directory in steam by clicking `Settings > Manage > Browse local files` on the game's page.)

## Building the Project

1. Open (or reopen) the solution:

   ```text
   Repo/CalMP_Melon.slnx
   ```

2. Use **Visual Studio 2022** or **Visual Studio 2026**.

3. Build the project by pressing:

   * **F7** (Build Solution)
   * **Ctrl + B**
