using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class CardSceneController : MonoBehaviour
{
    // [field: SerializeField] public HandController HandController { get; private set; }
    [field: SerializeField] public HandControllerV2 HandController2 { get; private set; }
    [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }

    [field: SerializeField] public CardData CardData { get; private set; }
    [field: SerializeField] public Transform DeckTransform { get; private set; }

    [field: SerializeField] public List<CardView> Lands { get; private set; }
    [field: SerializeField] public SplineContainer SplineLandsContainer { get; private set; }

    // TODO: Refazer HandController para ter hover/focus/drag apenas em cartas que estão na mão

    async void Start()
    {
        // await Task.Delay(TimeSpan.FromSeconds(1));
        // for (int i = 0; i < 7; i++)
        // {
        //     Card card           = new Card(CardData);
        //     CardView newCard    = CardViewCreator.CreateCardView(card, DeckTransform.transform.position, DeckTransform.rotation);
        //     _                   = HandView.AddCard(newCard);
        //     await Task.Delay(TimeSpan.FromSeconds(.52));
        // }
        HandController2.Setup();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Card card = new Card(CardData);
            CardView newCard = CardViewCreator.CreateCardView(card, DeckTransform.transform.position, DeckTransform.rotation);
            _ = HandController2.AddCard(newCard);
        }

        HandControllerResult handResult = HandController2.Execute();

        // HandControllerResult handResult = HandController2.Execute();
        // if (handResult.State == HandControllerState.CastCardRequested)
        // {
        //     CastCard(handResult.CardToCast);
        // }

    }

    public async Task CastCard(CardView cardView)
    {
        cardView.transform.DOMove(new Vector3(0f, 2f, 0f), .25f);
        await Task.Delay(TimeSpan.FromSeconds(0.25f));
        if (cardView.Card.Type == CardType.Land)
        {
            await HandController2.RemoveCard(cardView);
            Lands.Add(cardView);
            cardView.Card.IsInField = true;
            await UpdateLandPositions();
        }
        // HandController2.SetIdleState();
    }

    async Task UpdateLandPositions()
    {
        if (Lands.Count == 0) return;

        float cardSpacing = 0.21f;//1f / 25;
        float firstLandPosition = 0.5f - (Lands.Count - 1) * cardSpacing / 2;
        Spline spline = SplineLandsContainer.Spline;

        for (int i = 0; i < Lands.Count; i++)
        {
            float position = firstLandPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 up = spline.EvaluateUpVector(position);
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            Vector3 finalPosition = splinePosition + transform.position;// + i * Vector3.back;

            Lands[i].transform.DOMove(finalPosition, 0.12f);
            Lands[i].transform.DORotate(rotation.eulerAngles, 0.48f).SetEase(Ease.OutBounce);
            Lands[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.48f));
    }

}
