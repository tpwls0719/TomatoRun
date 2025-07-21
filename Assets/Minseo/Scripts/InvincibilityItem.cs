using UnityEngine;
using System.Collections;

public class InvincibilityItem : MonoBehaviour
{
    [Header("무적 효과 설정")]
    public float invincibilityDuration = 5f;      // 무적 지속 시간 (초)
    public float blinkInterval = 0.15f;           // 깜빡임 간격 (초)
    public Color invincibilityColor = new Color(1f, 0.8f, 0f, 0.7f); // 무적 상태 색상 (노란색)
    
    [Header("경고 설정")]
    public float warningTime = 1.5f;              // 무적 종료 전 경고 시간 (초)
    public float warningBlinkSpeed = 0.1f;        // 종료 전 경고시 깜빡임 속도
    
    [Header("효과음")]
    public AudioClip invincibilityStartSound;     // 무적 시작 효과음
    public AudioClip invincibilityEndSound;       // 무적 종료 효과음
    
    [Header("무적 모드 텍스트 효과")]
    public GameObject invincibilityTextObject;    // 무적 모드 텍스트/이미지 오브젝트
    public float textShakeAmount = 20f;           // 텍스트 흔들림 강도 (이미지용으로 더 증가)
    public float textShakeSpeed = 15f;            // 텍스트 흔들림 속도 (증가)
    
    private bool isInvincible = false;            // 현재 무적 상태인지
    private SpriteRenderer playerRenderer;        // 플레이어 스프라이트 렌더러
    private Color originalColor;                  // 원래 색상
    private AudioSource audioSource;              // 오디오 소스
    private Vector2 originalTextAnchoredPosition; // UI 텍스트의 원래 위치 (anchored position)
    
    // 다른 스크립트에서 무적 상태 확인용 프로퍼티
    public bool IsInvincible { get { return isInvincible; } }
    
    void Start()
    {
        // 플레이어의 SpriteRenderer 컴포넌트 찾기
        playerRenderer = GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer를 찾을 수 없습니다. 플레이어 객체에 추가해주세요.");
        }
        
        // 오디오 소스 컴포넌트 찾기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // 없으면 새로 추가
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 무적 모드 텍스트 설정 (UI 텍스트용)
        if (invincibilityTextObject != null)
        {
            // UI 요소인지 확인
            RectTransform textRectTransform = invincibilityTextObject.GetComponent<RectTransform>();
            if (textRectTransform != null)
            {
                originalTextAnchoredPosition = textRectTransform.anchoredPosition;
                invincibilityTextObject.SetActive(false); // 처음에는 비활성화
            }
            else
            {
                Debug.LogError("텍스트 오브젝트에 RectTransform이 없습니다. UI 텍스트가 맞는지 확인하세요.");
            }
        }
        else
        {
            Debug.LogWarning("무적 모드 텍스트 오브젝트가 지정되지 않았습니다. Inspector에서 설정해주세요.");
        }
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
        
        // 텍스트 활성화 및 애니메이션 시작
        if (invincibilityTextObject != null)
        {
            invincibilityTextObject.SetActive(true);
            StartCoroutine(TextShakeEffect());
        }
        
        // 깜빡임 효과 시작
        StartCoroutine(BlinkEffect());
        
        // 일반 무적 시간
        float normalInvincibilityTime = invincibilityDuration - warningTime;
        yield return new WaitForSeconds(normalInvincibilityTime);
        
        // 경고 깜빡임 (더 빠르게)
        StartCoroutine(WarningBlinkEffect());
        
        // 경고 시간
        yield return new WaitForSeconds(warningTime);
        
        // 무적 상태 종료
        isInvincible = false;
        currentInvincibilityRoutine = null; // 코루틴 참조 해제
        
        // 원래 색상으로 복원
        if (playerRenderer != null)
        {
            playerRenderer.color = originalColor;
            
            // 발광 효과 오브젝트가 있다면 비활성화
            Transform glowEffect = transform.Find("GlowEffect");
            if (glowEffect != null)
            {
                glowEffect.gameObject.SetActive(false);
            }
        }
        
        // 무적 모드 텍스트 비활성화 (UI 텍스트/이미지용)
        if (invincibilityTextObject != null)
        {
            // 원래 위치, 크기, 회전으로 리셋 후 비활성화
            RectTransform textRectTransform = invincibilityTextObject.GetComponent<RectTransform>();
            if (textRectTransform != null)
            {
                textRectTransform.anchoredPosition = originalTextAnchoredPosition;
                textRectTransform.localScale = Vector3.one;
                textRectTransform.rotation = Quaternion.identity; // 회전 리셋
            }
            invincibilityTextObject.SetActive(false);
        }
        
        // 종료 효과음
        if (audioSource != null && invincibilityEndSound != null)
        {
            audioSource.PlayOneShot(invincibilityEndSound);
        }
        
