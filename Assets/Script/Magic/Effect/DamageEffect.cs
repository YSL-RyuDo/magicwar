using UnityEngine;

[CreateAssetMenu(menuName = "MagicEffect/Damage")]
public class DamageEffect : MagicEffect
{
    public override void Apply(GameObject caster, GameObject target, CardRuntimeData runtime)
    {
        NetworkConnector network = NetworkConnector.Instance;
        if (network == null) return;

        var identity = target.GetComponent<CharacterIdentity>();
        if (identity == null) return;

        DamageRequestPacket pkt = new DamageRequestPacket
        {
            attackerId = SessionData.UserID,
            targetId = identity.UserId,
            cardId = runtime.ID,
            damage = runtime.attack
        };

        byte[] data = NetworkConnector.StructToBytes(pkt);

        // 클라 PacketType 기준
        network.SendPacket(PacketType.Hit, data);
    }
}
