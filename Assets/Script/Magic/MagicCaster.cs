using UnityEngine;

public class MagicCaster : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private CardDatabase cardDB;

    public Transform FirePoint => firePoint;

    public void Cast(int cardId, Vector2 mouseWorld)
    {
        if (firePoint == null)
        {
            Debug.LogError("[MagicCaster] firePoint null");
            return;
        }

        // 1. 방향 계산
        Vector2 dir =
            (mouseWorld - (Vector2)firePoint.position).normalized;

        // 2. 카드 에셋 (프리팹용)
        CardData card = cardDB.GetCard(cardId);
        if (card == null)
        {
            Debug.LogError($"[MagicCaster] CardData not found id={cardId}");
            return;
        }

        // 3. 런타임 스탯 (서버 기준)
        CardRuntimeData stat =
            CardStatManager.Instance.GetStat(cardId);

        if (stat == null)
        {
            Debug.LogError($"[MagicCaster] RuntimeCardStat not found id={cardId}");
            return;
        }

        // 4. 투사체 생성
        GameObject projectile = Instantiate(
            card.playerPrefab,
            firePoint.position,
            Quaternion.identity
        );

        MagicProjectile proj = projectile.GetComponent<MagicProjectile>();

        if (proj == null)
        {
            Debug.LogError("[MagicCaster] MagicProjectile missing");
            Destroy(projectile);
            return;
        }

        proj.Init(
            caster: gameObject,
            dir: dir,
            cardData: card,
            runtimeData: new CardRuntimeData
            {
                ID = cardId,
                attack = stat.attack,
                speed = stat.speed,
                amount = stat.amount,
                duration = stat.duration
            }
        );

    }
}
