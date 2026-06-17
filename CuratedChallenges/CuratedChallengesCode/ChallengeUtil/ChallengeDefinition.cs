using CuratedChallenges.ChallengeUtil.Progression;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace CuratedChallenges.ChallengeUtil;

public abstract class ChallengeDefinition
{
    public abstract string Id { get; }
    public string Name => new LocString("gameplay_ui", $"CHALLENGE_{Id}_TITLE").GetFormattedText();
    public abstract ModelId? CharacterId { get; }
    public abstract bool IsShared { get; }
    public string SpecialRules => new LocString("gameplay_ui", $"CHALLENGE_{Id}_SPECIAL_RULES").GetFormattedText();
    public string WinConditions => new LocString("gameplay_ui", $"CHALLENGE_{Id}_WIN_CONDITIONS").GetFormattedText();
    
    public virtual int? StartingGold => null;
    
    public virtual IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        return character.StartingDeck;
    }
    
    public virtual IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        return character.StartingRelics;
    }
    
    public virtual IEnumerable<ModelId> GetBlacklistedRelics()
    {
        return Enumerable.Empty<ModelId>();
    }
    
    public virtual IEnumerable<ModelId> GetStartingPotions()
    {
        return Enumerable.Empty<ModelId>();
    }
    
    public virtual IEnumerable<string> GetBlacklistedEvents()
    {
        return Enumerable.Empty<string>();
    }
    
    /// <summary>
    /// If true, Neow will offer normal run bonuses instead of skipping them.
    /// </summary>
    public virtual bool AllowNeowBonuses => false;
    
    /// <summary>
    /// If set, this challenge is hidden until the specified challenge ID is completed.
    /// For shared challenges, completion by any character counts.
    /// </summary>
    public virtual string HiddenUntilChallengeId => null;
    
    public bool IsHidden(ModelId characterId)
    {
        if (string.IsNullOrEmpty(HiddenUntilChallengeId))
            return false;
    
        var prerequisite = ChallengeRegistry.GetChallenge(HiddenUntilChallengeId);
        if (prerequisite == null)
            return true;
        
        // Character-specific prerequisites require completion by that character
        // Shared prerequisites can be completed by any character
    
        if (!prerequisite.IsShared)
        {
            return !ChallengeProgressHelper.IsChallengeCompleted(
                HiddenUntilChallengeId, 
                prerequisite.IsShared, 
                characterId
            );
        }
    
        return !IsPrerequisiteCompletedByAny(HiddenUntilChallengeId);
    }
    
    private static bool IsPrerequisiteCompletedByAny(string challengeId)
    {
        var data = ChallengeDataManager.GetData();
        
        // Check direct completion
        if (data.CompletedChallenges.ContainsKey(challengeId))
            return true;
        
        // Check character-specific completions (for shared challenges stored as "id_character")
        return data.CompletedChallenges.Keys.Any(k => k.StartsWith(challengeId + "_"));
    }
    
    public abstract ModifierModel CreateModifier();
}