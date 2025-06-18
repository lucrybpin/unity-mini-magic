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

    public void MoveCard(Card card, int playerIndex, ZoneType from, ZoneType to)
    {
        RemoveFromZone(card, from, playerIndex);
        AddToZone(card, to, playerIndex);
        Server.OnCardZoneChanged?.Invoke(card, playerIndex, from, to);
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
                card.IsInHand = true;
                card.IsInField = false;
                break;
            case ZoneType.Creature:
                Server.MatchState.PlayerStates[playerID].CreatureZone.Add(card);
                card.IsInHand = false;
                card.IsInField = true;
                break;
            case ZoneType.Resource:
                Server.MatchState.PlayerStates[playerID].ResourceZone.Add(card);
                card.IsInHand = false;
                card.IsInField = true;
                break;
            case ZoneType.Enchantment:
                Server.MatchState.PlayerStates[playerID].EnchantmentZone.Add(card);
                card.IsInHand = false;
                card.IsInField = true;
                break;
            case ZoneType.Graveyard:
                Server.MatchState.PlayerStates[playerID].GraveyardZone.Add(card);
                card.IsInHand = false;
                card.IsInField = false;
                break;
        }
    }

}
