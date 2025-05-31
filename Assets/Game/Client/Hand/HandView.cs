using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public enum HandViewState
{
    Paused,
    Idle,
    Hover,
    Drawing,
    Dragging,
    Inspecting,
}

public class HandView : MonoBehaviour
{
    [field: Header("Setup")]
    [field: SerializeField] public int MaxHandSize { get; private set; } = 10;

    [field: Header("Core")]
    [field: SerializeField] public HandViewState State { get; private set; }
    [field: SerializeField] public List<CardView> Cards { get; private set; }
    [field: SerializeField] public CardView CardUnderPointer { get; private set; }
    [field: SerializeField] public CardView HoveringCard { get; private set; }
    [field: SerializeField] public CardView SelectedCard { get; private set; }
    [field: SerializeField] public CardView InspectingCard { get; private set; }
    [field: SerializeField] public CastRegion CastRegion { get; private set; }

    [field: Header("Dependencies")]
    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }
    [field: SerializeField] public GameObject FocusBackground { get; private set; }

    public Action<CardView> OnCardOverCastRegion;

    Vector2 _mouseWorldPos;
    RaycastHit2D[] _hits;
    Vector2 _dragStartMousePos;

    void Awake()
    {
        Cards = new List<CardView>();
        Pause();
    }

    void Update()
    {
        if (State == HandViewState.Paused)
            return;

        _mouseWorldPos      = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _hits               = Physics2D.RaycastAll(_mouseWorldPos, Vector2.zero); // RaycastNonAlloc is deprecated. If I really need performance, I can try GetRayIntersectionNonAlloc later here

        CardUnderPointer    = null;
        CastRegion          = null;
        foreach (RaycastHit2D hit in _hits)
        {
            // Cursor Over Card
            if (hit.collider != null &&
                hit.collider.TryGetComponent<CardView>(out CardView cardView))
            {
                if (CardUnderPointer == null)
                    CardUnderPointer = cardView;
            }

            // Cursor Over Cast Region
            if (hit.collider != null &&
                hit.collider.TryGetComponent<CastRegion>(out CastRegion castRegion))
            {
                CastRegion = castRegion;
            }
        }

        // Hover
        if (CardUnderPointer != null &&
            CardUnderPointer.Card.IsInHand &&
            State != HandViewState.Drawing &&
            State != HandViewState.Dragging)
        {
            if (HoveringCard != null)
                ReturnCardToOriginalPosition(HoveringCard);
            HoverCard(CardUnderPointer);
        }
        // UnHover
        else if (CardUnderPointer == null &&
            HoveringCard != null &&
            State == HandViewState.Idle)
        {
            ReturnCardToOriginalPosition(HoveringCard);
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Select Card
            if (CardUnderPointer != null &&
                State != HandViewState.Inspecting)
            {
                SelectedCard = CardUnderPointer;
                _dragStartMousePos = _mouseWorldPos;
            }
            else
            {
                SelectedCard = null;
            }

            // Exit Inspect
            if ((InspectingCard != null && CardUnderPointer == null) ||
                (InspectingCard != null && CardUnderPointer != InspectingCard))
            {
                ExitInspect();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            // Drag Card
            if (SelectedCard != null &&
                SelectedCard.Card.IsInHand &&
                Vector3.Distance(_mouseWorldPos, _dragStartMousePos) > 0.1f)
            {
                Vector3 cardDragPosition        = _mouseWorldPos;
                cardDragPosition                += new Vector3(0f, 0f, -2);
                SelectedCard.transform.position = Vector3.Lerp(SelectedCard.transform.position, cardDragPosition, 25f * Time.deltaTime);
                SelectedCard.transform.rotation = Quaternion.RotateTowards(SelectedCard.transform.rotation, Quaternion.identity, 37 * Time.deltaTime);
                State                           = HandViewState.Dragging;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (State == HandViewState.Dragging)
            {
                // ReturnCardToOriginalPosition(SelectedCard);
                Pause(); // Client Controller will resolve the cast
                OnCardOverCastRegion?.Invoke(SelectedCard);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Inspect Card
            if (CardUnderPointer != null)
            {
                if (InspectingCard != null)
                    ExitInspect();
                InspectCard(CardUnderPointer);
                State = HandViewState.Inspecting;
            }
        }
    }

    public void Pause()
    {
        State = HandViewState.Paused;
    }

    public void Resume()
    {
        State = HandViewState.Idle;
    }

    public void ResolveCast(bool success)
    {
        if (success)
        {
            Resume();
        }
        else
        {
            Resume();
            _ = UpdateCardPositions();
        }
    }

    public async Task AddCard(CardView cardView)
    {
        State = HandViewState.Drawing;
        Cards.Add(cardView);
        await UpdateCardPositions();
        State = HandViewState.Idle;
    }

    public async Task RemoveCard(CardView cardView)
    {
        Cards.Remove(cardView);
        cardView.Card.IsInHand = false;
        await UpdateCardPositions();
    }

    async Task UpdateCardPositions()
    {
        if (Cards.Count == 0) return;

        float cardSpacing = 1f / MaxHandSize;
        float firstCardPosition = 0.5f - (Cards.Count - 1) * cardSpacing / 2;
        Spline spline = SplineContainer.Spline;

        for (int i = 0; i < Cards.Count; i++)
        {
            float position = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 up = spline.EvaluateUpVector(position);
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            Vector3 finalPosition = splinePosition + 0.01f * i * Vector3.back;

            Cards[i].transform.DOMove(finalPosition, 0.12f);
            Cards[i].transform.DORotate(rotation.eulerAngles, 0.48f).SetEase(Ease.OutBounce);
            Cards[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.48f));
    }

    void HoverCard(CardView card)
    {
        HoveringCard = card;
        card.transform.DOKill();
        card.transform.DOMove(card.OriginalPosition + card.transform.up, 0.125f);
    }

    void ReturnCardToOriginalPosition(CardView card)
    {
        card.transform.DOKill();
        card.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        card.transform.DOMove(card.OriginalPosition, .25f);
        card.transform.DORotateQuaternion(card.OriginalRotation, .25f);
        HoveringCard = null;
    }

    void InspectCard(CardView cardView)
    {
        Vector3 focusPosition = new Vector3(0f, 2.5f, -3f);

        InspectingCard = CardViewCreator.CreateCardView(cardView.Card, focusPosition, Quaternion.identity);
        InspectingCard.transform.name = "Inspecting card";
        InspectingCard.transform.localScale = Vector3.zero;
        InspectingCard.transform.DOScale(2.2f * Vector3.one, 0.25f).SetEase(Ease.OutBack);
        InspectingCard.Setup(cardView.Card.Clone()); // I need a deep copy here, shallow copy will affect the original card
        InspectingCard.Card.IsInHand = false;
        FocusBackground.SetActive(true);
    }

    public void ExitInspect()
    {
        UnityEngine.Object.Destroy(InspectingCard.gameObject);
        FocusBackground.SetActive(false);
        InspectingCard = null;
        State = HandViewState.Idle;
    }

}
