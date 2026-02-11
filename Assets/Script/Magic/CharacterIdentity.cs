using UnityEngine;

public class CharacterIdentity : MonoBehaviour
{
    public string UserId;
    public bool HasMagicBarrier { get;  set; }

    private void Start()
    {
        if (UserId == SessionData.UserID)
        {
            var caster = GetComponent<MagicCaster>();
            if (caster != null)
            {
                GameManager.Instance.RegisterMyCaster(caster);
            }
        }
    }

    public void SetMagicBarrier(bool value)
    {
        HasMagicBarrier = value;
    }
}
