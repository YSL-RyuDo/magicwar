using UnityEngine;

[CreateAssetMenu(menuName = "MagicEffect/Heal")]
public class HealEffect : MagicEffect
{
    public GameObject healFxPrefab;

    public override void Apply(
        GameObject caster,
        GameObject target,
        CardRuntimeData runtime)
    {
        if (caster == null)
            return;

        // 서버에 힐 요청
        PKT_CAST_SELF_MAGIC pkt = new PKT_CAST_SELF_MAGIC
        {
            cardId = 8 // Heal 카드
        };

        NetworkConnector.Instance.SendPacket(
            PacketType.PKT_CAST_SELF_MAGIC,
            NetworkConnector.StructToBytes(pkt)
        );

        if (healFxPrefab != null)
        {
            GameObject fx = GameObject.Instantiate(
                healFxPrefab,
                caster.transform.position,
                Quaternion.identity
            );
        }
    }
}
