using UnityEngine;

public class BlockArrowView : MonoBehaviour
{
    [field: SerializeField] public GameObject AttackerHead { get; private set; }
    [field: SerializeField] public GameObject DefensorHead { get; private set; }
    [field: SerializeField] public LineRenderer LineRenderer { get; private set; }
    [field: SerializeField] public CardView OriginCard { get; private set; }
    [field: SerializeField] public CardView DestinationCard { get; private set; }

    Vector3 _startingPosition;

    public void SetupArrow(Vector3 origin, Vector3 destination)
    {
        _startingPosition = origin;
        LineRenderer.SetPosition(0, origin);
        LineRenderer.SetPosition(1, destination);
        DefensorHead.transform.position = origin;
        AttackerHead.transform.position = destination;
    }
}
