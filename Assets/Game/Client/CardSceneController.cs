using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class CardSceneController : MonoBehaviour
{
    [field: SerializeField] public MatchServerController MatchController { get; private set; }
    // [field: SerializeField] public HandController HandController { get; private set; }
    [field: SerializeField] public HandControllerV2 HandController2 { get; private set; }
    [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }
    [field: SerializeField] public CardView CastingCard { get; private set; }
    [field: SerializeField] public CardView InteractinCard { get; private set; }

    [field: SerializeField] public List<CardData> CardData { get; private set; }
    [field: SerializeField] public Transform DeckTransform { get; private set; }

    [field: SerializeField] public List<CardView> Creatures { get; private set; }
    [field: SerializeField] public SplineContainer SplineCreaturesContainer { get; private set; }
    [field: SerializeField] public List<CardView> Lands { get; private set; }
    [field: SerializeField] public SplineContainer SplineLandsContainer { get; private set; }
    [field: SerializeField] public int ResourcesAvailable { get; private set; }

    // TODO: Refazer HandController para ter hover/focus/drag apenas em cartas que estão na mão

    async void Start()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        // for (int i = 0; i < 7; i++)
        // {
        //     Card card           = new Card(CardData);
        //     CardView newCard    = CardViewCreator.CreateCardView(card, DeckTransform.transform.position, DeckTransform.rotation);
        //     _                   = HandView.AddCard(newCard);
        //     await Task.Delay(TimeSpan.FromSeconds(.52));
        // }
        HandController2.Setup();
        ResourcesAvailable = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int index = UnityEngine.Random.Range(0, CardData.Count);
            Card card = new Card(CardData[index]);
            CardView newCard = CardViewCreator.CreateCardView(card, DeckTransform.transform.position, DeckTransform.rotation);
            _ = HandController2.AddCard(newCard);
        }

        HandControllerResult handResult = HandController2.Execute();

        // Cast
        if (handResult.State == HandControllerState.CastCardRequested &&
            CastingCard == null)
        {
            CastingCard = handResult.TargetCard;
            CastCard(handResult.TargetCard);
        }

        // Interact (Tap or Trigger Active Ability)
        if (handResult.State == HandControllerState.CardInteractionRequested &&
            InteractinCard == null)
        {
            InteractinCard = handResult.TargetCard;
            InteractWithCard(handResult.TargetCard);
        }

    }

    public async void CastCard(CardView cardView)
    {
        Debug.Log($">>>> Casting Card {cardView.Card}");
        // cardView.transform.DOMove(new Vector3(0f, 2f, 0f), .25f);
        // await Task.Delay(TimeSpan.FromSeconds(0.25f));
        if (cardView.Card.Type == CardType.Resource)
        {
            await HandController2.RemoveCard(cardView);
            Lands.Add(cardView);
            cardView.Card.IsInField = true;
            await UpdateLandPositions();
        }
        if (cardView.Card.Type == CardType.Creature)
        {
            cardView.transform.DOMove(new Vector3(0f, 2f, 0f), .25f);
            await Task.Delay(TimeSpan.FromSeconds(0.25f));
            // wait for player to tap mana for resource or cancel
            Debug.Log($">>>> Casting creature");
            if (cardView.Card.Cost <= ResourcesAvailable)
            {
                await HandController2.RemoveCard(cardView);
                Creatures.Add(cardView);
                cardView.Card.IsInField = true;
                ResourcesAvailable -= cardView.Card.Cost;
                await UpdateCreaturesPositions();
            }
            else
            {
                Debug.Log($">>>> Not enought Resources to cast Creature: {cardView.Card}");
            }
        }
        CastingCard = null;
        HandController2.ResolveCast();
    }

    public async void InteractWithCard(CardView cardView)
    {
        // TODO: Whe turn is working, check if is current turn stage permits interaction
        await Task.Yield();
        Debug.Log($">>>> Interacting Card {cardView.Card}");
        if (cardView.Card.Type == CardType.Resource)
        {
            if (cardView.Card.IsTapped == false)
            {
                await cardView.Tap();
                ResourcesAvailable += 1;
            }
            // else
            // {
            //     ResourcesAvailable -= 1;
            //     cardView.Untap();
            // }
            InteractinCard = null;
            HandController2.ResolveCardInteraciton();
            // Generate Resource
        }
        if (cardView.Card.Type == CardType.Creature)
        {
            if (cardView.Card.IsTapped == false)
            {
                await cardView.Tap();
                ResourcesAvailable += 1;
            }
            else
            {
                ResourcesAvailable -= 1;
                await cardView.Untap();
            }
            InteractinCard = null;
            HandController2.ResolveCardInteraciton();
        }
    }

    async Task UpdateCreaturesPositions()
    {
        if (Creatures.Count == 0) return;

        float cardSpacing = 0.22f;
        if (Creatures.Count > 5)
            cardSpacing /= 2;

        float firstCreaturePosition = 0.5f - (Creatures.Count - 1) * cardSpacing / 2;
        Spline spline = SplineCreaturesContainer.Spline;

        for (int i = 0; i < Creatures.Count; i++)
        {
            float position = firstCreaturePosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 up = spline.EvaluateUpVector(position);
            Quaternion rotation = Creatures[i].Card.IsTapped ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.identity;
            Vector3 finalPosition = splinePosition + transform.position + (i+1) * 0.025f * Vector3.back;

            Creatures[i].transform.DOMove(finalPosition, 0.12f);
            Creatures[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }

    async Task UpdateLandPositions()
    {
        if (Lands.Count == 0) return;

        float cardSpacing = 0.12f;
        if (Lands.Count > 9)
            cardSpacing /= 2;

        float firstLandPosition = 0.5f - (Lands.Count - 1) * cardSpacing / 2;
        Spline spline = SplineLandsContainer.Spline;

        for (int i = 0; i < Lands.Count; i++)
        {
            float position = firstLandPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 up = spline.EvaluateUpVector(position);
            Quaternion rotation = Lands[i].Card.IsTapped ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.identity;
            Vector3 finalPosition = splinePosition + transform.position + (i+1) * 0.025f * Vector3.back;

            Lands[i].transform.DOMove(finalPosition, 0.12f);
            Lands[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }


}
