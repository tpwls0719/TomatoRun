using UnityEngine;
using System.Collections;

public class InvincibilityItem : MonoBehaviour
{
    [Header("무적 효과 설정")]
    public float invincibilityDuration = 5f;      // 무적 지속 시간 (초)
    
    [Header("효과음")]
    public AudioClip invincibilityStartSound;     // 무적 시작 효과음
    public AudioClip invincibilityEndSound;       // 무적 종료 효과음
    
    [Header("무적 모드 텍스트")]
    public string invincibilityTextName = "InvincibilityText";  // 플레이어 자식 무적 텍스트 오브젝트 이름
    
    [Header("알약 이펙트")]
    public string pillEffectName = "PillEffect_01";  // 플레이어 자식 오브젝트 이름
    
    private GameObject activePillEffect;          // 현재 활성화된 알약 이펙트
    private GameObject invincibilityTextObject;   // 현재 활성화된 무적 텍스트 오브젝트
    
    private bool isInvincible = false;            // 현재 무적 상태인지
    private AudioSource audioSource;              // 오디오 소스
    
    // 다른 스크립트에서 무적 상태 확인용 프로퍼티
    public bool IsInvincible { get { return isInvincible; } }
    
    void Start()
    {
        // 오디오 소스 컴포넌트 찾기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // 없으면 새로 추가
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 무적 모드 텍스트 찾기 및 초기화
        FindInvincibilityText();
    }
    
    void Update()
    {
        // 테스트용 코드 (실제 게임에서는 주석 처리하거나 제거)
        if (Input.GetKeyDown(KeyCode.I) && !isInvincible)
        {
            ActivateInvincibility();
        }
    }
    
    // 현재 실행 중인 무적 코루틴 추적
    private Coroutine currentInvincibilityRoutine = null;
    
    // 무적 상태 활성화 메서드 (외부에서 호출 가능)
    public void ActivateInvincibility()
    {
        // 이미 실행 중인 무적 코루틴이 있으면 중지
        if (currentInvincibilityRoutine != null)
        {
            StopCoroutine(currentInvincibilityRoutine);
            currentInvincibilityRoutine = null;
            Debug.Log("기존 무적 상태 중지 후 새로 시작");
        }
        
        // 새로운 무적 코루틴 시작
        currentInvincibilityRoutine = StartCoroutine(InvincibilityRoutine());
        
        Debug.Log("무적 상태 활성화 - 지속 시간: " + invincibilityDuration + "초");
    }
    
    // 무적 상태 처리 코루틴
    private IEnumerator InvincibilityRoutine()
    {
        // 무적 상태 시작
        isInvincible = true;
        
        // 효과음 재생
        if (audioSource != null && invincibilityStartSound != null)
        {
            audioSource.PlayOneShot(invincibilityStartSound);
        }
        
        // 텍스트 활성화 (애니메이션 없음)
        ActivateInvincibilityText();
        
        // 돌진 이펙트 활성화
        ActivatePillEffect();
        
        // 무적 시간 대기
        yield return new WaitForSeconds(invincibilityDuration);
        
        // 무적 상태 종료
        isInvincible = false;
        currentInvincibilityRoutine = null; // 코루틴 참조 해제
        
        // 돌진 이펙트 비활성화
        DeactivatePillEffect();
        
        // 무적 모드 텍스트 비활성화
        DeactivateInvincibilityText();
        
        // 종료 효과음
        if (audioSource != null && invincibilityEndSound != null)
        {
            audioSource.PlayOneShot(invincibilityEndSound);
        }
        
        Debug.Log("무적 상태 종료");
    }
    
    [Header("장애물 충돌 효과")]
    public float obstacleKnockbackForce = 10f;    // 장애물이 날아가는 힘
    public float obstacleRotationForce = 300f;    // 장애물이 회전하는 힘
    public float obstacleDestroyDelay = 2f;       // 장애물이 날아간 후 사라지는 시간
    
    // 충돌 처리 - 아이템 획득 기능
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Pill" 태그를 가진 아이템과 충돌했을 때
        if (other.CompareTag("Pill"))
        {
            // 이미 비활성화된 아이템과 다시 충돌하는 것을 방지
            if (!other.gameObject.activeSelf)
                return;
                
            Debug.Log("알약 아이템 발견: " + other.gameObject.name + " - 충돌 처리 시작");
            
            // 충돌체를 즉시 비활성화하여 중복 충돌 방지
            Collider2D pillCollider = other.GetComponent<Collider2D>();
            if (pillCollider != null)
            {
                pillCollider.enabled = false;
                Debug.Log("알약 충돌체 비활성화됨");
            }
            
            // 무적 상태 활성화
            ActivateInvincibility();
            Debug.Log("무적 상태 활성화 요청됨");
            
            // 아이템 비활성화 전에 부모 관계 해제 (충돌 문제 방지)
            other.transform.SetParent(null);
            
            // UIManager에 알약 획득 알림 (UIManager가 있는 경우)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectPill();
                Debug.Log("UIManager에 알약 획득 알림 전송됨");
            }
            else
            {
                Debug.LogWarning("UIManager.Instance가 null입니다!");
            }
            
