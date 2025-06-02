using System;
using System.Threading.Tasks;
using UnityEngine;

public class CombatPhaseController
{
    [field: SerializeField] public MatchServerController Server { get; private set; }


    public CombatPhaseController(MatchServerController server)
    {
        Server = server;
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - CombatPhase Started...");

        // Beginning of Combat -> Declare Atkers -> Declare Blckers -> First Strike Damage -> Damage -> End Of Combat

        await BeginningOfCombat();

        Debug.Log($"<color='red'>Server:</color> Turn Controller - CombatPhase - Declare Attackers");
        // TODO: Declare atackers

    }

    async Task BeginningOfCombat()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - CombatPhaseController - Beginning of Combat");

        bool _player1SkippedBeginning = false;
        bool _player2SkippedBeginning = false;

        // TaskCompletionSource<bool> opponentPassedCombatPhase = new TaskCompletionSource<bool>();
        TaskCompletionSource<bool> playersSkippedBeginningOfCombat = new TaskCompletionSource<bool>();
        float stepTimeout = 30f;

        Server.OnPlayerPassedBeginningCombatStep += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(playersSkippedBeginningOfCombat.Task, timeout);
        Server.OnPlayerPassedBeginningCombatStep -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - CombatPhase - Combat Beginning End: timeout...");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - CombatPhase End: players are ready");

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - CombatPhaseController - Player {playerIndex} skipped combat beginning");

            if (playerIndex == 1)
                _player1SkippedBeginning = true;
            else if (playerIndex == 2)
                _player2SkippedBeginning = true;
            else
                Debug.Log($">>>> Combat Phase: Player index {playerIndex} asked to skip the turn. But it is not expected this index");

            if (_player1SkippedBeginning && _player2SkippedBeginning)
                playersSkippedBeginningOfCombat.SetResult(true);
        }

    }
}
