using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class AIController
{
    [field: SerializeField, NonSerialized] public MatchServerController Server { get; private set; }

    public AIController(MatchServerController server)
    {
        Server = server;
        server.OnPhaseStarted += OnPhaseStarted;
    }

    private void OnPhaseStarted(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Beginning:
                _ = ExecuteBeginningPhase();
                break;
            case GamePhase.MainPhase1:
                _ = ExecuteMainPhase();
                break;
            case GamePhase.Combat:
                _ = ExecuteCombatPhase();
                break;
            case GamePhase.MainPhase2:
                _ = ExecuteMainPhase();
                break;

        }
    }

    async Task ExecuteBeginningPhase()
    {
        Debug.Log($"<color='blue'>AI:</color> Execute Beginning Phase");

        await Task.Delay(1000); // Wait until upkeep phase starts
        Debug.Log($"<color='blue'>AI:</color> Skip Clicked in Beginning Phase");
        Server.OnPlayerSkipClicked?.Invoke(1);
    }

    async Task ExecuteMainPhase()
    {
        Debug.Log($"<color='blue'>AI:</color> Execute Main Phase");

        PlayerState me              = Server.MatchState.PlayerStates[1];
        bool isMyTurn               = Server.MatchState.CurrentPlayerIndex == me.PlayerId;
        List<Card> playableCards    = new List<Card>();

        // Bot Turn
        if (isMyTurn)
        {
            // First, try to play a resource if we haven't played one this turn
            if (me.ResourcesPlayedThisTurn == 0)
            {
                foreach (Card card in me.Hand)
                {
                    if (card.Type == CardType.Resource)
                    {
                        playableCards.Add(card);
                        break;
                    }
                }
            }

            // Then look for creatures to play
            foreach (Card card in me.Hand)
            {
                if (card.Type == CardType.Creature)
                {
                    playableCards.Add(card);
                }
            }

            // Try to play cards in order
            foreach (Card card in playableCards)
            {
                ExecutionResult canCastCard = await Server.CardController.CanPlayCard(me.PlayerId, card);
                if (!canCastCard.Success) continue;

                ExecutionResult result = await Server.CastCard(1, card);
                if (result.Success)
                {
                    Debug.Log($"<color='blue'>AI:</color> Successfully played {card.Name}");
                    await Task.Delay(1000); // Give time for animations
                }
            }
        }

        // End main phase
        await Task.Delay(TimeSpan.FromSeconds(1));
        Debug.Log($"<color='blue'>AI:</color> Skip Clicked in Main Phase");
        Server.OnPlayerSkipClicked?.Invoke(1);
    }

    async Task ExecuteCombatPhase()
    {
        Debug.Log($"<color='blue'>AI:</color> Execute Combat Phase");


        PlayerState opponent = Server.MatchState.PlayerStates[0];
        PlayerState me = Server.MatchState.PlayerStates[1];
        bool isMyTurn = Server.MatchState.CurrentPlayerIndex == me.PlayerId;

        // Begin Combat
        await WaitForCombatStep(CombatStep.BeginCombat);
        Debug.Log($"<color='blue'>AI:</color> Skip Clicked in Begin Combat Step");
        Server.OnPlayerSkipClicked?.Invoke(1);

        // Declare Attackers
        await WaitForCombatStep(CombatStep.DeclareAttackers);

        if (isMyTurn)
        {
            List<Card> attackers = new List<Card>();
            foreach (Card card in me.CreatureZone)
            {
                if(Server.CardController.CanAttack(card))
                    attackers.Add(card);
            }

            Server.SetAttackers(attackers);
        }

        Debug.Log($"<color='blue'>AI:</color> Skip Clicked in Declare Attackers Step");
        Server.OnPlayerSkipClicked?.Invoke(1);

        // Declare Blockers
        await WaitForCombatStep(CombatStep.DeclareBlockers);
        Debug.Log($"<color='blue'>AI:</color> Skip Clicked in Declare Blockers Step");
        if (!isMyTurn)
        {

            List<Card> Attackers = Server.TurnController.CombatPhase.Attackers;
            if (Attackers == null || Attackers.Count == 0)
            {
                Server.OnPlayerSkipClicked?.Invoke(1);
            }

            List<BlockData> blockers = new List<BlockData>();
            List<Card> occupiedCreatures = new List<Card>();

            // Foreach attacking creatures
            foreach (Card attacker in Attackers)
            {
                int accumulatedAttack = attacker.Attack;
                int accumulatedDefense = 0;

                // Foreach creature in my side, tap until threath is 0
                foreach (Card card in me.CreatureZone)
                {
                    if (accumulatedAttack <= accumulatedDefense)
                        break;

                    if (Server.CardController.CanBlock(card) && !occupiedCreatures.Contains(card))
                    {
                        BlockData foundBlockData = blockers.Find(x => x.Attacker == attacker);
                        if (foundBlockData == null)
                        {
                            // First blocker
                            List<Card> defenders    = new List<Card> { card };
                            BlockData blockData     = new BlockData(defenders, attacker);
                            blockers.Add(blockData);
                        }
                        else
                        {
                            // Not first blocker to this creature
                            foundBlockData.Blockers.Add(card);
                        }
                        occupiedCreatures.Add(card);
                        accumulatedDefense += card.Resistance;
                    }
                }
            }
            Server.SetBlockers(blockers);
            
            Server.OnPlayerSkipClicked?.Invoke(1);
        }

        // End Combat
        await WaitForCombatStep(CombatStep.EndCombat);
        Debug.Log($"<color='blue'>AI:</color> Skip Clicked in End Combat Step");
        Server.OnPlayerSkipClicked?.Invoke(1);
    }

    async Task WaitForCombatStep(CombatStep combatStep)
    {
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
        } while (Server.MatchState.CurrentCombatStep != combatStep);
    }

} 