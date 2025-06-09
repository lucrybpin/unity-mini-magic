using System;
using UnityEngine;

public class DeckView : MonoBehaviour
{
    public event Action OnDeckClicked;

    void OnMouseDown()
    {
        OnDeckClicked?.Invoke();
    }
}
