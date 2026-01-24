using UnityEngine;

public class MeteorStrikeField : FieldMagicBase
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterIdentity target = other.GetComponent<CharacterIdentity>();
        if (target == null) return;

        DamageRequestPacket pkt = new DamageRequestPacket
        {
            attackerId = SessionData.UserID,
            targetId = target.UserId,
            cardId = runtimeData.ID,
            damage = runtimeData.attack   // 풀 데미지
        };

        NetworkConnector.Instance.SendPacket(
            PacketType.Hit,
            NetworkConnector.StructToBytes(pkt)
        );
    }
}
