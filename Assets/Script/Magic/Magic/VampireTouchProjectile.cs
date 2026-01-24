using UnityEngine;

public class VampiricTouchProjectile : MagicProjectile
{
    private void Update()
    {
        // 투사체 이동
        transform.position +=
            (Vector3)(direction * runtimeData.speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterIdentity target = other.GetComponent<CharacterIdentity>();
        if (target == null) return;

        CharacterIdentity casterIdentity =
            caster.GetComponent<CharacterIdentity>();
        if (casterIdentity == null) return;

        // 자기 자신은 무시
        if (target.UserId == casterIdentity.UserId)
            return;

        if (target.UserId == GameManager.Instance.OpponentUserId)
        {
            // 상대방 피격 애니메이션
            target.GetComponent<EnemyController>()?.PlayHurt();
        }

        DamageRequestPacket pkt = new DamageRequestPacket
        {
            attackerId = casterIdentity.UserId,
            targetId = target.UserId,
            cardId = runtimeData.ID,
            damage = runtimeData.attack
        };

        NetworkConnector.Instance.SendPacket(
            PacketType.Hit,
            NetworkConnector.StructToBytes(pkt)
        );

        Destroy(gameObject);
    }

}
