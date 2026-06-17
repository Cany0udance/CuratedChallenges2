
/*

using CuratedChallenges.ChallengeUtil;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Random;

namespace CuratedChallenges.Modifiers;

public class CurseOfKnowledgeModifier : ChallengeModifier
{
    private static readonly IReadOnlyList<CardModel> _allCurseCards = new List<CardModel>
    {
        ModelDb.Card<Disintegration>(),
        ModelDb.Card<MindRot>(),
        ModelDb.Card<Gluttony>(),
        ModelDb.Card<Sloth>(),
        ModelDb.Card<WasteAway>(),
        ModelDb.Card<Inevitability>()
    };
    
    public CurseOfKnowledgeModifier(ChallengeDefinition challenge) : base(challenge) { }
    
    public override async Task BeforeTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side)
    {
        Player player = RunState.Players[0];
        CombatState combatState = player.Creature.CombatState;
        
        if (combatState == null)
            return;
        
        // Only trigger on enemy turn during round 1
        if (side == CombatSide.Player || combatState.RoundNumber != 1)
            return;
        
        Rng rng = player.PlayerRng.Rewards;
        
        List<CardModel> selectedCurses = new List<CardModel>();
        List<CardModel> availableCurses = new List<CardModel>(_allCurseCards);
        
        for (int i = 0; i < 2; i++)
        {
            int index = rng.NextInt(availableCurses.Count);
            CardModel curse = availableCurses[index];
            selectedCurses.Add(combatState.CreateCard(curse, player));
            availableCurses.RemoveAt(index);
        }
        
        CardModel chosenCurse = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(),
            selectedCurses,
            player);
            
        if (chosenCurse != null)
        {
            await ((KnowledgeDemon.IChoosable)chosenCurse).OnChosen();
        }
    }
}

*/