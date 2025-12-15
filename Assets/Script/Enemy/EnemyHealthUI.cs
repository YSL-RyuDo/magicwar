//using UnityEngine;
//using UnityEngine.UI;

//public class EnemyHealthUI : MonoBehaviour
//{
//    public Slider healthSlider;
//    public Transform followTarget;
//    public Vector3 offset = new Vector3(0, 1.2f, 0);

//    void Update()
//    {
//        // 체력바가 적 위에 따라감
//        if (followTarget != null)
//        {
//            transform.position = followTarget.position + offset;
//            transform.rotation = Quaternion.identity; // 회전 고정 (UI가 안 돌아감)
//        }
//    }

//    public void SetHealthRatio(float ratio)
//    {
//        if (healthSlider != null)
//        {
//            healthSlider.value = ratio;
//        }
//    }
//}
