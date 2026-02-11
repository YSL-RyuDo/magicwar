using UnityEngine;

[CreateAssetMenu(menuName = "MagicEffect/Blink")]
public class BlinkEffect : MagicEffect
{
    public GameObject blinkStartFx;
    public GameObject blinkEndFx;
    public float maxRange = 5f;

    public override void Apply(
        GameObject caster,
        GameObject target,
        CardRuntimeData runtime)
    {
        if (caster == null)
            return;

        Vector2 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 startPos = caster.transform.position;
        Vector2 dir = mouseWorld - startPos;

        if (dir.magnitude > maxRange)
            dir = dir.normalized * maxRange;

        Vector2 blinkPos = startPos + dir;

        if (blinkStartFx != null)
            Instantiate(blinkStartFx, startPos, Quaternion.identity);

        PKT_CAST_BLINK pkt = new PKT_CAST_BLINK
        {
            x = blinkPos.x,
            y = blinkPos.y
        };

        NetworkConnector.Instance.SendPacket(
            PacketType.CastBlink,
            NetworkConnector.StructToBytes(pkt)
        );

        if (blinkEndFx != null)
            Instantiate(blinkEndFx, blinkPos, Quaternion.identity);
    }
}
