using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace CuratedChallenges.Modifiers;

public class SplitPersonalityModifier : ChallengeModifier
{
public SplitPersonalityModifier() : base() { }
    
    public SplitPersonalityModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (room == null || !room.RoomType.IsCombatRoom())
            return false;
        
        // Find and remove one card reward if present
        CardReward cardReward = rewards.OfType<CardReward>().FirstOrDefault();
        if (cardReward != null)
        {
            rewards.Remove(cardReward);
            return true;
        }
        
        return false;
    }
}