using CuratedChallenges.ChallengeUtil;
using CuratedChallenges.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CuratedChallenges.Challenges;

public class UltimateFormChallenge : ChallengeDefinition
{
    public override string Id => "ULTIMATE_FORM";

    public override ModelId? CharacterId => null;  // Shared challenge
    public override bool IsShared => true;


    private static Dictionary<ModelId, CardModel> CardUpgrades
    {
        get
        {
            return new Dictionary<ModelId, CardModel>()
            {
                // Starter card upgrades from VibrantHalo
                { ModelDb.Card<Bash>().Id, (CardModel)ModelDb.Card<Break>() },
                { ModelDb.Card<Neutralize>().Id, (CardModel)ModelDb.Card<Suppress>() },
                { ModelDb.Card<Bodyguard>().Id, (CardModel)ModelDb.Card<Protector>() },
                { ModelDb.Card<FallingStar>().Id, (CardModel)ModelDb.Card<MeteorShower>() },
                { ModelDb.Card<Dualcast>().Id, (CardModel)ModelDb.Card<Quadcast>() }
            };
        }
    }

    private static Dictionary<ModelId, RelicModel> RelicUpgrades
    {
        get
        {
            return new Dictionary<ModelId, RelicModel>()
            {
                // Starter relic upgrades from RecursiveCore
                { ModelDb.Relic<BurningBlood>().Id, (RelicModel)ModelDb.Relic<BlackBlood>() },
                { ModelDb.Relic<RingOfTheSnake>().Id, (RelicModel)ModelDb.Relic<RingOfTheDrake>() },
                { ModelDb.Relic<DivineRight>().Id, (RelicModel)ModelDb.Relic<GalacticDust>() },
                { ModelDb.Relic<BoundPhylactery>().Id, (RelicModel)ModelDb.Relic<PhylacteryUnbound>() },
                { ModelDb.Relic<CrackedCore>().Id, (RelicModel)ModelDb.Relic<InfusedCore>() }
            };
        }
    }

    public override IEnumerable<CardModel> GetStartingDeck(CharacterModel character)
    {
        var deck = new List<CardModel>();
        var ultimateStrike = ModelDb.GetById<CardModel>(new ModelId("CARD", "ULTIMATE_STRIKE"));
        var ultimateDefend = ModelDb.GetById<CardModel>(new ModelId("CARD", "ULTIMATE_DEFEND"));

        foreach (var card in character.StartingDeck)
        {
            // Replace Strike with Ultimate Strike
            if (card.Id.Entry.Contains("STRIKE", StringComparison.OrdinalIgnoreCase))
            {
                deck.Add(ultimateStrike);
            }
            // Replace Defend with Ultimate Defend
            else if (card.Id.Entry.Contains("DEFEND", StringComparison.OrdinalIgnoreCase))
            {
                deck.Add(ultimateDefend);
            }
            // Upgrade starter cards if possible
            else if (CardUpgrades.TryGetValue(card.Id, out var upgradedCard))
            {
                deck.Add(upgradedCard);
            }
            else
            {
                deck.Add(card);
            }
        }

        return deck;
    }

    public override IReadOnlyList<RelicModel> GetStartingRelics(CharacterModel character)
    {
        var relics = new List<RelicModel>();

        foreach (var relic in character.StartingRelics)
        {
            // Upgrade starter relics if possible
            if (RelicUpgrades.TryGetValue(relic.Id, out var upgradedRelic))
            {
                relics.Add(upgradedRelic);
            }
            else
            {
                relics.Add(relic);
            }
        }

        return relics;
    }

    public override ModifierModel CreateModifier()
    {
        var modifier = ModelDb.Modifier<UltimateFormModifier>().ToMutable() as UltimateFormModifier;
        modifier.SetChallenge(this);
        return modifier;
    }
}