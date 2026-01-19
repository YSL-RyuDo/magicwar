using UnityEngine;

[CreateAssetMenu(menuName = "CardEffect/Slow")]
public class SlowEffect : MagicEffect
{
    public override void Apply(
    GameObject caster,
    GameObject target,
    CardRuntimeData runtimeData)
    {
        IMovable movable =
            target.GetComponentInParent<IMovable>();

        if (movable == null)
        {
            Debug.Log("[SLOW] IMovable not found");
            return;
        }

        float slowRatio =
            Mathf.Clamp01(runtimeData.amount / 100f);

        float duration = runtimeData.duration;

        Debug.Log($"[SLOW] ratio={slowRatio} duration={duration}");

        movable.ApplySlow(slowRatio, duration);
    }

}
