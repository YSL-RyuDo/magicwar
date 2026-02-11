using UnityEngine;

[CreateAssetMenu(menuName = "MagicEffect/MagicWall")]
public class MagicWallEffect : MagicEffect
{
    public GameObject wallFx;

    public override void Apply(
        GameObject caster,
        GameObject target,
        CardRuntimeData runtime)
    {
        if (wallFx == null)
            return;

        Instantiate(
            wallFx,
            target.transform.position,
            Quaternion.identity
        );
    }
}
