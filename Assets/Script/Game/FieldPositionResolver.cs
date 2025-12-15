using UnityEngine;

public enum TeamSide
{
    Bottom,
    Top
}

public class FieldPositionResolver : MonoBehaviour
{
    public Transform bottomAnchor;
    public Transform topAnchor;

    public Vector3 Resolve(TeamSide side, float x, float y)
    {
        Vector3 local = new Vector3(x, y, 0f);

        if (side == TeamSide.Top)
        {
            // Top은 시점만 뒤집고, 기준은 Top Anchor
            local.y = -local.y;
            local.x = -local.x;
            return topAnchor.position + local;
        }
        else
        {
            return bottomAnchor.position + local;
        }
    }
}
