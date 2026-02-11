using UnityEngine;

[CreateAssetMenu(menuName = "MagicEffect/MagicBarrier")]
public class MagicBarrierEffect : MagicEffect
{
    public override void Apply(
        GameObject caster,
        GameObject target,
        CardRuntimeData runtime)
    {
        if (caster == null)
            return;

        PKT_CAST_SELF_MAGIC pkt = new PKT_CAST_SELF_MAGIC
        {
            cardId = 9
        };

        NetworkConnector.Instance.SendPacket(
            PacketType.PKT_CAST_SELF_MAGIC,
            NetworkConnector.StructToBytes(pkt)
        );
    }
}