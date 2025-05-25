using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Splines;
using System.Threading.Tasks;
using DG.Tweening;

[Serializable]
public class HandController : MonoBehaviour
{
    [field: SerializeField] public int MaxHandSize                  { get; private set; } = 10;
    [field: SerializeField] public SplineContainer SplineContainer  { get; private set; }
    [field: SerializeField] public List<CardView> Cards             { get; private set; } = new List<CardView>();

    [field: SerializeField] public CardHoverAndFocusController CardHoverController { get; private set; }

    bool _isDrawing = false;
    bool _isDragging = false;


    [field: SerializeField] public CardView SelectedCard            { get; private set; }
    Vector2 _mouseWorldPos;
    RaycastHit2D _hit;

    void Update()
    {
        if (!_isDrawing &&  !_isDragging)
            CardHoverController.Execute();

        // Draggin Behavior
        if (Input.GetMouseButton(0))
        {
            CardHoverController.TryUnFocus();

            _mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _hit = Physics2D.Raycast(_mouseWorldPos, Vector2.zero);

            if (_hit.collider != null)
            {
                if (_hit.collider.TryGetComponent<CardView>(out CardView card))
                {
                    if (SelectedCard == null)
                    {
                        _isDragging = true;
                        SelectedCard = card;
                    }
                }
            }

            if (SelectedCard != null)
            {
                Vector3 cardDragPosition = _mouseWorldPos;
                cardDragPosition += new Vector3(0f, 0f, -2f);
                SelectedCard.transform.position = Vector3.MoveTowards(SelectedCard.transform.position, cardDragPosition, 43f * Time.deltaTime);
                SelectedCard.transform.rotation = Quaternion.RotateTowards(SelectedCard.transform.rotation, Quaternion.identity, 70f * Time.deltaTime);
            }
        }
        else
        {
            _isDragging = false;
            if (SelectedCard != null)
            {
                SelectedCard = null;
                Debug.Log($">>>> Released");
            }
        }

    }

    public async Task AddCard(CardView cardView)
    {
        _isDrawing = true;
        Cards.Add(cardView);
        await UpdateCardPositions();
        _isDrawing = false;
    }

    async Task UpdateCardPositions()
    {
        if (Cards.Count == 0) return;

        float cardSpacing       = 1f / MaxHandSize;
        float firstCardPosition = 0.5f - (Cards.Count - 1) * cardSpacing / 2;
        Spline spline           = SplineContainer.Spline;

        for (int i = 0; i < Cards.Count; i++)
        {
            float position          = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition  = spline.EvaluatePosition(position);
            Vector3 forward         = spline.EvaluateTangent(position);
            Vector3 up              = spline.EvaluateUpVector(position);
            Quaternion rotation     = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            Vector3 finalPosition   = splinePosition + transform.position + 0.01f * i * Vector3.back;

            Cards[i].transform.DOMove(finalPosition, 0.12f);
            Cards[i].transform.DORotate(rotation.eulerAngles, 0.48f).SetEase(Ease.OutBounce);
            Cards[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.48f));
    }
}
