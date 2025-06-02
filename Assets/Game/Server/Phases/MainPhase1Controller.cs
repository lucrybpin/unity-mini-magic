using System;
using System.Threading.Tasks;
using UnityEngine;

public class MainPhase1Controller
{
    [field: SerializeField] public MatchServerController Server { get; private set; }

    public MainPhase1Controller(MatchServerController server)
    {
        Server = server;
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - MainPhase1Controller - Executing Main Phase ...");

        TaskCompletionSource<bool> opponentPassedMainPhase1 = new TaskCompletionSource<bool>();
        float mainPhaseTimeout = 120f;

        Server.OnPlayerPassedMainPhase1 += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(mainPhaseTimeout));
        Task finished = await Task.WhenAny(opponentPassedMainPhase1.Task, timeout);
        Server.OnPlayerPassedMainPhase1 -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 1 End: timeout...");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Main Phase 1 End: passed by opponent");

        void OnPlayerSkip(int playerIndex)
        {
            if (playerIndex != Server.MatchState.CurrentPlayerIndex)
                opponentPassedMainPhase1.SetResult(true);
        }
    }

}
