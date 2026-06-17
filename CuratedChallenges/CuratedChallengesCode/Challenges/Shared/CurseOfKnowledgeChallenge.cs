/*

using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace CuratedChallenges.Challenges;

public class CurseOfKnowledgeChallenge : ChallengeDefinition
{
    public override string Id => "CURSE_OF_KNOWLEDGE";
    public override string Name => "Curse of Knowledge";
    public override ModelId? CharacterId => null;  // Shared challenge
    public override bool IsShared => true;
    
    public override string SpecialRules => 
        "At the end of the enemy's first turn, choose 1 of 2 [red]Knowledge Demon Debuffs[/red].";
    
    public override string WinConditions => "Defeat the Act 3 boss.";
    
    public override ModifierModel CreateModifier()
    {
        return new CurseOfKnowledgeModifier(this);
    }
}

*/