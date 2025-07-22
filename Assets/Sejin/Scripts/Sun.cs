using UnityEngine;

public class Sun : MonoBehaviour
{
    public int lifeBoost = 100;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 플레이어 체력 확장
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ExtendMaxHealth(lifeBoost);
            }

            // 2. 게임매니저에게 UI 업데이트 등 알리기
            if (GameM.Instance != null)
            {
                GameM.Instance.PlayerCollectSunlight();
            }

            // 3. 햇빛 아이템 제거
            Destroy(gameObject);
        }
    }
}
