using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(PermanentQuestRewards.Core), "PermanentQuestRewards", "1.6.1", "Lzc4")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace PermanentQuestRewards
{
    public static class PQR_SaveScope
    {
        private static string cachedSaveKey = "";
        private static float nextRefreshTime = 0f;

        public static bool IsGameLoaded()
        {
            try
            {
                return GameManager.GetInventoryComponent() != null;
            }
            catch
            {
                return false;
            }
        }

        public static string GetScopedKey(string baseKey)
        {
            string saveKey = GetCurrentSaveKey();

            if (string.IsNullOrEmpty(saveKey))
                return "";

            return baseKey + "_" + saveKey;
        }

        public static string GetCurrentSaveKey()
        {
            if (!IsGameLoaded())
                return "";

            if (Time.time < nextRefreshTime && !string.IsNullOrEmpty(cachedSaveKey))
                return cachedSaveKey;

            nextRefreshTime = Time.time + 5f;

            string key = TryFindLatestSaveFileKey();

            if (!string.IsNullOrEmpty(key))
            {
                cachedSaveKey = key;
                return cachedSaveKey;
            }

            key = TryFindLoadedWorldFallbackKey();

            if (!string.IsNullOrEmpty(key))
            {
                cachedSaveKey = key;
                return cachedSaveKey;
            }

            return "";
        }

        private static string TryFindLoadedWorldFallbackKey()
        {
            try
            {
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (!string.IsNullOrEmpty(scene))
                    return "Fallback_" + Sanitize(scene);
            }
            catch
            {
            }

            return "";
        }

        private static string TryFindLatestSaveFileKey()
        {
            try
            {
                List<string> roots = new List<string>();

                if (!string.IsNullOrEmpty(Application.persistentDataPath))
                    roots.Add(Application.persistentDataPath);

                string localLow = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    .Replace("Local", "LocalLow");

                if (!string.IsNullOrEmpty(localLow))
                    roots.Add(Path.Combine(localLow, "Hinterland", "TheLongDark"));

                string bestFile = "";
                DateTime bestTime = DateTime.MinValue;

                foreach (string root in roots)
                {
                    if (string.IsNullOrEmpty(root))
                        continue;

                    if (!Directory.Exists(root))
                        continue;

                    string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string lower = file.ToLower();

                        if (!lower.Contains("survival") &&
                            !lower.Contains("sandbox") &&
                            !lower.EndsWith(".sav") &&
                            !lower.EndsWith(".dat"))
                            continue;

                        FileInfo info = new FileInfo(file);

                        if (info.LastWriteTime > bestTime)
                        {
                            bestTime = info.LastWriteTime;
                            bestFile = file;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(bestFile))
                {
                    FileInfo info = new FileInfo(bestFile);
                    return "SaveFile_" + Sanitize(info.Name);
                }
            }
            catch
            {
            }

            return "";
        }

        private static string Sanitize(string value)
        {
            string result = "";

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                    result += c;
                else
                    result += "_";
            }

            return result;
        }
    }

    public static class PermanentFlags
    {
        private const string KEY_HOLSTER = "PQR_RifleHolster_Unlocked";
        private const string KEY_TOOLBELT = "PQR_ToolBelt_Unlocked";

        private static bool sessionRifleHolsterUnlocked = false;
        private static bool sessionToolBeltUnlocked = false;

        public static bool HasRifleHolster
        {
            get
            {
                if (sessionRifleHolsterUnlocked)
                    return true;

                string key = PQR_SaveScope.GetScopedKey(KEY_HOLSTER);

                if (!string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(key, 0) == 1)
                    return true;

                return false;
            }
        }

        public static bool HasToolBelt
        {
            get
            {
                if (sessionToolBeltUnlocked)
                    return true;

                string key = PQR_SaveScope.GetScopedKey(KEY_TOOLBELT);

                if (!string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(key, 0) == 1)
                    return true;

                return false;
            }
        }

        public static void UnlockRifleHolster()
        {
            sessionRifleHolsterUnlocked = true;

            string key = PQR_SaveScope.GetScopedKey(KEY_HOLSTER);

            if (!string.IsNullOrEmpty(key))
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }

            PQR_WeightLogic.ForceRefreshCache();
            PQR_Debug.DumpStatus("Rifle Holster freigeschaltet");
            MelonLogger.Msg("[PQR] Rifle Holster permanent aktiv.");
        }

        public static void UnlockToolBelt()
        {
            sessionToolBeltUnlocked = true;

            string key = PQR_SaveScope.GetScopedKey(KEY_TOOLBELT);

            if (!string.IsNullOrEmpty(key))
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }

            PQR_WeightLogic.ForceRefreshCache();
            PQR_Debug.DumpStatus("Tool Belt freigeschaltet");
            MelonLogger.Msg("[PQR] Tool Belt permanent aktiv.");
        }
    }

    public static class KnownWeights
    {
        public static readonly List<string> RIFLES_BY_WEIGHT = new List<string>
        {
            "GEAR_RifleAntique",
            "GEAR_Rifle_Barbs",
            "GEAR_RifleBarbs",
            "GEAR_RifleSimple",
            "GEAR_Rifle",
            "GEAR_RifleCurator",
            "GEAR_Rifle_Curator",
            "GEAR_Rifle_Vaughns",
            "GEAR_RifleVaughns",
            "GEAR_RifleBunker",
            "GEAR_Rifle_Bunker"
        };

        public static readonly HashSet<string> RIFLES = new HashSet<string>
        {
            "GEAR_RifleAntique",
            "GEAR_Rifle_Barbs",
            "GEAR_RifleBarbs",
            "GEAR_RifleSimple",
            "GEAR_Rifle",
            "GEAR_RifleCurator",
            "GEAR_Rifle_Curator",
            "GEAR_Rifle_Vaughns",
            "GEAR_RifleVaughns",
            "GEAR_RifleBunker",
            "GEAR_Rifle_Bunker"
        };

        public static readonly List<string> TOOLS_BY_WEIGHT = new List<string>
        {
            "GEAR_HeavyHammer",
            "GEAR_HatchetImprovised",
            "GEAR_Hatchet",
            "GEAR_Hacksaw",
            "GEAR_Prybar",
            "GEAR_SimpleTools",
            "GEAR_QualityTools",
            "GEAR_KnifeImprovised",
            "GEAR_KnifeSurvival",
            "GEAR_CougarClawKnife",
            "GEAR_KnifeHunting",
            "GEAR_Knife"
        };

        public static readonly HashSet<string> TOOLS = new HashSet<string>
        {
            "GEAR_HeavyHammer",
            "GEAR_HatchetImprovised",
            "GEAR_Hatchet",
            "GEAR_Hacksaw",
            "GEAR_Prybar",
            "GEAR_SimpleTools",
            "GEAR_QualityTools",
            "GEAR_KnifeImprovised",
            "GEAR_KnifeSurvival",
            "GEAR_CougarClawKnife",
            "GEAR_KnifeHunting",
            "GEAR_Knife"
        };
    }

    public class Core : MelonMod
    {
        public const string RIFLE_HOLSTER_NAME = "GEAR_RifleScabbardA";
        public const string TOOL_BELT_NAME = "GEAR_ToolBelt";

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("[PQR] PermanentQuestRewards v1.6.1 geladen");
            MelonLogger.Msg("[PQR] Debug wird beim Freischalten automatisch in die Melon-Konsole geschrieben.");

            PrintStartMessage();

            PQR_RuntimePatcher.PatchWeightMethods(HarmonyInstance);
        }

        private static void PrintStartMessage()
        {
            MelonLogger.Msg(" ");
            MelonLogger.Msg("============================================================");
            MelonLogger.Msg(" PermanentQuestRewards v1.6.1");
            MelonLogger.Msg("============================================================");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" Dear Hinterland Developers,");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" your complete lack of usable modding documentation");
            MelonLogger.Msg(" for IL2CPP internals, ItemWeight handling, inventory");
            MelonLogger.Msg(" refresh behavior and quest reward systems turned this");
            MelonLogger.Msg(" tiny feature into an absurd reverse-engineering disaster.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" A mechanic this small should never require this much");
            MelonLogger.Msg(" reflection abuse, runtime patching, inventory checking,");
            MelonLogger.Msg(" recursion protection and blind guessing.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" The fact that modders have to brute-force discover");
            MelonLogger.Msg(" how your weight system behaves at runtime is honestly");
            MelonLogger.Msg(" ridiculous.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" Whoever designed ItemWeight serialization without");
            MelonLogger.Msg(" proper exposure or documentation deserves immediate");
            MelonLogger.Msg(" observation by senior engineers.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" Sincerely,");
            MelonLogger.Msg(" exhausted IL2CPP modders.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg("------------------------------------------------------------");
            MelonLogger.Msg(" COMMUNITY NOTE");
            MelonLogger.Msg("------------------------------------------------------------");
            MelonLogger.Msg(" This mod was only created because RaverLP wanted");
            MelonLogger.Msg(" these mechanics to finally work properly.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" Lzc4 originally had no interest in making this mod");
            MelonLogger.Msg(" at all, but ended up building it anyway purely");
            MelonLogger.Msg(" for RaverLP.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" So if you enjoy this mod, thank RaverLP.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg(" Support your local Antifa.");
            MelonLogger.Msg(" ");
            MelonLogger.Msg("============================================================");
            MelonLogger.Msg(" ");
        }
    }

    public static class PQR_Util
    {
        public static string NormalizeGearName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            return name.Replace("(Clone)", "").Trim();
        }

        public static string GetRuntimeInstanceKey(GearItem gear)
        {
            if (gear == null)
                return "";

            try
            {
                if (gear.Pointer != IntPtr.Zero)
                    return "ptr:" + gear.Pointer.ToInt64();
            }
            catch
            {
            }

            try
            {
                return "hash:" + gear.GetHashCode();
            }
            catch
            {
            }

            return "";
        }
    }

    public static class PQR_WeightLogic
    {
        private static float nextPresenceCacheClearTime = 0f;
        private static float nextSelectionResetTime = 0f;

        private static readonly Dictionary<string, bool> inventoryPresenceCache = new Dictionary<string, bool>();

        private static readonly HashSet<string> selectedRifleInstanceKeys = new HashSet<string>();
        private static readonly HashSet<string> selectedToolInstanceKeys = new HashSet<string>();
        private static readonly Dictionary<string, int> selectedToolTypeCounts = new Dictionary<string, int>();

        public static void ForceRefreshCache()
        {
            nextPresenceCacheClearTime = 0f;
            nextSelectionResetTime = 0f;
            inventoryPresenceCache.Clear();
            selectedRifleInstanceKeys.Clear();
            selectedToolInstanceKeys.Clear();
            selectedToolTypeCounts.Clear();
        }

        public static bool ShouldReduce(GearItem gear)
        {
            if (gear == null)
                return false;

            if (!gear.m_InPlayerInventory)
                return false;

            string gearName = PQR_Util.NormalizeGearName(gear.name);

            if (PermanentFlags.HasRifleHolster && KnownWeights.RIFLES.Contains(gearName))
                return ShouldReduceRifle(gear, gearName);

            if (PermanentFlags.HasToolBelt && KnownWeights.TOOLS.Contains(gearName))
                return ShouldReduceTool(gear, gearName);

            return false;
        }

        private static void ResetSelectionWindowIfNeeded()
        {
            if (Time.time < nextSelectionResetTime)
                return;

            nextSelectionResetTime = Time.time + 0.25f;
            selectedRifleInstanceKeys.Clear();
            selectedToolInstanceKeys.Clear();
            selectedToolTypeCounts.Clear();
        }

        private static bool ShouldReduceRifle(GearItem gear, string gearName)
        {
            string targetType = GetCurrentHeaviestRifleName();

            if (string.IsNullOrEmpty(targetType))
                return false;

            if (targetType != gearName)
                return false;

            ResetSelectionWindowIfNeeded();

            string key = PQR_Util.GetRuntimeInstanceKey(gear);

            if (string.IsNullOrEmpty(key))
                return true;

            if (selectedRifleInstanceKeys.Contains(key))
                return true;

            if (selectedRifleInstanceKeys.Count >= 1)
                return false;

            selectedRifleInstanceKeys.Add(key);
            return true;
        }

        private static bool ShouldReduceTool(GearItem gear, string gearName)
        {
            int budgetForType = GetToolBudgetForType(gearName);

            if (budgetForType <= 0)
                return false;

            ResetSelectionWindowIfNeeded();

            string key = PQR_Util.GetRuntimeInstanceKey(gear);

            if (string.IsNullOrEmpty(key))
                return false;

            if (selectedToolInstanceKeys.Contains(key))
                return true;

            if (selectedToolInstanceKeys.Count >= 3)
                return false;

            int alreadyForType = 0;

            if (selectedToolTypeCounts.ContainsKey(gearName))
                alreadyForType = selectedToolTypeCounts[gearName];

            if (alreadyForType >= budgetForType)
                return false;

            selectedToolInstanceKeys.Add(key);
            selectedToolTypeCounts[gearName] = alreadyForType + 1;

            return true;
        }

        public static string GetCurrentHeaviestRifleName()
        {
            foreach (string rifleName in KnownWeights.RIFLES_BY_WEIGHT)
            {
                if (InventoryHasItemCached(rifleName))
                    return rifleName;
            }

            return "";
        }

        public static List<string> GetCurrentTopThreeToolNames()
        {
            List<string> result = new List<string>();
            int remainingSlots = 3;

            foreach (string toolName in KnownWeights.TOOLS_BY_WEIGHT)
            {
                if (!InventoryHasItemCached(toolName))
                    continue;

                for (int i = 0; i < remainingSlots; i++)
                    result.Add(toolName);

                remainingSlots = 0;
                break;
            }

            return result;
        }

        private static int GetToolBudgetForType(string gearName)
        {
            int remainingSlots = 3;

            foreach (string toolName in KnownWeights.TOOLS_BY_WEIGHT)
            {
                if (!InventoryHasItemCached(toolName))
                    continue;

                if (toolName == gearName)
                    return remainingSlots;

                remainingSlots--;

                if (remainingSlots <= 0)
                    return 0;
            }

            return 0;
        }

        public static bool InventoryHasItemCached(string gearName)
        {
            if (Time.time >= nextPresenceCacheClearTime)
            {
                nextPresenceCacheClearTime = Time.time + 0.25f;
                inventoryPresenceCache.Clear();
            }

            if (inventoryPresenceCache.ContainsKey(gearName))
                return inventoryPresenceCache[gearName];

            bool result = false;

            try
            {
                Inventory inventory = GameManager.GetInventoryComponent();

                if (inventory != null)
                    result = inventory.HasNonRuinedItem(gearName);
            }
            catch
            {
                result = false;
            }

            inventoryPresenceCache[gearName] = result;
            return result;
        }
    }

    public static class PQR_RuntimePatcher
    {
        [ThreadStatic]
        private static int weightCallDepth;

        public static void PatchWeightMethods(HarmonyLib.Harmony harmony)
        {
            try
            {
                MethodInfo prefix = typeof(PQR_RuntimePatcher).GetMethod(
                    nameof(ItemWeightPrefix),
                    BindingFlags.Public | BindingFlags.Static
                );

                MethodInfo postfix = typeof(PQR_RuntimePatcher).GetMethod(
                    nameof(ItemWeightPostfix),
                    BindingFlags.Public | BindingFlags.Static
                );

                int patched = 0;

                MethodInfo[] methods = typeof(GearItem).GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance
                );

                foreach (MethodInfo method in methods)
                {
                    if (method == null)
                        continue;

                    if (method.ReturnType == null)
                        continue;

                    if (method.ReturnType.Name != "ItemWeight")
                        continue;

                    if (method.Name != "GetItemWeightKG" &&
                        method.Name != "GetSingleItemWeightKG" &&
                        method.Name != "get_WeightKG")
                        continue;

                    try
                    {
                        harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                        patched++;
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Warning("[PQR] Patch fehlgeschlagen: " + method.Name + " | " + e.Message);
                    }
                }

                MelonLogger.Msg("[PQR] ItemWeight-Methoden gepatcht: " + patched);
            }
            catch (Exception e)
            {
                MelonLogger.Error("[PQR] PatchWeightMethods Fehler: " + e);
            }
        }

        public static void ItemWeightPrefix()
        {
            weightCallDepth++;
        }

        public static void ItemWeightPostfix(GearItem __instance, ref object __result)
        {
            try
            {
                if (__instance == null)
                    return;

                if (__result == null)
                    return;

                if (weightCallDepth != 1)
                    return;

                if (!PQR_WeightLogic.ShouldReduce(__instance))
                    return;

                object boxedWeight = __result;

                if (PQR_ItemWeightHelper.HalveBoxedItemWeight(boxedWeight))
                    __result = boxedWeight;
            }
            catch (Exception e)
            {
                MelonLogger.Error("[PQR] ItemWeightPostfix Fehler: " + e);
            }
            finally
            {
                weightCallDepth--;

                if (weightCallDepth < 0)
                    weightCallDepth = 0;
            }
        }
    }

    public static class PQR_ItemWeightHelper
    {
        public static bool HalveBoxedItemWeight(object boxed)
        {
            if (boxed == null)
                return false;

            if (TryMultiplyLongField(boxed, "m_Units", 0.5f))
                return true;

            if (TryMultiplyLongField(boxed, "m_Value", 0.5f))
                return true;

            if (TryMultiplyLongField(boxed, "m_RawValue", 0.5f))
                return true;

            if (TryMultiplyLongField(boxed, "RawValue", 0.5f))
                return true;

            return false;
        }

        private static bool TryMultiplyLongField(object boxed, string fieldName, float multiplier)
        {
            try
            {
                FieldInfo field = boxed.GetType().GetField(
                    fieldName,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance
                );

                if (field == null)
                    return false;

                if (field.FieldType != typeof(long))
                    return false;

                long oldValue = (long)field.GetValue(boxed);
                long newValue = (long)(oldValue * multiplier);

                field.SetValue(boxed, newValue);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static class PQR_Debug
    {
        public static void DumpStatus(string reason)
        {
            try
            {
                PQR_WeightLogic.ForceRefreshCache();

                MelonLogger.Msg("[PQR-DEBUG] ========================================");
                MelonLogger.Msg("[PQR-DEBUG] Grund: " + reason);
                MelonLogger.Msg("[PQR-DEBUG] Save geladen: " + PQR_SaveScope.IsGameLoaded());
                MelonLogger.Msg("[PQR-DEBUG] Save-Key: " + PQR_SaveScope.GetCurrentSaveKey());
                MelonLogger.Msg("[PQR-DEBUG] Rifle Holster aktiv: " + PermanentFlags.HasRifleHolster);
                MelonLogger.Msg("[PQR-DEBUG] Tool Belt aktiv: " + PermanentFlags.HasToolBelt);
                MelonLogger.Msg("[PQR-DEBUG] Schwerste erkannte Rifle: " + PQR_WeightLogic.GetCurrentHeaviestRifleName());

                List<string> tools = PQR_WeightLogic.GetCurrentTopThreeToolNames();

                string toolText = "";

                foreach (string tool in tools)
                {
                    if (toolText.Length > 0)
                        toolText += ", ";

                    toolText += tool;
                }

                MelonLogger.Msg("[PQR-DEBUG] Top-3 Tool-Slots: " + toolText);

                MelonLogger.Msg("[PQR-DEBUG] Rifle Presence:");
                foreach (string rifle in KnownWeights.RIFLES_BY_WEIGHT)
                {
                    if (PQR_WeightLogic.InventoryHasItemCached(rifle))
                        MelonLogger.Msg("[PQR-DEBUG]   " + rifle + " vorhanden");
                }

                MelonLogger.Msg("[PQR-DEBUG] Tool Presence:");
                foreach (string tool in KnownWeights.TOOLS_BY_WEIGHT)
                {
                    if (PQR_WeightLogic.InventoryHasItemCached(tool))
                        MelonLogger.Msg("[PQR-DEBUG]   " + tool + " vorhanden");
                }

                MelonLogger.Msg("[PQR-DEBUG] ========================================");
            }
            catch (Exception e)
            {
                MelonLogger.Error("[PQR-DEBUG] DumpStatus Fehler: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(GearItem), nameof(GearItem.PerformInteraction))]
    public static class Patch_GearItem_PerformInteraction
    {
        static void Postfix(GearItem __instance)
        {
            if (__instance == null)
                return;

            string itemName = PQR_Util.NormalizeGearName(__instance.name);

            if (itemName == Core.RIFLE_HOLSTER_NAME)
            {
                if (!PermanentFlags.HasRifleHolster)
                    PermanentFlags.UnlockRifleHolster();

                MelonCoroutines.Start(ConsumeItem(__instance));
            }
            else if (itemName == Core.TOOL_BELT_NAME)
            {
                if (!PermanentFlags.HasToolBelt)
                    PermanentFlags.UnlockToolBelt();

                MelonCoroutines.Start(ConsumeItem(__instance));
            }
            else
            {
                PQR_WeightLogic.ForceRefreshCache();
            }
        }

        private static IEnumerator ConsumeItem(GearItem item)
        {
            yield return null;
            yield return null;

            if (item == null)
                yield break;

            try
            {
                Inventory inventory = GameManager.GetInventoryComponent();

                if (inventory != null && item.gameObject != null)
                {
                    inventory.DestroyGear(item.gameObject);
                    MelonLogger.Msg("[PQR] Quest-Item verbraucht.");
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error("[PQR] Fehler beim Entfernen: " + e);
            }

            yield return null;

            PQR_WeightLogic.ForceRefreshCache();
            PQR_Debug.DumpStatus("Nach Quest-Item-Verbrauch");
        }
    }
}
