using System;
using System.Collections.Generic;

[Serializable]
public class PlayerState
{
    public int PlayerId;
    public int Life;
    public List<Card> Hand;
    public List<Card> Deck;
    public List<Card> ResourceZone;
    public List<Card> CreatureZone;
    public List<Card> EnchantmentZone;
    public List<Card> Graveyard;

    public PlayerState()
    {
        Hand = new List<Card>();
        Deck = new List<Card>();
        ResourceZone = new List<Card>();
        CreatureZone = new List<Card>();
        EnchantmentZone = new List<Card>();
        Life = 20;
    }
}
