using CuratedChallenges.Challenges;
using CuratedChallenges.Challenges.Defect;
using CuratedChallenges.Challenges.Regent;
using CuratedChallenges.Challenges.Silent;
using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.MainMenu;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CuratedChallenges.CuratedChallengesCode;

//You're recommended but not required to keep all your code in this package and all your assets in the CuratedChallenges folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string
        ModId = "CuratedChallenges"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        Harmony harmony = new(ModId);

        harmony.PatchAll();
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