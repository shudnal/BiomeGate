using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ConditionalConfigSync;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using static HitData;

namespace BiomeGate
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    [BepInDependency("_shudnal.ConditionalConfigSync", "1.0.5")]
    public class BiomeGate : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.BiomeGate";
        public const string pluginName = "Biome Gate";
        public const string pluginVersion = "1.0.7";

        private readonly Harmony harmony = new Harmony(pluginID);

        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };

        public static BiomeGate instance;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> loggingEnabled;

        public static ConfigEntry<Heightmap.Biome> gatedBiomes;
        public static ConfigEntry<bool> adminPermitted;

        public static ConfigEntry<DamageModifier> damageReceivedModifier;
        public static ConfigEntry<float> damageDoneModifier;
        public static ConfigEntry<float> noiseModifier;
        public static ConfigEntry<float> sneakModifier;
        public static ConfigEntry<float> raiseSkillModifier;
        public static ConfigEntry<bool> showRepeatMessage;
        public static ConfigEntry<bool> showStartMessage;
        public static ConfigEntry<bool> preventBuilding;
        public static ConfigEntry<bool> preventExploring;
        public static ConfigEntry<bool> preventInteraction;
        public static ConfigEntry<bool> showStatusEffectIcon;

        public static ConfigEntry<bool> globalKeyPermitted;
        public static ConfigEntry<string> globalKeyAvailability;

        public static ConfigEntry<bool> playerKeyPermitted;
        public static ConfigEntry<string> playerKeyAvailability;

        public static ConfigEntry<bool> playerItemPermitted;
        public static ConfigEntry<string> playerItemAvailability;

        private static readonly Dictionary<string, string> biomeGlobalKeys = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> biomePlayerKeys = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> biomePlayerItems = new Dictionary<string, string>();

        private void Awake()
        {
            harmony.PatchAll();

            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);

            Game.isModded = true;
        }

        private void OnDestroy()
        {
            Config.Save();
            harmony?.UnpatchSelf();
        }

        public static void LogInfo(object data)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogInfo(data);
        }

        private void ConfigInit()
        {

            modEnabled = config("General", "Enabled", defaultValue: true, "Enable the mod.");
            configLocked = config("General", "Lock Configuration", defaultValue: true, "Configuration is locked and can be changed by server admins only.");
            loggingEnabled = config("General", "Logging enabled", defaultValue: false, "Enable logging. [Not Synced with Server]", false);

            gatedBiomes = config("Gating", "Biomes", defaultValue: Heightmap.Biome.None, "Biome list.");
            adminPermitted = config("Gating", "Ignore admins", defaultValue: true, "Admins and host will not be affected");
            preventBuilding = config("Gating", "Prevent building", defaultValue: true, "Prevent usage of hammers, hoe, cultivator");
            preventExploring = config("Gating", "Prevent exploring", defaultValue: true, "Prevent minimap from exploring");
            preventInteraction = config("Gating", "Prevent interactions", defaultValue: true, "Prevent interaction with any interactable object");
            
            damageReceivedModifier = config("Gating", "Damage received modifier", defaultValue: DamageModifier.VeryWeak, "Damage modifier for incoming damage");
            damageDoneModifier = config("Gating", "Damage dealt modifier", defaultValue: 0f, "Damage modifier for damage done by affected players");
            noiseModifier = config("Gating", "Noise modifier", defaultValue: 10f, "Noise modifier. How far away player will be heared.");
            sneakModifier = config("Gating", "Sneak modifier", defaultValue: -1f, "Sneak efficacy modifier");
            raiseSkillModifier = config("Gating", "Raise skill modifier", defaultValue: -100f, "Modifier for skill raise. 0 is normal skill raise. Other status effects can also modify skill raise.");
            showRepeatMessage = config("Gating", "Show repeat message", defaultValue: true, "Show repeated message every 20 seconds");
            showStartMessage = config("Gating", "Show start message", defaultValue: true, "Show message when debuff is applied");
            showStatusEffectIcon = config("Gating", "Show status effect icon", defaultValue: true, "Show status effect in list when debuff is applied.");

            damageReceivedModifier.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            damageDoneModifier.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            noiseModifier.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            sneakModifier.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            raiseSkillModifier.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            showRepeatMessage.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            showStartMessage.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();
            showStatusEffectIcon.SettingChanged += (sender, args) => SE_BiomeGate.UpdateBiomeGateProperties();

            globalKeyPermitted = config("Gating - Global keys", "Disable gating if global key is set", defaultValue: false, "If enabled - corresponding global key should be set to disable biome gating");
            globalKeyAvailability = config("Gating - Global keys", "Biome global keys", defaultValue: "Swamp:defeated_gdking,Mountain:defeated_bonemass,Plains:defeated_dragon,Mistlands:defeated_goblinking,AshLands:defeated_queen,DeepNorth:defeated_fader", "Comma-separated list of biome-globalkey pairs. If globalkey is set biome will not be gated.");

            globalKeyAvailability.SettingChanged += (sender, args) => UpdateGlobalKeysList();

            UpdateGlobalKeysList();

            playerKeyPermitted = config("Gating - Player keys", "Disable gating if player key is set", defaultValue: false, "If enabled - corresponding player key should be set to disable biome gating");
            playerKeyAvailability = config("Gating - Player keys", "Biome player keys", defaultValue: "Swamp:GP_TheElder,Mountain:GP_Bonemass,Plains:GP_Moder,Mistlands:GP_Yagluth,AshLands:GP_Queen,DeepNorth:GP_Fader", "Comma-separated list of biome-playerkey pairs. If playerkey is set biome will not be gated.");

            playerKeyAvailability.SettingChanged += (sender, args) => UpdatePlayerKeysList();

            UpdatePlayerKeysList();

            playerItemPermitted = config("Gating - Player items", "Disable gating if player has item known", defaultValue: false, "If enabled - corresponding item should be known by player to disable biome gating");
            playerItemAvailability = config("Gating - Player items", "Biome player items", defaultValue: "Swamp:$item_cryptkey,Mountain:$item_wishbone,Plains:$item_dragontear,Mistlands:$item_yagluththing,AshLands:$item_seekerqueen_drop,DeepNorth:$item_fader_drop", "Comma-separated list of biome-item pairs. If item is known by player biome will not be gated.");
            
            playerItemAvailability.SettingChanged += (sender, args) => UpdatePlayerItemsList();

            UpdatePlayerItemsList();
        }

        private static void UpdateGlobalKeysList()
        {
            biomeGlobalKeys.Clear();

            foreach (string biomeKey in globalKeyAvailability.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pair = biomeKey.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2)
                    continue;

                biomeGlobalKeys[pair[0].Trim()] = pair[1].Trim();
            };
        }

        private static void UpdatePlayerKeysList()
        {
            biomePlayerKeys.Clear();

            foreach (string biomeKey in playerKeyAvailability.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pair = biomeKey.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2)
                    continue;

                biomePlayerKeys[pair[0].Trim()] = pair[1].Trim();
            };
        }

        private static void UpdatePlayerItemsList()
        {
            biomePlayerItems.Clear();

            foreach (string biomeKey in playerItemAvailability.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] pair = biomeKey.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length != 2)
                    continue;

                biomePlayerItems[pair[0].Trim()] = pair[1].Trim();
            };
        }

        private static bool IsAdmin()
        {
            if (!adminPermitted.Value)
                return false;

            if (!ZNet.instance)
                return true;

            return ZNet.instance.LocalPlayerIsAdminOrHost();
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, description);

            configSync.AddConfigEntry(configEntry, ConfigSyncMode.Conditional, serverControlledByDefault: synchronizedSetting);

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);

        public void FixedUpdate()
        {
            if (Player.m_localPlayer is Player player && player.GetSEMan() is SEMan seman && player.IsOwner())
            {
                if (seman.HaveStatusEffect(SE_BiomeGate.statusEffectBiomeGateHash))
                {
                    if (RemoveStatusEffect(player))
                        seman.RemoveStatusEffect(SE_BiomeGate.statusEffectBiomeGateHash);
                }
                else if (modEnabled.Value)
                {
                    if (AddStatusEffect(player))
                        seman.AddStatusEffect(SE_BiomeGate.statusEffectBiomeGateHash);
                }
            }
        }

        public static bool RemoveStatusEffect(Player player)
        {
            return !modEnabled.Value ||
                   gatedBiomes.Value == Heightmap.Biome.None || player.m_currentBiome == Heightmap.Biome.None ||
                   !gatedBiomes.Value.HasFlag(player.m_currentBiome) ||
                   IsAdmin() ||
                   IsGatingDisabledByProgression(player);
        }

        public static bool AddStatusEffect(Player player)
        {
            return gatedBiomes.Value != Heightmap.Biome.None && 
                   player.m_currentBiome != Heightmap.Biome.None && 
                   gatedBiomes.Value.HasFlag(player.m_currentBiome) &&
                   !player.InCutscene() &&
                   !player.IsTeleporting() &&
                   !player.m_isLoading &&
                   !player.IsDead() && 
                   !IsAdmin() &&
                   !IsGatingDisabledByProgression(player);
        }

        public static bool IsGatingDisabledByProgression(Player player) => IsBiomeGlobalKeyEnabled(player) || IsBiomePlayerKeyEnabled(player) || IsBiomePlayerItemEnabled(player);

        private static bool IsBiomeGlobalKeyEnabled(Player player) => globalKeyPermitted.Value && biomeGlobalKeys.TryGetValue(player.m_currentBiome.ToString(), out string globalKey) && ZoneSystem.instance?.GetGlobalKey(globalKey) == true;
        
        private static bool IsBiomePlayerKeyEnabled(Player player) => playerKeyPermitted.Value && biomePlayerKeys.TryGetValue(player.m_currentBiome.ToString(), out string playerKey) && player.HaveUniqueKey(playerKey);

        private static bool IsBiomePlayerItemEnabled(Player player) => playerItemPermitted.Value && biomePlayerItems.TryGetValue(player.m_currentBiome.ToString(), out string playerItem) && player.IsKnownMaterial(playerItem);

        private static bool IsGated(Humanoid human) => human is Player player && player.GetSEMan()?.HaveStatusEffect(SE_BiomeGate.statusEffectBiomeGateHash) == true && Ship.GetLocalShip() == null;

        [HarmonyPatch]
        public static class PreventInteractions
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(Player).Assembly.GetTypes()
                    .Where(p => typeof(Interactable).IsAssignableFrom(p))
                    .Where(p => p.Name != "Interactable")
                    .Where(p => p.Name != "Ladder")
                    .Where(p => p.Name != "ShipControlls")
                    .Where(p => p.Name != "Sadle")
                    .Where(p => p.Name != "Chair")
                    .Where(p => p.Name != "TombStone")
                    .SelectMany(t => new List<MethodBase>() { AccessTools.Method(t, "Interact"), AccessTools.Method(t, "UseItem") });
            }

            private static bool Prefix(Humanoid __0)
            {
                if (!preventInteraction.Value)
                    return true;

                if (__0 != null && IsGated(__0))
                {
                    __0.Message(MessageHud.MessageType.Center, "$msg_nobuildzone");
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetPlaceMode))]
        public static class Player_SetPlaceMode_PreventBuilding
        {
            private static void Prefix(Player __instance, ref PieceTable buildPieces, ref PieceTable __state)
            {
                if (!preventBuilding.Value)
                    return;

                if (!IsGated(__instance))
                    return;

                __state = buildPieces;
                buildPieces = null;
            }

            private static void Postfix(ref PieceTable buildPieces, ref PieceTable __state)
            {
                if (__state == null)
                    return;

                buildPieces = __state;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.InPlaceMode))]
        public static class Player_InPlaceMode_PreventBuilding
        {
            private static bool Prefix(Player __instance, ref bool __result)
            {
                if (!preventBuilding.Value)
                    return true;

                if (!IsGated(__instance))
                    return true;

                __result = false;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateExplore))]
        public static class Minimap_UpdateExplore_PreventExplore
        {
            private static void Prefix(Minimap __instance, Player player)
            {
                if (!preventExploring.Value)
                    return;

                if (!IsGated(player))
                    return;

                __instance.m_exploreTimer = 0;
            }
        }
    }
}
