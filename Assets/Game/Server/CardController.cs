using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class CardController
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; } // possibly a circular reference

    public CardController(MatchServerController matchServerController)
    {
        Server = matchServerController;
    }

    public Task<ExecutionResult> CanPlayCard(int playerIndex, Card card)
    {
        PlayerState playerState = Server.MatchState.PlayerStates[playerIndex];
        ExecutionResult result = new ExecutionResult();

        int availableResource = 0;

        foreach (Card resourceCard in playerState.ResourceZone)
        {
            if (!resourceCard.IsTapped)
                availableResource++;
        }

        if (!playerState.Hand.Contains(card))
        {
            result.Success = false;
            result.Message = $"CardController - {card.Name} is not in hand";
            return Task.FromResult(result);
        }

        if (card.Data.Type != CardType.Resource && card.Data.Cost > availableResource)
        {
            result.Success = false;
            result.Message = $"CardController - Can't play card {card.Name} because there is not enough resources";
            return Task.FromResult(result);
        }

        // Play only in current player turn and main phase
        GamePhase currentPhase  = Server.MatchState.CurrentPhase;
        bool isPlayerTurn       = Server.MatchState.CurrentPlayerIndex == playerIndex;
        bool isMainPhase        = currentPhase == GamePhase.MainPhase1 || currentPhase == GamePhase.MainPhase2;

        switch (card.Data.Type)
        {
            case CardType.Instant:
                result.Success = true;
                result.Message = $"CardController - success";
                break;

            case CardType.Resource:
                if (playerState.ResourcesPlayedThisTurn != 0)
                {
                    result.Success = false;
                    result.Message = $"CardController - Can't play card {card.Name}. Already casted a resource this turn";
                    break;
                }

                if (!isMainPhase)
                {
                    result.Success = false;
                    result.Message = $"CardController - Can't play card {card.Name} because it is not the main phase";
                    break;
                }

                if (!isPlayerTurn)
                {
                    result.Success = false;
                    result.Message = $"CardController - Can't play card {card.Name} because it is not player turn";
                    break;
                }

                result.Success = true;
                result.Message = $"CardController - success";
                break;

            case CardType.Sorcery:
            case CardType.Creature:
            case CardType.Enchantment:
            case CardType.Artifact:

                if (!isMainPhase)
                {
                    result.Success = false;
                    result.Message = $"CardController - Can't play card {card.Name} because it is not the main phase";
                    break;
                }

                if (!isPlayerTurn)
                {
                    result.Success = false;
                    result.Message = $"CardController - Can't play card {card.Name} because it is not player turn";
                    break;
                }

                result.Success = true;
                result.Message = $"CardController - success";
                break;
        }

        return Task.FromResult(result);
    }

    public async Task ProcessCardPlay(int playerIndex, Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Card {card.Name}");

        PlayerState playerState = Server.MatchState.PlayerStates[playerIndex];

        await PlayManaCost(playerState, card.Cost);

        switch (card.Type)
        {
            case CardType.Resource:
                Server.ZonesController.MoveCard(card, ZoneType.Hand, ZoneType.Resource, playerIndex);
                playerState.ResourcesPlayedThisTurn++;
                // playerState.ResourceZone.Add(card);
                await ProcessResource(card);
                break;

            case CardType.Creature:
                Server.ZonesController.MoveCard(card, ZoneType.Hand, ZoneType.Creature, playerIndex);
                // playerState.CreatureZone.Add(card);
                await ProcessCreature(card);
                break;

            case CardType.Enchantment:
                Server.ZonesController.MoveCard(card, ZoneType.Hand, ZoneType.Enchantment, playerIndex);
                // playerState.EnchantmentZone.Add(card);
                await ProcessEnchantment(card);
                break;

            case CardType.Sorcery:
                await ProcessSorcery(card);
                Server.ZonesController.MoveCard(card, ZoneType.Hand, ZoneType.Graveyard, playerIndex);
                // playerState.GraveyardZone.Add(card);
                break;

            case CardType.Instant:
                await ProcessInstant(card);
                Server.ZonesController.MoveCard(card, ZoneType.Hand, ZoneType.Graveyard, playerIndex);
                // playerState.GraveyardZone.Add(card);
                break;
        }
    }

    Task<bool> PlayManaCost(PlayerState playerState, int manaCost)
    {
        int tappedResources = 0;

        foreach (var resource in playerState.ResourceZone)
        {
            if (!resource.IsTapped && tappedResources < manaCost)
            {
                resource.Tap();
                tappedResources++;
            }
        }

        return Task.FromResult(true);
    }

    Task<bool> ProcessResource(Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Resource");
        return Task.FromResult(true);
    }

    Task<bool> ProcessCreature(Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Creature");
        card.CanAttack = false;
        return Task.FromResult(true);
    }

    Task<bool> ProcessEnchantment(Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Enchantment");
        return Task.FromResult(true);
    }

    Task<bool> ProcessInstant(Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Instant");
        return Task.FromResult(true);
    }

    Task<bool> ProcessSorcery(Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Sorcery");
        return Task.FromResult(true);
    }


}