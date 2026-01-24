using UnityEngine;

public class PoisonMistProjectile : FieldMagicBase
{
    private void OnTriggerStay2D(Collider2D other)
    {
        CharacterIdentity target = other.GetComponent<CharacterIdentity>();
        if (target == null) return;

        SendPoisonDot(target);
    }

    private void SendPoisonDot(CharacterIdentity target)
    {
        NetworkConnector network = NetworkConnector.Instance;
        if (network == null) return;

        if (target.UserId == GameManager.Instance.OpponentUserId)
        {
            target.GetComponent<EnemyController>()?.PlayHurt();
        }

        DamageRequestPacket pkt = new DamageRequestPacket
        {
            attackerId = SessionData.UserID,
            targetId = target.UserId,
            cardId = runtimeData.ID,
            damage = 0 // 서버에서 DOT 처리
        };

        network.SendPacket(
            PacketType.Hit,
            NetworkConnector.StructToBytes(pkt)
        );
    }
}
