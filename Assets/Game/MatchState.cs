using System;
using System.Collections.Generic;

[Serializable]
public class MatchState
{
    public int CurrentPlayerIndex;
    public GamePhase CurrentPhase;
    public CombatStep CurrentCombatStep;
    public int TurnNumber;

    public List<PlayerState> PlayerStates;

    public MatchState()
    {
        PlayerStates = new List<PlayerState>();
    }
}
