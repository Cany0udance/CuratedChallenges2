using System.Reflection;
using CuratedChallenges.ChallengeUtil;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace CuratedChallenges.Patches;

[HarmonyPatch(typeof(AncientEventModel), "GenerateInitialOptionsWrapper")]
public static class AncientEventDuplicateRelicPatch
{
    private static readonly Dictionary<Type, AncientPoolConfig> PoolConfigs = new()
    {
        {
            typeof(Tezcatara), new AncientPoolConfig(
                poolPropertyNames: new[] { "OptionPool1", "OptionPool2", "OptionPool3" }
            )
        },
        {
            typeof(Vakuu), new AncientPoolConfig(
                poolPropertyNames: new[] { "Pool1", "Pool2", "Pool3" }
            )
        },
        {
            typeof(Orobas), new AncientPoolConfig(
                poolPropertyNames: new[] { "OptionPool1", "OptionPool2", "OptionPool3" },
                extraPoolMembers: new Dictionary<int, string[]>
                {
                    { 0, new[] { "PrismaticGemOption", "DiscoveryTotems" } }
                }
            )
        },
        {
            typeof(Pael), new AncientPoolConfig(
                poolPropertyNames: new[] { "OptionPool1", "OptionPool2", "OptionPool3" },
                extraPoolMembers: new Dictionary<int, string[]>
                {
                    { 1, new[] { "PaelsClawOption", "PaelsToothOption", "PaelsGrowthOption" } },
                    { 2, new[] { "PaelsLegionOption" } }
                }
            )
        }
    };

    static void Postfix(AncientEventModel __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (!RunManager.Instance.IsInProgress) return;

        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null || !ChallengeRunTracker.IsChallengeRun(runState)) return;

        var challengeMod = runState.Modifiers.OfType<ChallengeModifier>().FirstOrDefault();
        if (challengeMod == null) return;

        var player = __instance.Owner;
        var ownedRelicIds = new HashSet<ModelId>(player.Relics.Select(r => r.Id));
        var blacklistedIds = new HashSet<ModelId>(challengeMod.Challenge.GetBlacklistedRelics());

        var hasProblematicRelics = __result.Any(option =>
            option.Relic != null &&
            (ownedRelicIds.Contains(option.Relic.Id) || blacklistedIds.Contains(option.Relic.Id)));

        if (!hasProblematicRelics) return;

        int targetOptionCount = __result.Count;
        var excludedIds = new HashSet<ModelId>(ownedRelicIds);
        excludedIds.UnionWith(blacklistedIds);

        var pools = DiscoverPools(__instance, excludedIds, player);
        List<EventOption> newOptions;

        if (pools != null && pools.Count > 0)
        {
            newOptions = ReplaceWithPoolAwareness(__result, pools, excludedIds, player);
        }
        else
        {
            newOptions = ReplaceFlat(__instance, __result, excludedIds, player);
        }

        if (newOptions.Count < targetOptionCount)
        {
            var usedRelicIds = new HashSet<ModelId>(
                newOptions.Where(o => o.Relic != null).Select(o => o.Relic.Id));
            var usedTextKeys = new HashSet<string>(newOptions.Select(o => o.TextKey));

            var backfillCandidates = __instance.AllPossibleOptions
                .Where(o => !usedTextKeys.Contains(o.TextKey))
                .Where(o => o.Relic == null ||
                    (!excludedIds.Contains(o.Relic.Id) && !usedRelicIds.Contains(o.Relic.Id)))
                .Where(o => IsOptionValidForPlayer(o, player));

            foreach (var candidate in backfillCandidates)
            {
                if (newOptions.Count >= targetOptionCount) break;
                newOptions.Add(candidate);
                usedTextKeys.Add(candidate.TextKey);
                if (candidate.Relic != null)
                    usedRelicIds.Add(candidate.Relic.Id);
            }
        }

