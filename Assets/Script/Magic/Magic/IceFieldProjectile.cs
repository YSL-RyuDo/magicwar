using UnityEngine;

public class IceFieldProjectile : FieldMagicBase
{
    private void OnTriggerStay2D(Collider2D other)
    {
        CharacterIdentity target = other.GetComponent<CharacterIdentity>();
        if (target == null) return;

        SendIceFieldSlow(target);
    }

    private void SendIceFieldSlow(CharacterIdentity target)
    {
        NetworkConnector network = NetworkConnector.Instance;
        if (network == null) return;

        DamageRequestPacket pkt = new DamageRequestPacket
        {
            attackerId = SessionData.UserID,
            targetId = target.UserId,
            cardId = runtimeData.ID,
            damage = 0
        };

        network.SendPacket(
            PacketType.Hit,
            NetworkConnector.StructToBytes(pkt)
        );
    }
}
