using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MainPhase1Controller
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; }

    public MainPhase1Controller(MatchServerController server)
    {
        Server = server;
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 1 - Executing Main Phase ...");

        bool _player1Skipped = false;
        bool _player2Skipped = false;
        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float mainPhaseTimeout = 120f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Server.OnTimerChanged?.Invoke(mainPhaseTimeout);
        Task timeout = Task.Delay(TimeSpan.FromSeconds(mainPhaseTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 1 - End: timeout...");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 1 - End: passed by opponent");

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 1 - Player {playerIndex} pressed skip");

            if (playerIndex == 0)
                _player1Skipped = true;
            else if (playerIndex == 1)
                _player2Skipped = true;

            if (_player1Skipped && _player2Skipped)
                skipped.SetResult(true);
        }
    }

}
