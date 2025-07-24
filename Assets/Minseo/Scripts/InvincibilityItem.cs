using UnityEngine;
using System.Collections;

public class InvincibilityItem : MonoBehaviour
{
    [Header("무적 효과 설정")]
    public float invincibilityDuration = 5f;

    [Header("속도 증가 효과")]
    public float speedBoostMultiplier = 1.5f; // 배경/플랫폼 속도 증가 배수

    [Header("효과음")]
    public AudioClip invincibilityStartSound;
    public AudioClip invincibilityEndSound;

    [Header("무적 모드 텍스트")]
    public string invincibilityTextName = "InvincibilityText";

    [Header("알약 이펙트")]
    public string pillEffectName = "PillEffect_01";

    private GameObject activePillEffect;
    private GameObject invincibilityTextObject;

    private bool isInvincible = false;
    private AudioSource audioSource;

    public bool IsInvincible => isInvincible;

    private Coroutine currentInvincibilityRoutine = null;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        FindInvincibilityText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isInvincible)
        {
            ActivateInvincibility();
        }
    }

    public void ActivateInvincibility()
    {
        if (currentInvincibilityRoutine != null)
        {
            StopCoroutine(currentInvincibilityRoutine);
            currentInvincibilityRoutine = null;
        }

        currentInvincibilityRoutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        if (audioSource && invincibilityStartSound)
            audioSource.PlayOneShot(invincibilityStartSound);

        ActivateInvincibilityText();
        ActivatePillEffect();

        // ✅ 속도 증가
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGlobalScrollSpeed(speedBoostMultiplier);
            Debug.Log("스크롤 속도 증가");
        }

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        currentInvincibilityRoutine = null;

        DeactivatePillEffect();
        DeactivateInvincibilityText();

        // ✅ 속도 원복
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGlobalScrollSpeed();
            Debug.Log("스크롤 속도 원복");
        }

        if (audioSource && invincibilityEndSound)
            audioSource.PlayOneShot(invincibilityEndSound);

        Debug.Log("무적 상태 종료");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pill"))
        {
            if (!other.gameObject.activeSelf)
                return;

            Collider2D pillCollider = other.GetComponent<Collider2D>();
            if (pillCollider != null)
                pillCollider.enabled = false;

            ActivateInvincibility();
            other.transform.SetParent(null);

            if (UIManager.Instance != null)
                UIManager.Instance.CollectPill();

            StartCoroutine(DeactivateItemDelayed(other.gameObject, 0.1f));
        }
        else if (other.CompareTag("Hit") && isInvincible)
        {
            ApplyKnockbackEffectForTrigger(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pill"))
        {
            if (!collision.gameObject.activeSelf)
                return;

            Collider2D pillCollider = collision.gameObject.GetComponent<Collider2D>();
            if (pillCollider != null)
                pillCollider.enabled = false;

            ActivateInvincibility();
            collision.transform.SetParent(null);

            if (UIManager.Instance != null)
                UIManager.Instance.CollectPill();

            StartCoroutine(DeactivateItemDelayed(collision.gameObject, 0.1f));
        }

        if (!isInvincible)
            return;

        if (collision.gameObject.CompareTag("Hit"))
        {
            ApplyKnockbackEffect(collision.gameObject);
        }
    }

    private void ApplyKnockbackEffect(GameObject obstacle)
    {
        if (obstacle.GetComponent<Rigidbody2D>() != null)
            return;

        Collider2D[] colliders = obstacle.GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.isTrigger = true;

        Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>() ?? obstacle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.freezeRotation = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 dir = new Vector2(0.7f, 0.7f).normalized;
        rb.AddForce(dir * 10f, ForceMode2D.Impulse);

        float torque = Random.Range(-1f, 1f) > 0 ? 300f : -300f;
        rb.AddTorque(torque);

        obstacle.transform.SetParent(null);
        Destroy(obstacle, 2f);
    }

    private void ApplyKnockbackEffectForTrigger(GameObject obstacle)
    {
        if (obstacle.GetComponent<Rigidbody2D>() != null)
            return;

        Collider2D[] colliders = obstacle.GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.enabled = false;

        Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>() ?? obstacle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.freezeRotation = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 dir = new Vector2(0.7f, 0.7f).normalized;
        rb.AddForce(dir * 10f, ForceMode2D.Impulse);

        float torque = Random.Range(-1f, 1f) > 0 ? 300f : -300f;
        rb.AddTorque(torque);

        obstacle.transform.SetParent(null);
        Destroy(obstacle, 2f);
    }

    private IEnumerator DeactivateItemDelayed(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (item != null && item.activeSelf)
            item.SetActive(false);
    }

    private void ActivatePillEffect()
    {
        Transform effectTransform = transform.Find(pillEffectName);
        if (effectTransform != null)
        {
            activePillEffect = effectTransform.gameObject;
            activePillEffect.SetActive(true);
        }
    }

    private void DeactivatePillEffect()
    {
        if (activePillEffect != null)
        {
            activePillEffect.SetActive(false);
            activePillEffect = null;
        }
    }

    private void FindInvincibilityText()
    {
        Transform textTransform = transform.Find(invincibilityTextName);
        if (textTransform != null)
        {
            invincibilityTextObject = textTransform.gameObject;
            invincibilityTextObject.SetActive(false);
        }
    }

    private void ActivateInvincibilityText()
    {
        if (invincibilityTextObject != null)
            invincibilityTextObject.SetActive(true);
    }

    private void DeactivateInvincibilityText()
    {
        if (invincibilityTextObject != null)
            invincibilityTextObject.SetActive(false);
    }

    public void ResetInvincibilityState()
    {
        if (currentInvincibilityRoutine != null)
        {
            StopCoroutine(currentInvincibilityRoutine);
            currentInvincibilityRoutine = null;
        }

        StopAllCoroutines();
        isInvincible = false;

        DeactivateInvincibilityText();
        DeactivatePillEffect();

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGlobalScrollSpeed();
    }
}
