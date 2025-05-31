using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class TurnController
{
    [field: SerializeField] public MatchServerController Server { get; private set; }

    public TurnController(MatchServerController matchServerController)
    {
        Server = matchServerController;
    }

    public async Task StartTurn()
    {
        Debug.Log($"<color='red'>Server:</color> TurnController - Starting Turn {Server.MatchState.TurnNumber} - Player {Server.MatchState.CurrentPlayerIndex}");

        int currentPlayerIndex = Server.MatchState.CurrentPlayerIndex;
        Server.MatchState.PlayerStates[currentPlayerIndex].ResourcesPlayedThisTurn = 0;

        await ExecutePhase(GamePhase.Beginning);
        await ExecutePhase(GamePhase.MainPhase1);
    }

    async Task ExecutePhase(GamePhase phase)
    {
        Server.OnPhaseEnded?.Invoke(Server.MatchState.CurrentPhase);
        Server.MatchState.CurrentPhase = phase;
        Server.OnPhaseStarted?.Invoke(phase);

        switch (phase)
        {
            case GamePhase.Beginning:
                await ExecuteBeginningPhase();
                break;
            case GamePhase.MainPhase1:
                await ExecuteMainPhase();
                break;
            case GamePhase.Combat:
                break;
            case GamePhase.MainPhase2:
                break;
            case GamePhase.EndPhase:
                break;
        }
    }

    async Task ExecuteBeginningPhase()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Executing Beginning Phase...");
        
        // Untap -> Upkeep -> Draw

        int playerIndex         = Server.MatchState.CurrentPlayerIndex;
        PlayerState playerState = Server.MatchState.PlayerStates[playerIndex];

        // Untap Resources
        foreach (Card resource in playerState.ResourceZone)
        {
            if (resource.IsTapped)
                resource.Untap();
        }

        // Untap Creatures
        foreach (Card creature in playerState.CreatureZone)
        {
            if (creature.IsTapped)
                creature.Untap();
        }

        // Upkeep
        await Upkeep();

        // Draw Card
        if (Server.MatchState.TurnNumber > 1)
        {
            Card newCard = await Server.DrawCard(playerIndex);
        }
    }

    async Task ExecuteMainPhase()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Executing Main Phase ...");

        TaskCompletionSource<bool> opponentPassedUpkeep = new TaskCompletionSource<bool>();
        float upkeepTimeout = 120f;

        // Server.OnPlayerPassedUpkeep += OnPlayerPassedUpkep;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(upkeepTimeout));
        Task finished = await Task.WhenAny(opponentPassedUpkeep.Task, timeout);
        // Server.OnPlayerPassedUpkeep -= OnPlayerPassedUpkep;
    }

    async Task Upkeep()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Upkeep started...");

        TaskCompletionSource<bool> opponentPassedUpkeep = new TaskCompletionSource<bool>();
        float upkeepTimeout = 10f;

        Server.OnPlayerPassedUpkeep += OnPlayerPassedUpkep;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(upkeepTimeout));
        Task finished = await Task.WhenAny(opponentPassedUpkeep.Task, timeout);
        Server.OnPlayerPassedUpkeep -= OnPlayerPassedUpkep;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Upkeep End: timeout...");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Upkeep End: passed by opponent");

        void OnPlayerPassedUpkep(int playerIndex)
        {
            if (playerIndex != Server.MatchState.CurrentPlayerIndex)
                opponentPassedUpkeep.SetResult(true);
        }
    }

}