# BiomeGate
Prevent players from exploring and interacting with selected biomes. Protect your server from racing ahead lovers.

Player in selected biomes will be affected by status effect which
* reduce damage dealt
* increase damage received
* reduce sneak and increase noise
* prevent skills from raising
* prevent building (hammer, hoe, cultivator and custom hammers)
* prevent interaction with all interactable objects
* prevent minimap exploring

You can allow admins to ignore effect (enabled by default).

Effects are configurable.

## Installation (manual)
Copy BiomeGate.dll to your BepInEx\Plugins\ folder

## Configurating
The best way to handle configs is [Configuration Manager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/).

Or [Official BepInEx Configuration Manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/).

### Damage modifier exceptions

Use **Gating > Damage modifier ignored skills** to select skills that ignore **Damage dealt modifier**. Configuration Manager shows a checkbox for each skill. By default, no skills are selected and the damage modifier applies as before. Changes apply to the next attack.

For mining and wood cutting, select **Pickaxes** and **WoodCutting**. In the config file, use:

```ini
[Gating]
Damage modifier ignored skills = Pickaxes, WoodCutting
```

Exceptions use the skill of each hit. They apply to all targets, including creatures. A pickaxe attack against a creature also ignores the modifier when **Pickaxes** is selected. An axe hit uses **WoodCutting** only when the attack selects that skill for the target. Hits that still use **Axes**, such as hits on some destructible objects, keep the modifier unless **Axes** is also selected.

This setting does not change skill gain or other BiomeGate restrictions. Skills that are not listed in this setting keep the damage modifier.

## Dependencies

- [BepInExPack Valheim 5.4.2350](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
- [ConditionalConfigSync 1.0.5](https://thunderstore.io/c/valheim/p/shudnal/ConditionalConfigSync/)

Install ConditionalConfigSync as a separate dependency; do not copy its DLLs into this mod's package.

## Donation
[Buy Me a Coffee](https://buymeacoffee.com/shudnal)

## Discord
[Join server](https://discord.gg/e3UtQB8GFK)
