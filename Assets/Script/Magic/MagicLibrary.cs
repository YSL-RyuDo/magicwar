using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Magic/Magic Library")]
public class MagicLibrary : ScriptableObject
{
    public List<CardData> magics;

    public CardData GetMagic(string name)
    {
        return magics.Find(m => m.cardName == name);
    }
}
