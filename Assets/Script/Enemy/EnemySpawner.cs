using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject healthBarPrefab;

    public void SpawnEnemy(Vector3 position)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        GameObject healthBar = Instantiate(healthBarPrefab);

        Enemy enemyScript = enemy.GetComponent<Enemy>();
        EnemyHealthUI healthUI = healthBar.GetComponent<EnemyHealthUI>();

        healthUI.followTarget = enemy.transform;
        enemyScript.healthUI = healthUI;
    }
}
