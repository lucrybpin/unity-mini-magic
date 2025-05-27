using System;
using UnityEngine;

[Serializable]
public class Card
{
    [field: SerializeField] public CardData Data { get; private set; }

    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public CardType Type { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public int Attack { get; private set; }
    [field: SerializeField] public int Defense { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public bool IsInHand { get; set; }
    [field: SerializeField] public bool IsInField { get; set; }
    [field: SerializeField] public bool IsTapped { get; set; }


    public Card(CardData cardData)
    {
        Data = cardData;
        Name = Data.Name;
        Type = Data.Type;
        Description = Data.Description;
        Cost = Data.Cost;
        Sprite = Data.Sprite;
        Attack = Data.Attack;
        Defense = Data.Defense;
        IsInHand = true;
        IsInField = false;
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
