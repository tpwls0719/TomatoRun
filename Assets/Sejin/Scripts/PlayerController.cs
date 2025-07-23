using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 500f;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool isDead = false;

    private Rigidbody2D playerRigidbody;
    private Animator animator;
    private AudioSource playerAudio;
    private SpriteRenderer spriteRenderer;

    public int maxHealth = 3;
    private int currentHealth;

    // [추가] 속도 관련 변수
    private float originalSpeed = 5f;
    private float boostedSpeed;
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;

    // [추가] 무적 관련 변수
    private bool isInvincible = false;
    private float invincibleEndTime = 0f;
    public float invincibleDuration = 3f;

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // playerAudio = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        boostedSpeed = originalSpeed;
    }
    
    // 플레이어 상태를 초기화하는 메서드 (GameManager에서 호출)
    public void ResetPlayerState()
    {
        Debug.Log("플레이어 상태 초기화 작업 시작");
        
        // 기본 상태 초기화
        jumpCount = 0;
        isGrounded = false;
        isDead = false;
        currentHealth = maxHealth;
        
        // 물리 상태 초기화
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
        
        // 애니메이션 상태 초기화
        if (animator != null)
        {
            animator.SetBool("Grounded", isGrounded);
            // 사망 상태에서 일반 상태로 복귀 (필요한 경우)
            animator.ResetTrigger("Die");
        }
        
        Debug.Log("플레이어 상태 초기화 완료 - 체력: " + currentHealth + "/" + maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        // 점프
        if (Input.GetMouseButtonDown(0) && jumpCount < 2)
        {
            jumpCount++;
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            // playerAudio.Play();
        }

        // 속도 부스트 시간 종료 체크
        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            boostedSpeed = originalSpeed;
            Debug.Log("속도 부스트 종료");
        }

        // 무적 시간 종료 체크
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
            StopCoroutine("BlinkEffect");
            spriteRenderer.enabled = true;
            Debug.Log("무적 상태 해제");
        }

        Move();

        animator.SetBool("Grounded", isGrounded);
    }

    // 좌우 이동
    void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");
        Vector3 movement = new Vector3(moveInput * boostedSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(movement);
    }

    private void Die()
    {
        animator.SetTrigger("Die");
        playerRigidbody.linearVelocity = Vector2.zero;
        isDead = true;

        // GameManager.instance.EndGame();
    }

    private void TakeDamage(int damage)
    {
        Debug.Log("TakeDamage 메서드 호출 - 데미지: " + damage);
        
        // 무적 상태 확인
        InvincibilityItem invincibilityController = GetComponent<InvincibilityItem>();
        if (invincibilityController != null && invincibilityController.IsInvincible)
        {
            Debug.Log("무적 상태로 인해 데미지를 받지 않습니다!");
            return;
        }
        
        Debug.Log("장애물과 충돌! UIManager로 데미지 처리");

        // UIManager를 통해 하트 UI 업데이트 (UIManager에서 하트 개수와 게임오버 관리)
        if (UIManager.Instance != null)
        {
            Debug.Log("UIManager.Instance 찾음. TakeDamage 호출");
            UIManager.Instance.TakeDamage();
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null입니다. UIManager가 씬에 있는지 확인하세요");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D 호출 - 충돌 오브젝트: " + other.name + ", 태그: " + other.tag);
        
        if (other.tag == "Dead" && !isDead)
        {
            Debug.Log("죽음");
            Die();
        }
        else if (other.tag == "Hit" && !isDead)
        {
            Debug.Log("Hit 태그 장애물과 충돌! TakeDamage 호출");
            TakeDamage(1); // 체력 1 깎기
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // 햇빛 아이템 효과
    public void ExtendMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log("햇빛으로 체력 확장! 현재 최대 체력: " + maxHealth);
    }

    // 속도 부스트
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        boostedSpeed = originalSpeed * multiplier;
        isSpeedBoosted = true;
        speedBoostEndTime = Time.time + duration;
        Debug.Log("속도 증가! 지속시간: " + duration + "초");
    }

    // 무적 모드 활성화 메서드
    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibleEndTime = Time.time + duration;
        StartCoroutine(BlinkEffect());
        Debug.Log("무적 상태 시작! " + duration + "초간");
    }

    // 반짝임 효과 코루틴
    IEnumerator BlinkEffect()
    {
        while (isInvincible)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.enabled = true;
    }
}
