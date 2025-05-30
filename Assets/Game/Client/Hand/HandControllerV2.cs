using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

[Serializable]
public class HandControllerV2
{
    [field: SerializeField] public int MaxHandSize { get; private set; } = 10;
    [Header("Info")]
    [field: SerializeField] public List<CardView> Cards { get; private set; }
    [field: SerializeField] public CardView CardUnderPointer { get; private set; }
    [field: SerializeField] public CardView HoveringCard { get; private set; }
    [field: SerializeField] public CardView SelectedCard { get; private set; }
    [field: SerializeField] public CardView InspectingCard { get; private set; }
    [field: SerializeField] public CastRegion CastRegion { get; private set; }

    [Header("Dependencies")]
    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public CardView CardPrefab { get; private set; }
    [field: SerializeField] public GameObject FocusBackground { get; private set; }
    [SerializeField] HandControllerResult _data;

    Vector2 _mouseWorldPos;
    RaycastHit2D[] _hits;
    Vector2 _dragStartMousePos;

    // The best solution would probably split it in a real state machine
    // but it is working fine so far, I will proceed to more important features now

    public void Setup()
    {
        Cards = new List<CardView>();
        _data = new HandControllerResult();
        _data.State = HandControllerState.Idle;
        SelectedCard = null;
    }

    public HandControllerResult Execute()
    {
        if (_data.State == HandControllerState.CastCardRequested)
            return _data;

        if (_data.State == HandControllerState.CardInteractionRequested)
            return _data;

        if (_data.State == HandControllerState.DrawingCard)
            return _data;

        _mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _hits = Physics2D.RaycastAll(_mouseWorldPos, Vector2.zero); // RaycastNonAlloc is deprecated. If I really need performance, I can try GetRayIntersectionNonAlloc later here

        CardUnderPointer = null;
        CastRegion = null;
        _data.State = HandControllerState.Idle;

        if (InspectingCard != null)
            _data.State = HandControllerState.InspectingCard;

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

        if (Input.GetMouseButtonDown(0))
        {
            // Select Card
            if (CardUnderPointer != null &&
                _data.State != HandControllerState.InspectingCard)
            {
                SelectedCard = CardUnderPointer;
                _dragStartMousePos = _mouseWorldPos;
                if (SelectedCard.Card.IsInField)
                {
                    _data.State = HandControllerState.CardInteractionRequested;
                    _data.TargetCard = SelectedCard;
                }
            }
            else
            {
                SelectedCard = null;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            // Drag Card
            if (SelectedCard != null &&
                SelectedCard.Card.IsInHand &&
                Vector3.Distance(_mouseWorldPos, _dragStartMousePos) > 0.1f)
            {
                Vector3 cardDragPosition = _mouseWorldPos;
                cardDragPosition += new Vector3(0f, 0f, -2f);
                // SelectedCard.transform.position = Vector3.MoveTowards(SelectedCard.transform.position, cardDragPosition, 25 * Time.deltaTime);
                SelectedCard.transform.position = Vector3.Lerp(SelectedCard.transform.position, cardDragPosition, 25f * Time.deltaTime);
                SelectedCard.transform.rotation = Quaternion.RotateTowards(SelectedCard.transform.rotation, Quaternion.identity, 37 * Time.deltaTime);
                _data.State = HandControllerState.DraggingCard;
            }

            // ExitInspect
            if (InspectingCard != null && CardUnderPointer == null ||
                InspectingCard != null && CardUnderPointer != InspectingCard)
            {
                ExitInspect();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (CastRegion != null &&
                SelectedCard != null &&
                _data.State != HandControllerState.InspectingCard)
            {
                _data.State = HandControllerState.CastCardRequested;
                _data.TargetCard = SelectedCard;
                return _data;
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
                _data.State = HandControllerState.InspectingCard;
            }
        }

        // Hover
        if (CardUnderPointer != null &&
            CardUnderPointer.Card.IsInHand &&
            _data.State == HandControllerState.Idle)
        {
            if (HoveringCard != null)
                ReturnCardToOriginalPosition(HoveringCard);
            HoverCard(CardUnderPointer);
        }
        // UnHover
        else if (CardUnderPointer == null &&
            HoveringCard != null &&
            _data.State == HandControllerState.Idle)
        {
            ReturnCardToOriginalPosition(HoveringCard);
        }

        return _data;
    }


    public async Task AddCard(CardView cardView)
    {
        Cards.Add(cardView);
        cardView.Card.IsInHand = true;
        _data.State = HandControllerState.DrawingCard;
        await UpdateCardPositions();
        _data.State = HandControllerState.Idle;
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

    void InspectCard(CardView card)
    {
        Vector3 focusPosition = new Vector3(0f, 2.5f, -3f);
        InspectingCard = UnityEngine.Object.Instantiate(CardPrefab, focusPosition, Quaternion.identity);
        InspectingCard.transform.name = "Inspecting card";
        InspectingCard.transform.localScale = Vector3.zero;
        InspectingCard.transform.DOScale(2.2f * Vector3.one, 0.25f).SetEase(Ease.OutBack);
        InspectingCard.Setup(card.Card.Clone()); // I need a deep copy here, shallow copy will affect the original card
        InspectingCard.Card.IsInHand = false;
        FocusBackground.SetActive(true);
    }

    public void ExitInspect()
    {
        if (HoveringCard != null)
            ReturnCardToOriginalPosition(HoveringCard);

        UnityEngine.Object.Destroy(InspectingCard.gameObject);
        FocusBackground.SetActive(false);
        InspectingCard = null;
    }

    public void ResolveCast()
    {
        SelectedCard = null;
        _data.State = HandControllerState.Idle;
    }

    public void ResolveCardInteraciton()
    {
        SelectedCard = null;
        _data.State = HandControllerState.Idle;
    }

}
