using UnityEngine;

public class MagicCaster : MonoBehaviour
{
    [Header("발사 위치")]
    public Transform firePoint;

    [Header("마법 슬롯에 등록된 마법 이름들 (QWER)")]
    public string[] selectedMagics = new string[4]; // Q, W, E, R

    [Header("마법 데이터 (이름 → 프리팹, 속도 등)")]
    public MagicLibrary magicLibrary;

    private string selectedMagic = null;

    void Start()
    {
        // 임시 테스트용 등록
        selectedMagics[0] = "Fireball";  // Q
        selectedMagics[1] = "Iceball";   // W
        selectedMagics[2] = "Lightning"; // E
        selectedMagics[3] = "IceField";    // R
    }

    void Update()
    {
        HandleMagicSelection();
        HandleMagicCasting();
    }

    void HandleMagicSelection()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedMagic = selectedMagics[0];
            Debug.Log($"[Q] 마법 선택됨: {selectedMagic}");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            selectedMagic = selectedMagics[1];
            Debug.Log($"[W] 마법 선택됨: {selectedMagic}");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            selectedMagic = selectedMagics[2];
            Debug.Log($"[E] 마법 선택됨: {selectedMagic}");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            selectedMagic = selectedMagics[3];
            Debug.Log($"[R] 마법 선택됨: {selectedMagic}");
        }
    }

    void HandleMagicCasting()
    {
        if (Input.GetMouseButtonDown(0) && selectedMagic != null)
        {
            Debug.Log($"[{selectedMagic}] 마법 발사!");
            CastMagic(selectedMagic);
            selectedMagic = null;
        }
    }

    void CastMagic(string magicName)
    {
        MagicData data = magicLibrary.GetMagic(magicName);
        if (data == null)
        {
            Debug.LogWarning($"'{magicName}' 마법이 라이브러리에 등록되지 않았습니다.");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 direction = (mouseWorldPos - firePoint.position).normalized;

        // 발사자에 따라 프리팹 선택
        GameObject prefabToUse = CompareTag("Player") ? data.playerPrefab : data.enemyPrefab;
        if (prefabToUse == null)
        {
            Debug.LogWarning($"{magicName}에 대한 프리팹이 존재하지 않습니다.");
            return;
        }

        GameObject magic = Instantiate(prefabToUse, firePoint.position, Quaternion.identity);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        magic.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Rigidbody2D rb = magic.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * data.projectileSpeed;
        }

        var magicProjectile = magic.GetComponent<IMagicProjectile>();
        if (magicProjectile != null)
        {
            magicProjectile.shooterTag = gameObject.tag;
            magicProjectile.SetDamage(data.damage);
            if (data.applySlow)
            {
                magicProjectile.SetSlowEffect(0.5f, 3f);
            }
        }


        Debug.Log($"{magicName} 발사됨!");
    }
}
