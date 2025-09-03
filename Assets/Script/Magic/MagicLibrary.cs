using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Magic/Magic Library")]
public class MagicLibrary : ScriptableObject
{
    public List<MagicData> magics;

    public MagicData GetMagic(string name)
    {
        return magics.Find(m => m.magicName == name);
    }
}
