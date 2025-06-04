using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public struct BlockData
{
    public List<Card> Blockers;
    public Card Attacker;
}

[Serializable]
public class CombatPhaseController
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; }

    [field: SerializeField] public List<Card> Attackers { get; private set; }
    [field: SerializeField] public List<BlockData> Blockers { get; private set; }

    public CombatPhaseController(MatchServerController server)
    {
        Server = server;
        Attackers = new List<Card>();
        Blockers = new List<BlockData>();
    }

    public async Task Execute()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Started...");

        // Beginning of Combat -> Declare Atkers -> Declare Blckers -> First Strike Damage -> Damage -> End Of Combat

        await BeginningOfCombat();

        await DeclareAttackers();

        await DeclareBlockers();

        await CombatDamage();

    }

    async Task BeginningOfCombat()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Beginning of Combat");

        Server.MatchState.CurrentCombatStep = CombatStep.BeginCombat;
        Server.OnCombatStepStarted?.Invoke(CombatStep.BeginCombat);
        bool _player1Skipped = false;
        bool _player2Skipped = false;

        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float stepTimeout = 30f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat Beginning End: timeout");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - End: players are ready");

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Player {playerIndex} skipped combat beginning");

            if (playerIndex == 0)
                _player1Skipped = true;
            else if (playerIndex == 1)
                _player2Skipped = true;

            if (_player1Skipped && _player2Skipped)
                skipped.SetResult(true);
        }

        Server.OnCombatStepEnded?.Invoke(CombatStep.BeginCombat);
    }

    async Task DeclareAttackers()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Declare Atackers step started.");

        Server.MatchState.CurrentCombatStep = CombatStep.DeclareAttackers;
        Server.OnCombatStepStarted?.Invoke(CombatStep.DeclareAttackers);

        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float stepTimeout = 30f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat Beginning End: timeout");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - End: player ready to next step");

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Player {playerIndex} skipped combat beginning");

            if (playerIndex == Server.MatchState.CurrentPlayerIndex)
                skipped.SetResult(true);
        }

        Server.OnCombatStepEnded?.Invoke(CombatStep.DeclareAttackers);
    }

    public void SetAttackers(List<Card> cards)
    {
        if (cards != null)
        {
            Attackers = cards;
        }
    }

    async Task DeclareBlockers()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Declare Blockers step started.");

        Server.MatchState.CurrentCombatStep = CombatStep.DeclareBlockers;
        Server.OnCombatStepStarted?.Invoke(CombatStep.DeclareBlockers);

        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float stepTimeout = 30f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Player {playerIndex} skipped blockers");

            if (playerIndex != Server.MatchState.CurrentPlayerIndex)
                skipped.SetResult(true);
        }

        Server.OnCombatStepEnded?.Invoke(CombatStep.DeclareBlockers);
    }

    async Task CombatDamage()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat Damage step started.");

        Server.MatchState.CurrentCombatStep = CombatStep.CombatDamage;
        Server.OnCombatStepStarted?.Invoke(CombatStep.CombatDamage);

        foreach (Card attacker in Attackers)
        {
            BlockData blockData = Blockers.Find(x => x.Attacker == attacker);

            if (blockData.Blockers != null && blockData.Blockers.Count > 0)
            {
                // TODO: Implement Blocking
            }
            else
            {
                Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Creature {attacker.Name} deal {attacker.Attack} damage to defending player. ");
                // TODO: Implement Damage to Player
            }
        }

        Server.OnCombatStepEnded?.Invoke(CombatStep.CombatDamage);
    }
}
