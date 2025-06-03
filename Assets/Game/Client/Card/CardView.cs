using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using System;

public class CardView : MonoBehaviour
{
    [field: SerializeField] public TMP_Text Name { get; private set; }
    [field: SerializeField] public TMP_Text Type { get; private set; }
    [field: SerializeField] public TMP_Text Description { get; private set; }
    [field: SerializeField] public TMP_Text Cost { get; private set; }
    [field: SerializeField] public TMP_Text Attack { get; private set; }
    [field: SerializeField] public TMP_Text Defense { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public GameObject GameObject { get; private set; }
    [field: SerializeField] public GameObject CombatSection { get; private set; }
    [field: SerializeField] public Vector3 OriginalPosition { get; private set; }
    [field: SerializeField] public Quaternion OriginalRotation { get; private set; }
    [field: SerializeField] public Card Card { get; private set; }

    public void Setup(Card card)
    {
        Card = card;
        Name.text = Card.Name;
        Type.text = Card.Type.ToString();
        Description.text = Card.Description;
        Cost.text = Card.Cost.ToString();
        SpriteRenderer.sprite = Card.Sprite;
        Attack.text = Card.Attack.ToString();
        Defense.text = Card.Defense.ToString();
        if (Card.Attack == 0 && Card.Defense == 0)
        {
            CombatSection.SetActive(false);
        }
    }

    public void UpdateOriginalPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        OriginalPosition = position;
        OriginalRotation = rotation;
    }

    public bool IsVisuallyTapped()
    {
        Quaternion tappedRotation = Quaternion.Euler(0f, 0f, -90f);
        if (transform.rotation == tappedRotation)
            return true;

        return false;
    }

    public async Task Tap()
    {
        Quaternion tappedRotation = Quaternion.Euler(0f, 0f, -90f);
        transform.DORotateQuaternion(tappedRotation, .12f);
        Card.Tap();
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }

    public async Task Untap()
    {
        transform.DORotateQuaternion(Quaternion.identity, .12f);
        Card.Untap();
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }

    public void Hover(float offsetScale = 1f)
    {
        transform.DOKill();
        transform.DOMove(OriginalPosition +  (offsetScale * transform.up), 0.125f);
    }

    public void ReturnCardToOriginalPosition()
    {
        transform.DOKill();
        transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        transform.DOMove(OriginalPosition, .25f);
        transform.DORotateQuaternion(OriginalRotation, .25f);
    }
}
