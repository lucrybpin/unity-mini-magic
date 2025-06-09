using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum PlayerViewState
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
  [field: SerializeField] public PlayerViewState ClientState { get; private set; }
  [field: SerializeField] public bool DrawEnabled { get; private set; }

  [field: Header("External Controllers and Dependencies")]
  [field: SerializeField] public UIController UIController { get; private set; }
  [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }

  [field: Header("Player View Elements")]
  [field: SerializeField] public PlayerView PlayerView { get; private set; }

  [field: Header("Opponent View Elements")]
  [field: SerializeField] public PlayerView OpponentView { get; private set; }

  [field: SerializeField] public List<Card> Attackers { get; private set; }

  async void Start()
  {
    //Setup
    ClientState = PlayerViewState.Idle;
    DrawEnabled = false;

    Server.OnPhaseStarted += OnPhaseStarted;
    Server.OnPhaseEnded += OnPhaseEnded;
    Server.OnCombatStepStarted += OnCombatStepStart;
    Server.OnCombatStepEnded += OnCombatStepEnded;
    bool result = await Server.PrepareNewMatch(CardListPlayer1, CardListPlayer2);
    PlayerView.OnDeckClicked += OnDeckClick;
    PlayerView.OnCardCastRequested += OnCardCastRequested;
    PlayerView.OnCardClicked += OnCardClicked;
    UIController.OnButtonSkipClicked += OnButtonSkipClick;

    // Draw Starting Hand
    PlayerView.ResumeHand();
    await UIController.ShowMessage("Draw 7 Cards");
    _ = OpponentDrawHand();
    PlayerView.ViewState = PlayerViewState.Idle;
    PlayerView.DrawEnabled = true;
    DrawEnabled = true;
    await WaitForPlayer1DrawCards(7);
    DrawEnabled = false;

    // Start Game
    Server.StartMatch();
  }

  void OnDestroy()
  {
    Server.OnPhaseStarted -= OnPhaseStarted;
    Server.OnPhaseEnded -= OnPhaseEnded;
    Server.OnCombatStepStarted -= OnCombatStepStart;
    Server.OnCombatStepEnded -= OnCombatStepEnded;

    PlayerView.OnDeckClicked -= OnDeckClick;
    PlayerView.OnCardCastRequested -= OnCardCastRequested;
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
        UIController.SetButtonSkipVisibility(true, "End Main Phase 2");
        UIController.SetButtonSkipOpponentVisibility(true, "End Main Phase 2");
        break;
      case GamePhase.EndPhase:
        UIController.SetButtonSkipVisibility(true, "Pass End");
        UIController.SetButtonSkipOpponentVisibility(true, "Pass End");
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
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
      case GamePhase.EndPhase:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
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
        if (Server.MatchState.CurrentPlayerIndex == PlayerIndex)
        {
          Attackers = new List<Card>();
          UIController.SetButtonSkipVisibility(true, "Proceed");
          // UIController.SetButtonSkipOpponentVisibility(true, "Skip Combat Beginning");
        }
        break;
      case CombatStep.DeclareBlockers:
        UIController.SetButtonSkipOpponentVisibility(true, "Skip Combat Blockers");
        break;
      case CombatStep.CombatDamage:
        break;
      case CombatStep.EndCombat:
        UIController.SetButtonSkipVisibility(true, "Skip Combat End");
        UIController.SetButtonSkipOpponentVisibility(true, "Skip Combat End");
        break;
    }
  }

  void OnCombatStepEnded(CombatStep combatStep)
  {
    Debug.Log($"<color='green'>Client:</color> Combat Step {combatStep} ended");

    int playerIndex = Server.MatchState.CurrentPlayerIndex;

    switch (combatStep)
    {
      case CombatStep.BeginCombat:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
      case CombatStep.DeclareAttackers:
        UIController.SetButtonSkipVisibility(false);
        Server.SetAttackers(Attackers);
        // Tap Attacking Creatures
        PlayerView attackingPlayer = playerIndex == 0 ? PlayerView : OpponentView;
        attackingPlayer.TapAttackers(Attackers);
        break;
      case CombatStep.DeclareBlockers:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
      case CombatStep.CombatDamage:
        break;
      case CombatStep.EndCombat:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        break;
    }
  }

  // Client Events
  void OnDeckClick(int playerIndex)
  {
    _ = OnDeckClickTask(playerIndex);
  }

  void OnCardClicked(CardView cardView, int playerIndex)
  {
    _ = OnCardClickedTask(cardView, playerIndex);
  }

  void OnCardCastRequested(CardView cardView, int playerIndex)
  {
    if (cardView != null)
      _ = OnCardCastRequestedTask(cardView, playerIndex);
  }

  void OnButtonSkipClick(int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Button End Phase/Step Clicked by Player {playerIndex}");
    Server.OnPlayerSkipClicked(playerIndex);
  }

  // Support Methods
  async Task OnDeckClickTask(int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Deck Clicked");

    if (Server.MatchState.CurrentPlayerIndex != playerIndex)
      return;

    PlayerView playerView = (playerIndex == 0) ? PlayerView : OpponentView;
    Card card             = await Server.DrawCard(playerIndex);
    CardView newCard      = CardViewCreator.CreateCardView(card, playerIndex);
    await playerView.DrawCard(newCard); ;
  }

  async Task OnCardClickedTask(CardView cardView, int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Card Clicked {cardView.Card.Name} - {cardView.Card.InstanceID}");

    // Ellect Attackers during Declare Attackers Step
    bool isAttackingPlayerCall = playerIndex == Server.MatchState.CurrentPlayerIndex;
    bool isDeclareAttackersStep = Server.MatchState.CurrentPhase == GamePhase.Combat &&
      Server.MatchState.CurrentCombatStep == CombatStep.DeclareAttackers;
    if (isAttackingPlayerCall && isDeclareAttackersStep && !Attackers.Contains(cardView.Card))
    {
      Attackers.Add(cardView.Card);
      cardView.Hover(0.34f);
    }
    else if (isDeclareAttackersStep && Attackers.Contains(cardView.Card))
    {
      Attackers.Remove(cardView.Card);
      cardView.ReturnCardToOriginalPosition();
    }

    await Task.Delay(1);
  }

  async Task OnCardCastRequestedTask(CardView cardView, int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Trying to cast {cardView.Card.Name}");

    PlayerView playerView = (playerIndex == 0) ? PlayerView : OpponentView;
    ExecutionResult result = await Server.CastCard(playerIndex, cardView.Card);
    if (!result.Success)
    {
      Debug.Log($"<color='green'>Client:</color> card casted failed. {result.Message}");
      _ = UIController.ShowMessage(result.Message);
      playerView.ResolveCast(false);
      return;
    }

    switch (cardView.Card.Type)
    {
      case CardType.Resource:
        await ProcessResource(cardView, playerView);
        break;

      case CardType.Creature:
        await ProcessCreature(cardView, playerView);
        break;

      case CardType.Enchantment:
        await ProcessEnchantment(cardView, playerView);
        break;

      case CardType.Sorcery:
        await ProcessSorcery(cardView, playerView);
        break;

      case CardType.Instant:
        await ProcessInstant(cardView, playerView);
        break;
    }

    playerView.ResolveCast(result.Success);

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

  async Task<bool> ProcessResource(CardView cardView, PlayerView playerView)
  {
    Debug.Log($"<color='green'>Client:</color> Processing Resource");

    // Logic

    // View
    await playerView.ProcessResource(cardView);

    return true;
  }

  async Task<bool> ProcessCreature(CardView cardView, PlayerView playerView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Creature");

    // Logic

    // View
    List<Card> playerResourcesZone = Server.MatchState.PlayerStates[playerView.PlayerIndex].ResourceZone;
    await playerView.ProcessCreature(cardView, playerResourcesZone);

    return true;
  }

  Task<bool> ProcessEnchantment(CardView cardView, PlayerView playerView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Enchantment");
    // Logic
    // View
    return Task.FromResult(true);
  }

  Task<bool> ProcessInstant(CardView cardView, PlayerView playerView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Instant");
    // Logic
    // View
    return Task.FromResult(true);
  }

  Task<bool> ProcessSorcery(CardView cardView, PlayerView playerView)
  {
    Debug.Log($"<color='green'>Client:</color> - Processing Sorcery");
    // Logic
    // View
    return Task.FromResult(true);
  }

  async Task OpponentDrawHand()
  {
    PlayerState opponentState = Server.MatchState.PlayerStates[1];
    for (int i = 0; i < opponentState.Hand.Count; i++)
    {
      Card card = opponentState.Hand[i];
      CardView newCard = CardViewCreator.CreateCardView(card, 1);
      await OpponentView.DrawCard(newCard);
      await Task.Delay(TimeSpan.FromSeconds(0.2f));
    }
  }

}
