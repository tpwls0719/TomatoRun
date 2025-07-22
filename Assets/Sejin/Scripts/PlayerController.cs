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

    //  [추가] 속도 관련 변수
    private float originalSpeed = 5f;
    private float boostedSpeed;
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;

    //  [추가] 무적 관련 변수
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

        //  부스트 시간 종료 체크
        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            boostedSpeed = originalSpeed;
            Debug.Log("⏱️ 부스트 종료");
        }

        //  무적 시간 종료 체크
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
            StopCoroutine("BlinkEffect");
            spriteRenderer.enabled = true;
            Debug.Log("🛡️ 무적 해제");
        }

        Move();

        animator.SetBool("Grounded", isGrounded);
    }

    //  좌우 이동
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
        if (isInvincible) return; //  무적 중에는 데미지 무시

        currentHealth -= damage;
        Debug.Log("데미지! 현재 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Dead" && !isDead)
        {
            Debug.Log("죽음");
            Die();
        }
        else if (other.tag == "Hit" && !isDead)
        {
            TakeDamage(1);
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

    // ☀️ 햇빛 아이템 효과
    public void ExtendMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log("☀️ 햇빛으로 체력 확장! 현재 최대 체력: " + maxHealth);
    }

    // 속도 부스트
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        boostedSpeed = originalSpeed * multiplier;
        isSpeedBoosted = true;
        speedBoostEndTime = Time.time + duration;
        Debug.Log("🚀 속도 증가! 지속 시간: " + duration + "초");
    }

    //  무적 모드 활성화 함수
    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibleEndTime = Time.time + duration;
        StartCoroutine(BlinkEffect());
        Debug.Log("🛡️ 무적 시작! " + duration + "초 동안");
    }

    // 반짝이 효과 코루틴
    IEnumerator BlinkEffect()
    {
        while (isInvincible)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(1f);
        }

        spriteRenderer.enabled = true;
    }
}
