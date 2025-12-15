using UnityEngine;

public class FireballProjectile : MagicProjectile
{
    private void Update()
    {
        transform.position +=
            (Vector3)(direction * runtimeData.speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterIdentity hit = other.GetComponent<CharacterIdentity>();
        if (hit == null) return;

        CharacterIdentity casterId =
            caster.GetComponent<CharacterIdentity>();

        if (casterId == null) return;
        if (hit.UserId == casterId.UserId) return;

        foreach (var effect in cardData.effects)
            effect.Apply(caster, other.gameObject, runtimeData);

        Destroy(gameObject);
    }
}
