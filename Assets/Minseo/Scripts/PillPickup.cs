using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PillPickup : MonoBehaviour
{
    void Awake()
    {
        // 트리거로 설정
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어만
        if (!other.CompareTag("Player")) return;

        // 1) 무적 모드 발동
        var inv = other.GetComponent<InvincibilityItem>();
        if (inv != null)
            inv.ActivateInvincibility();

        // 2) UIManager 점수/아이콘 갱신
        if (UIManager.Instance != null)
            UIManager.Instance.CollectPill();

        // 3) 자신(알약) 비활성화
        gameObject.SetActive(false);
    }
}
