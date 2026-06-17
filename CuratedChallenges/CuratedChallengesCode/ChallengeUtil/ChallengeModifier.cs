using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CuratedChallenges.ChallengeUtil;

public class ChallengeModifier : ModifierModel
{
    [SavedProperty]
    public string ChallengeId { get; private set; }
    
    public ChallengeDefinition Challenge { get; private set; }
    public static bool IsStartingChallengeRun = false;
    
    public virtual IEnumerable<IWinCondition> WinConditions => Enumerable.Empty<IWinCondition>();
    
    public ChallengeModifier() : base() { }
    
    public ChallengeModifier(ChallengeDefinition challenge)
    {
        Challenge = challenge;
        ChallengeId = challenge.Id;
    }
    
    public override LocString Title => new LocString("challenges", ChallengeId + ".title");
    public override LocString Description => new LocString("challenges", ChallengeId + ".description");
    public override bool ClearsPlayerDeck => true;
    public bool AllowNeowBonuses => Challenge?.AllowNeowBonuses ?? false;
    
    protected override string IconPath => ImageHelper.GetImagePath("packed/modifiers/curated_challenge.png");
    
    public void SetChallenge(ChallengeDefinition challenge)
    {
        Challenge = challenge;
        ChallengeId = challenge.Id;
    }
    
    protected override void AfterRunCreated(RunState runState)
    {
        base.AfterRunCreated(runState);
        
        IsStartingChallengeRun = false;
        
        foreach (Player player in runState.Players)
        {
            AddStartingDeck(runState, player);
            AddStartingRelics(player);
        }
        
        OnChallengeRunCreated(runState);
    }
    
    private void AddStartingDeck(RunState runState, Player player)
    {
        var startingDeck = Challenge.GetStartingDeck(player.Character);
    
        foreach (var cardTemplate in startingDeck)
        {
            if (cardTemplate.IsMutable)
            {
                cardTemplate.Owner = player;
                player.Deck.AddInternal(cardTemplate, silent: true);
            }
            else
            {
                var mutableCard = runState.CreateCard(cardTemplate, player);
                player.Deck.AddInternal(mutableCard, silent: true);
            }
        }
    }
    
    private void AddStartingRelics(Player player)
    {
        var challengeRelics = Challenge.GetStartingRelics(player.Character);
        
        foreach (var relic in challengeRelics)
        {
            var mutableRelic = relic.IsMutable ? relic : relic.ToMutable();
            mutableRelic.FloorAddedToDeck = 1;
            SaveManager.Instance.MarkRelicAsSeen(mutableRelic);
            player.AddRelicInternal(mutableRelic, silent: true);
        }
    }
    

    
    protected override void AfterRunLoaded(RunState runState)
    {
        base.AfterRunLoaded(runState);
        
        if (!string.IsNullOrEmpty(ChallengeId))
        {
            Challenge = ChallengeRegistry.GetChallenge(ChallengeId);
        }
    }
    
    protected virtual void OnChallengeRunCreated(RunState runState) { }
    
    public void CheckWinConditions(IRunState runState, Player player)
    {
        if (runState.IsGameOver) return;
        
        foreach (var condition in WinConditions)
        {
            if (condition.CheckCondition(runState, player))
            {
                condition.TriggerVictory(runState);
                return;
            }
        }
    }
}