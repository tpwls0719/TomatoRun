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

        // 이미 비활성화된 상태면 중복 처리 방지
        if (!gameObject.activeSelf) return;

        Debug.Log($"PillPickup: {gameObject.name} 충돌 감지 - InvincibilityItem에서 처리됨");
        
        // InvincibilityItem에서 모든 처리를 담당하므로 
        // 여기서는 별도 처리를 하지 않고 비활성화만 수행
        // (실제로는 InvincibilityItem에서 비활성화 처리됨)
    }
}
