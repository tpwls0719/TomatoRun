using UnityEngine;

public class Pill : MonoBehaviour
{
    public float boostDuration = 5f;      // 속도 부스트 지속 시간
    public float speedMultiplier = 2f;    // 속도 배수
    public float invincibleDuration = 3f; // 무적 지속 시간

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ✅ 속도 부스트 적용
                player.ActivateSpeedBoost(speedMultiplier, boostDuration);

                // ✅ 무적 상태 적용
                player.SetInvincible(invincibleDuration);
            }

            if (GameM.Instance != null)
            {
                GameM.Instance.PlayerCollectPill();
            }

            Destroy(gameObject); // 아이템 제거
        }
    }
}
