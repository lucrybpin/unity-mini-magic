using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public struct ExecutionResult
{
  public bool Success;
  public string Message;
}

[Serializable]
public class MatchServerController
{
  [field: SerializeField] public MatchState MatchState { get; private set; }
  [field: SerializeField] public bool IsGameActive { get; private set; }

  [field: Header("Controllers")]
  [field: SerializeField] public TurnController TurnController { get; private set; }
  [field: SerializeField] public CardController CardController { get; private set; }
  [field: SerializeField] public ZonesController ZonesController { get; private set; }
  [field: SerializeField] public AIController AIController { get; private set; }
  // [field: SerializeField] public CombatController CombatController { get; private set; }

  public Action<GamePhase> OnPhaseStarted;
  public Action<GamePhase> OnPhaseEnded;
  public Action<CombatStep> OnCombatStepStarted;
  public Action<CombatStep> OnCombatStepEnded;
  public Action<int> OnPlayerSkipClicked;

  public Action<int, Card> OnPlayerDrawCard;
  public Action<int, Card> OnPlayerCastCard;

  public Task<bool> PrepareNewMatch(List<CardData> CardListPlayer1, List<CardData> CardListPlayer2)
  {
    Debug.Log($"<color='red'>Server:</color> Preparing New Match");

    TurnController = new TurnController(this);
    CardController = new CardController(this);
    ZonesController = new ZonesController(this);
    AIController = new AIController(this);

    MatchState = new MatchState();
    MatchState.TurnNumber = 1;
    MatchState.CurrentPlayerIndex = 1;//UnityEngine.Random.Range(0, 2);
    MatchState.CurrentPhase = GamePhase.Preparing;
    MatchState.CurrentCombatStep = CombatStep.None;

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

    for (int i = 0; i < 7; i++)
    {
      DrawCard(1);
    }

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
    Debug.Log($"<color='red'>Server:</color> Starting Match");
    await TurnController.StartTurn();
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

    OnPlayerDrawCard?.Invoke(playerIndex, drawnCard);

    return Task.FromResult(drawnCard);
  }

  public async Task<ExecutionResult> CastCard(int playerIndex, Card card)
  {
    Debug.Log($"<color='red'>Server:</color> Player {playerIndex} trying to cast card {card.Name}");

    PlayerState playerState = MatchState.PlayerStates[playerIndex];

    // Can cast
    ExecutionResult result = await CardController.CanPlayCard(playerIndex, card);
    if (!result.Success)
    {
      Debug.Log($"<color='red'>Server:</color> {result.Message}");
      return new ExecutionResult() { Success = false, Message = result.Message};
    }

    // Process
    await CardController.ProcessCardPlay(playerIndex, card);

    Debug.Log($"<color='red'>Server:</color> {result.Message}");

    OnPlayerCastCard?.Invoke(playerIndex, card);

    return new ExecutionResult() { Success = true, Message = "Success" };
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

  public void SetAttackers(List<Card> cards)
  {
    TurnController.SetAttackers(cards);
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
