using UnityEngine;

public class ResolutionFix : MonoBehaviour
{
    void Awake()
    {
        Screen.SetResolution(576, 864, false);
    }
}
