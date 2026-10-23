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

### Player damage received

Use **Gating > Damage received modifier** to change incoming damage for affected players. Set it to **Normal** to keep the player's existing resistances and immunities. This setting is separate from **Damage dealt modifier** and **Creature damage received multiplier**.

### Damage modifier exceptions

Use **Gating > Damage modifier ignored skills** to select skills that ignore **Damage dealt modifier**. Configuration Manager shows a checkbox for each skill. By default, no skills are selected and the damage modifier applies as before. Changes apply to the next attack.

For mining and wood cutting, select **Pickaxes** and **WoodCutting**. In the config file, use:

```ini
[Gating]
Damage modifier ignored skills = Pickaxes, WoodCutting
```

Exceptions use the skill of each hit. They apply to all targets, including creatures. A pickaxe attack against a creature also ignores the modifier when **Pickaxes** is selected. An axe hit uses **WoodCutting** only when the attack selects that skill for the target. Hits that still use **Axes**, such as hits on some destructible objects, keep the modifier unless **Axes** is also selected.

This setting does not change skill gain or other BiomeGate restrictions. Skills that are not listed in this setting keep the damage modifier.

### Creature damage received

Use **Gating > Creature damage received multiplier** to change damage received by creatures in gated biomes. The default value is **1**, which disables this check. The setting applies to all non-player creatures, including bosses and tamed animals, regardless of the damage source.

The check uses the creature's current biome and the existing global key rules. If **Disable gating if global key is set** is enabled and the biome's global key is set, the multiplier does not apply. Player keys, known items, and admin exceptions do not affect this setting.

For example:

```ini
[Gating]
Damage dealt modifier = 0.5
Damage modifier ignored skills = Pickaxes, WoodCutting
Creature damage received multiplier = 0.5
```

When both restrictions are active, ordinary weapon hits against creatures receive both damage modifiers. Hits against objects receive only the player's modifier. Hits that use **Pickaxes** or **WoodCutting** skip the player's modifier, so mining and wood cutting keep their damage. A pickaxe hit against a creature still receives the creature multiplier.

The multiplier applies to damage types after blocking, armor, and resistances. It also applies to each tick of fire, spirit, and poison damage, using the creature's position and global keys at that time. Displayed damage numbers use the damage before this multiplier. Health loss and stagger use the modified damage. **Damage dealt modifier** still affects the displayed numbers. Direct health costs are not changed.

Use a finite value of **0** or more. Negative values and non-finite values are ignored. Changes apply to the next hit or damage tick. Install the mod on all clients and the server so the same setting applies when ownership of a creature changes.

### Admin exceptions

When **Ignore admins** is enabled, the host is exempt from player gating. Connected administrators are exempt after BiomeGate receives its initial configuration sync and the server confirms their admin status. This exemption is unavailable when BiomeGate is installed only on the client. **Ignore admins** does not affect the creature damage multiplier.

## Dependencies

- [BepInExPack Valheim 5.4.2350](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
- [ConditionalConfigSync 1.0.5](https://thunderstore.io/c/valheim/p/shudnal/ConditionalConfigSync/)

Install ConditionalConfigSync as a separate dependency; do not copy its DLLs into this mod's package.

## Donation
[Buy Me a Coffee](https://buymeacoffee.com/shudnal)

## Discord
[Join server](https://discord.gg/e3UtQB8GFK)
