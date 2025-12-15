using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    private Dictionary<string, CharacterControllerBase> characters
        = new Dictionary<string, CharacterControllerBase>();

    private void Awake()
    {
        Instance = this;
    }

    public void Register(string userId, CharacterControllerBase character)
    {
        characters[userId] = character;
    }

    public void Unregister(string userId)
    {
        characters.Remove(userId);
    }

    public CharacterControllerBase GetCharacter(string userId)
    {
        characters.TryGetValue(userId, out var c);
        return c;
    }
}
