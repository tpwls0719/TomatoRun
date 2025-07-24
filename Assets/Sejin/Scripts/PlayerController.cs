using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public AudioClip deathClip;
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
        playerAudio = GetComponent<AudioSource>();

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

        //마우스 왼쪽 버튼을 눌르고 최대 점프 횟수에 도달하지 않았다면
        if (Input.GetMouseButtonDown(0) && jumpCount < 2)
        {
            //점프 횟수 증가
            jumpCount++;
            //점프 직전에 속도를 순간적으로 제로로 변경
            playerRigidbody.linearVelocity = Vector2.zero;
            //리지드바디에 위쪽으로 힘을 주기
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            //오디오 소스 재생
            playerAudio.Play();
        }
        animator.SetBool("Grounded", isGrounded);

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
         //애니메이터의 Die 트리거 파라미터를 셋
        animator.SetTrigger("Die");

        //오디오 소스에 할당된 오디오 클립을 deathClip으로 변경
        playerAudio.clip = deathClip;
        //사망 효과음 재생
        playerAudio.Play();

        // 속도를 제로(0, 0)로 변경
        playerRigidbody.linearVelocity = Vector2.zero;
        // 사망 상태를 true로 변경
        isDead = true;


        GameManager.Instance.EndGame();

    }

    private void TakeDamage(int damage)
{
    Debug.Log("TakeDamage 메서드 호출 - 데미지: " + damage);

    InvincibilityItem invincibilityController = GetComponent<InvincibilityItem>();
    if (invincibilityController != null && invincibilityController.IsInvincible)
    {
        Debug.Log("무적 상태로 인해 데미지를 받지 않습니다!");
        return;
    }

    currentHealth -= damage;

    // UIManager에 현재 체력 상태를 전달
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateHeartDisplay(currentHealth);
    }

    if (currentHealth <= 0 && !isDead)
    {
        Die();
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

        if (other.tag == "Hit" && !isDead)
        {
            Debug.Log("Hit 태그 장애물과 충돌! TakeDamage 호출");
            TakeDamage(1); // 체력 1 깎기
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 어떤 콜라이더와 닿았으며, 충돌 표면이 위쪽을 보고 있으면
        if (collision.contacts[0].normal.y > 0.7f)
        {
            // isGrounded를 true로 변경하고, 누적 점프 횟수를 0으로 리셋
            isGrounded = true;
            jumpCount = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 어떤 콜라이더에서 떼어진 경우 isGrounded를 false로 변경
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
