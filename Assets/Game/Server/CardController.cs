using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class CardController
{
    [field: SerializeField] public MatchServerController Server { get; private set; } // possibly a circular reference

    public CardController(MatchServerController matchServerController)
    {
        Server = matchServerController;
    }

    public Task<bool> CanPlayCard(int playerIndex, Card card)
    {
        PlayerState playerState = Server.MatchState.PlayerStates[playerIndex];

        int availableResource = playerState.ResourceZone.Count;

        if(!playerState.Hand.Contains(card))
        {
            Debug.Log($"<color='red'>Server:</color> CastController - Can't play card {card.Name}. Not in hand");
            return Task.FromResult(false);
        }

        if (card.Data.Cost > availableResource)
        {
            Debug.Log($"<color='red'>Server:</color> CastController - Can't play card {card.Name}. Not enough resources");
            return Task.FromResult(false);
        }

        switch (card.Data.Type)
        {
            case CardType.Instant:
                return Task.FromResult(true);
            case CardType.Resource:
            case CardType.Sorcery:
            case CardType.Creature:
            case CardType.Enchantment:
            case CardType.Artifact:
                // Play only in current player turn and main phase
                GamePhase currentPhase = Server.MatchState.CurrentPhase;
                bool isPlayerTurn = Server.MatchState.CurrentPlayerIndex == playerIndex;
                bool isMainPhase = currentPhase == GamePhase.MainPhase1 || currentPhase == GamePhase.MainPhase2;

                if(!isMainPhase)
                    Debug.Log($"<color='red'>Server:</color> CastController - Can't play card {card.Name}. Not main phase");
                else if(!isPlayerTurn)
                    Debug.Log($"<color='red'>Server:</color> CastController - Can't play card {card.Name}. Not player turn");
                else
                    Debug.Log($"<color='red'>Server:</color> CastController - Can play card {card.Name}.");

                return Task.FromResult(isPlayerTurn && isMainPhase);
        }

        return Task.FromResult(false);
    }

    public async Task ProcessCardPlay(int playerIndex, Card card)
    {
        Debug.Log($"<color='red'>Server:</color> CastController - Processing Card {card.Name}");

        PlayerState playerState = Server.MatchState.PlayerStates[playerIndex];

        await PlayManaCost(playerState, card.Cost);

        switch (card.Type)
        {
            case CardType.Resource:
                playerState.ResourceZone.Add(card);
                await ProcessResource(card);
                break;

            case CardType.Creature:
                playerState.CreatureZone.Add(card);
                await ProcessCreature(card);
                break;

            case CardType.Enchantment:
                playerState.EnchantmentZone.Add(card);
                await ProcessEnchantment(card);
                break;

            case CardType.Sorcery:
                await ProcessSorcery(card);
                playerState.Graveyard.Add(card);
                break;

            case CardType.Instant:
                await ProcessInstant(card);
                playerState.Graveyard.Add(card);
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