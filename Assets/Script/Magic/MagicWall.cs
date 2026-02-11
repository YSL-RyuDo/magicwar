using UnityEngine;

public class MagicWall : MagicProjectile
{
    public override void Init(
        GameObject caster,
        Vector2 dir,
        CardData cardData,
        CardRuntimeData runtimeData)
    {
        // MagicProjectile 기본 초기화는 유지
        base.Init(caster, Vector2.zero, cardData, runtimeData);

        // 벽은 방향, 이동 없음
        // 수명은 카드 duration 기준으로 다시 설정
        CancelInvoke();
        Destroy(gameObject, runtimeData.duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MagicProjectile proj =
            other.GetComponent<MagicProjectile>();

        if (proj == null)
            return;

        // 시전자 본인 투사체면 무시하고 싶으면 여기서 체크 가능
        if (proj.Caster == this.caster)
            return;

        Destroy(proj.gameObject);
    }
}
