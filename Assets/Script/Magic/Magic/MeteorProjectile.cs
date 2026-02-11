using UnityEngine;

public class MeteorProjectile : MagicProjectile
{
    private Vector2 targetPos;
    private bool arrived = false;
    private bool hitTarget = false;
    public void SetTarget(Vector2 target)
    {
        targetPos = target;
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * runtimeData.speed * Time.deltaTime);

        if (hitTarget)
            return;

        if (!arrived &&
            Vector2.Distance(transform.position, targetPos) <= 0.1f)
        {
            arrived = true;

            GameManager.Instance.OnMeteorArrived(
                runtimeData.ID,
                targetPos,
                caster
            );

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterIdentity hit = other.GetComponent<CharacterIdentity>();
        if (hit == null) return;

        CharacterIdentity casterId =
            caster.GetComponent<CharacterIdentity>();

        if (casterId == null) return;
        if (hit.UserId == casterId.UserId) return;

        hitTarget = true;

        if (hit.UserId == GameManager.Instance.OpponentUserId)
        {
            //hit.GetComponent<EnemyController>()?.PlayHurt();
        }

        DamageRequestPacket pkt = new DamageRequestPacket
        {
            attackerId = casterId.UserId,
            targetId = hit.UserId,
            cardId = runtimeData.ID,
            damage = runtimeData.attack / 2
        };

        NetworkConnector.Instance.SendPacket(
            PacketType.Hit,
            NetworkConnector.StructToBytes(pkt)
        );

        Destroy(gameObject); // 맞았으면 즉시 소멸
    }
}
