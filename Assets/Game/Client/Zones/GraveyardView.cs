using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class GraveyardView : MonoBehaviour
{
    [field: SerializeField] public List<CardView> Cards { get; private set; }

    [field: Header("Dependencies")]
    [field: SerializeField] public CardViewCreator CardViewCreator { get; private set; }
    [field: SerializeField] public Transform Origin { get; private set; }

    void Awake()
    {
        Cards = new List<CardView>();
    }

    public async Task AddCard(CardView cardView)
    {
        Cards.Add(cardView);
        cardView.transform.SetParent(transform);
        await UpdateCardPositions();
    }

    public async Task UpdateCardPositions()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Vector3 position = Origin.position + i * Vector3.up + i * 0.01f * Vector3.back;
            Quaternion rotation = Quaternion.identity;
            Cards[i].transform.DOMove(position, 0.12f);
            Cards[i].transform.DORotateQuaternion(rotation, 0.12f);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }

}
