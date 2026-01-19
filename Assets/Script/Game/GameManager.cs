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

    // Magic Cast + Cooldown
    //private void TryCastSelectedMagic()
    //{
    //    if (SelectedMagicSlot < 0)
    //        return;

    //    int cardId = EquippedMagicIds[SelectedMagicSlot];
    //    if (cardId < 0)
    //        return;

    //    if (!IsMagicReady(SelectedMagicSlot))
    //        return;

    //    CardRuntimeData stat =
    //        CardStatManager.Instance.GetStat(cardId);

    //    if (stat == null)
    //        return;

    //    Vector2 mouseWorld =
    //        Camera.main.ScreenToWorldPoint(Input.mousePosition);

    //    MyCaster.Cast(cardId, mouseWorld);

    //    var controller = MyCaster.GetComponent<CharacterController2D>();
    //    if (controller != null)
    //    {
    //        controller.ApplyCastLock(250); // 0.25초
    //    }

    //    Vector2 dir =
    //        (mouseWorld - (Vector2)MyCaster.FirePoint.position).normalized;

    //    PKT_CAST_MAGIC pkt = new PKT_CAST_MAGIC
    //    {
    //        cardId = cardId,
    //        dirX = dir.x,
    //        dirY = dir.y
    //    };

    //    network.SendPacket(
    //        PacketType.CastMagic,
    //        NetworkConnector.StructToBytes(pkt)
    //    );

    //    StartCooldown(SelectedMagicSlot, stat.cooldown);
    //}
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

        GameUIController.Instance?.UpdateHP(pkt.currentHp, pkt.maxHp);
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

        // 내가 쏜 마법은 로컬에서 이미 생성했으므로 무시
        if (casterId == MyUserId)
            return;

        CardData card = cardDB.GetCard(pkt.cardId);
        if (card == null)
            return;

        CharacterIdentity enemy =
            FindObjectsByType<CharacterIdentity>(FindObjectsSortMode.None)
            .FirstOrDefault(x => x.UserId == casterId);

        if (enemy == null)
            return;

        MagicCaster caster = enemy.GetComponent<MagicCaster>();
        if (caster == null)
            return;

        GameObject projObj = Instantiate(
            card.playerPrefab,               // 카드별 프리팹
            caster.FirePoint.position,
            Quaternion.identity
        );

        MagicProjectile proj =
            projObj.GetComponent<MagicProjectile>();

        if (proj == null)
        {
            Debug.LogError(
                $"[SpawnMagic] MagicProjectile not found on prefab {card.playerPrefab.name}"
            );
            Destroy(projObj);
            return;
        }

        // 방향은 서버 기준 그대로 사용
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
        PKT_SPAWN_FIELD_MAGIC pkt =
            NetworkConnector.ByteArrayToStructure<PKT_SPAWN_FIELD_MAGIC>(body);

        string casterId =
            pkt.casterId.TrimEnd('\0', ' ');

        // 내가 시전한 건 이미 로컬에서 생성했으므로 무시
        if (casterId == MyUserId)
            return;

        CardData card = cardDB.GetCard(pkt.cardId);
        if (card == null)
            return;

        // 상대 캐릭터 찾기
        CharacterIdentity enemy =
            FindObjectsByType<CharacterIdentity>(FindObjectsSortMode.None)
            .FirstOrDefault(x => x.UserId == casterId);

        if (enemy == null)
            return;

        EnemyController enemyCtrl =
            enemy.GetComponent<EnemyController>();

        if (enemyCtrl == null)
            return;

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

        GameObject obj = Instantiate(
            card.playerPrefab,
            worldPos,
            Quaternion.identity
        );

        MagicProjectile proj =
            obj.GetComponent<MagicProjectile>();

        if (proj == null)
        {
            Destroy(obj);
            return;
        }

        proj.Init(
            caster: enemy.gameObject,
            dir: Vector2.zero,
            cardData: card,
            runtimeData: new CardRuntimeData
            {
                ID = pkt.cardId,
                attack = pkt.attack,
                radius = pkt.radius,
                duration = pkt.duration,
                amount = pkt.amount
            }
        );
    }

}
