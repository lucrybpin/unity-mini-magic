using DG.Tweening;
using UnityEngine;

public class CardViewCreator : MonoBehaviour
{
    [field: SerializeField] public CardView CardViewPrefab { get; private set; }

    public CardView CreateCardView(Card card, int playerIndex = -1)
    {
        CardView cardView               = Instantiate(CardViewPrefab);
        cardView.transform.position = 21 * Vector3.up;
        cardView.transform.localScale   = Vector3.one;
        cardView.transform.DOScale(Vector3.one, 0.25f);
        cardView.Setup(card, playerIndex);
        return cardView;
    }
    public CardView CreateCardView(Card card, int playerIndex, Vector3 position, Quaternion rotation)
    {
        CardView cardView               = Instantiate(CardViewPrefab, position, rotation);
        cardView.transform.localScale   = Vector3.one;
        cardView.transform.DOScale(Vector3.one, 0.25f);
        cardView.Setup(card, playerIndex);
        return cardView;
    }
}
