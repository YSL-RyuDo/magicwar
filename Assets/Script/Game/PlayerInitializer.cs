using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    private void Start()
    {
        CharacterIdentity identity = GetComponent<CharacterIdentity>();
        if (identity != null)
        {
            identity.UserId = SessionData.UserID;
        }
    }
}
