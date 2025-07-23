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

    //  [추�] �도 관변    private float originalSpeed = 5f;
    private float boostedSpeed;
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;

    //  [추�] 무적 관변    private bool isInvincible = false;
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
    
    // �레�어 �태�초기�하메서(GameManager�서 �출)
    public void ResetPlayerState()
    {
        Debug.Log("�레�어 �태 초기�작");
        
        // 기본 �태 초기        jumpCount = 0;
        isGrounded = false;
        isDead = false;
        currentHealth = maxHealth;
        
        // 물리 �태 초기        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
        
        // �니메이�태 초기        if (animator != null)
        {
            animator.SetBool("Grounded", isGrounded);
            // �망 �태�서 �반 �태�복� (�요경우)
            animator.ResetTrigger("Die");
        }
        
        Debug.Log("�레�어 �태 초기�료 - 체력: " + currentHealth + "/" + maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        // �프
        if (Input.GetMouseButtonDown(0) && jumpCount < 2)
        {
            jumpCount++;
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            // playerAudio.Play();
        }

        //  부�트 �간 종료 체크
        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            boostedSpeed = originalSpeed;
            Debug.Log("�️ 부�트 종료");
        }

        //  무적 �간 종료 체크
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
            StopCoroutine("BlinkEffect");
            spriteRenderer.enabled = true;
            Debug.Log("���무적 �제");
        }

        Move();

        animator.SetBool("Grounded", isGrounded);
    }

    //  좌우 �동
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
        Debug.Log("TakeDamage 메서�출 ��지: " + damage);
        
        // 무적 �태 �인
        InvincibilityItem invincibilityController = GetComponent<InvincibilityItem>();
        if (invincibilityController != null && invincibilityController.IsInvincible)
        {
            Debug.Log("무적 �태��롰�지�받� �습�다!");
            return;
        }
        
        Debug.Log("�애물과 충돌! UIManager롰�지 처리");


        // UIManager륵해 �트 UI �데�트 (UIManager�서 �트 개수� 게임�버 관�
        if (UIManager.Instance != null)
        {
            Debug.Log("UIManager.Instance 찾음. TakeDamage �출");
            UIManager.Instance.TakeDamage();
        }
        else
        {
            Debug.LogError("UIManager.Instance가 null�니 UIManager가 �에 �는지 �인�세");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D �출 충돌�브�트: " + other.name + ", �그: " + other.tag);
        
        if (other.tag == "Dead" && !isDead)
        {
            Debug.Log("죽음");
            Die();
        }
        else if (other.tag == "Hit" && !isDead)
        {
            Debug.Log("Hit �그 �애물과 충돌! TakeDamage �출");
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

    // �︇빛 �이�과
    public void ExtendMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log("�︇빛�로 체력 �장! �재 최� 체력: " + maxHealth);
    }

    // �도 부�트
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        boostedSpeed = originalSpeed * multiplier;
        isSpeedBoosted = true;
        speedBoostEndTime = Time.time + duration;
        Debug.Log(" �도 증�! 지�간: " + duration + "�);
    }

    //  무적 모드 �성�수
    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibleEndTime = Time.time + duration;
        StartCoroutine(BlinkEffect());
        Debug.Log("���무적 �작! " + duration + "촙안");
    }

    // 반짝�과 코루    IEnumerator BlinkEffect()
    {
        while (isInvincible)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(1f);
        }

        spriteRenderer.enabled = true;
    }
}
