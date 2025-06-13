using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class AIController
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; }

    public AIController(MatchServerController server)
    {
        Server = server;
        server.OnPhaseStarted += OnPhaseStarted;
    }

    private void OnPhaseStarted(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Beginning:
                _ = ExecuteBeginningPhase();
                break;
            case GamePhase.MainPhase1:
                _ = ExecuteMainPhase();
                break;
        }
    }

    public async Task ExecuteBeginningPhase()
    {
        Debug.Log($"<color='red'>AI:</color> Execute Beginning Phase");

        await Task.Delay(1000); // Wait until upkeep phase starts
        Server.OnPlayerSkipClicked?.Invoke(1);
    }

    private async Task ExecuteMainPhase()
    {
        Debug.Log($"<color='red'>AI:</color> Execute Beginning Phase");

        PlayerState me = Server.MatchState.PlayerStates[1];
        List<Card> playableCards = new List<Card>();

        // First, try to play a resource if we haven't played one this turn
        if (me.ResourcesPlayedThisTurn == 0)
        {
            foreach (Card card in me.Hand)
            {
                if (card.Type == CardType.Resource)
                {
                    playableCards.Add(card);
                    break;
                }
            }
        }

        // Then look for creatures to play
        foreach (Card card in me.Hand)
        {
            if (card.Type == CardType.Creature)
            {
                playableCards.Add(card);
            }
        }

        // Try to play cards in order
        foreach (Card card in playableCards)
        {
            ExecutionResult result = await Server.CastCard(1, card);
            if (result.Success)
            {
                Debug.Log($"<color='red'>AI:</color> Successfully played {card.Name}");
                await Task.Delay(1000); // Give time for animations
            }
        }

        // End main phase
        // Server.OnPlayerSkipClicked?.Invoke(1);
    }
} 