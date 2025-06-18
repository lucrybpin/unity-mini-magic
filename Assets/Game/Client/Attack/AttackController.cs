
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttackController
{

    [field: SerializeField] public List<Card> Attackers { get; private set; }

    public void ClearAttackers()
    {
        Attackers = new List<Card>();
    }

    public void HandleCardClick(MatchServerController server, int playerClickedIndex, CardView cardView)
    {
        bool isMyTurn = playerClickedIndex == server.MatchState.CurrentPlayerIndex;
        bool isDeclareAttackersStep = server.MatchState.CurrentPhase == GamePhase.Combat &&
          server.MatchState.CurrentCombatStep == CombatStep.DeclareAttackers;

        if (isMyTurn && isDeclareAttackersStep && !Attackers.Contains(cardView.Card))
        {
            if (server.CardController.CanAttack(cardView.Card))
            {
                Attackers.Add(cardView.Card);
                cardView.Hover(0.34f);
            }
        }
        else if (isDeclareAttackersStep && Attackers.Contains(cardView.Card))
        {
            Attackers.Remove(cardView.Card);
            cardView.ReturnCardToOriginalPosition();
        }
    }
}
