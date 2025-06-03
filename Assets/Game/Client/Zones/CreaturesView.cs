using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class CreaturesView : MonoBehaviour
{
    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public List<CardView> Creatures { get; private set; }

    void Awake()
    {
        Creatures = new List<CardView>();
    }

    public async Task AddCard(CardView cardView)
    {
        Creatures.Add(cardView);
        cardView.Card.IsInField = true;
        await UpdateResourcesPositions();
    }

    public async Task UpdateResourcesPositions()
    {
        if (Creatures.Count == 0) return;

        float cardSpacing = 0.21f;
        if (Creatures.Count > 9)
            cardSpacing /= 2;

        float firstLandPosition = 0.5f - (Creatures.Count - 1) * cardSpacing / 2;
        Spline spline = SplineContainer.Spline;

        for (int i = 0; i < Creatures.Count; i++)
        {
            float position = firstLandPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 up = spline.EvaluateUpVector(position);
            Quaternion rotation = Creatures[i].Card.IsTapped ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.identity;
            Vector3 finalPosition = splinePosition + transform.position + (i + 1) * 0.025f * Vector3.back;

            Creatures[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
            Creatures[i].transform.DOMove(finalPosition, 0.12f);
            Creatures[i].transform.DORotateQuaternion(rotation, 0.12f);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }

}