        Debug.Log("무적 상태 종료");
    }
    
    // 일반 발광 효과
    private IEnumerator BlinkEffect()
    {
        // 무적 상태에 사용할 발광 색상
        Color glowColor = new Color(1f, 0.9f, 0.2f, 1f); // 황금색 계열
        
        if (playerRenderer != null)
        {
            // 발광 효과를 위한 색상 변경
            playerRenderer.color = glowColor;
            
            // 외곽선 효과를 위한 추가 발광 오브젝트가 있다면 활성화
            Transform glowEffect = transform.Find("GlowEffect");
            if (glowEffect != null)
            {
                glowEffect.gameObject.SetActive(true);
            }
        }
        
        // 무적 상태가 종료되거나 경고 시간이 시작될 때까지 유지
        float pulseSpeed = 2.0f;
        float t = 0;
        
        while (isInvincible)
        {
            if (playerRenderer != null)
            {
                // 색상 강도를 펄스 효과로 변경 (깜빡임 대신 호흡 효과)
                t += Time.deltaTime * pulseSpeed;
                float pulse = 0.8f + Mathf.PingPong(t, 0.4f); // 0.8~1.2 사이 호흡
                
                // 색상 강도 조절
                Color enhancedColor = new Color(
                    glowColor.r * pulse,
                    glowColor.g * pulse,
                    glowColor.b * pulse,
                    1.0f
                );
                
                playerRenderer.color = enhancedColor;
            }
            
            yield return null;
        }
    }
    
    // 경고 발광 효과 (점점 빨간색으로 변화)
    private IEnumerator WarningBlinkEffect()
    {
        // 발광 효과 중단
        StopCoroutine(BlinkEffect());
        
        // 경고 색상 (빨간색 계열)
        Color warningColor = new Color(1f, 0.3f, 0.3f, 1f);
        
        // 무적 상태가 종료될 때까지 빨간색 경고 효과
        float pulseSpeed = 5.0f; // 경고 시에는 더 빠르게 펄스
        float t = 0;
        
        while (isInvincible)
        {
            if (playerRenderer != null)
            {
                t += Time.deltaTime * pulseSpeed;
                float pulse = 0.7f + Mathf.PingPong(t, 0.6f); // 0.7~1.3 사이로 더 극적인 변화
                
                // 경고 색상 강도 조절
                Color enhancedWarningColor = new Color(
                    warningColor.r * pulse,
                    warningColor.g * pulse * 0.5f, // 빨간색 강조
                    warningColor.b * pulse * 0.5f, // 빨간색 강조
                    1.0f
                );
                
                playerRenderer.color = enhancedWarningColor;
            }
            
            yield return null;
        }
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
                
            // 이미 처리된 아이템인지 확인 (중복 방지)
            if (other.GetComponent<Collider2D>().isTrigger == false)
                return;
                
            Debug.Log("알약 아이템 획득: " + other.gameObject.name);
            
            // 충돌체를 즉시 비활성화하여 중복 충돌 방지
            Collider2D pillCollider = other.GetComponent<Collider2D>();
            if (pillCollider != null)
            {
                pillCollider.enabled = false;
            }
            
            // 무적 상태 활성화
            ActivateInvincibility();
            
            // 아이템 비활성화 전에 부모 관계 해제 (충돌 문제 방지)
            other.transform.SetParent(null);
            
            // UIManager에 알약 획득 알림 (UIManager가 있는 경우)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectPill();
            }
            
            // 약간의 지연 후 아이템 비활성화 (충돌 처리 완료 보장)
            StartCoroutine(DeactivateItemDelayed(other.gameObject, 0.1f));
        }
    }
    
    // 일반 충돌 처리 - 장애물 충돌 시 무적 상태면 장애물이 날아가는 효과
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 무적 상태일 때만 처리
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
    
    // 무적 모드 UI 이미지/텍스트 흔들림 효과
    private IEnumerator TextShakeEffect()
    {
        if (invincibilityTextObject == null)
        {
            Debug.LogWarning("무적 모드 이미지/텍스트 오브젝트가 null입니다.");
            yield break;
        }
            
        // UI 요소의 RectTransform 가져오기
        RectTransform textRectTransform = invincibilityTextObject.GetComponent<RectTransform>();
        if (textRectTransform == null)
        {
            Debug.LogError("UI 이미지/텍스트의 RectTransform을 찾을 수 없습니다. UI 요소가 맞는지 확인하세요.");
            yield break;
        }
        
        Debug.Log("무적 모드 이미지/텍스트 흔들림 애니메이션 시작!");
            
        // 원래 위치와 크기 기억 (UI 좌표계 사용)
        Vector2 originalPos = textRectTransform.anchoredPosition;
        Vector3 originalScale = textRectTransform.localScale;
        float elapsedTime = 0f;
        
        // 무적 상태가 지속되는 동안 계속 흔들림
        while (isInvincible)
        {
            elapsedTime += Time.deltaTime;
            
            // 더 강한 사인파와 코사인파를 이용해 흔들림
            float xOffset = Mathf.Sin(elapsedTime * textShakeSpeed) * textShakeAmount;
            float yOffset = Mathf.Cos(elapsedTime * textShakeSpeed * 1.3f) * textShakeAmount;
            
            // UI 이미지/텍스트에 흔들림 효과 적용 (anchoredPosition 사용)
            textRectTransform.anchoredPosition = new Vector2(
                originalPos.x + xOffset,
                originalPos.y + yOffset
            );
            
            // 크기도 더 강하게 변화 (0.8~1.3 사이)
            float scaleMultiplier = 1f + Mathf.Sin(elapsedTime * textShakeSpeed * 0.7f) * 0.25f;
            textRectTransform.localScale = new Vector3(
                originalScale.x * scaleMultiplier, 
                originalScale.y * scaleMultiplier, 
                originalScale.z
            );
            
            // 회전도 추가 (좌우로 약간 흔들림)
            float rotation = Mathf.Sin(elapsedTime * textShakeSpeed * 0.5f) * 15f; // ±15도
            textRectTransform.rotation = Quaternion.Euler(0, 0, rotation);
            
            yield return null;
        }
        
        // 무적 상태가 종료되면 원래 위치와 크기, 회전으로 복구
        textRectTransform.anchoredPosition = originalPos;
        textRectTransform.localScale = originalScale;
        textRectTransform.rotation = Quaternion.identity;
        
        Debug.Log("무적 모드 이미지/텍스트 흔들림 애니메이션 종료!");
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
}
