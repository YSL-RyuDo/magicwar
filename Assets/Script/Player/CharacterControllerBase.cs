using UnityEngine;
using System.Collections;
public class CharacterControllerBase : MonoBehaviour
{
    protected bool isSlowed;
    protected bool isStunned;

    protected float slowRatio = 1f;

    Coroutine slowRoutine;
    Coroutine stunRoutine;

    public void ApplySlow(float ratio, int durationMs)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowCoroutine(ratio, durationMs));
    }

    IEnumerator SlowCoroutine(float ratio, int durationMs)
    {
        isSlowed = true;
        slowRatio = ratio;

        yield return new WaitForSeconds(durationMs / 1000f);

        isSlowed = false;
        slowRatio = 1f;
    }

    public void ApplyStun(int durationMs)
    {
        if (stunRoutine != null)
            StopCoroutine(stunRoutine);

        stunRoutine = StartCoroutine(StunCoroutine(durationMs));
    }

    IEnumerator StunCoroutine(int durationMs)
    {
        isStunned = true;

        yield return new WaitForSeconds(durationMs / 1000f);

        isStunned = false;
    }

}