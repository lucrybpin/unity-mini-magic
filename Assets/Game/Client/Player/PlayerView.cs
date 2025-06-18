using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [field: Header("Setup")]
    [field: SerializeField] public int PlayerIndex { get; private set; }
    [field: SerializeField] public AIController AIController { get; private set; }

    [field: SerializeField] public MatchClientController ClientController { get; private set; }

    [field: Header("Player View Elements")]
    [field: SerializeField] public DeckView DeckView { get; private set; }
    [field: SerializeField] public HandView HandView { get; private set; }
    [field: SerializeField] public CreaturesView CreaturesView { get; private set; }
    [field: SerializeField] public ResourcesView ResourcesView { get; private set; }
    [field: SerializeField] public GraveyardView GraveyardView { get; private set; }

    [field: Header("State and Controls")]
    [field: SerializeField] public PlayerViewState ViewState { get; set; }
    [field: SerializeField] public bool DrawEnabled { get; set; }

    public event Action<int> OnDeckClicked;
    public event Action<CardView, int> OnCardCastRequested;
    public event Action<CardView, int> OnCardClicked;

    void Awake()
    {
        DeckView.OnDeckClicked += OnDeckClick;
        HandView.OnCardOverCastRegion += OnCardCastRequest;
        HandView.OnCardClicked += OnCardClick;
    }

    void OnDestroy()
    {
        DeckView.OnDeckClicked -= OnDeckClick;
        HandView.OnCardOverCastRegion -= OnCardCastRequest;
        HandView.OnCardClicked -= OnCardClick;
    }

    void SetAI(AIController aiController)
    {
        AIController = aiController;
        DeckView.OnDeckClicked -= OnDeckClick;
        HandView.OnCardOverCastRegion -= OnCardCastRequest;
        HandView.OnCardClicked -= OnCardClick;
    }

    // Player Events

    void OnDeckClick()
    {
        if (DrawEnabled && ViewState != PlayerViewState.DrawingCard)
            OnDeckClicked?.Invoke(PlayerIndex);
    }

    void OnCardCastRequest(CardView cardView)
    {
        if (cardView != null)
            OnCardCastRequested?.Invoke(cardView, PlayerIndex);
    }

    void OnCardClick(CardView cardView)
    {
        if (cardView != null)
            OnCardClicked?.Invoke(cardView, PlayerIndex);
    }

    void OnCardCasted(Card card)
    {

    }

    // Methods

    public async Task DrawCard(CardView newCard)
    {
        while (ViewState == PlayerViewState.DrawingCard)
            await Task.Delay(500);

        ViewState = PlayerViewState.DrawingCard;
        newCard.transform.position = DeckView.transform.position;
        newCard.transform.rotation = DeckView.transform.rotation;
        await HandView.AddCard(newCard);
        ViewState = PlayerViewState.Idle;
    }

    public async Task MoveToGraveyard(CardView cardView)
    {
        switch (cardView.Card.Type)
        {
            case CardType.Creature:
                await CreaturesView.RemoveCard(cardView);
                break;
            // case CardType.Resource:
            //     await ResourcesView.RemoveCard
        }

        await GraveyardView.AddCard(cardView);
    }


    public async Task ProcessResource(CardView cardView)
    {
        await HandView.RemoveCard(cardView);
        await ResourcesView.AddCard(cardView);
    }

    public async Task ProcessCreature(CardView cardView, List<Card> ServerResourceZone)
    {
        // Move to Creatures Zone
        await HandView.RemoveCard(cardView);
        await CreaturesView.AddCard(cardView);
    }

    public void TapAttackers(List<Card> attackers)
    {
        foreach (Card card in attackers)
        {
            CardView creatureCard = CreaturesView.FindCardView(card);
            creatureCard.Refresh();
        }
    }

    public void ResolveCast(bool success)
    {
        HandView.ResolveCast(success);
    }

    public void PauseHand()
    {
        HandView.Pause();
    }

    public void ResumeHand()
    {
        HandView.Resume();
    }



}
