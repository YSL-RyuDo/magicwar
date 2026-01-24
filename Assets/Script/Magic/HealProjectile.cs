using UnityEngine;

public class HealProjectile : MagicProjectile
{
    private float elapsed;
    private const float LIFE_TIME = 2f;

    private void Start()
    {
        elapsed = 0f;
    }

    private void Update()
    {
        if (caster != null)
        {
            transform.position =
                caster.transform.position + Vector3.up * 0.8f;
        }

        elapsed += Time.deltaTime;

        if (elapsed >= LIFE_TIME)
        {
            Destroy(gameObject);
        }
    }
}
