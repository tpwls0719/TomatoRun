using UnityEngine;

public class Water : MonoBehaviour
{
    public int scoreValue = 100;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("💧 Player가 물과 충돌 - 점수 증가 + 오브젝트 비활성화");

            // UIManager를 통한 점수 추가
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectWaterDrop();
            }
            
            // 게임 오브젝트 비활성화 (오브젝트 풀링 사용)
            gameObject.SetActive(false);
        }
    }
}
