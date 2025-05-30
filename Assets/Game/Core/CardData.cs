using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Resource,
    Creature,
    Instant,
    Sorcery,
    Enchantment,
    Artifact
}

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    // Basic Info
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public CardType Type { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }

    // Creature Stats
    [field: SerializeField] public int Attack { get; private set; }
    [field: SerializeField] public int Defense { get; private set; }

    // Effects
    [field: SerializeField] public List<string> Effects { get; private set; }
}
