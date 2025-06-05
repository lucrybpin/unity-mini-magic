using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class EndPhaseController
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; }

    public EndPhaseController(MatchServerController server)
    {
        Server = server;
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - End Phase - Started...");

        bool _player1Skipped = false;
        bool _player2Skipped = false;

        TaskCompletionSource<bool> opponentPassedUpkeep = new TaskCompletionSource<bool>();
        float upkeepTimeout = 10f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(upkeepTimeout));
        Task finished = await Task.WhenAny(opponentPassedUpkeep.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - End Phase - Upkeep End: timeout");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - End Phase - Upkeep End: skipped by players");

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - End Phase - Player {playerIndex} pressed skip");

            if (playerIndex == 0)
                _player1Skipped = true;
            else if (playerIndex == 1)
                _player2Skipped = true;

            if (_player1Skipped && _player2Skipped)
                opponentPassedUpkeep.SetResult(true);
        }
    }
}
