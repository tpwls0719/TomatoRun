using UnityEngine;
using System.Collections;

public class SunLight : MonoBehaviour
{
    [Header("이펙트 설정")]
    public string healEffectName = "ChargeEffect";  // 플레이어 자식 이펙트 오브젝트 이름
    public float effectDuration = 3.0f;       // 이펙트 지속 시간 (3초)
    
    [Header("사운드 설정")]
    public AudioClip collectionSound;         // 획득 시 재생할 사운드
    
    private AudioSource audioSource;          // 오디오 소스
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 오디오 소스 컴포넌트 찾기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // 없으면 새로 추가
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 플레이어가 햇빛 아이템을 먹었을 때 생명력 회복
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 이미 처리된 아이템인지 확인 (중복 방지)
            if (!gameObject.activeSelf)
                return;
                
            Debug.Log("햇빛 아이템 획득: " + gameObject.name);
            
            // 플레이어에서 힐링 이펙트 재생
            if (!string.IsNullOrEmpty(healEffectName))
            {
                PlayHealEffect(other.gameObject);
            }
            
            // 사운드 재생
            if (audioSource != null && collectionSound != null)
            {
                audioSource.PlayOneShot(collectionSound);
            }
            
            // UIManager를 통해 생명력(하트) 1 회복
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectSunlight();
            }
            
            // 햇빛 아이템 비활성화 (풀링)
            gameObject.SetActive(false);
        }
    }
    
    // 플레이어에서 힐링 이펙트 재생
    private void PlayHealEffect(GameObject player)
    {
        if (!string.IsNullOrEmpty(healEffectName))
        {
            // 플레이어 자식에서 힐링 이펙트 찾기
            Transform effectTransform = player.transform.Find(healEffectName);
            
            if (effectTransform != null)
            {
                GameObject effectObject = effectTransform.gameObject;
                
                // 이미 활성화된 이펙트가 있으면 먼저 정리
                if (effectObject.activeSelf)
                {
                    // 파티클 시스템 강제 정지
                    ParticleSystem existingParticles = effectObject.GetComponent<ParticleSystem>();
                    if (existingParticles != null)
                    {
                        existingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                    effectObject.SetActive(false);
                }
                
                // 이펙트 활성화
                effectObject.SetActive(true);
                
                // 파티클 시스템이 있다면 재생
                ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 완전 초기화
                    particles.Play(); // 새로 시작
                }
                
                // 플레이어의 PlayerController에서 코루틴 실행 (더 안전함)
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.StartCoroutine(DeactivateEffectAfterDelay(effectObject, effectDuration));
                    Debug.Log("PlayerController에서 햇빛 힐링 이펙트 코루틴 시작 - " + effectDuration + "초간");
                }
                else
                {
                    // 플레이어의 MonoBehaviour에서 코루틴 실행 (fallback)
                    MonoBehaviour playerMono = player.GetComponent<MonoBehaviour>();
                    if (playerMono != null)
                    {
                        playerMono.StartCoroutine(DeactivateEffectAfterDelay(effectObject, effectDuration));
                        Debug.Log("플레이어 MonoBehaviour에서 햇빛 힐링 이펙트 코루틴 시작 - " + effectDuration + "초간");
                    }
                    else
                    {
                        Debug.LogWarning("플레이어에 적절한 컴포넌트가 없어 이펙트 타이머를 설정할 수 없습니다.");
                        // 최후의 수단: 즉시 비활성화
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
    
    // 일정 시간 후 이펙트 비활성화 (정적 메서드로 변경)
    private static IEnumerator DeactivateEffectAfterDelay(GameObject effectObject, float delay)
    {
        Debug.Log($"이펙트 타이머 시작: {delay}초 후 종료 예정");
        
        yield return new WaitForSeconds(delay);
        
        if (effectObject != null)
        {
            // 파티클 시스템이 있다면 완전히 정지
            ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 방출 정지 + 기존 파티클 제거
                Debug.Log("파티클 시스템 정지 완료");
                
                // 파티클이 완전히 사라질 때까지 대기
                yield return new WaitForSeconds(0.5f);
            }
            
            // 이펙트 오브젝트 비활성화
            effectObject.SetActive(false);
            Debug.Log("햇빛 힐링 이펙트 종료 - " + effectObject.name + " 비활성화됨");
        }
        else
        {
            Debug.LogWarning("이펙트 오브젝트가 null입니다. 이미 파괴되었을 수 있습니다.");
        }
    }
}
