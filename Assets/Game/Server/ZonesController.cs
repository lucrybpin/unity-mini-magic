using System.Threading.Tasks;
using UnityEngine;

public enum ZoneType
{
    Hand,
    Graveyard,
    Exile,
    Resource,
    Creature,
    Enchantment,
    Deck,
}

public class ZonesController
{
    [field: SerializeField] public MatchServerController Server { get; private set; }

    public ZonesController(MatchServerController matchServerController)
    {
        Server = matchServerController;
    }

    public void MoveCard(Card card, ZoneType from, ZoneType to, int playerID)
    {
        RemoveFromZone(card, from, playerID);
        AddToZone(card, to, playerID);
        Debug.Log($"<color='red'>Server:</color> ZonesController - Moved Card {card.Name} from {from} to {to}");
    }

    void RemoveFromZone(Card card, ZoneType zone, int playerID)
    {
        switch (zone)
        {
            case ZoneType.Deck:
                Server.MatchState.PlayerStates[playerID].Deck.Remove(card);
                break;
            case ZoneType.Hand:
                Server.MatchState.PlayerStates[playerID].Hand.Remove(card);
                break;
            case ZoneType.Creature:
                Server.MatchState.PlayerStates[playerID].CreatureZone.Remove(card);
                break;
            case ZoneType.Resource:
                Server.MatchState.PlayerStates[playerID].ResourceZone.Remove(card);
                break;
            case ZoneType.Enchantment:
                Server.MatchState.PlayerStates[playerID].EnchantmentZone.Remove(card);
                break;
            case ZoneType.Graveyard:
                Server.MatchState.PlayerStates[playerID].GraveyardZone.Remove(card);
                break;
        }
    }

    void AddToZone(Card card, ZoneType zone, int playerID)
    {
        switch (zone)
        {
            case ZoneType.Deck:
                Server.MatchState.PlayerStates[playerID].Deck.Add(card);
                break;
            case ZoneType.Hand:
                Server.MatchState.PlayerStates[playerID].Hand.Add(card);
                break;
            case ZoneType.Creature:
                Server.MatchState.PlayerStates[playerID].CreatureZone.Add(card);
                break;
            case ZoneType.Resource:
                Server.MatchState.PlayerStates[playerID].ResourceZone.Add(card);
                break;
            case ZoneType.Enchantment:
                Server.MatchState.PlayerStates[playerID].EnchantmentZone.Add(card);
                break;
            case ZoneType.Graveyard:
                Server.MatchState.PlayerStates[playerID].GraveyardZone.Add(card);
                break;
        }
    }

}
