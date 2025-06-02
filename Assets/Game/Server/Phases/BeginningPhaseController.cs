using System;
using System.Threading.Tasks;
using UnityEngine;

public class BeginningPhaseController
{
    [field: SerializeField] public MatchServerController Server { get; private set; }
    public BeginningPhaseController(MatchServerController server)
    {
        Server = server;
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - BaginningPhaseController - Executing Beginning Phase...");

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

    async Task Upkeep()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Upkeep started...");

        TaskCompletionSource<bool> opponentPassedUpkeep = new TaskCompletionSource<bool>();
        float upkeepTimeout = 10f;

        Server.OnPlayerPassedUpkeep += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(upkeepTimeout));
        Task finished = await Task.WhenAny(opponentPassedUpkeep.Task, timeout);
        Server.OnPlayerPassedUpkeep -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Upkeep End: timeout...");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Upkeep End: passed by opponent");

        void OnPlayerSkip(int playerIndex)
        {
            if (playerIndex != Server.MatchState.CurrentPlayerIndex)
                opponentPassedUpkeep.SetResult(true);
        }
    }
}
