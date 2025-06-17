using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class TurnController
{
    [field: NonSerialized][field: SerializeField] public MatchServerController Server { get; private set; }
    [field: SerializeField] public BeginningPhaseController BeginningPhase { get; private set; }
    [field: SerializeField] public MainPhase1Controller MainPhase1 { get; private set; }
    [field: SerializeField] public CombatPhaseController CombatPhase { get; private set; }
    [field: SerializeField] public MainPhase2Controller MainPhase2 { get; private set; }

    public TurnController(MatchServerController matchServerController)
    {
        Server = matchServerController;
        BeginningPhase = new BeginningPhaseController(Server);
        MainPhase1 = new MainPhase1Controller(Server);
        CombatPhase = new CombatPhaseController(Server);
        MainPhase2 = new MainPhase2Controller(Server);
    }

    public async Task StartTurn()
    {
        Debug.Log($"<color='red'>Server:</color> TurnController - Starting Turn {Server.MatchState.TurnNumber} - Player {Server.MatchState.CurrentPlayerIndex}");

        int currentPlayerIndex = Server.MatchState.CurrentPlayerIndex;
        Server.MatchState.PlayerStates[currentPlayerIndex].ResourcesPlayedThisTurn = 0;

        await ExecutePhase(GamePhase.Beginning);
        await ExecutePhase(GamePhase.MainPhase1);
        await ExecutePhase(GamePhase.Combat);
        await ExecutePhase(GamePhase.MainPhase2);
        await ExecutePhase(GamePhase.EndPhase);

        Server.MatchState.CurrentPlayerIndex = (Server.MatchState.CurrentPlayerIndex + 1) % Server.MatchState.PlayerStates.Count;
        Server.MatchState.TurnNumber++;
        await StartTurn();
    }

    async Task ExecutePhase(GamePhase phase)
    {
        Server.OnPhaseEnded?.Invoke(Server.MatchState.CurrentPhase);
        Server.MatchState.CurrentPhase = phase;
        Server.OnPhaseStarted?.Invoke(phase);

        Debug.Log($"------- ------- ------- ------- ------- ------ -------");
        

        switch (phase)
        {
            case GamePhase.Beginning:
                await BeginningPhase.Execute();
                break;
            case GamePhase.MainPhase1:
                await MainPhase1.Execute();
                break;
            case GamePhase.Combat:
                await CombatPhase.Execute();
                break;
            case GamePhase.MainPhase2:
                await MainPhase2.Execute();
                break;
            case GamePhase.EndPhase:
                break;
        }
    }

    public void SetAttackers(List<Card> cards)
    {
        CombatPhase.SetAttackers(cards);
    }

    public void SetBlockers(List<BlockData> blockers)
    {
        CombatPhase.SetBlockers(blockers);
    }

    public List<Card> GetAttackers()
    {
        return CombatPhase.Attackers;
    }
}