using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Card
{
    [field: SerializeField] public CardData Data { get; private set; }

    // Base Info
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public CardType Type { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }

    // Creature Stats
    [field: SerializeField] public int Attack { get; private set; }
    [field: SerializeField] public int Defense { get; private set; }
    [field: SerializeField] public int CastTurnIndex { get; set; }

    // Game Info
    [field: SerializeField] public string InstanceID { get; private set; }
    [field: SerializeField] public bool IsInHand { get; set; }
    [field: SerializeField] public bool IsInField { get; set; }
    [field: SerializeField] public bool IsTapped { get; private set; }

    // Effects
    [field: SerializeField] public List<string> Effects { get; private set; }

    public Card(CardData cardData)
    {
        Data = cardData;
        // Basic Info
        Name = Data.Name;
        Type = Data.Type;
        Description = Data.Description;
        Cost = Data.Cost;
        Sprite = Data.Sprite;
        // Creature Info
        Attack = Data.Attack;
        Defense = Data.Defense;
        // Game Info
        InstanceID = Guid.NewGuid().ToString();
        IsInHand = true;
        IsInField = false;
        IsTapped = false;
        // Effeccts
    }

    public void Tap()
    {
        IsTapped = true;
    }

    public void Untap()
    {
        IsTapped = false;
    }

    public Card Clone()
    {
        Card clonedCard = new Card(Data);
        clonedCard.IsInField = this.IsInField;
        clonedCard.IsInHand = this.IsInHand;
        clonedCard.IsTapped = this.IsTapped;
        return clonedCard;
    }
}
