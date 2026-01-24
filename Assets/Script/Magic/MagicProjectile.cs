using UnityEngine;

public abstract class MagicProjectile : MonoBehaviour
{
    public GameObject caster;
    protected CardData cardData;
    protected CardRuntimeData runtimeData;
    public CardRuntimeData RuntimeData => runtimeData;
    public GameObject Caster => caster;
    protected Vector2 direction;

    public virtual void Init(
        GameObject caster,
        Vector2 dir,
        CardData cardData,
        CardRuntimeData runtimeData)
    {
        this.caster = caster;
        this.direction = dir.normalized;
        this.cardData = cardData;
        this.runtimeData = runtimeData;

        Destroy(gameObject, 10f);
    }
}
