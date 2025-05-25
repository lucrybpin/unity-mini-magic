using System;
using System.Threading.Tasks;
using UnityEngine;

public class CardSceneController : MonoBehaviour
{
    [field: SerializeField] public HandController HandController { get; private set; }
    [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }

    [field: SerializeField] public CardData CardData { get; private set; }
    [field: SerializeField] public Transform DeckTransform { get; private set; }

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Card card = new Card(CardData);
            CardView newCard = CardViewCreator.CreateCardView(card, DeckTransform.transform.position, DeckTransform.rotation);
            _ = HandController.AddCard(newCard);
        }

        HandControllerResult handResult = HandController.Execute();
        if (handResult.State == HandControllerState.CastCardRequested)
        {
            CastCard(handResult.CardToCast);
        }
        
    }

    public void CastCard(Card card)
    {
        Debug.Log($">>>> Castin card: {card.Name}");
    }
}
