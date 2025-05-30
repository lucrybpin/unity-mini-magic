using System;
using UnityEngine;

public class DeckView : MonoBehaviour
{
    public event Action OnDeckClick;

    void OnMouseDown()
    {
        OnDeckClick?.Invoke();
    }
}
