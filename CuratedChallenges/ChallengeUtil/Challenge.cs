using CuratedChallenges.Modifiers;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace CuratedChallenges.ChallengeUtil;

public class Challenge
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ModelId? CharacterId { get; set; }
    public bool IsShared { get; set; }
    public bool Selected { get; set; }
    
    public IEnumerable<CardModel> StartingDeck { get; set; }
    public IReadOnlyList<RelicModel> StartingRelics { get; set; }
    public int? StartingGold { get; set; }
    
    public string SpecialRules { get; set; }
    public string WinConditions { get; set; }
    
    // UI state
    public Rect2 HitboxRect { get; set; }
    
    // Store the definition so we can create modifiers
    public ChallengeDefinition Definition { get; private set; }
    public CharacterModel Character { get; private set; }
    
    public Challenge(ChallengeDefinition definition, CharacterModel character)
    {
        Definition = definition;
        Character = character;
    
        // Generate unique ID for shared challenges per character
        Id = definition.IsShared 
            ? $"{definition.Id}_{character.Id.Entry}" 
            : definition.Id;
    
        Name = definition.Name;
        CharacterId = definition.CharacterId;
        IsShared = definition.IsShared;
        Selected = false;
        
        StartingDeck = definition.GetStartingDeck(character);
        StartingRelics = definition.GetStartingRelics(character);
        StartingGold = definition.StartingGold;
        
        SpecialRules = definition.SpecialRules;
        WinConditions = definition.WinConditions;
        
        HitboxRect = new Rect2(0, 0, 300, 80);
    }
    
    public ModifierModel CreateModifier()
    {
        var template = Definition.CreateModifier();
        return (ModifierModel)template.MutableClone();
    }
}