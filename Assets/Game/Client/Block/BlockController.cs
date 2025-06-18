using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockController
{
    [field: Header("Dependencies")]
    [field: SerializeField] public BlockView BlockView { get; private set; }

    [field: Header("Core")]
    [field: SerializeField] public CardView CurrentBlockingCreature { get; private set; }
    [field: SerializeField] public List<BlockData> Blockers { get; private set; }

    public void ClearBlockers()
    {
        Blockers = new List<BlockData>();
    }

    public void ClearView()
    {
        BlockView.ClearAllArrows();
    }

    public void SetBlockers(List<BlockData> blockers)
    {
        Blockers = blockers;
        BlockView.UpdateBlockersView(Blockers);
    }

    public void HandleCardClick(MatchServerController server, int playerClickedIndex, CardView cardView)
    {
        bool isDeclareBlockersStep = server.MatchState.CurrentPhase == GamePhase.Combat && server.MatchState.CurrentCombatStep == CombatStep.DeclareBlockers;
        bool isMyTurn = playerClickedIndex == server.MatchState.CurrentPlayerIndex;
        List<Card> attackers = server.TurnController.GetAttackers();

        if (attackers == null || attackers.Count == 0)
            return;

        if (!isMyTurn && isDeclareBlockersStep)
        {
            // Clicked in Blocker
            if (server.MatchState.PlayerStates[playerClickedIndex].CreatureZone.Contains(cardView.Card) &&
                server.CardController.CanBlock(cardView.Card))
            {
                BlockData foundBlockData = Blockers.Find(x => x.Blockers.Contains(cardView.Card));
                if (foundBlockData != null)
                {
                    foundBlockData.Blockers.Remove(cardView.Card);
                    if (foundBlockData.Blockers.Count == 0)
                        Blockers.Remove(foundBlockData);

                    BlockView.UpdateBlockersView(Blockers);
                    CurrentBlockingCreature = null;
                }
                else
                {
                    CurrentBlockingCreature = cardView;
                }
            }
            // Clicked in Attacker
            int attackingPlayerIndex = server.MatchState.CurrentPlayerIndex;
            if (server.MatchState.PlayerStates[attackingPlayerIndex].CreatureZone.Contains(cardView.Card) && attackers.Contains(cardView.Card))
            {
                if (CurrentBlockingCreature != null)
                {
                    BlockData foundBlockData = Blockers.Find(x => x.Attacker == cardView.Card);

                    if (foundBlockData != null)
                    {
                        if (!foundBlockData.Blockers.Contains(CurrentBlockingCreature.Card))
                        {
                            foundBlockData.Blockers.Add(CurrentBlockingCreature.Card);
                        }
                    }
                    else
                    {
                        List<Card> blockers = new List<Card>();
                        blockers.Add(CurrentBlockingCreature.Card);
                        BlockData blockData = new BlockData(blockers, cardView.Card);
                        Blockers.Add(blockData);
                        CurrentBlockingCreature = null;
                    }
                    BlockView.UpdateBlockersView(Blockers);
                }
            }

        }
    }
}
