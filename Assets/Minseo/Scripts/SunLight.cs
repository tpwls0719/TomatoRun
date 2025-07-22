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
                
                // 이펙트 활성화
                effectObject.SetActive(true);
                
                // 파티클 시스템이 있다면 재생
                ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Play();
                }
                
                // 3초 후 이펙트 비활성화
                StartCoroutine(DeactivateEffectAfterDelay(effectObject, effectDuration));
                
                Debug.Log("햇빛 힐링 이펙트 재생 시작 - " + effectDuration + "초간");
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
    
    // 일정 시간 후 이펙트 비활성화
    private IEnumerator DeactivateEffectAfterDelay(GameObject effectObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (effectObject != null)
        {
            // 파티클 시스템이 있다면 정지
            ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Stop();
            }
            
            effectObject.SetActive(false);
            Debug.Log("햇빛 힐링 이펙트 종료");
        }
    }
}
