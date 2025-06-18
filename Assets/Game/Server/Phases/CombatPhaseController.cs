using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// public class CombatResult
// {
//     public List<Card> 
// }

[Serializable]
public class BlockData
{
    public List<Card> Blockers;
    public Card Attacker;

    public BlockData(List<Card> blockers, Card attacker)
    {
        Blockers = blockers;
        Attacker = attacker;
    }
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

        // Beginning of Combat -> Declare Atkers -> Declare Blckers -> Damage -> End Of Combat

        await BeginningOfCombat();

        await DeclareAttackers();

        await DeclareBlockers();

        await CombatDamage();

        await EndOfCombat();

    }

    async Task BeginningOfCombat()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Beginning of Combat");

        Server.MatchState.CurrentCombatStep = CombatStep.BeginCombat;
        Server.OnCombatStepStarted?.Invoke(CombatStep.BeginCombat);
        bool _player1Skipped = false;
        bool _player2Skipped = false;
        Attackers.Clear();
        Blockers.Clear();

        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float stepTimeout = 30f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Server.OnTimerChanged?.Invoke(stepTimeout);
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat Beginning End: timeout");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - End: players are ready");

        Server.OnCombatStepEnded?.Invoke(CombatStep.BeginCombat);

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
    }

    async Task DeclareAttackers()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Declare Atackers step started.");

        Server.MatchState.CurrentCombatStep = CombatStep.DeclareAttackers;
        Server.OnCombatStepStarted?.Invoke(CombatStep.DeclareAttackers);

        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float stepTimeout = 30f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Server.OnTimerChanged?.Invoke(stepTimeout);
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        Server.OnCombatStepEnded?.Invoke(CombatStep.DeclareAttackers);

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
    }

    public void SetAttackers(List<Card> cards)
    {
        if (cards != null)
        {
            Attackers = cards;

            foreach (Card card in cards)
                card.Tap();
        }
    }

    public void SetBlockers(List<BlockData> blockers)
    {
        if (blockers != null)
        {
            Blockers = blockers;
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
        Server.OnTimerChanged?.Invoke(stepTimeout);
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        Server.OnCombatStepEnded?.Invoke(CombatStep.DeclareBlockers);

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Player {playerIndex} skipped blockers");

            if (playerIndex != Server.MatchState.CurrentPlayerIndex)
                skipped.SetResult(true);
        }
    }

    async Task CombatDamage()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat Damage step started.");

        Server.MatchState.CurrentCombatStep = CombatStep.CombatDamage;
        Server.OnCombatStepStarted?.Invoke(CombatStep.CombatDamage);

        foreach (Card attacker in Attackers)
        {
            BlockData blockData = Blockers.Find(x => x.Attacker == attacker);

            if (blockData != null && blockData.Blockers != null && blockData.Blockers.Count > 0)
            {
                int remainingDamage = attacker.Attack;
                foreach (Card blocker in blockData.Blockers)
                {
                    int damageToBlocker = Math.Min(blocker.Resistance, remainingDamage);
                    blocker.ReceiveDamage(damageToBlocker);
                    remainingDamage -= damageToBlocker;
                    attacker.ReceiveDamage(blocker.Attack);
                    Server.OnCardChangedState?.Invoke(blocker);
                    Server.OnCardChangedState?.Invoke(attacker);
                }
            }
            else
            {
                Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Creature {attacker.Name} deal {attacker.Attack} damage to defending player. ");
                int defendingPlayerIndex = (Server.MatchState.CurrentPlayerIndex + 1) % Server.MatchState.PlayerStates.Count;
                PlayerState defendingPlayer = Server.MatchState.PlayerStates[defendingPlayerIndex];
                defendingPlayer.Life -= attacker.Attack;
                Server.OnPlayerLifeChanged?.Invoke(defendingPlayerIndex, defendingPlayer.Life);
            }
        }
        if (Attackers != null && Attackers.Count != 0)
        {
            Server.OnTimerChanged?.Invoke(10);
            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        Server.OnCombatStepEnded?.Invoke(CombatStep.CombatDamage);
    }

    async Task EndOfCombat()
    {
        Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - End of Combat");

        Server.MatchState.CurrentCombatStep = CombatStep.EndCombat;
        Server.OnCombatStepStarted?.Invoke(CombatStep.EndCombat);
        bool _player1Skipped = false;
        bool _player2Skipped = false;

        // Put dead creatures from combat in graveyard
        int attackingPlayer = Server.MatchState.CurrentPlayerIndex;
        int defendingPlayer = (Server.MatchState.CurrentPlayerIndex + 1) % Server.MatchState.PlayerStates.Count;
        foreach (Card attacker in Attackers)
        {
            if (attacker.Resistance == 0)
                Server.ZonesController.MoveCard(attacker, attackingPlayer, ZoneType.Creature, ZoneType.Graveyard);
        }

        foreach (BlockData blockData in Blockers)
        {
            foreach (Card blocker in blockData.Blockers)
            {
                if(blocker.Resistance == 0)
                    Server.ZonesController.MoveCard(blocker, defendingPlayer, ZoneType.Creature, ZoneType.Graveyard);
            }
        }

        TaskCompletionSource<bool> skipped = new TaskCompletionSource<bool>();
        float stepTimeout = 10f;

        Server.OnPlayerSkipClicked += OnPlayerSkip;
        Server.OnTimerChanged?.Invoke(stepTimeout);
        Task timeout = Task.Delay(TimeSpan.FromSeconds(stepTimeout));
        Task finished = await Task.WhenAny(skipped.Task, timeout);
        Server.OnPlayerSkipClicked -= OnPlayerSkip;

        if (finished == timeout)
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat End End: timeout");
        else
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Combat End End: players are ready");

        Server.MatchState.CurrentCombatStep = CombatStep.None;
        Server.OnCombatStepEnded?.Invoke(CombatStep.EndCombat);

        void OnPlayerSkip(int playerIndex)
        {
            Debug.Log($"<color='red'>Server:</color> Turn Controller - Combat Phase - Player {playerIndex} skipped combat end");

            if (playerIndex == 0)
                _player1Skipped = true;
            else if (playerIndex == 1)
                _player2Skipped = true;

            if (_player1Skipped && _player2Skipped)
                skipped.SetResult(true);
        }
    }
}
