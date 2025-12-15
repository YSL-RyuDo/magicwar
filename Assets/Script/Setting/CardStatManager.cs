using System.Collections.Generic;
using UnityEngine;

public class CardStatManager : MonoBehaviour
{
    public static CardStatManager Instance;

    private Dictionary<int, CardRuntimeData> statTable =
        new Dictionary<int, CardRuntimeData>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetStat(CardRuntimeData stat)
    {
        statTable[stat.ID] = stat;
    }

    public CardRuntimeData GetStat(int cardId)
    {
        return statTable.TryGetValue(cardId, out var stat)
            ? stat
            : null;
    }
}
