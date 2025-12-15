using UnityEngine;

public abstract class MagicEffect : ScriptableObject
{
    public abstract void Apply(GameObject caster, GameObject target, CardRuntimeData runtime);
}
