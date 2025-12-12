//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System;

//public class GameSystem : MonoBehaviour
//{
//    [Header("Magic Buttons (순서: Q, W, E, R)")]
//    [SerializeField] private Button[] magicButtons = new Button[4];

//    [Header("Card Database")]
//    [SerializeField] private CardDatabase cardDB;

//    [Header("MagicCaster 참조")]
//    [SerializeField] private MagicCaster magicCaster;

//    private NetworkConnector network;

//    void Start()
//    {
//        network = FindFirstObjectByType<NetworkConnector>();

//        if (network != null)
//        {
//            // 서버에서 쿨타임 및 런타임 데이터 응답 등록
//            network.Dispatcher.Register(PacketType.ResponseUserMagicCooldowns, OnReceiveCooldowns);
//            network.Dispatcher.Register(PacketType.DamageResult, OnReceiveDamageResult);

//            string userId = SessionData.UserID;
//            //network.SendPacket(PacketType.RequestMagicCooldowns, userId);
//        }

//        // 버튼 클릭 이벤트 등록
//        for (int i = 0; i < magicButtons.Length; i++)
//        {
//            int index = i;
//            magicButtons[i].onClick.AddListener(() => OnClickMagicButton(index));
//        }
//    }

//    // 버튼 직접 눌렀을 때
//    public void OnClickMagicButton(int index)
//    {
//        if (magicCaster != null)
//        {
//            int cardId = magicCaster.selectedMagicIds[index];
//            if (cardId > 0)
//            {
//                Debug.Log($"[GameSystem] 버튼 클릭 → 카드 {cardId} 시전");
//                magicCaster.CastMagic(cardId);
//            }
//        }
//    }

//    private void OnReceiveCooldowns(byte[] body)
//    {
//        int offset = 0;

//        // 슬롯 개수 읽기
//        ushort slotCount = BitConverter.ToUInt16(body, offset);
//        offset += 2;

//        Debug.Log("[GameSystem] 받은 슬롯 개수: " + slotCount);

//        for (int slot = 0; slot < slotCount && slot < magicButtons.Length; slot++)
//        {
//            int structSize = Marshal.SizeOf<MagicSlotData>();

//            if (offset + structSize > body.Length)
//            {
//                Debug.LogError("[GameSystem] 슬롯 데이터 범위 초과");
//                break;
//            }

//            // 구조체 변환
//            MagicSlotData data = NetworkConnector.ByteArrayToStructure<MagicSlotData>(body, offset);
//            offset += structSize;

//            Debug.Log($"[Magic Slot {slot}] ID={data.id}, CD={data.cooldown}, ATK={data.attack}");

//            // 런타임 데이터 매핑
//            CardRuntimeData runtimeData = new CardRuntimeData
//            {
//                ID = data.id,
//                cooldown = data.cooldown,
//                attack = data.attack,
//                projectileSpeed = data.speed,
//                radius = data.radius,
//                duration = data.duration,
//                amount = data.amount
//            };

//            // MagicCaster 저장
//            if (magicCaster != null)
//                magicCaster.SetRuntimeData(data.id, runtimeData, slot);
//        }
//    }





//    private void OnReceiveDamageResult(byte[] body)
//    {
//        if (body.Length < Marshal.SizeOf<DamagePacket>())
//        {
//            Debug.LogError("[GameSystem] DamagePacket 크기 오류");
//            return;
//        }

//        DamagePacket pkt = NetworkConnector.ByteArrayToStructure<DamagePacket>(body);

//        string targetId = pkt.targetId.TrimEnd('\0');
//        int damage = pkt.damage;

//        Debug.Log($"[Damage] Target={targetId}, Damage={damage}");

//        // ------- 내 캐릭터 -------
//        if (SessionData.UserID == targetId)
//        {
//            var player = GameObject.FindWithTag("Player");
//            if (player != null)
//            {
//                var health = player.GetComponent<CharacterHealth>();
//                if (health != null)
//                    health.TakeDamage(damage);
//            }
//            return;
//        }

//        // ------- 상대 -------
//        if (SessionData.OpponentID == targetId)
//        {
//            var enemy = GameObject.FindWithTag("Enemy");
//            if (enemy != null)
//            {
//                var enemyComp = enemy.GetComponent<Enemy>();
//                if (enemyComp != null)
//                    enemyComp.TakeDamage(damage);
//            }
//            return;
//        }
//    }


//    // 카드 DB 검색
//    private CardData FindCardData(int id)
//    {
//        foreach (var card in cardDB.cards)
//        {
//            if (card.ID == id) return card;
//        }
//        Debug.LogWarning($"[GameSystem] Card ID {id} not found in database");
//        return null;
//    }
//}
