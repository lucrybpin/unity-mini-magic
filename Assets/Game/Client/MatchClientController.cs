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
  [field: SerializeField] public MatchServerController Server { get; private set; }
  [field: SerializeField] public PlayerViewState ClientState { get; private set; }
  [field: SerializeField] public BlockController BlockController { get; private set; }
  [field: SerializeField] public AttackController AttackController { get; private set; }


  [field: Header("External Controllers and Dependencies")]
  [field: SerializeField] public UIController UIController { get; private set; }
  [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }


  [field: Header("Player View Elements")]
  [field: SerializeField] public PlayerView PlayerView { get; private set; }


  [field: Header("Opponent View Elements")]
  [field: SerializeField] public PlayerView OpponentView { get; private set; }

  async void Start()
  {
    //Setup
    ClientState = PlayerViewState.Idle;

    Server.OnPhaseStarted += OnPhaseStarted;
    Server.OnPhaseEnded += OnPhaseEnded;
    Server.OnCombatStepStarted += OnCombatStepStart;
    Server.OnCombatStepEnded += OnCombatStepEnded;
    Server.OnPlayerCastCard += OnPlayerCastCard;
    Server.OnCardChangedState += OnCardChangedState;
    Server.OnCardZoneChanged += OnCardZoneChanged;
    Server.OnPlayerLifeChanged += OnPlayerReceiveDamage;
    Server.OnTimerChanged += OnTimerChanged;
    bool result = await Server.PrepareNewMatch(CardListPlayer1, CardListPlayer2);
    PlayerView.OnDeckClicked += OnDeckClick;
    PlayerView.OnCardCastRequested += OnCardCastRequested;
    PlayerView.OnCardClicked += OnCardClicked;
    UIController.OnButtonSkipClicked += OnButtonSkipClick;

    // Draw Starting Hand
    PlayerView.ResumeHand();
    await UIController.ShowMessage("Draw 7 Cards");
    PlayerView.ViewState = PlayerViewState.Idle;
    PlayerView.DrawEnabled = true;
    await WaitForPlayer1DrawCards(7);
    PlayerView.DrawEnabled = false;

    // Start Game
    Server.StartMatch();
  }

  void OnDestroy()
  {
    Server.OnPhaseStarted -= OnPhaseStarted;
    Server.OnPhaseEnded -= OnPhaseEnded;
    Server.OnCombatStepStarted -= OnCombatStepStart;
    Server.OnCombatStepEnded -= OnCombatStepEnded;
    Server.OnPlayerCastCard -= OnPlayerCastCard;
    Server.OnCardChangedState -= OnCardChangedState;
    Server.OnCardZoneChanged -= OnCardZoneChanged;
    Server.OnPlayerLifeChanged -= OnPlayerReceiveDamage;
    Server.OnTimerChanged -= OnTimerChanged;

    PlayerView.OnDeckClicked -= OnDeckClick;
    PlayerView.OnCardCastRequested -= OnCardCastRequested;
    PlayerView.OnCardClicked -= OnCardClicked;
    UIController.OnButtonSkipClicked -= OnButtonSkipClick;
  }


  // Server Events

  async void OnPlayerCastCard(int playerIndex, Card card)
  {
    Debug.Log($"<color='green'>Client:</color> Player {playerIndex} casted card {card.Name}");

    PlayerView playerView = (playerIndex == 0) ? PlayerView : OpponentView;

    CardView cardView = playerView.HandView.GetCardView(card);

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

    playerView.ResolveCast(true);

    Debug.Log($"<color='green'>Client:</color> card casted successfully");
  }

  void OnPhaseStarted(GamePhase phase)
  {
    Debug.Log($"<color='green'>Client:</color> Phase {phase} started");

    switch (phase)
    {
      case GamePhase.Beginning:
        UIController.SetButtonSkipVisibility(true, "Pass Upkeep");
        break;
      case GamePhase.MainPhase1:
        UIController.SetButtonSkipVisibility(true, "End Main Phase 1");
        break;
      case GamePhase.Combat:
        // Handled in OnCombatStepStart
        break;
      case GamePhase.MainPhase2:
        UIController.SetButtonSkipVisibility(true, "End Main Phase 2");
        break;
      case GamePhase.EndPhase:
        UIController.SetButtonSkipVisibility(true, "Pass End");
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

    int playerIndex = Server.MatchState.CurrentPlayerIndex;

    switch (combatStep)
    {
      case CombatStep.BeginCombat:
        UIController.SetButtonSkipVisibility(true, "Skip Combat Beginning");
        break;

      case CombatStep.DeclareAttackers:
        AttackController.ClearAttackers();
        if (playerIndex == 0)
          UIController.SetButtonSkipVisibility(true, "Proceed");
        break;

      case CombatStep.DeclareBlockers:
        BlockController.ClearBlockers();
        if(playerIndex == 1)
          UIController.SetButtonSkipVisibility(true, "Skip Combat Blockers");
        break;

      case CombatStep.CombatDamage:
        break;

      case CombatStep.EndCombat:

        BlockController.ClearView();
        UIController.SetButtonSkipVisibility(true, "Skip Combat End");
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
        break;
      case CombatStep.DeclareAttackers:
        UIController.SetButtonSkipVisibility(false);
        if(playerIndex == 0)
          Server.SetAttackers(AttackController.Attackers);
        // Tap Attacking Creatures
        PlayerView attackingPlayer = playerIndex == 0 ? PlayerView : OpponentView;
        List<Card> attackers = Server.TurnController.GetAttackers();
        attackingPlayer.TapAttackers(attackers);
        break;
      case CombatStep.DeclareBlockers:
        UIController.SetButtonSkipVisibility(false);
        if (playerIndex == 1)
          Server.SetBlockers(BlockController.Blockers);
        else
        {
          PlayerView blockingPlayer = playerIndex == 0 ? PlayerView : OpponentView;
          List<BlockData> blockers = Server.TurnController.GetBlockers();
          BlockController.SetBlockers(blockers);
        }

        break;
      case CombatStep.CombatDamage:
        break;
      case CombatStep.EndCombat:
        UIController.SetButtonSkipVisibility(false);
        UIController.SetButtonSkipOpponentVisibility(false);
        AttackController.ClearAttackers();
        BlockController.ClearBlockers();
        break;
    }
  }

  private void OnCardChangedState(Card card)
  {
    CardView cardView = FindCardView(card);
    cardView.Refresh();
  }

  private async void OnCardZoneChanged(Card card, int playerIndex, ZoneType from, ZoneType to)
  {
    Debug.Log($"<color='green'>Client:</color> Card Moved From {from} to {to}");

    PlayerView playerView = (playerIndex == 0) ? PlayerView : OpponentView;

    // Draw Card
    if (from == ZoneType.Deck && to == ZoneType.Hand)
    {
      Debug.Log($"<color='green'>Client:</color> Player {playerIndex} drew card {card.Name}");
      CardView newCard = CardViewCreator.CreateCardView(card, playerIndex);
      _ = playerView.DrawCard(newCard); ;
    }

    // Creature to Graveyard
    if (from == ZoneType.Creature && to == ZoneType.Graveyard)
    {
      Debug.Log($"<color='green'>Client:</color> creature {card} from player {playerIndex} died.");
      CardView cardView = FindCardView(card);
      await playerView.MoveToGraveyard(cardView);
    }
  }

  private void OnPlayerReceiveDamage(int playerIndex, int life)
  {
    Debug.Log($"<color='green'>Client:</color> Player {playerIndex}, received Damage. Life: {life}");
    UIController.UpdatePlayerLife(playerIndex, life);
  }

  private void OnTimerChanged(float time)
  {
    UIController.SetTimer(time);
  }

  // Client Events
  void OnDeckClick(int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Deck Clicked");

    if (Server.MatchState.CurrentPlayerIndex != playerIndex && Server.MatchState.CurrentPhase != GamePhase.Preparing)
      return;

    PlayerView playerView = (playerIndex == 0) ? PlayerView : OpponentView;
    Server.DrawCard(playerIndex);
  }

  async void OnCardClicked(CardView cardView, int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Card Clicked {cardView.Card.Name} - {cardView.Card.InstanceID}");

    // Ellect Attackers
    AttackController.HandleCardClick(Server, playerIndex, cardView);

    // Ellect Blockers
    BlockController.HandleCardClick(Server, playerIndex, cardView);

    await Task.Delay(1);
  }

  async void OnCardCastRequested(CardView cardView, int playerIndex)
  {
    if (cardView == null)
      return;

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
  }

  void OnButtonSkipClick(int playerIndex)
  {
    Debug.Log($"<color='green'>Client:</color> Button End Phase/Step Clicked by Player {playerIndex}");
    Server.OnPlayerSkipClicked(playerIndex);
  }

  // Support Methods
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

  // TODO: instead of using find every time, use only at start and after that add to the _allCards list
  public CardView FindCardView(Card card)
  {
    List<CardView> _allCards = new List<CardView>(FindObjectsByType<CardView>(FindObjectsSortMode.None));
    return _allCards.Find(x => x.Card == card);
  }
}
