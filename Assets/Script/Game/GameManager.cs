using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public MagicCaster MyCaster { get; private set; }

    [Header("Runtime Data")]
    public int[] EquippedMagicIds = new int[4] { -1, -1, -1, -1 };

    // 슬롯별 쿨타임 종료 시각
    private float[] magicCooldownEndTime = new float[4];

    public int SelectedMagicSlot { get; private set; } = -1;

    public string MyUserId { get; private set; }
    public string OpponentUserId { get; private set; }

    private NetworkConnector network;

    [SerializeField] private CardDatabase cardDB;
    private int lastEnemyHp = -1;
    private int lastMyHp = -1;

    // Life Cycle
    private void Awake()
    {
        Screen.SetResolution(576, 864, false);
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        network = FindFirstObjectByType<NetworkConnector>();

        MyUserId = SessionData.UserID;
        OpponentUserId = SessionData.OpponentID;

        RegisterPacketHandlers();

        RequestUserMagic();
        RequestHP();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) SelectMagic(0);
        if (Input.GetKeyDown(KeyCode.W)) SelectMagic(1);
        if (Input.GetKeyDown(KeyCode.E)) SelectMagic(2);
        if (Input.GetKeyDown(KeyCode.R)) SelectMagic(3);

        if (Input.GetMouseButtonDown(0))
        {
            TryCastSelectedMagic();
        }
    }

    // MyCaster
    public void RegisterMyCaster(MagicCaster caster)
    {
        if (MyCaster != null) return;
        MyCaster = caster;
    }

    // Magic Select

    private void SelectMagic(int slotIndex)
    {
        int cardId = EquippedMagicIds[slotIndex];
        if (cardId < 0) return;

        SelectedMagicSlot = slotIndex;
    }

    public void OnMagicButtonPressed(int slotIndex)
    {
        SelectMagic(slotIndex);
    }

    private void TryCastSelectedMagic()
    {
        if (SelectedMagicSlot < 0)
            return;

        int cardId = EquippedMagicIds[SelectedMagicSlot];
        if (cardId < 0)
            return;

        if (!IsMagicReady(SelectedMagicSlot))
            return;

        // 런타임 스탯 (서버에서 받은 것)
        CardRuntimeData stat =
            CardStatManager.Instance.GetStat(cardId);
        if (stat == null)
            return;

        CardData card = cardDB.GetCard(cardId);
        if (card == null)
            return;

        Vector2 mouseWorld =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        bool isFieldMagic =
            card.playerPrefab.GetComponent<IFieldMagic>() != null;

        MyCaster.Cast(cardId, mouseWorld);

        MyCaster.GetComponent<CharacterController2D>()
            ?.ApplyCastLock(250);

        if (isFieldMagic)
        {
            Transform myAnchor =
                FindFirstObjectByType<CharacterController2D>()?.transform;

            if (myAnchor == null)
                return;

            Vector2 local =
                mouseWorld - (Vector2)myAnchor.position;

            PKT_CAST_FIELD_MAGIC pkt = new PKT_CAST_FIELD_MAGIC
            {
                cardId = cardId,
                localX = local.x,
                localY = local.y
            };

            network.SendPacket(
                PacketType.CastFieldMagic,
                NetworkConnector.StructToBytes(pkt)
            );
        }
        else
        {
            Vector2 dir =
                (mouseWorld - (Vector2)MyCaster.FirePoint.position).normalized;

            PKT_CAST_MAGIC pkt = new PKT_CAST_MAGIC
            {
                cardId = cardId,
                dirX = dir.x,
                dirY = dir.y
            };

            network.SendPacket(
                PacketType.CastMagic,
                NetworkConnector.StructToBytes(pkt)
            );
        }

        StartCooldown(SelectedMagicSlot, stat.cooldown);
    }

    private bool IsMagicReady(int slot)
    {
        return Time.time >= magicCooldownEndTime[slot];
    }

    private void StartCooldown(int slot, float cooldown)
    {
        magicCooldownEndTime[slot] = Time.time + cooldown;
    }

    // UI에서 직접 호출하는 함수들
    public float GetCooldownRemain(int slot)
    {
        return Mathf.Max(0f, magicCooldownEndTime[slot] - Time.time);
    }

    public float GetCooldownTotal(int slot)
    {
        int cardId = EquippedMagicIds[slot];
        if (cardId < 0) return 0f;

        CardRuntimeData stat =
            CardStatManager.Instance.GetStat(cardId);

        return stat != null ? stat.cooldown : 0f;
    }

    // Packet Register
    private void RegisterPacketHandlers()
    {
        if (network == null) return;

        network.Dispatcher.Register(PacketType.Die, OnDie);
        network.Dispatcher.Register(PacketType.Victory, OnVictory);
        network.Dispatcher.Register(PacketType.Defeat, OnDefeat);

        network.Dispatcher.Register(PacketType.SpawnMagic, OnSpawnMagic);
        network.Dispatcher.Register(PacketType.SpawnFieldMagic, OnSpawnFieldMagic);

        network.Dispatcher.Register(PacketType.ResponseUserMagic, OnReceiveUserMagic);
        network.Dispatcher.Register(PacketType.ResponseHP, OnReceiveHP);
        network.Dispatcher.Register(PacketType.ResponseMagicDetail, OnReceiveMagicDetail);
        network.Dispatcher.Register(PacketType.PKT_REMOVE_PROJECTILE,OnRemoveProjectile);

    }

    private void OnDestroy()
    {
        if (network == null) return;

        network.Dispatcher.Unregister(PacketType.Die, OnDie);
        network.Dispatcher.Unregister(PacketType.Victory, OnVictory);
        network.Dispatcher.Unregister(PacketType.Defeat, OnDefeat);

        network.Dispatcher.Unregister(PacketType.SpawnMagic, OnSpawnMagic);
        network.Dispatcher.Unregister(PacketType.ResponseUserMagic, OnReceiveUserMagic);
        network.Dispatcher.Unregister(PacketType.ResponseHP, OnReceiveHP);
        network.Dispatcher.Unregister(PacketType.PKT_RES_MAGIC_DETAIL, OnReceiveMagicDetail);
        network.Dispatcher.Unregister(PacketType.PKT_REMOVE_PROJECTILE, OnRemoveProjectile);
    }

    // HP
    private void RequestHP()
    {
        network.SendPacket(PacketType.RequestHP, System.Array.Empty<byte>());
    }

    private void OnReceiveHP(byte[] body)
    {
        PKT_RES_HP_STRUCT pkt =
            NetworkConnector.ByteArrayToStructure<PKT_RES_HP_STRUCT>(body);

        string id = pkt.userId.TrimEnd('\0', ' ');

        if (id == MyUserId)
        {
            GameUIController.Instance?.UpdateHP(pkt.currentHp, pkt.maxHp);

            if (pkt.currentHp < lastMyHp)
                MyCaster.GetComponent<Animator>()?.SetTrigger("hurt");

            lastMyHp = pkt.currentHp;
        }
        else if (id == OpponentUserId)
        {
            var enemy =
                FindObjectsByType<CharacterIdentity>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.UserId == id);

            if (enemy != null && pkt.currentHp < lastEnemyHp)
                enemy.GetComponent<EnemyController>()?.PlayHurt();

            lastEnemyHp = pkt.currentHp;
        }
    }


    // User Magic
    private void RequestUserMagic()
    {
        network.SendPacket(PacketType.RequestUserMagic, System.Array.Empty<byte>());
    }

    private void OnReceiveUserMagic(byte[] body)
    {
        PKT_RES_USER_MAGIC pkt =
            NetworkConnector.ByteArrayToStructure<PKT_RES_USER_MAGIC>(body);

        for (int i = 0; i < 4; i++)
        {
            EquippedMagicIds[i] = pkt.cardIds[i];

            if (pkt.cardIds[i] >= 0)
                RequestMagicDetail(pkt.cardIds[i]);
        }

        GameUIController.Instance?.RefreshMagicSlots();
    }

    private void RequestMagicDetail(int cardId)
    {
        PKT_REQ_MAGIC_DETAIL pkt =
            new PKT_REQ_MAGIC_DETAIL { cardId = cardId };

        network.SendPacket(
            PacketType.PKT_REQ_MAGIC_DETAIL,
            NetworkConnector.StructToBytes(pkt)
        );
    }

    private void OnReceiveMagicDetail(byte[] body)
    {
        PKT_RES_MAGIC_DETAIL_GAME_STRUCT pkt =
            NetworkConnector.ByteArrayToStructure<PKT_RES_MAGIC_DETAIL_GAME_STRUCT>(body);

        CardRuntimeData runtime = new CardRuntimeData
        {
            ID = pkt.cardId,
            attack = pkt.attack,
            speed = pkt.speed,
            cooldown = pkt.cooldown,
            radius = pkt.radius,
            duration = pkt.duration,
            amount = pkt.amount
        };

        CardStatManager.Instance.SetStat(runtime);
    }

    // Enemy Magic Spawn
    private void OnSpawnMagic(byte[] body)
    {
        PKT_SPAWN_MAGIC pkt =
            NetworkConnector.ByteArrayToStructure<PKT_SPAWN_MAGIC>(body);

        string casterId = pkt.casterId.TrimEnd('\0', ' ');

        // 내가 쏜 마법은 무시
        if (casterId == MyUserId)
            return;

        CharacterIdentity enemy =
            FindObjectsByType<CharacterIdentity>(FindObjectsSortMode.None)
            .FirstOrDefault(x => x.UserId == casterId);

        if (enemy == null)
            return;

        enemy.GetComponent<EnemyController>()?.PlayAttack();

        CardData card = cardDB.GetCard(pkt.cardId);
        if (card == null)
            return;

        MagicCaster caster = enemy.GetComponent<MagicCaster>();
        if (caster == null)
            return;

        GameObject projObj = Instantiate(
            card.playerPrefab,
            caster.FirePoint.position,
            Quaternion.identity
        );

        MagicProjectile proj = projObj.GetComponent<MagicProjectile>();
        if (proj == null)
        {
            Destroy(projObj);
            return;
        }

        Vector2 dir = new Vector2(-pkt.dirX, -pkt.dirY).normalized;

        proj.Init(
            enemy.gameObject,
            dir,
            card,
            new CardRuntimeData
            {
                ID = pkt.cardId,
                attack = pkt.attack,
                speed = pkt.projectileSpeed
            }
        );
    }

    // Result
    private void OnDie(byte[] body)
    {
        PKT_DIE pkt =
            NetworkConnector.ByteArrayToStructure<PKT_DIE>(body);

        string deadId = pkt.deadUserId.TrimEnd('\0', ' ');

        if (deadId == MyUserId)
        {
            MyCaster.GetComponent<CharacterController2D>()?.Die();
        }
        else
        {
            var enemy =
                FindObjectsByType<CharacterIdentity>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.UserId == deadId);

            enemy?.GetComponent<EnemyController>()?.Die();
        }
    }

    private void OnVictory(byte[] body)
    {
        GameUIController.Instance?.ShowVictoryUI();
    }

    private void OnDefeat(byte[] body)
    {
        GameUIController.Instance?.ShowDefeatUI();
    }

    private void OnSpawnFieldMagic(byte[] body)
    {
        Debug.Log("[필드 생성] 패킷 수신 시작");

        if (body == null || body.Length == 0)
        {
            Debug.LogError("[필드 생성] 패킷 바디가 비어있음");
            return;
        }

        PKT_SPAWN_FIELD_MAGIC pkt =
            NetworkConnector.ByteArrayToStructure<PKT_SPAWN_FIELD_MAGIC>(body);

        string casterId = pkt.casterId.TrimEnd('\0', ' ');
        Debug.Log($"[필드 생성] casterId = {casterId}");

        Debug.Log($"[필드 생성] 내 ID = {MyUserId}");

        if (casterId == MyUserId)
        {
            Debug.Log("[필드 생성] 자기 자신 패킷 → 현재 코드에서 무시됨");
            return;
        }

        Debug.Log($"[필드 생성] cardId = {pkt.cardId}");

        CardData fieldCard = cardDB.GetCard(pkt.cardId);
        if (fieldCard == null)
        {
            Debug.LogError("[필드 생성] CardData를 찾지 못함");
            return;
        }

        Debug.Log($"[필드 생성] 카드 이름 = {fieldCard.name}");
        Debug.Log($"[필드 생성] 프리팹 = {fieldCard.playerPrefab?.name}");

        CharacterIdentity enemy =
            FindObjectsByType<CharacterIdentity>(FindObjectsSortMode.None)
            .FirstOrDefault(x => x.UserId == casterId);

        if (enemy == null)
        {
            Debug.LogError("[필드 생성] CharacterIdentity(적)을 찾지 못함");
            return;
        }

        Debug.Log($"[필드 생성] 적 오브젝트 = {enemy.gameObject.name}");

        EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
        if (enemyCtrl == null)
        {
            Debug.LogError("[필드 생성] EnemyController가 없음");
            return;
        }

        Debug.Log($"[필드 생성] 적 진영 = {enemyCtrl.enemySide}");

        Debug.Log(
            $"[필드 생성] local 좌표 = ({pkt.localX}, {pkt.localY})"
        );

        Vector3 worldPos;
        if (enemyCtrl.enemySide == TeamSide.Top)
        {
            worldPos = enemyCtrl.transform.position +
                       new Vector3(-pkt.localX, -pkt.localY, 0f);
        }
        else
        {
            worldPos = enemyCtrl.transform.position +
                       new Vector3(pkt.localX, pkt.localY, 0f);
        }

        Debug.Log(
            $"[필드 생성] 월드 좌표 = {worldPos}"
        );

        GameObject obj = Instantiate(
            fieldCard.playerPrefab,
            worldPos,
            Quaternion.identity
        );

        Debug.Log($"[필드 생성] 프리팹 인스턴스 생성됨 = {obj.name}");

        MagicProjectile proj = obj.GetComponent<MagicProjectile>();
        if (proj == null)
        {
            Debug.LogError("[필드 생성] MagicProjectile 컴포넌트가 없음");
            Destroy(obj);
            return;
        }

        Debug.Log("[필드 생성] MagicProjectile Init 호출");

        proj.Init(
            caster: enemy.gameObject,
            dir: Vector2.zero,
            cardData: fieldCard,
            runtimeData: new CardRuntimeData
            {
                ID = pkt.cardId,
                attack = pkt.attack,
                radius = pkt.radius,
                duration = pkt.duration,
                amount = pkt.amount
            }
        );

        Debug.Log("[필드 생성] 필드 생성 완료");
    }

    public void OnMeteorArrived(int cardId, Vector2 worldPos, GameObject casterObj)
    {
        int fieldCardId = cardId;
        if (cardId == 5)          // MeteorStrike
            fieldCardId = 12;

        CardData fieldCard = cardDB.GetCard(fieldCardId);
        if (fieldCard == null)
            return;

        GameObject fieldObj = Instantiate(
        fieldCard.playerPrefab,   // ← MeteorStrikeField 프리팹
        worldPos,
        Quaternion.identity
    );

        MagicProjectile fieldProj = fieldObj.GetComponent<MagicProjectile>();
        if (fieldProj != null)
        {
            fieldProj.Init(
                casterObj,
                Vector2.zero,
                fieldCard,
                CardStatManager.Instance.GetStat(cardId)
            );
        }

        Vector2 casterPos = casterObj.transform.position;
        Vector2 local = worldPos - casterPos;

        PKT_CAST_FIELD_MAGIC pkt = new PKT_CAST_FIELD_MAGIC
        {
            cardId = cardId,
            localX = local.x,
            localY = local.y
        };

        network.SendPacket(
            PacketType.CastFieldMagic,
            NetworkConnector.StructToBytes(pkt)
        );
    }

    private void OnRemoveProjectile(byte[] body)
    {
        PKT_REMOVE_PROJECTILE pkt =
            NetworkConnector.ByteArrayToStructure<PKT_REMOVE_PROJECTILE>(body);

        string casterId = pkt.casterId.TrimEnd('\0', ' ');

        foreach (var proj in
            FindObjectsByType<MagicProjectile>(FindObjectsSortMode.None))
        {
            var id = proj.Caster
                ?.GetComponent<CharacterIdentity>()
                ?.UserId;

            if (id == casterId &&
                proj.RuntimeData.ID == pkt.cardId)
            {
                Destroy(proj.gameObject);
            }
        }
    }

}
