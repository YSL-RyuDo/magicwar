using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameSystem : MonoBehaviour
{
    [Header("Magic Buttons (순서: Q, W, E, R)")]
    [SerializeField] private Button[] magicButtons = new Button[4];

    [Header("Card Database")]
    [SerializeField] private CardDatabase cardDB;

    [Header("MagicCaster 참조")]
    [SerializeField] private MagicCaster magicCaster;

    private NetworkConnector network;

    void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        if (network != null)
        {
            // 서버에서 쿨타임 및 런타임 데이터 응답 등록
            network.Dispatcher.Register(PacketType.ResponseUserMagicCooldowns, OnReceiveCooldowns);
            network.Dispatcher.Register(PacketType.DamageResult, OnReceiveDamageResult);

            string userId = SessionData.UserID;
            network.SendPacket(PacketType.RequestMagicCooldowns, userId);
        }

        // 버튼 클릭 이벤트 등록
        for (int i = 0; i < magicButtons.Length; i++)
        {
            int index = i;
            magicButtons[i].onClick.AddListener(() => OnClickMagicButton(index));
        }
    }

    // 버튼 직접 눌렀을 때
    public void OnClickMagicButton(int index)
    {
        if (magicCaster != null)
        {
            int cardId = magicCaster.selectedMagicIds[index];
            if (cardId > 0)
            {
                Debug.Log($"[GameSystem] 버튼 클릭 → 카드 {cardId} 시전");
                magicCaster.CastMagic(cardId);
            }
        }
    }

    // 서버에서 마법 데이터 수신
    private void OnReceiveCooldowns(string msg)
    {
        Debug.Log("[GameSystem] 받은 카드 데이터: " + msg);
        // 예시 포맷:
        // "1:3:50:10:0:0:1,5:5:20:8:2:3:1"
        // ID:Cooldown:Attack:Speed:Radius:Duration:Amount

        string[] cards = msg.Split(',');
        for (int slot = 0; slot < cards.Length && slot < magicButtons.Length; slot++)
        {
            string[] parts = cards[slot].Split(':');
            if (parts.Length < 7) continue;

            int id = int.Parse(parts[0]);
            int cooldown = int.Parse(parts[1]);
            int attack = int.Parse(parts[2]);
            float speed = float.Parse(parts[3]);
            float radius = float.Parse(parts[4]);
            float duration = float.Parse(parts[5]);
            int amount = int.Parse(parts[6]);

            // 런타임 데이터 객체 생성
            CardRuntimeData runtimeData = new CardRuntimeData
            {
                ID = id,
                cooldown = cooldown,
                attack = attack,
                projectileSpeed = speed,
                radius = radius,
                duration = duration,
                amount = amount
            };

            // MagicCaster에 등록
            if (magicCaster != null)
                magicCaster.SetRuntimeData(id, runtimeData, slot);

            // 버튼 UI 업데이트
            //var card = FindCardData(id);
            //if (card != null)
            //{
            //    magicButtons[slot].image.sprite = card.cardImage;

            //    // 쿨타임 표시
            //    Text cooldownText = magicButtons[slot].GetComponentInChildren<Text>();
            //    if (cooldownText != null)
            //        cooldownText.text = cooldown.ToString() + "s";
            //}
        }
    }

    private void OnReceiveDamageResult(string msg)
    {
        // msg: "targetUserId,damage"
        string[] tokens = msg.Split(',');
        if (tokens.Length < 2) return;

        string targetId = tokens[0];
        int damage = int.Parse(tokens[1]);

        if (SessionData.UserID == targetId)
        {
            // 내가 맞음
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var health = player.GetComponent<CharacterHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage); // 기존 메서드 그대로 사용
                }
            }
        }
        else if (SessionData.OpponentID == targetId)
        {
            // 상대방 맞음
            var enemy = GameObject.FindWithTag("Enemy");
            if (enemy != null)
            {
                var enemyComp = enemy.GetComponent<Enemy>();
                if (enemyComp != null)
                {
                    enemyComp.TakeDamage(damage); // 기존 메서드 그대로 사용
                }
            }
        }
    }

    // 카드 DB 검색
    private CardData FindCardData(int id)
    {
        foreach (var card in cardDB.cards)
        {
            if (card.ID == id) return card;
        }
        Debug.LogWarning($"[GameSystem] Card ID {id} not found in database");
        return null;
    }
}
