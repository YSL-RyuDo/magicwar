using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    private void Start()
    {
        CharacterIdentity identity = GetComponent<CharacterIdentity>();
        if (identity != null)
        {
            identity.UserId = SessionData.OpponentID;
        }
    }
}
