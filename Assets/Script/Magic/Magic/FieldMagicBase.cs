using UnityEngine;

public abstract class FieldMagicBase : MagicProjectile, IFieldMagic
{
    protected virtual void Start()
    {
        ApplyVisualScale(runtimeData.radius);
        Destroy(gameObject, runtimeData.duration);
    }

    private void ApplyVisualScale(float radius)
    {
        // radius = 반지름 기준
        float diameter = radius * 0.05f;

        transform.localScale = new Vector3(
            diameter,
            diameter,
            1f
        );

        Debug.Log(
            $"[FieldMagic] {name} visualRadius={radius}, scale={transform.localScale}"
        );
    }
}
