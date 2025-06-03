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
  [field: SerializeField] public int PlayerIndex { get; private set; }
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
  [field: SerializeField] public CreaturesView CreaturesView { get; private set; }
  [field: SerializeField] public ResourcesView ResourcesView { get; private set; }

  [field: SerializeField] public List<Card> Attackers { get; private set; }

  async void Start()
  {
    //Setup
    ClientState             = MatchClientControllerState.Idle;
    DrawEnabled             = false;
    Server.OnPhaseStarted   += OnPhaseStarted;
    Server.OnPhaseEnded     += OnPhaseEnded;
    Server.OnCombatStepStarted += OnCombatStepStart;
    Server.OnCombatStepEnded += OnCombatStepEnded;
    bool result             = await Server.PrepareNewMatch(CardListPlayer1, CardListPlayer2);
    DeckView.OnDeckClick    += OnDeckClick;
    HandView.OnCardOverCastRegion += OnCardOverCastRegion;
    HandView.OnCardClicked += OnCardClicked;
    UIController.OnButtonSkipClicked += OnButtonSkipClick;

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
    Server.OnCombatStepStarted -= OnCombatStepStart;
    Server.OnCombatStepEnded -= OnCombatStepEnded;
    DeckView.OnDeckClick    -= OnDeckClick;
    HandView.OnCardOverCastRegion -= OnCardOverCastRegion;
    HandView.OnCardClicked -= OnCardClicked;
    UIController.OnButtonSkipClicked -= OnButtonSkipClick;
  }

  // Server Events
  void OnPhaseStarted(GamePhase phase)
  {
    Debug.Log($"<color='green'>Client:</color> Phase {phase} started");

    switch (phase)
    {
      case GamePhase.Beginning:
        UIController.SetButtonSkipVisibility(true, "Pass Upkeep");
        UIController.SetButtonSkipOpponentVisibility(true, "Pass Upkeep");
        break;
      case GamePhase.MainPhase1:
        UIController.SetButtonSkipVisibility(true, "End Main Phase 1");
        UIController.SetButtonSkipOpponentVisibility(true, "End Main Phase 1");
        break;
      case GamePhase.Combat:
        // Handled in OnCombatStepStart
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
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
      case GamePhase.MainPhase1:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
      case GamePhase.Combat:
        // Handled in OnCombatStepEnded
        break;
      case GamePhase.MainPhase2:
        break;
      case GamePhase.EndPhase:
        break;
    }
  }

  void OnCombatStepStart(CombatStep combatStep)
  {
    Debug.Log($"<color='green'>Client:</color> Combat Step {combatStep} started");

    switch (combatStep)
    {
      case CombatStep.BeginCombat:
        UIController.SetButtonSkipVisibility(true, "Skip Combat Beginning");
        UIController.SetButtonSkipOpponentVisibility(true, "Skip Combat Beginning");
        break;
      case CombatStep.DeclareAttackers:
        Attackers = new List<Card>();
        UIController.SetButtonSkipVisibility(true, "Proceed");
        // UIController.SetButtonSkipOpponentVisibility(true, "Skip Combat Beginning");
        break;
      case CombatStep.DeclareBlockers:
        break;
      case CombatStep.CombatDamage:
        break;
      case CombatStep.EndCombat:
        break;
    }
  }

  void OnCombatStepEnded(CombatStep combatStep)
  {
    Debug.Log($"<color='green'>Client:</color> Combat Step {combatStep} started");

    switch (combatStep)
    {
      case CombatStep.BeginCombat:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
      case CombatStep.DeclareAttackers:
        Server.SetAttackers(Attackers);
        break;
      case CombatStep.DeclareBlockers:
        break;
      case CombatStep.CombatDamage:
        break;
      case CombatStep.EndCombat:
        break;
    }
  }

  // Client Events
  void OnDeckClick()
  {
    if (DrawEnabled && ClientState != MatchClientControllerState.DrawingCard)
      _ = OnDeckClickTask();
  }

  void OnCardClicked(CardView cardView)
  {
    _ = OnCardClickedTask(cardView);
  }

  void OnCardOverCastRegion(CardView cardView)
  {
    if(cardView != null)
      _ = OnCardOverCastRegionTask(cardView);
  }

  void OnButtonSkipClick(int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Button End Phase/Step Clicked by Player {playerIndex}");
    Server.OnPlayerSkipClicked(playerIndex);
  }

  // Support Methods
  async Task OnDeckClickTask()
  {
    Debug.Log($"<color='green'>Client:</color> Deck Clicked");

    ClientState       = MatchClientControllerState.DrawingCard;
    Card card         = await Server.DrawCard(0);
    CardView newCard  = CardViewCreator.CreateCardView(card, DeckView.transform.position, DeckView.transform.rotation);
    await HandView.AddCard(newCard);
    ClientState       = MatchClientControllerState.Idle;
  }

  async Task OnCardClickedTask(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> Card Clicked {cardView.Name} - {cardView.Card.InstanceID}");

    if (Server.MatchState.CurrentPhase == GamePhase.Combat &&
       Server.MatchState.CurrentCombatStep == CombatStep.DeclareAttackers)
    {
      Attackers.Add(cardView.Card);
    }
    await Task.Delay(1);
  }

  async Task OnCardOverCastRegionTask(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> Trying to cast {cardView.Card.Name}");

    ExecutionResult result = await Server.CastCard(PlayerIndex, cardView.Card);
    if (!result.Success)
    {
      Debug.Log($"<color='green'>Client:</color> card casted failed. {result.Message}");
      _ = UIController.ShowMessage(result.Message);
      HandView.ResolveCast(false);
      return;
    }

    switch (cardView.Card.Type)
    {
      case CardType.Resource:
        await ProcessResource(cardView);
        break;

      case CardType.Creature:
        await ProcessCreature(cardView);
        break;

      case CardType.Enchantment:
        await ProcessEnchantment(cardView);
        break;

      case CardType.Sorcery:
        await ProcessSorcery(cardView);
        break;

      case CardType.Instant:
        await ProcessInstant(cardView);
        break;
    }

    HandView.ResolveCast(result.Success);

    Debug.Log($"<color='green'>Client:</color> card casted successfully");
  }

  async Task WaitForPlayer1DrawCards(int amountOfCards)
  {
    int expectedHandSize = Server.MatchState.PlayerStates[0].Hand.Count + amountOfCards;
    while (Server.MatchState.PlayerStates[0].Hand.Count < expectedHandSize)
    {
      await Task.Delay(TimeSpan.FromSeconds(.25f));
    }
  }

  async Task<bool> ProcessResource(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> Processing Resource");

    await HandView.RemoveCard(cardView);
    await ResourcesView.AddCard(cardView);
    return true;
  }

  async Task<bool> ProcessCreature(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Creature");
    await ResourcesView.SyncStatesWithServer(Server.MatchState.PlayerStates[PlayerIndex].ResourceZone);
    await HandView.RemoveCard(cardView);
    await CreaturesView.AddCard(cardView);
    return true;
  }

  Task<bool> ProcessEnchantment(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Enchantment");
    return Task.FromResult(true);
  }

  Task<bool> ProcessInstant(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Instant");
    return Task.FromResult(true);
  }

  Task<bool> ProcessSorcery(CardView cardView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Sorcery");
    return Task.FromResult(true);
  }
}
