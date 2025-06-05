using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MainPhase2Controller
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; }

    public MainPhase2Controller(MatchServerController server)
    {
        Server = server;
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 2 - Executing Main Phase ...");

        bool _player1Skipped = false;
        bool _player2Skipped = false;
        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float mainPhaseTimeout = 120f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(mainPhaseTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 2 - End: timeout...");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 2 - End: passed by opponent");

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 2 - Player {playerIndex} pressed skip");

            if (playerIndex == 0)
                _player1Skipped = true;
            else if (playerIndex == 1)
                _player2Skipped = true;

            if (_player1Skipped && _player2Skipped)
                skipped.SetResult(true);
        }
    }
}
