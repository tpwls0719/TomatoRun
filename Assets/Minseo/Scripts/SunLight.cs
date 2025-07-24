using UnityEngine;
using System.Collections;

public class SunLight : MonoBehaviour
{
    [Header("이펙트 설정")]
    public string healEffectName = "ChargeEffect";  // 플레이어 자식 이펙트 오브젝트 이름
    public float effectDuration = 3.0f;             // 이펙트 지속 시간 (3초)

    [Header("사운드 설정")]
    public AudioClip collectionSound;               // 획득 시 재생할 사운드

    private AudioSource audioSource;

    void Start()
    {
        // 오디오 소스 컴포넌트 찾기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!gameObject.activeSelf)
                return;

            Debug.Log("햇빛 아이템 획득: " + gameObject.name);

            // ⭐ 플레이어 체력 회복
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("Heal() 호출 직전 currentHealth: " + playerController.currentHealth);
                playerController.Heal(1);
            }
            else
            {
                Debug.LogWarning("PlayerController 찾기 실패");
            }

            // 이펙트 재생
            if (!string.IsNullOrEmpty(healEffectName))
            {
                PlayHealEffect(other.gameObject);
            }

            // 사운드 재생 (위치 기반, 임시 오디오 오브젝트 사용)
            if (collectionSound != null)
            {
                Debug.Log("사운드 재생 시도: " + collectionSound.name);
                PlayOneShot(transform.position);
            }

            // UI 하트 반영
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectSunlight();
            }

            // 비활성화 (풀링)
            gameObject.SetActive(false);
        }
    }

    private void PlayHealEffect(GameObject player)
    {
        if (!string.IsNullOrEmpty(healEffectName))
        {
            Transform effectTransform = player.transform.Find(healEffectName);

            if (effectTransform != null)
            {
                GameObject effectObject = effectTransform.gameObject;

                // 기존 이펙트 정리
                if (effectObject.activeSelf)
                {
                    ParticleSystem existingParticles = effectObject.GetComponent<ParticleSystem>();
                    if (existingParticles != null)
                    {
                        existingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                    effectObject.SetActive(false);
                }

                // 이펙트 활성화
                effectObject.SetActive(true);

                // 파티클 재생
                ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particles.Play();
                }

                // 코루틴으로 일정 시간 후 비활성화
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.StartCoroutine(DeactivateEffectAfterDelay(effectObject, effectDuration));
                }
                else
                {
                    MonoBehaviour playerMono = player.GetComponent<MonoBehaviour>();
                    if (playerMono != null)
                    {
                        playerMono.StartCoroutine(DeactivateEffectAfterDelay(effectObject, effectDuration));
                    }
                    else
                    {
                        effectObject.SetActive(false);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"플레이어 자식에서 '{healEffectName}' 이펙트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("힐링 이펙트 이름이 설정되지 않았습니다.");
        }
    }

    private static IEnumerator DeactivateEffectAfterDelay(GameObject effectObject, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (effectObject != null)
        {
            ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                yield return new WaitForSeconds(0.5f);
            }

            effectObject.SetActive(false);
        }
    }

    // 임시 오디오 오브젝트를 사용해 사운드 재생
    private void PlayOneShot(Vector3 soundPosition)
    {
        if (collectionSound != null)
        {
            GameObject tempAudio = new GameObject("TempSunAudio");
            tempAudio.transform.position = soundPosition;

            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = collectionSound;
            tempSource.spatialBlend = 0f; // 2D 사운드
            tempSource.Play();

            Destroy(tempAudio, collectionSound.length);

            Debug.Log("햇빛 획득 사운드 재생됨: " + collectionSound.name);
        }
    }
}
