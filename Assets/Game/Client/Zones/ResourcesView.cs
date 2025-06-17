using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class ResourcesView : MonoBehaviour
{
    [field: SerializeField] public SplineContainer SplineContainer { get; private set; }
    [field: SerializeField] public List<CardView> Resources { get; private set; }

    void Awake()
    {
        Resources = new List<CardView>();
    }

    public async Task AddCard(CardView cardView)
    {
        Resources.Add(cardView);
        cardView.Card.IsInField = true;
        await UpdateResourcesPositions();
    }

    public async Task UpdateResourcesPositions()
    {
        if (Resources.Count == 0) return;

        float cardSpacing = 0.12f;
        if (Resources.Count > 9)
            cardSpacing /= 2;

        float firstLandPosition = 0.5f - (Resources.Count - 1) * cardSpacing / 2;
        Debug.Log($">>>> Resources.Count = {Resources.Count}");
        
        Debug.Log($">>>> firstLandPosition = {firstLandPosition}");
        
        Spline spline = SplineContainer.Spline;

        for (int i = 0; i < Resources.Count; i++)
        {
            float position = firstLandPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(position);
            Vector3 forward = spline.EvaluateTangent(position);
            Vector3 up = spline.EvaluateUpVector(position);
            Quaternion rotation = Resources[i].Card.IsTapped ? Quaternion.Euler(0f, 0f, -90f) : Quaternion.identity;
            Vector3 finalPosition = splinePosition + transform.position + (i + 1) * 0.025f * Vector3.back;

            Resources[i].UpdateOriginalPositionAndRotation(finalPosition, rotation);
            Resources[i].transform.DOMove(finalPosition, 0.12f);
            Resources[i].transform.DORotateQuaternion(rotation, 0.12f);
        }
        await Task.Delay(TimeSpan.FromSeconds(0.12f));
    }
}
