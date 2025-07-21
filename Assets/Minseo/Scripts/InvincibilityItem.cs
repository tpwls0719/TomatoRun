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
    
    private bool isInvincible = false;            // 현재 무적 상태인지
    private SpriteRenderer playerRenderer;        // 플레이어 스프라이트 렌더러
    private Color originalColor;                  // 원래 색상
    private AudioSource audioSource;              // 오디오 소스
    
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
    }
    
    void Update()
    {
        // 테스트용 코드 (실제 게임에서는 주석 처리하거나 제거)
        if (Input.GetKeyDown(KeyCode.I) && !isInvincible)
        {
            ActivateInvincibility();
        }
    }
    
    // 무적 상태 활성화 메서드 (외부에서 호출 가능)
    public void ActivateInvincibility()
    {
        if (!isInvincible)
        {
            StartCoroutine(InvincibilityRoutine());
        }
        else
        {
            // 이미 무적 상태라면 타이머만 리셋
            StopAllCoroutines();
            StartCoroutine(InvincibilityRoutine());
        }
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
        
        // 원래 색상으로 복원
        if (playerRenderer != null)
        {
            playerRenderer.color = originalColor;
        }
        
        // 종료 효과음
        if (audioSource != null && invincibilityEndSound != null)
        {
            audioSource.PlayOneShot(invincibilityEndSound);
        }
    }
    
    // 일반 깜빡임 효과
    private IEnumerator BlinkEffect()
    {
        // 무적 상태가 종료되거나 경고 시간이 시작될 때까지 반복
        while (isInvincible)
        {
            if (playerRenderer != null)
            {
                // 무적 색상으로 변경
                playerRenderer.color = invincibilityColor;
                yield return new WaitForSeconds(blinkInterval);
                
                // 원래 색상으로 복원
                playerRenderer.color = originalColor;
                yield return new WaitForSeconds(blinkInterval);
            }
            else
            {
                yield return null;
            }
        }
    }
    
    // 경고 깜빡임 효과 (더 빠르게)
    private IEnumerator WarningBlinkEffect()
    {
        // 깜빡임 효과 중단
        StopCoroutine(BlinkEffect());
        
        // 경고 색상 (빨간색 계열)
        Color warningColor = new Color(1f, 0.3f, 0.3f, 0.7f);
        
        // 무적 상태가 종료될 때까지 더 빠르게 깜빡임
        while (isInvincible)
        {
            if (playerRenderer != null)
            {
                // 경고 색상으로 변경
                playerRenderer.color = warningColor;
                yield return new WaitForSeconds(warningBlinkSpeed);
                
                // 원래 색상으로 복원
                playerRenderer.color = originalColor;
                yield return new WaitForSeconds(warningBlinkSpeed);
            }
            else
            {
                yield return null;
            }
        }
    }
    
    // 충돌 처리 - 아이템 획득 기능
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "InvincibilityPill" 태그를 가진 아이템과 충돌했을 때
        if (other.CompareTag("Pill"))
        {
            // 무적 상태 활성화
            ActivateInvincibility();
            
            // 아이템 비활성화 (아이템 풀링 사용 중이면 SetActive(false)로 변경)
            other.gameObject.SetActive(false);
        }
    }
}