            // 약간의 지연 후 아이템 비활성화 (충돌 처리 완료 보장)
            StartCoroutine(DeactivateItemDelayed(other.gameObject, 0.1f));
        }
        // "Hit" 태그를 가진 장애물과 충돌했을 때 (무적 상태에서 튕기는 효과)
        else if (other.CompareTag("Hit") && isInvincible)
        {
            Debug.Log("무적 상태에서 장애물 충돌! 튕기는 효과 적용: " + other.name);
            ApplyKnockbackEffectForTrigger(other.gameObject);
            
            // 효과음 재생 (선택 사항)
            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f); // 약간의 랜덤 피치
                audioSource.PlayOneShot(audioSource.clip, 0.5f);
            }
        }
    }
    
    // 일반 충돌 처리 - 장애물 충돌 시 무적 상태면 장애물이 날아가는 효과
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 알약 아이템과의 충돌 처리 (OnTriggerEnter2D가 작동하지 않을 경우를 대비)
        if (collision.gameObject.CompareTag("Pill"))
        {
            Debug.Log("일반 충돌로 알약 발견: " + collision.gameObject.name);
            
            // 이미 비활성화된 아이템과 다시 충돌하는 것을 방지
            if (!collision.gameObject.activeSelf)
                return;
            
            // 충돌체를 즉시 비활성화하여 중복 충돌 방지
            Collider2D pillCollider = collision.gameObject.GetComponent<Collider2D>();
            if (pillCollider != null)
            {
                pillCollider.enabled = false;
                Debug.Log("알약 충돌체 비활성화됨 (일반 충돌)");
            }
            
            // 무적 상태 활성화
            ActivateInvincibility();
            Debug.Log("무적 상태 활성화 요청됨 (일반 충돌)");
            
            // 아이템 비활성화 전에 부모 관계 해제
            collision.transform.SetParent(null);
            
            // UIManager에 알약 획득 알림
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectPill();
                Debug.Log("UIManager에 알약 획득 알림 전송됨 (일반 충돌)");
            }
            
            // 아이템 비활성화
            StartCoroutine(DeactivateItemDelayed(collision.gameObject, 0.1f));
            return;
        }
        
        // 무적 상태일 때만 장애물 처리
        if (!isInvincible)
            return;
            
        // 충돌한 오브젝트가 장애물인지 확인
        if (collision.gameObject.CompareTag("Hit"))
        {
            // 장애물 날아가는 효과 적용
            ApplyKnockbackEffect(collision.gameObject);
            
            // 효과음 재생 (선택 사항)
            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(0.8f, 1.2f); // 약간의 랜덤 피치
                audioSource.PlayOneShot(audioSource.clip, 0.5f);
            }
        }
    }
    
    // 장애물에 날아가는 효과를 적용하는 메서드
    private void ApplyKnockbackEffect(GameObject obstacle)
    {
        // 이미 충돌 처리된 장애물인지 확인 (중복 처리 방지)
        if (obstacle.GetComponent<Rigidbody2D>() != null)
            return;
            
        Debug.Log("장애물 충돌 효과 적용: " + obstacle.name);
        
        // 장애물의 콜라이더를 트리거로 변경 (다른 물체와 더 이상 충돌하지 않도록)
        Collider2D[] colliders = obstacle.GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.isTrigger = true;
        }
        
        // 장애물에 Rigidbody2D 추가 (없으면)
        Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obstacle.AddComponent<Rigidbody2D>();
        }
        
        // 장애물의 물리 속성 설정
        rb.gravityScale = 1f;  // 중력 적용
        rb.freezeRotation = false; // 회전 허용
        rb.linearVelocity = Vector2.zero; // 기존 속도 초기화
        rb.angularVelocity = 0f;
        
        // 플레이어의 진행 방향 반대쪽(오른쪽 위)으로 튕겨나가는 효과
        Vector2 knockbackDirection = new Vector2(0.7f, 0.7f).normalized; // 오른쪽 위 대각선 방향
        rb.AddForce(knockbackDirection * obstacleKnockbackForce, ForceMode2D.Impulse);
        
        // 회전 효과 (랜덤 방향)
        float randomRotation = Random.Range(-1f, 1f) > 0 ? obstacleRotationForce : -obstacleRotationForce;
        rb.AddTorque(randomRotation);
        
        // 장애물의 부모 관계 해제 (플랫폼이 있다면)
        obstacle.transform.SetParent(null);
        
        // 일정 시간 후에 장애물 제거
        Destroy(obstacle, obstacleDestroyDelay);
    }
    
    // 트리거용 장애물에 날아가는 효과를 적용하는 메서드 (Trigger Collider용)
    private void ApplyKnockbackEffectForTrigger(GameObject obstacle)
    {
        // 이미 충돌 처리된 장애물인지 확인 (중복 처리 방지)
        if (obstacle.GetComponent<Rigidbody2D>() != null)
            return;
            
        Debug.Log("트리거 장애물 충돌 효과 적용: " + obstacle.name);
        
        // 장애물의 콜라이더를 일시적으로 비활성화 (다른 물체와 더 이상 충돌하지 않도록)
        Collider2D[] colliders = obstacle.GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // 장애물에 Rigidbody2D 추가 (없으면)
        Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obstacle.AddComponent<Rigidbody2D>();
        }
        
        // 장애물의 물리 속성 설정
        rb.gravityScale = 1f;  // 중력 적용
        rb.freezeRotation = false; // 회전 허용
        rb.linearVelocity = Vector2.zero; // 기존 속도 초기화
        rb.angularVelocity = 0f;
        
        // 플레이어의 진행 방향 반대쪽(오른쪽 위)으로 튕겨나가는 효과
        Vector2 knockbackDirection = new Vector2(0.7f, 0.7f).normalized; // 오른쪽 위 대각선 방향
        rb.AddForce(knockbackDirection * obstacleKnockbackForce, ForceMode2D.Impulse);
        
        // 회전 효과 (랜덤 방향)
        float randomRotation = Random.Range(-1f, 1f) > 0 ? obstacleRotationForce : -obstacleRotationForce;
        rb.AddTorque(randomRotation);
        
        // 장애물의 부모 관계 해제 (플랫폼이 있다면)
        obstacle.transform.SetParent(null);
        
        // 일정 시간 후에 장애물 제거
        Destroy(obstacle, obstacleDestroyDelay);
    }
    
    // 아이템을 지연해서 비활성화하는 코루틴 (중복 충돌 방지)
    private IEnumerator DeactivateItemDelayed(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (item != null && item.activeSelf)
        {
            item.SetActive(false);
            Debug.Log("아이템 비활성화 완료: " + item.name);
        }
    }
    
    // 알약 이펙트 활성화
    private void ActivatePillEffect()
    {
        // 플레이어의 자식에서 이펙트 찾기
        Transform effectTransform = transform.Find(pillEffectName);
        
        if (effectTransform != null)
        {
            activePillEffect = effectTransform.gameObject;
            activePillEffect.SetActive(true);
            
            Debug.Log("알약 이펙트 활성화: " + activePillEffect.name);
        }
        else
        {
            Debug.LogWarning($"플레이어 자식에서 '{pillEffectName}' 이펙트를 찾을 수 없습니다.");
        }
    }
    
    // 알약 이펙트 비활성화
    private void DeactivatePillEffect()
    {
        if (activePillEffect != null)
        {
            activePillEffect.SetActive(false);
            activePillEffect = null;
            Debug.Log("알약 이펙트 비활성화");
        }
    }
    
    // 무적 텍스트 찾기
    private void FindInvincibilityText()
    {
        // 플레이어의 자식에서 무적 텍스트 찾기
        Transform textTransform = transform.Find(invincibilityTextName);
        
        if (textTransform != null)
        {
            invincibilityTextObject = textTransform.gameObject;
            invincibilityTextObject.SetActive(false); // 처음에는 비활성화
            Debug.Log("무적 텍스트 찾음: " + invincibilityTextObject.name);
        }
        else
        {
            Debug.LogWarning($"플레이어 자식에서 '{invincibilityTextName}' 텍스트를 찾을 수 없습니다.");
        }
    }
    
    // 무적 텍스트 활성화
    private void ActivateInvincibilityText()
    {
        if (invincibilityTextObject != null)
        {
            invincibilityTextObject.SetActive(true);
            Debug.Log("무적 텍스트 활성화: " + invincibilityTextObject.name);
        }
        else
        {
            Debug.LogWarning("무적 텍스트가 설정되지 않았습니다. 플레이어 자식에 텍스트 오브젝트를 추가해주세요.");
        }
    }
    
    // 무적 텍스트 비활성화
    private void DeactivateInvincibilityText()
    {
        if (invincibilityTextObject != null)
        {
            invincibilityTextObject.SetActive(false);
            Debug.Log("무적 텍스트 비활성화");
        }
    }
    
    // 무적 상태 초기화 메서드 (GameManager에서 호출)
    public void ResetInvincibilityState()
    {
        Debug.Log("무적 상태 초기화 시작");
        
        // 현재 실행 중인 무적 코루틴 정지
        if (currentInvincibilityRoutine != null)
        {
            StopCoroutine(currentInvincibilityRoutine);
            currentInvincibilityRoutine = null;
        }
        
        // 모든 코루틴 정지 (안전장치)
        StopAllCoroutines();
        
        // 무적 상태 해제
        isInvincible = false;
        
        // 무적 텍스트 비활성화
        DeactivateInvincibilityText();
        
        // 알약 이펙트 비활성화
        DeactivatePillEffect();
        
        Debug.Log("무적 상태 초기화 완료");
    }
}
