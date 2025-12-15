using UnityEngine;

public class CharacterIdentity : MonoBehaviour
{
    public string UserId;

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
}
