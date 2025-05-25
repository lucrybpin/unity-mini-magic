using UnityEngine;

public enum CardType
{
    Land,
    Creature,
    Instant,
    Sorcery,
    Enchantment,
    Artifact
}

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Name          { get; private set; }
    [field: SerializeField] public CardType Type        { get; private set; }
    [field: SerializeField] public string Description   { get; private set; }
    [field: SerializeField] public int Cost             { get; private set; }
    [field: SerializeField] public int Attack           { get; private set; }
    [field: SerializeField] public int Defense          { get; private set; }
    [field: SerializeField] public Sprite Sprite        { get; private set; }

}
