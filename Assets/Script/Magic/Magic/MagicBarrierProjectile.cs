using UnityEngine;

public class MagicBarrierProjectile : MonoBehaviour
{
    private float elapsed;
    private const float LIFE_TIME = 5f;
    private Transform caster;

    public void Init(GameObject casterObj)
    {
        caster = casterObj.transform;
    }

    private void Update()
    {
        if (caster != null)
        {
            transform.position =
                caster.position + Vector3.up * 0.8f;
        }

        elapsed += Time.deltaTime;
        if (elapsed >= LIFE_TIME)
        {
            Destroy(gameObject);
        }
    }
}
