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

    //  [ì¶”ê] ë„ ê´€ë³€    private float originalSpeed = 5f;
    private float boostedSpeed;
    private bool isSpeedBoosted = false;
    private float speedBoostEndTime = 0f;

    //  [ì¶”ê] ë¬´ì  ê´€ë³€    private bool isInvincible = false;
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
    
    // Œë ˆ´ì–´ íƒœë¥ì´ˆê¸°”í•˜ë©”ì„œ(GameManagerì„œ ¸ì¶œ)
    public void ResetPlayerState()
    {
        Debug.Log("Œë ˆ´ì–´ íƒœ ì´ˆê¸°œì‘");
        
        // ê¸°ë³¸ íƒœ ì´ˆê¸°        jumpCount = 0;
        isGrounded = false;
        isDead = false;
        currentHealth = maxHealth;
        
        // ë¬¼ë¦¬ íƒœ ì´ˆê¸°        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
        
        //  ë‹ˆë©”ì´íƒœ ì´ˆê¸°        if (animator != null)
        {
            animator.SetBool("Grounded", isGrounded);
            // ¬ë§ íƒœì„œ ¼ë°˜ íƒœë¡ë³µê („ìš”ê²½ìš°)
            animator.ResetTrigger("Die");
        }
        
        Debug.Log("Œë ˆ´ì–´ íƒœ ì´ˆê¸°„ë£Œ - ì²´ë ¥: " + currentHealth + "/" + maxHealth);
    }

    void Update()
    {
        if (isDead) return;

        // í”„
        if (Input.GetMouseButtonDown(0) && jumpCount < 2)
        {
            jumpCount++;
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.AddForce(new Vector2(0, jumpForce));
            // playerAudio.Play();
        }

        //  ë¶€¤íŠ¸ œê°„ ì¢…ë£Œ ì²´í¬
        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            boostedSpeed = originalSpeed;
            Debug.Log("±ï¸ ë¶€¤íŠ¸ ì¢…ë£Œ");
        }

        //  ë¬´ì  œê°„ ì¢…ë£Œ ì²´í¬
        if (isInvincible && Time.time >= invincibleEndTime)
        {
            isInvincible = false;
            StopCoroutine("BlinkEffect");
            spriteRenderer.enabled = true;
            Debug.Log("›¡ï¸ë¬´ì  ´ì œ");
        }

        Move();

        animator.SetBool("Grounded", isGrounded);
    }

    //  ì¢Œìš° ´ë™
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
        Debug.Log("TakeDamage ë©”ì„œ¸ì¶œ °ëì§€: " + damage);
        
        // ë¬´ì  íƒœ •ì¸
        InvincibilityItem invincibilityController = GetComponent<InvincibilityItem>();
        if (invincibilityController != null && invincibilityController.IsInvincible)
        {
            Debug.Log("ë¬´ì  íƒœ´ëë¡°ëì§€ë¥ë°›ì ŠìŠµˆë‹¤!");
            return;
        }
        
        Debug.Log("¥ì• ë¬¼ê³¼ ì¶©ëŒ! UIManagerë¡°ëì§€ ì²˜ë¦¬");


        // UIManagerë¥µí•´ ˜íŠ¸ UI …ë°´íŠ¸ (UIManagerì„œ ˜íŠ¸ ê°œìˆ˜€ ê²Œì„¤ë²„ ê´€ë¦
        if (UIManager.Instance != null)
        {
            Debug.Log("UIManager.Instance ì°¾ìŒ. TakeDamage ¸ì¶œ");
            UIManager.Instance.TakeDamage();
        }
        else
        {
            Debug.LogError("UIManager.Instanceê°€ null…ë‹ˆ UIManagerê°€ ¬ì— ˆëŠ”ì§€ •ì¸˜ì„¸");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D ¸ì¶œ ì¶©ëŒ¤ë¸ŒíŠ¸: " + other.name + ", œê·¸: " + other.tag);
        
        if (other.tag == "Dead" && !isDead)
        {
            Debug.Log("ì£½ìŒ");
            Die();
        }
        else if (other.tag == "Hit" && !isDead)
        {
            Debug.Log("Hit œê·¸ ¥ì• ë¬¼ê³¼ ì¶©ëŒ! TakeDamage ¸ì¶œ");
            TakeDamage(1); // ì²´ë ¥ 1 ê¹ê¸°
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

    // €ï¸‡ë¹› „ì´¨ê³¼
    public void ExtendMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log("€ï¸‡ë¹›¼ë¡œ ì²´ë ¥ •ì¥! „ì¬ ìµœë ì²´ë ¥: " + maxHealth);
    }

    // ë„ ë¶€¤íŠ¸
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        boostedSpeed = originalSpeed * multiplier;
        isSpeedBoosted = true;
        speedBoostEndTime = Time.time + duration;
        Debug.Log(" ë„ ì¦ê! ì§€œê°„: " + duration + "ì´);
    }

    //  ë¬´ì  ëª¨ë“œ œì„±¨ìˆ˜
    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibleEndTime = Time.time + duration;
        StartCoroutine(BlinkEffect());
        Debug.Log("›¡ï¸ë¬´ì  œì‘! " + duration + "ì´™ì•ˆ");
    }

    // ë°˜ì§¨ê³¼ ì½”ë£¨    IEnumerator BlinkEffect()
    {
        while (isInvincible)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(1f);
        }

        spriteRenderer.enabled = true;
    }
}
