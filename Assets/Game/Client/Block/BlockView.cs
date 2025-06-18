using System.Collections.Generic;
using UnityEngine;

public class BlockView : MonoBehaviour
{
    [field: SerializeField] public BlockArrowView BlockArrowViewPrefab { get; private set; }

    [field: SerializeField] public List<BlockData> Blockers { get; private set; }

    List<BlockArrowView> _arrows = new List<BlockArrowView>();
    List<CardView> _allCards;

    public void UpdateBlockersView(List<BlockData> blockers)
    {
        Blockers = blockers;

        ClearAllArrows();

        if (blockers == null || blockers.Count == 0)
            return;

        _allCards = new List<CardView>(FindObjectsByType<CardView>(FindObjectsSortMode.None));

        foreach (BlockData blockData in blockers)
        {
            // List<BlockArrowView> arrows = new List<BlockArrowView>();
            // Create arrows
            foreach (Card card in blockData.Blockers)
            {
                CardView blockerCardView = _allCards.Find(x => x.Card == card);
                CardView attackerCardView = _allCards.Find(x => x.Card == blockData.Attacker);
                BlockArrowView arrow = Instantiate<BlockArrowView>(BlockArrowViewPrefab, transform);
                arrow.SetupArrow(attackerCardView.transform.position, blockerCardView.transform.position);
                _arrows.Add(arrow);
            }
        }
    }
    public void ClearAllArrows()
    {
        if (_arrows != null)
        {
            foreach (BlockArrowView arrow in _arrows)
            {
                if (arrow != null && arrow.gameObject != null)
                    Destroy(arrow.gameObject);
            }
            _arrows.Clear();
        }
    }

}
