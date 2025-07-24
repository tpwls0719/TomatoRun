using UnityEngine;
using System.Collections;

public class InvincibilityItem : MonoBehaviour
{
    [Header("무적 효과 설정")]
    public float invincibilityDuration = 5f;

    [Header("속도 증가 효과")]
    public float speedBoostMultiplier = 1.5f; // 배경/플랫폼 속도 증가 배수

    [Header("플레이어 크기 변화")]
    public bool enableScaleChange = true; // 크기 변화 사용 여부
    public float scaleMultiplier = 1.5f; // 플레이어 크기 배수

    [Header("카메라 쉐이크")]
    public bool enableCameraShake = true; // 카메라 쉐이크 사용 여부
    public float shakeIntensity = 1.5f; // 쉐이크 강도
    public float shakeDuration = 0.5f; // 쉐이크 지속 시간

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
        
        // 알약 이펙트가 존재하는지 미리 확인
        CheckPillEffectExists();
    }
    
    // 알약 이펙트 존재 여부 확인
    private void CheckPillEffectExists()
    {
        Transform effectTransform = transform.Find(pillEffectName);
        if (effectTransform == null)
        {
            effectTransform = FindChildRecursive(transform, pillEffectName);
        }
        
        if (effectTransform != null)
        {
            Debug.Log($"알약 이펙트 '{pillEffectName}' 발견됨: {effectTransform.name}");
        }
        else
        {
            Debug.LogWarning($"알약 이펙트 '{pillEffectName}'를 찾을 수 없습니다. Unity 에디터에서 플레이어 자식 오브젝트 이름을 확인해주세요.");
            
            // 모든 자식 오브젝트 목록을 출력하여 실제 이름들 확인
            Debug.Log("=== 플레이어의 모든 자식 오브젝트 목록 ===");
            LogAllChildren(transform, 0);
            Debug.Log("=== 자식 오브젝트 목록 끝 ===");
        }
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

        // ✅ 플레이어 크기 변화
        if (enableScaleChange)
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayerScale(scaleMultiplier, invincibilityDuration);
                Debug.Log($"플레이어 크기 {scaleMultiplier}배로 변경");
            }
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

        // ✅ 플레이어 크기 원복
        if (enableScaleChange)
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayerScale(1f, 0f);
                Debug.Log("플레이어 크기 원복");
            }
        }

        if (audioSource && invincibilityEndSound)
            audioSource.PlayOneShot(invincibilityEndSound);

        Debug.Log("무적 상태 종료");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pill"))
        {
            // 이미 비활성화된 아이템이나 이미 무적 상태면 중복 처리 방지
            if (!other.gameObject.activeSelf || isInvincible)
                return;

            Debug.Log($"알약 충돌 감지: {other.gameObject.name}");

            // 충돌체를 즉시 비활성화하여 중복 충돌 방지
            Collider2D pillCollider = other.GetComponent<Collider2D>();
            if (pillCollider != null)
                pillCollider.enabled = false;

            // UIManager에 알약 획득 알림 (무적 활성화 전에 먼저 처리)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectPill();
                Debug.Log("UIManager에 알약 획득 알림 전송");
            }

            // 무적 상태 활성화 (플레이어에서 실행)
            ActivateInvincibility();
            
            // 알약 오브젝트 정리
            other.transform.SetParent(null);
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
            // 이미 비활성화된 아이템이나 이미 무적 상태면 중복 처리 방지
            if (!collision.gameObject.activeSelf || isInvincible)
                return;

            Debug.Log($"알약 일반 충돌 감지: {collision.gameObject.name}");

            // 충돌체를 즉시 비활성화하여 중복 충돌 방지
            Collider2D pillCollider = collision.gameObject.GetComponent<Collider2D>();
            if (pillCollider != null)
                pillCollider.enabled = false;

            // UIManager에 알약 획득 알림 (무적 활성화 전에 먼저 처리)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectPill();
                Debug.Log("UIManager에 알약 획득 알림 전송 (일반 충돌)");
            }

            // 무적 상태 활성화 (플레이어에서 실행)
            ActivateInvincibility();
            
            // 알약 오브젝트 정리
            collision.transform.SetParent(null);
            StartCoroutine(DeactivateItemDelayed(collision.gameObject, 0.1f));
            return;
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
        // 이미 Rigidbody2D가 있다면 중복 처리 방지
        Rigidbody2D existingRb = obstacle.GetComponent<Rigidbody2D>();
        if (existingRb != null)
        {
            Debug.Log($"{obstacle.name}에 이미 Rigidbody2D가 있어서 넉백 효과를 적용하지 않습니다.");
            return;
        }

        // 모든 콜라이더를 트리거로 설정
        Collider2D[] colliders = obstacle.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (col != null)
                col.isTrigger = true;
        }

        // Rigidbody2D 추가 및 설정
        try
        {
            Rigidbody2D rb = obstacle.AddComponent<Rigidbody2D>();
            if (rb != null)
            {
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
                
                Debug.Log($"{obstacle.name}에 넉백 효과 적용 완료");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{obstacle.name}에 Rigidbody2D 추가 중 오류 발생: {e.Message}");
        }
    }

    private void ApplyKnockbackEffectForTrigger(GameObject obstacle)
    {
        // 이미 Rigidbody2D가 있다면 중복 처리 방지
        Rigidbody2D existingRb = obstacle.GetComponent<Rigidbody2D>();
        if (existingRb != null)
        {
            Debug.Log($"{obstacle.name}에 이미 Rigidbody2D가 있어서 넉백 효과를 적용하지 않습니다.");
            return;
        }

        // 모든 콜라이더 비활성화
        Collider2D[] colliders = obstacle.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Rigidbody2D 추가 및 설정
        try
        {
            Rigidbody2D rb = obstacle.AddComponent<Rigidbody2D>();
            if (rb != null)
            {
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
                
                Debug.Log($"{obstacle.name}에 넉백 효과 적용 완료");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{obstacle.name}에 Rigidbody2D 추가 중 오류 발생: {e.Message}");
        }
    }

    private IEnumerator DeactivateItemDelayed(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (item != null && item.activeSelf)
            item.SetActive(false);
    }

    private void ActivatePillEffect()
    {
        Debug.Log($"알약 이펙트 활성화 시도 - 찾는 이펙트 이름: {pillEffectName}");
        
        // 먼저 직접 자식에서 찾기
        Transform effectTransform = transform.Find(pillEffectName);
        
        // 직접 자식에서 찾지 못했다면 모든 자식을 재귀적으로 검색
        if (effectTransform == null)
        {
            effectTransform = FindChildRecursive(transform, pillEffectName);
        }
        
        if (effectTransform != null)
        {
            activePillEffect = effectTransform.gameObject;
            activePillEffect.SetActive(true);
            Debug.Log($"알약 이펙트 활성화 완료: {activePillEffect.name}");
        }
        else
        {
            Debug.LogWarning($"플레이어 자식에서 '{pillEffectName}' 이펙트를 찾을 수 없습니다. 이펙트 없이 계속 진행합니다.");
            
            // 가능한 대안 이펙트 이름들을 시도해보기
            string[] possibleNames = {
                "PillEffect", 
                "Pill_Effect", 
                "PillEffect01", 
                "Effect", 
                "ParticleSystem"
            };
            
            bool foundAlternative = false;
            foreach (string altName in possibleNames)
            {
                effectTransform = FindChildRecursive(transform, altName);
                if (effectTransform != null)
                {
                    activePillEffect = effectTransform.gameObject;
                    activePillEffect.SetActive(true);
                    Debug.Log($"대안 이펙트 발견 및 활성화: {activePillEffect.name}");
                    foundAlternative = true;
                    break;
                }
            }
            
            if (!foundAlternative)
            {
                // 모든 자식 오브젝트 이름을 출력해서 확인
                Debug.Log("=== 이펙트를 찾을 수 없어서 모든 자식 오브젝트 목록 출력 ===");
                LogAllChildren(transform, 0);
                Debug.Log("=== 자식 오브젝트 목록 끝 ===");
                
                Debug.Log("이펙트가 없어도 무적 기능은 정상 작동합니다.");
            }
        }
    }
    
    // 재귀적으로 자식 오브젝트를 찾는 메서드
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
            
            // 재귀적으로 자식의 자식들도 검색
            Transform found = FindChildRecursive(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    // 모든 자식 오브젝트를 로그로 출력하는 메서드
    private void LogAllChildren(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Debug.Log($"{indent}- {child.name} (활성화: {child.gameObject.activeSelf})");
            
            // 자식의 자식들도 출력
            LogAllChildren(child, depth + 1);
        }
    }

    private void DeactivatePillEffect()
    {
        if (activePillEffect != null)
        {
            activePillEffect.SetActive(false);
            activePillEffect = null;
            Debug.Log("알약 이펙트 비활성화 완료");
        }
        else
        {
            Debug.LogWarning("비활성화할 알약 이펙트가 없습니다.");
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
            
        // 플레이어 크기 초기화
        if (enableScaleChange)
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetPlayerScale(1f, 0f);
                Debug.Log("무적 상태 리셋 - 플레이어 크기 원복");
            }
        }
    }
    
    // 플레이어가 커진 상태에서 착지했을 때 호출되는 메서드
    public void OnPlayerLandedWhileScaled()
    {
        if (enableCameraShake && isInvincible)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShakeCamera(shakeDuration, shakeIntensity);
                Debug.Log($"커진 상태에서 착지! 카메라 쉐이크 발생 - 강도: {shakeIntensity}, 지속시간: {shakeDuration}초");
            }
            else
            {
                Debug.LogWarning("GameManager Instance를 찾을 수 없습니다.");
            }
        }
    }
}
