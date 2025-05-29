using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum MatchClientControllerState
{
  Idle,
  DrawingCard,
}

public class MatchClientController : MonoBehaviour
{
  [field: Header("Setup")]
  [field: SerializeField] public List<CardData> CardListPlayer1 { get; private set; }
  [field: SerializeField] public List<CardData> CardListPlayer2 { get; private set; }

  [field: Header("Core Components and SubControllers")]
  [field: SerializeField] public MatchServerController Server { get; private set; }
  [field: SerializeField] public MatchClientControllerState ClientState { get; private set; }
  [field: SerializeField] public bool DrawEnabled { get; private set; }

  [field: Header("External Controllers and Dependencies")]
  [field: SerializeField] public UIController UIController { get; private set; }
  [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }
  [field: SerializeField] public Transform DeckTransform { get; private set; }

  [field: Header("View Elements")]
  [field: SerializeField] public DeckView DeckView { get; private set; }
  [field: SerializeField] public HandView HandView { get; private set; }

  async void Start()
  {
    //Setup
    ClientState             = MatchClientControllerState.Idle;
    DrawEnabled             = false;
    Server.OnPhaseStarted   += OnPhaseStarted;
    Server.OnPhaseEnded     += OnPhaseEnded;
    bool result             = await Server.PrepareNewMatch(CardListPlayer1, CardListPlayer2);
    DeckView.OnDeckClick    += OnDeckClick;
    UIController.OnButtonPassUpkeepClick += OnButtonPassUpkeepClick;

    // Draw Starting Hand
    HandView.Resume();
    await UIController.ShowMessage("Draw 7 Cards");
    DrawEnabled = true;
    await WaitForPlayer1DrawCards(7);
    DrawEnabled = false;

    // Start Game
    Server.StartMatch();
  }

  void OnDestroy()
  {
    Server.OnPhaseStarted   -= OnPhaseStarted;
    Server.OnPhaseEnded     -= OnPhaseEnded;
    DeckView.OnDeckClick    -= OnDeckClick;
    UIController.OnButtonPassUpkeepClick -= OnButtonPassUpkeepClick;
  }

  // Server Events
  void OnPhaseStarted(GamePhase phase)
  {
    Debug.Log($"<color='green'>Client:</color> Phase {phase} started");

    switch (phase)
    {
      case GamePhase.Beginning:
        UIController.SetButtonPassUpkeepVisibility(true);
        break;
      case GamePhase.MainPhase1:
        break;
      case GamePhase.Combat:
        break;
      case GamePhase.MainPhase2:
        break;
      case GamePhase.EndPhase:
        break;
    }
  }

  void OnPhaseEnded(GamePhase phase)
  {
    Debug.Log($"<color='green'>Client:</color> Phase {phase} ended");

    switch (phase)
    {
      case GamePhase.Beginning:
        UIController.SetButtonPassUpkeepVisibility(false);
        break;
      case GamePhase.MainPhase1:
        break;
      case GamePhase.Combat:
        break;
      case GamePhase.MainPhase2:
        break;
      case GamePhase.EndPhase:
        break;
    }
  }

  // Client Events
  void OnDeckClick()
  {
    if (DrawEnabled && ClientState != MatchClientControllerState.DrawingCard)
      _ = OnDeckClickTask();
  }

  private void OnButtonPassUpkeepClick(int playerId)
  {
    Debug.Log($"<color='green'>Client:</color> Button Pass Upkeep Clicked by Player {playerId}");
    Server.OnPlayerPassedUpkeep(playerId);
  }

  // Support Methods
  async Task OnDeckClickTask()
  {
    Debug.Log($"<color='green'>Client:</color> Deck Clicked");

    ClientState = MatchClientControllerState.DrawingCard;
    Card card = await Server.DrawCard(0);
    CardView newCard = CardViewCreator.CreateCardView(card, DeckView.transform.position, DeckView.transform.rotation);
    await HandView.AddCard(newCard);
    ClientState = MatchClientControllerState.Idle;
  }

  async Task WaitForPlayer1DrawCards(int amountOfCards)
  {
    int expectedHandSize = Server.MatchState.PlayerStates[0].Hand.Count + amountOfCards;
    while (Server.MatchState.PlayerStates[0].Hand.Count < expectedHandSize)
    {
      await Task.Delay(TimeSpan.FromSeconds(.25f));
    }
  }
}
