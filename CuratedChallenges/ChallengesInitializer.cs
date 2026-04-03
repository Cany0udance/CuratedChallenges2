using CuratedChallenges.Challenges;
using CuratedChallenges.Challenges.Defect;
using CuratedChallenges.Challenges.Regent;
using CuratedChallenges.Challenges.Silent;
using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.ChallengeUtil.Progression;
using CuratedChallenges.MainMenu;
using CuratedChallenges.Patches;
using CuratedChallenges.Patches.SpecificChallenges.Blindbot;
using CuratedChallenges.Patches.SpecificChallenges.BurnBright;
using CuratedChallenges.Patches.SpecificChallenges.CursedCombo;
using CuratedChallenges.Patches.SpecificChallenges.FastTrack;
using CuratedChallenges.Patches.SpecificChallenges.HardCarry;
using CuratedChallenges.Patches.SpecificChallenges.HotPotato;
using CuratedChallenges.Patches.SpecificChallenges.ShootingStar;
using CuratedChallenges.Patches.SpecificChallenges.SplitPersonality;
using CuratedChallenges.Patches.SpecificChallenges.TheRoost;
using CuratedChallenges.Patches.SpecificChallenges.UltimateForm;
using CuratedChallenges.Patches.SpecificChallenges.VakuuTakeTheWheel;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CuratedChallenges;

[ModInitializer("Initialize")]
public class ChallengesInitializer
{
    
    public static void Initialize()
    {
        var harmony = new Harmony("curatedchallenges.curatedchallenges");
        harmony.PatchAll(typeof(ChallengesInitializer).Assembly);
        
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(ChallengeModifier));
        
        // Register challenges
        try
        {
            RegisterChallenges();
            ChallengeRegistry.MarkInitialized();
            ChallengeRegistry.OnRegistryReady += () =>
            {
                ChallengesLocTablePatch.PopulateChallengeTranslations(LocManager.Instance);
            };
        }
        catch (Exception e)
        {
        }
    }
    
    private static void RegisterChallenges()
    {
        
        // Ironclad
        
        ChallengeRegistry.RegisterChallenge(new TheRoostChallenge());
        ChallengeRegistry.RegisterChallenge(new CursedComboChallenge());
        ChallengeRegistry.RegisterChallenge(new VeryCursedComboChallenge());
        
        // Silent
        
        ChallengeRegistry.RegisterChallenge(new HotPotatoChallenge());
        
        // Regent
        
        ChallengeRegistry.RegisterChallenge(new FastTrackChallenge());
        ChallengeRegistry.RegisterChallenge(new ShootingStarChallenge());
        
        // Necrobinder
        
        ChallengeRegistry.RegisterChallenge(new SneckrobinderChallenge());
        ChallengeRegistry.RegisterChallenge(new HardCarryChallenge());
        
        // Defect
        
        ChallengeRegistry.RegisterChallenge(new BlindbotChallenge());
        ChallengeRegistry.RegisterChallenge(new PowerSurgeChallenge());
        
        // Shared

        ChallengeRegistry.RegisterChallenge(new UltimateFormChallenge());
        ChallengeRegistry.RegisterChallenge(new BurnBrightChallenge());
        ChallengeRegistry.RegisterChallenge(new VakuuTakeTheWheelChallenge());
        ChallengeRegistry.RegisterChallenge(new VakuuTakeThePrismaticWheelChallenge());
        ChallengeRegistry.RegisterChallenge(new SplitPersonalityChallenge());
    }
}