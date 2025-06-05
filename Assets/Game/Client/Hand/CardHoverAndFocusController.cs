using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class CardHoverAndFocusController
{
    [field: SerializeField] public CardView CurrentHoveringCard { get; private set; }
    [field: SerializeField] public CardView FocusedCard { get; private set; }
    [field: SerializeField] public GameObject CardPrefab { get; private set; }
    [field: SerializeField] public GameObject FocusBackground { get; private set; }

    Vector2 _mouseWorldPos;
    RaycastHit2D _hit;
    GameObject _focusInstance;

    public void Execute()
    {
        _mouseWorldPos  = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _hit            = Physics2D.Raycast(_mouseWorldPos, Vector2.zero);

        if (_hit.collider != null)
        {
            if (_hit.collider.gameObject == _focusInstance)
                return;

            if (_hit.collider.TryGetComponent<CardView>(out CardView card))
            {
                // Hover New Card
                if (!card.Card.IsInField)
                    Hover(card);

                // Hover Another Card
                if (card != CurrentHoveringCard && CurrentHoveringCard != null)
                {
                    UnHover(CurrentHoveringCard);
                    Hover(card);
                }

                CurrentHoveringCard = card;

                // Focus
                if (Input.GetMouseButtonDown(1))
                {
                    if (FocusedCard != null)
                        TryUnFocus();
                    FocusedCard = card;
                    Focus(card);
                }
            }
            else
            {
                // Exit Hover
                if (CurrentHoveringCard != null)
                {
                    UnHover(CurrentHoveringCard);
                    CurrentHoveringCard = null;
                }

                // Exit Focus
                if (Input.GetMouseButtonDown(0))
                {
                    if (FocusedCard != null)
                        TryUnFocus();
                    FocusedCard = null;
                }
            }
        }
        else
        {
            // Exit Hover
            if (CurrentHoveringCard != null)
            {
                UnHover(CurrentHoveringCard);
                CurrentHoveringCard = null;
            }

            // Exit Focus
            if (Input.GetMouseButtonDown(0))
            {
                if (FocusedCard != null)
                    TryUnFocus();
                FocusedCard = null;
            }
        }
    }

    void Hover(CardView card)
    {
        card.transform.DOKill();
        card.transform.DOMove(card.OriginalPosition + card.transform.up, 0.125f);
    }

    void UnHover(CardView card)
    {
        card.transform.DOKill();
        card.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        card.transform.DOMove(card.OriginalPosition, .25f);
        card.transform.DORotateQuaternion(card.OriginalRotation, .25f);
    }

    void Focus(CardView card)
    {
        Vector3 focusPosition = new Vector3(0f, 2.5f, -3f);
        _focusInstance = UnityEngine.Object.Instantiate(CardPrefab, focusPosition, Quaternion.identity);
        _focusInstance.transform.localScale = Vector3.zero;
        _focusInstance.transform.DOScale(2.2f * Vector3.one, 0.25f).SetEase(Ease.OutBack);

        CardView cardView = _focusInstance.GetComponent<CardView>();
        cardView.Setup(card.Card, card.OwnerIndex);

        FocusBackground.SetActive(true);
    }

    public void TryUnFocus()
    {
        if (_focusInstance == null)
            return;

        UnityEngine.Object.Destroy(_focusInstance.gameObject);
        FocusBackground.SetActive(false);
    }
}
