using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MatchServerController
{
  [field: SerializeField] public MatchState MatchState { get; private set; }
  [field: SerializeField] public bool IsGameActive { get; private set; }

  [field: Header("Controllers")]
  [field: SerializeField] public TurnController TurnController { get; private set; }
  // [field: SerializeField] public CombatController CombatController { get; private set; }
  // [field: SerializeField] public CardController CardController { get; private set; }
  // [field: SerializeField] public ZoneController ZoneController { get; private set; }

  public Action<GamePhase> OnPhaseStarted;
  public Action<GamePhase> OnPhaseEnded;
  public Action<int> OnPlayerPassedUpkeep;

  public Task<bool> PrepareNewMatch(List<CardData> CardListPlayer1, List<CardData> CardListPlayer2)
  {
    Debug.Log($"<color='red'>Server:</color> Preparing New Match");

    MatchState = new MatchState();
    MatchState.TurnNumber = 1;
    MatchState.CurrentPlayerIndex = 0;
    MatchState.CurrentPhase = GamePhase.Beginning;

    // Player 1 Deck
    PlayerState Player1State = new PlayerState();
    Player1State.PlayerId = 0;
    Player1State.Life = 20;

    foreach (CardData cardData in CardListPlayer1)
      Player1State.Deck.Add(new Card(cardData));

    ShuffleDeck(Player1State.Deck);

    // Player 2 Deck
    PlayerState Player2State = new PlayerState();
    Player2State.PlayerId = 1;
    Player2State.Life = 20;

    foreach (CardData cardData in CardListPlayer2)
      Player2State.Deck.Add(new Card(cardData));

    ShuffleDeck(Player2State.Deck);

    MatchState.PlayerStates.Add(Player1State);
    MatchState.PlayerStates.Add(Player2State);

    Debug.Log($"<color='red'>Server:</color> Waiting for clients draw their hand");

    // // Load
    // string path = Path.Combine(Application.persistentDataPath, "matchstate.txt");
    // string json = File.ReadAllText(path);
    // MatchState = JsonUtility.FromJson<MatchState>(json);

    // // Save
    // string matchStateString = SerializeMatchState();
    // path = Path.Combine(Application.persistentDataPath, "matchstate.txt");
    // File.WriteAllText(path, matchStateString);
    // Debug.Log($">>>> State Saved in: {path}");

    return Task.FromResult(true);
  }

  public async void StartMatch()
  {
    TurnController = new TurnController(this);
    Debug.Log($"<color='red'>Server:</color> Starting Match");
    
    await TurnController.StartTurn();
  }

  public void ShuffleDeck(List<Card> deck)
  {
    Debug.Log($"<color='red'>Server:</color> Shuffling Deck");
    // I know that Fisher-Yates in-place is more performatic, but this is
    // intuitive for me to understand. And I am the maintainer of this code

    List<Card> tempDeck = new List<Card>(deck);
    System.Random random = new System.Random();
    deck.Clear();

    while (tempDeck.Count > 0)
    {
      int randomIndex = random.Next(tempDeck.Count);
      deck.Add(tempDeck[randomIndex]);
      tempDeck.RemoveAt(randomIndex);
    }
  }

  public Task<Card> DrawCard(int playerIndex)
  {
    Debug.Log($"<color='red'>Server:</color> Player {playerIndex} drawing card");

    PlayerState playerState = MatchState.PlayerStates[playerIndex];
    if (playerState.Deck.Count == 0)
    {
      return Task.FromResult<Card>(null);
    }

    Card drawnCard = playerState.Deck[0];
    playerState.Deck.RemoveAt(0);
    playerState.Hand.Add(drawnCard);
    drawnCard.IsInHand = true;

    return Task.FromResult(drawnCard);
  }

  public string SaveState()
  {
    Debug.Log($"<color='red'>Server:</color> Saving Match State");

    return JsonUtility.ToJson(MatchState, true);
  }

  public Task<bool> LoadState(string jsonState)
  {
    Debug.Log($"<color='red'>Server:</color> Loading Match State...");

    try
    {
      MatchState = JsonUtility.FromJson<MatchState>(jsonState);
      Debug.Log($"<color='red'>Server:</color> Success Loading Match State");
      return Task.FromResult(true);
    }
    catch (System.Exception ex)
    {
      Debug.Log($"<color='red'>Server:</color> Error Loading Match State: {ex.Message}");
      return Task.FromResult(false);
    }
  }
}
