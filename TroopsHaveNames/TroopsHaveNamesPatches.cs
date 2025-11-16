using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TroopsHaveNames
{
    [HarmonyPatch]
    public static class TroopNamePatches
    {
        private static Dictionary<string, List<string>> femNames = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> menNames = new Dictionary<string, List<string>>();
        private static Dictionary<CharacterObject, bool> characters = new Dictionary<CharacterObject, bool>();
        private static List<CharacterObject> nobles = new List<CharacterObject>();


        // cached reflection FieldInfo for Agent._name
        private static FieldInfo agentNameField = typeof(Agent).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance);


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignEvents), nameof(CampaignEvents.OnGameLoadFinished))]
        static void LoadFinished_Postfix()
        {
            nobles.Clear();
            characters.Clear();
            femNames.Clear();
            menNames.Clear();
            PopulateNames();
            PopulateCharacters();
            PopulateNobles();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignEvents), nameof(CampaignEvents.OnNewGameCreated))]
        static void OnNewGameCreated_Postfix()
        {
            nobles.Clear();
            characters.Clear();
            femNames.Clear();
            menNames.Clear();
            PopulateNames();
            PopulateCharacters();
            PopulateNobles();
        }



        // Hook mission spawn (Postfix). This overload is the usual one used in vanilla.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        public static void SpawnAgent_Postfix(Agent __result, AgentBuildData agentBuildData, Mission __instance)
        {
            try
            {
                // safety
                if (__instance == null) return;
                if (__result == null) return;
                var character = __result.Character;
                if (character == null) return;

                // don't rename heroes
                if (character.IsHero) return;

                if (TroopsHaveNamesSettings.Instance.HideEnemyNames && __result.Team != null && !__result.Team.IsPlayerAlly)
                {
                    if (agentNameField != null)
                    {
                        agentNameField.SetValue(__result, new TextObject("", null));
                    }
                    return;
                }
                
                if(TroopsHaveNamesSettings.Instance.Tier > character.GetBattleTier()) return;

                if (!TroopsHaveNamesSettings.Instance.Affect && characters.TryGetValue((CharacterObject)character, out var IsTroop))
                {
                    if (TroopsHaveNamesSettings.Instance.Debug) InformationManager.DisplayMessage(new InformationMessage($"Name '{character.Name}', IsTroop: {IsTroop}"));
                    if (!IsTroop) return;
                }

                if(TroopsHaveNamesSettings.Instance.OnlyNoble && 
                    (!character.IsSoldier || !nobles.Contains(character)))
                {
                    return;
                }

                // OnlyBattle check: only run in battle missions if requested
                if (TroopsHaveNamesSettings.Instance.OnlyBattle)
                {
                    if (!__instance.IsFieldBattle && !__instance.IsSallyOutBattle && !__instance.IsSiegeBattle) return;
                } 
                
                if (TroopsHaveNamesSettings.Instance.OnlyNonBattle)
                {
                    if (__instance.IsFieldBattle || __instance.IsSallyOutBattle || __instance.IsSiegeBattle) return;
                }

                // OnlyAlly check
                if (TroopsHaveNamesSettings.Instance.OnlyAlly &&
                    (__instance.IsFieldBattle || __instance.IsSallyOutBattle || __instance.IsSiegeBattle))
                {
                    if (__result.Team == null || !__result.Team.IsPlayerAlly) return;

                }

                // Multiplayer: only server/recorder should assign authoritative names
                if (GameNetwork.IsMultiplayer && !GameNetwork.IsServerOrRecorder) return;

                // Ensure name pools are ready
                if ((femNames.Count == 0 && menNames.Count == 0) || characters.Count == 0)
                {
                    // try to populate (defensive)
                    PopulateNames();
                    PopulateCharacters();
                    PopulateNobles();
                }


                // Choose correct pool by culture + gender

                var nameDict = __result.IsFemale ? femNames : menNames;
                if (nameDict == null || nameDict.Count == 0) return;

                // get culture key
                var cultureKey = !string.IsNullOrEmpty(character.Culture?.StringId)
                                 ? character.Culture.StringId
                                 : character.Culture?.Name?.ToString();

                List<string> pool = null;
                if (!string.IsNullOrEmpty(cultureKey) && nameDict.TryGetValue(cultureKey, out var cultList) && cultList != null && cultList.Count > 0)
                {
                    pool = cultList;
                }
                else if (TroopsHaveNamesSettings.Instance.WithoutNames && nameDict.TryGetValue("All", out var allList) && allList != null && allList.Count > 0)
                {
                    pool = allList;
                }
                else
                {
                    // nothing to pick from
                    return;
                }


                // pick a name not in used if possible
                string pick = pool[MBRandom.RandomInt(pool.Count)];

                // append id if requested (shows template in brackets)
                string originalTemplate = character.Name?.ToString() ?? "";
                string finalName = pick;

                    bool IsSurname = false;

                    if (TroopsHaveNamesSettings.Instance.Surnames && pool.Count > 1 && TroopsHaveNamesSettings.Instance.SurnamesTier <= character.GetBattleTier())
                    {
                        IsSurname = true;
                        if (TroopsHaveNamesSettings.Instance.SurnamesNoble &&
                            (!character.IsSoldier || !nobles.Contains(character)))
                        {
                            IsSurname = false;
                        }
                    }


                    if (IsSurname)
                    {
                        var newPool = pool.Where(p => p != pick).ToList();
                        var surname = newPool[MBRandom.RandomInt(newPool.Count)];
                        finalName += " " + surname;

                    }
                    if (TroopsHaveNamesSettings.Instance.IdName && !string.IsNullOrEmpty(originalTemplate))
                        finalName += " [" + originalTemplate + "]";

                

                // set into agent private _name field so Agent.Name returns our TextObject
                if (agentNameField != null)
                {
                    agentNameField.SetValue(__result, new TextObject(finalName, null));
                }

                // optional debug message
                if (TroopsHaveNamesSettings.Instance.Debug)
                {
                    InformationManager.DisplayMessage(new InformationMessage($"Assigned name '{finalName}' to agent #{__result.Index} ({originalTemplate})"));
                }
            }
            catch (Exception ex)
            {
                if (TroopsHaveNamesSettings.Instance.Debug)
                    InformationManager.DisplayMessage(new InformationMessage($"SpawnAgent_Postfix error: {ex.GetType().Name}: {ex.Message}"));
            }
        }

            static void PopulateNames()
        {
            femNames.Clear();
            menNames.Clear();
            if (TroopsHaveNamesSettings.Instance.Debug) InformationManager.DisplayMessage(new InformationMessage($"Populate Names Fired!"));
            var cultures = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
            if (cultures == null) return;

            foreach(var culture in cultures)
            {
                
                var women = NameGenerator.Current.GetNameListForCulture(culture, true).Select(n=> n.ToString()).ToList();
                var men = NameGenerator.Current.GetNameListForCulture(culture, false).Select(n => n.ToString()).ToList();

                if (TroopsHaveNamesSettings.Instance.Debug) InformationManager.DisplayMessage(new InformationMessage($"Are there women: {women.Count}, Are there men: {men.Count}"));

                if ((!femNames.TryGetValue(culture.StringId, out var female) || female == null || female.Count == 0)
                    && women.Count > 0)
                {
                    femNames.Add(culture.StringId, women);
                }
                if ((!menNames.TryGetValue(culture.StringId, out var male) || male == null || male.Count == 0)
                    && men.Count > 0)
                {
                    menNames.Add(culture.StringId, men);
                }
            }

            // build "All" fallback lists (distinct, and only if there is anything)
            var allFem = femNames.Values.SelectMany(x => x).Distinct().ToList();
            if (allFem.Count > 0) femNames["All"] = allFem;

            var allMale = menNames.Values.SelectMany(x => x).Distinct().ToList();
            if (allMale.Count > 0) menNames["All"] = allMale;

            if (TroopsHaveNamesSettings.Instance.Debug) InformationManager.DisplayMessage(new InformationMessage($"Amount of menNames: {menNames.Count}, Amount of femNames: {femNames.Count}"));
        }

        static void PopulateCharacters()
        {
            characters.Clear();
            if (TroopsHaveNamesSettings.Instance.Debug) InformationManager.DisplayMessage(new InformationMessage($"Populate Chars Fired!"));
            var characterList = CharacterObject.All.Where(c=> !c.IsHero).ToList();
            if (characterList == null) return;

            foreach (var character in characterList)
            {
                if(!characters.TryGetValue(character, out var isTroop))
                {
                    if (!character.IsSoldier && character.Occupation != Occupation.Mercenary && character.Occupation != Occupation.CaravanGuard && character.Occupation != Occupation.Guard && character.Occupation != Occupation.Bandit)
                    {
                        isTroop = false;
                    }
                    else
                    {
                        isTroop = true;
                    }
                    
                    characters.Add(character, isTroop);
                }
            }

            if (TroopsHaveNamesSettings.Instance.Debug) InformationManager.DisplayMessage(new InformationMessage($"Amount of chars: {characters.Count}"));
        }

        static void PopulateNobles()
        {
            nobles.Clear();
            if (TroopsHaveNamesSettings.Instance.Debug)
                InformationManager.DisplayMessage(new InformationMessage("Populate Nobles Fired!"));

            var cultures = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
            if (cultures == null || cultures.Count == 0) return;

            // Use a hashset to avoid duplicates (fast lookup)
            var visited = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase); // use StringId for stable identity

            foreach (var culture in cultures)
            {
                if (culture == null) continue;

                // get the elite basic troop for this culture (may be null)
                var root = culture.EliteBasicTroop;
                if (root == null) continue;

                // BFS queue for upgrade chain
                var queue = new Queue<CharacterObject>();
                queue.Enqueue(root);

                while (queue.Count > 0)
                {
                    var ch = queue.Dequeue();
                    if (ch == null) continue;

                    // Skip heroes, template-only and child templates if you don't want them
                    if (ch.IsHero) continue;


                    // use StringId if available; fall back to StringId-like or Name
                    var id = !string.IsNullOrEmpty(ch.StringId) ? ch.StringId : ch.Name?.ToString();
                    if (string.IsNullOrEmpty(id)) continue;

                    // if not yet added, add to nobles and mark visited
                    if (!visited.Contains(id))
                    {
                        visited.Add(id);
                        nobles.Add(ch);
                    }

                    // enqueue upgrade targets (if any)
                    var upgrades = ch.UpgradeTargets;
                    if (upgrades != null)
                    {
                        foreach (var up in upgrades)
                        {
                            if (up == null) continue;
                            var upId = !string.IsNullOrEmpty(up.StringId) ? up.StringId : up.Name?.ToString();
                            if (string.IsNullOrEmpty(upId)) continue;
                            if (!visited.Contains(upId))
                                queue.Enqueue(up);
                        }
                    }
                }
            }

            if (TroopsHaveNamesSettings.Instance.Debug)
                InformationManager.DisplayMessage(new InformationMessage($"Amount of nobles: {nobles.Count}"));
        }

    }
}
