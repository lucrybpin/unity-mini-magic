using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Splines;
using System.Threading.Tasks;
using DG.Tweening;

public enum HandControllerState
{
    Paused, // Called when the upper control must solve something. 
    Idle,
    DrawingCard,
    DraggingCard,
    CastCardRequested,
    CardInteractionRequested, // Usually Tap, or active ability
    InspectingCard,
}

[Serializable]
public struct HandControllerResult
{
    public HandControllerState State;
    public CardView TargetCard;
}

[Serializable]
public class HandController : MonoBehaviour
{
    [field: SerializeField] public int MaxHandSize { get; private set; } = 10;
    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public List<CardView> Cards { get; private set; } = new List<CardView>();
    [field: SerializeField] public CardHoverAndFocusController CardHoverController { get; private set; }
    [field: SerializeField] public CardView SelectedCard { get; private set; }
    [SerializeField] HandControllerResult _result;

    Vector2 _mouseWorldPos;
    RaycastHit2D _hit;

    void Start()
    {
        _result = new HandControllerResult();
        _result.State = HandControllerState.Idle;
    }

    public Task<HandControllerResult> Execute()
    {
        if (_result.State == HandControllerState.CastCardRequested)
            _result.State = HandControllerState.Paused;

        if (_result.State == HandControllerState.Paused)
            return Task.FromResult(_result);

        if (_result.State != HandControllerState.DrawingCard &&
            _result.State != HandControllerState.DraggingCard &&
            _result.State != HandControllerState.CastCardRequested)
            CardHoverController.Execute();

        // Click and Draggin Behavior
        if (Input.GetMouseButton(0))
        {
            CardHoverController.TryUnFocus();

            _mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // _hit = Physics2D.Raycast(_mouseWorldPos, Vector2.zero);
            RaycastHit2D[] hits = Physics2D.RaycastAll(_mouseWorldPos, Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.TryGetComponent<CardView>(out CardView card))
                    {
                        if (SelectedCard == null)
                        {
                            _result.State = HandControllerState.DraggingCard;
                            SelectedCard = card;

                            if (SelectedCard.Card.IsInField && SelectedCard.Card.Type == CardType.Resource)
                            {
                                _ = SelectedCard.Tap();
                            }
                        }
                    }
                }
            }


            if (SelectedCard != null)
            {
                if (!SelectedCard.Card.IsInField)
                {
                    Vector3 cardDragPosition = _mouseWorldPos;
                    cardDragPosition += new Vector3(0f, 0f, -2f);
                    SelectedCard.transform.position = Vector3.MoveTowards(SelectedCard.transform.position, cardDragPosition, 73f * Time.deltaTime);
                    SelectedCard.transform.rotation = Quaternion.RotateTowards(SelectedCard.transform.rotation, Quaternion.identity, 70f * Time.deltaTime);
                }
            }
        }
        else
        {
            if (SelectedCard != null)
            {
                _mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _hit = Physics2D.Raycast(_mouseWorldPos, Vector2.zero);
                RaycastHit2D[] hits = Physics2D.RaycastAll(_mouseWorldPos, Vector2.zero);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.TryGetComponent<CastRegion>(out CastRegion castRegion))
                        {
                            _result.State = HandControllerState.CastCardRequested;
                            _result.TargetCard = SelectedCard;
                        }
                        else
                        {
                            _result.State = HandControllerState.Idle;
                        }
                    }
                    else
                    {
                        _result.State = HandControllerState.Idle;
                    }
                }
                SelectedCard = null;
            }
        }
        return Task.FromResult(_result);
    }

    public async Task AddCard(CardView cardView)
    {
        _result.State = HandControllerState.DrawingCard;
        Cards.Add(cardView);
        await UpdateCardPositions();
        _result.State = HandControllerState.Idle;
    }

    public async Task RemoveCard(CardView cardView)
    {
        Cards.Remove(cardView);
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
            Vector3 finalPosition = splinePosition + transform.position + 0.01f * i * Vector3.back;

            Cards[i].transform.DOMove(finalPosition, 0.12f);
            Cards[i].transform.DORotate(rotation.eulerAngles, 0.48f).SetEase(Ease.OutBounce);
            Cards[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.48f));
    }

    public void SetIdleState()
    {
        _result.State = HandControllerState.Idle;
    }
}
