using UnityEngine;

public class MagicCaster : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private CardDatabase cardDB;

    private CharacterController2D controller;
    public Transform FirePoint => firePoint;

    private void Awake()
    {
        controller = GetComponent<CharacterController2D>();
    }

    public void Cast(int cardId, Vector2 mouseWorld)
    {
        CardData card = cardDB.GetCard(cardId);
        if (card == null)
            return;

        CardRuntimeData stat =
            CardStatManager.Instance.GetStat(cardId);
        if (stat == null)
            return;

        controller?.PlayAttack();

        // MeteorStrike는 무조건 투사체
        bool isMeteor = card.ID == 5;

        bool isFieldMagic =
            !isMeteor &&
            card.playerPrefab.GetComponent<IFieldMagic>() != null;

        if (isFieldMagic)
            CastField(card, stat, mouseWorld);
        else
            CastProjectile(card, stat, mouseWorld);
    }


    private void CastProjectile(
    CardData card,
    CardRuntimeData stat,
    Vector2 mouseWorld)
    {
        Vector2 dir =
            (mouseWorld - (Vector2)firePoint.position).normalized;

        GameObject obj = Instantiate(
            card.playerPrefab,
            firePoint.position,
            Quaternion.identity
        );

        MagicProjectile proj = obj.GetComponent<MagicProjectile>();
        if (proj == null)
        {
            Destroy(obj);
            return;
        }

        proj.Init(
            caster: gameObject,
            dir: dir,
            cardData: card,
            runtimeData: new CardRuntimeData
            {
                ID = card.ID,
                attack = stat.attack,
                speed = stat.speed,
                radius = stat.radius,
                duration = stat.duration,
                amount = stat.amount
            }
        );

        MeteorProjectile meteor = proj as MeteorProjectile;
        if (meteor != null)
        {
            meteor.SetTarget(mouseWorld);
        }
    }

    private void CastField(
        CardData card,
        CardRuntimeData stat,
        Vector2 pos)
    {
        GameObject obj = Instantiate(
            card.playerPrefab,
            pos,
            Quaternion.identity
        );

        MagicProjectile proj = obj.GetComponent<MagicProjectile>();
        if (proj == null)
        {
            Destroy(obj);
            return;
        }

        proj.Init(
            caster: gameObject,
            dir: Vector2.zero,
            cardData: card,
            runtimeData: new CardRuntimeData
            {
                ID = card.ID,
                attack = stat.attack,
                speed = 0f,
                radius = stat.radius,
                duration = stat.duration,
                amount = stat.amount
            }
        );
    }
}