        if (newOptions.Count > 0)
        {
            __result = newOptions;
        }
    }

    private static List<List<EventOption>> DiscoverPools(
        AncientEventModel instance,
        HashSet<ModelId> excludedRelicIds,
        Player player)
    {
        var ancientType = instance.GetType();
        if (!PoolConfigs.TryGetValue(ancientType, out var config))
            return null;

        var pools = new List<List<EventOption>>();
        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        foreach (var poolName in config.PoolPropertyNames)
        {
            var prop = ancientType.GetProperty(poolName, bindingFlags);
            if (prop == null) return null;

            var poolOptions = GetEventOptionsFromProperty(prop, instance);
            if (poolOptions == null) return null;

            pools.Add(poolOptions);
        }

        if (config.ExtraPoolMembers != null)
        {
            foreach (var kvp in config.ExtraPoolMembers)
            {
                int poolIndex = kvp.Key;
                if (poolIndex < 0 || poolIndex >= pools.Count) continue;

                foreach (var extraPropName in kvp.Value)
                {
                    var prop = ancientType.GetProperty(extraPropName, bindingFlags);
                    if (prop == null) continue;

                    var extraOptions = GetEventOptionsFromProperty(prop, instance);
                    if (extraOptions != null)
                        pools[poolIndex].AddRange(extraOptions);
                }
            }
        }

        for (int i = 0; i < pools.Count; i++)
        {
            pools[i] = pools[i]
                .Where(o => o.Relic == null || !excludedRelicIds.Contains(o.Relic.Id))
                .Where(o => IsOptionValidForPlayer(o, player))
                .ToList();
        }

        return pools;
    }

    private static List<EventOption> GetEventOptionsFromProperty(
        PropertyInfo prop, AncientEventModel instance)
    {
        object value;
        try { value = prop.GetValue(instance); }
        catch { return null; }

        if (value == null) return null;
        if (value is EventOption singleOption)
            return new List<EventOption> { singleOption };
        if (value is IEnumerable<EventOption> options)
            return options.ToList();
        return null;
    }

    private static List<EventOption> ReplaceWithPoolAwareness(
        IReadOnlyList<EventOption> originalOptions,
        List<List<EventOption>> pools,
        HashSet<ModelId> excludedRelicIds,
        Player player)
    {
        var newOptions = new List<EventOption>();
        var offeredRelicIds = new HashSet<ModelId>();
        var usedTextKeys = new HashSet<string>();

        for (int i = 0; i < originalOptions.Count; i++)
        {
            var option = originalOptions[i];
            if (usedTextKeys.Contains(option.TextKey)) continue;

            bool isProblematic = option.Relic != null && excludedRelicIds.Contains(option.Relic.Id);

            if (!isProblematic)
            {
                newOptions.Add(option);
                usedTextKeys.Add(option.TextKey);
                if (option.Relic != null)
                    offeredRelicIds.Add(option.Relic.Id);
                continue;
            }

            EventOption replacement = null;

            if (i < pools.Count)
            {
                replacement = pools[i].FirstOrDefault(o =>
                    !usedTextKeys.Contains(o.TextKey) &&
                    (o.Relic == null || !offeredRelicIds.Contains(o.Relic.Id)));
            }

            if (replacement == null)
            {
                for (int p = 0; p < pools.Count; p++)
                {
                    replacement = pools[p].FirstOrDefault(o =>
                        !usedTextKeys.Contains(o.TextKey) &&
                        (o.Relic == null || !offeredRelicIds.Contains(o.Relic.Id)));
                    if (replacement != null) break;
                }
            }

            if (replacement != null)
            {
                newOptions.Add(replacement);
                usedTextKeys.Add(replacement.TextKey);
                if (replacement.Relic != null)
                    offeredRelicIds.Add(replacement.Relic.Id);
            }
        }

        return newOptions;
    }

    private static List<EventOption> ReplaceFlat(
        AncientEventModel instance,
        IReadOnlyList<EventOption> originalOptions,
        HashSet<ModelId> excludedRelicIds,
        Player player)
    {
        var availableOptions = instance.AllPossibleOptions
            .Where(o => o.Relic == null || !excludedRelicIds.Contains(o.Relic.Id))
            .Where(o => IsOptionValidForPlayer(o, player))
            .ToList();

        var newOptions = new List<EventOption>();
        var offeredRelicIds = new HashSet<ModelId>();
        var usedTextKeys = new HashSet<string>();

        foreach (var option in originalOptions)
        {
            if (usedTextKeys.Contains(option.TextKey)) continue;

            if (option.Relic != null && excludedRelicIds.Contains(option.Relic.Id))
            {
                var replacement = availableOptions.FirstOrDefault(o =>
                    !usedTextKeys.Contains(o.TextKey) &&
                    (o.Relic == null || !offeredRelicIds.Contains(o.Relic.Id)));

                if (replacement != null)
                {
                    newOptions.Add(replacement);
                    usedTextKeys.Add(replacement.TextKey);
                    if (replacement.Relic != null)
                        offeredRelicIds.Add(replacement.Relic.Id);
                }
            }
            else
            {
                newOptions.Add(option);
                usedTextKeys.Add(option.TextKey);
                if (option.Relic != null)
                    offeredRelicIds.Add(option.Relic.Id);
            }
        }

        return newOptions;
    }

    private static bool IsOptionValidForPlayer(EventOption option, Player player)
    {
        if (option.Relic == null) return true;

        var relicType = option.Relic.GetType();

        if (relicType == typeof(PaelsClaw))
            return player.Deck.Cards.Count(c => ModelDb.Enchantment<Goopy>().CanEnchant(c)) >= 3;
        if (relicType == typeof(PaelsTooth))
            return player.Deck.Cards.Count(c => c.IsRemovable) >= 5;
        if (relicType == typeof(PaelsLegion))
            return !player.HasEventPet();
        if (relicType == typeof(TouchOfOrobas))
            return player.Relics.Any(r => r.Rarity == RelicRarity.Starter);
        if (relicType == typeof(ArchaicTooth))
        {
            var tooth = (ArchaicTooth)ModelDb.Relic<ArchaicTooth>().ToMutable();
            return tooth.SetupForPlayer(player);
        }
        if (relicType == typeof(PandorasBox))
            return !player.RunState.Modifiers.Any(m => m.ClearsPlayerDeck);
        if (relicType == typeof(Ectoplasm) || relicType == typeof(Sozu))
            return player.RunState.CurrentActIndex == 1;
        if (relicType == typeof(PhilosophersStone) || relicType == typeof(VelvetChoker))
            return player.RunState.CurrentActIndex == 2;
        if (relicType == typeof(DustyTome))
            return true;
        if (relicType == typeof(BeautifulBracelet))
            return player.Deck.Cards.Count(c => ModelDb.Enchantment<Swift>().CanEnchant(c)) >= 4;
        if (relicType == typeof(TriBoomerang))
            return player.Deck.Cards.Count(c => ModelDb.Enchantment<Instinct>().CanEnchant(c)) >= 3;

        return true;
    }

    private class AncientPoolConfig
    {
        public string[] PoolPropertyNames { get; }
        public Dictionary<int, string[]> ExtraPoolMembers { get; }

        public AncientPoolConfig(string[] poolPropertyNames, Dictionary<int, string[]> extraPoolMembers = null)
        {
            PoolPropertyNames = poolPropertyNames;
            ExtraPoolMembers = extraPoolMembers;
        }
    }
}