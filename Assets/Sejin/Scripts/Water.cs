using UnityEngine;

public class Water : MonoBehaviour
{
    public int scoreValue = 100;
    
    [Header("이펙트 설정")]
    public GameObject collectionEffect;       // 획득 시 재생할 이펙트 프리팹
    public float effectDuration = 1.0f;       // 이펙트 지속 시간
    
    [Header("사운드 설정")]
    public AudioClip collectionSound;         // 획득 시 재생할 사운드
    
    private AudioSource audioSource;          // 오디오 소스
    
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 이미 처리된 아이템인지 확인 (중복 방지)
            if (!gameObject.activeSelf)
                return;
                
            Debug.Log("💧 Player가 물과 충돌 - 점수 증가 + 오브젝트 비활성화");
            
            // 현재 위치 저장 (오브젝트 비활성화 전)
            Vector3 effectPosition = transform.position;
            
            // UIManager를 통한 점수 추가
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CollectWaterDrop();
            }
            
            // 게임 오브젝트 비활성화 (오브젝트 풀링 사용)
            gameObject.SetActive(false);
            
            // 오브젝트 비활성화 후 이펙트 재생
            if (collectionEffect != null)
            {
                PlayCollectionEffect(effectPosition);
            }
            
            // 사운드 재생 (AudioSource가 비활성화되므로 별도 처리 필요)
            if (collectionSound != null)
            {
                PlayCollectionSound(effectPosition);
            }
        }
    }
    
    // 획득 이펙트 재생
    private void PlayCollectionEffect(Vector3 effectPosition)
    {
        if (collectionEffect != null)
        {
            // 저장된 위치에 이펙트 생성
            GameObject effect = Instantiate(collectionEffect, effectPosition, Quaternion.identity);
            
            // 이펙트를 부모 오브젝트에서 분리 (독립적으로 동작)
            effect.transform.SetParent(null);
            
            // 파티클 시스템이 있다면 재생
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Play();
            }
            
            // 이펙트 오브젝트를 일정 시간 후 제거
            Destroy(effect, effectDuration);
            
            Debug.Log("물방울 획득 이펙트 재생: " + effect.name);
        }
    }
    
    // 획득 사운드 재생 (오브젝트가 비활성화되므로 별도 AudioSource 생성)
    private void PlayCollectionSound(Vector3 soundPosition)
    {
        if (collectionSound != null)
        {
            // 임시 AudioSource 오브젝트 생성
            GameObject tempAudio = new GameObject("TempWaterAudio");
            tempAudio.transform.position = soundPosition;
            
            AudioSource tempAudioSource = tempAudio.AddComponent<AudioSource>();
            tempAudioSource.clip = collectionSound;
            tempAudioSource.Play();
            
            // 사운드 재생 완료 후 오브젝트 제거
            Destroy(tempAudio, collectionSound.length);
            
            Debug.Log("물방울 획득 사운드 재생");
        }
    }
}
