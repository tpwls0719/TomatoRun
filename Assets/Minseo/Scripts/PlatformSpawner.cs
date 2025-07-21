using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("플랫폼 프리팹")]
    public GameObject longPlatformPrefab;   // 긴 플랫폼 프리팹
    public GameObject shortPlatformPrefab;  // 짧은 플랫폼 프리팹
    [Range(0f, 1f)]
    public float longPlatformChance = 0.5f; // 긴 플랫폼 선택 확률 (50%)
    
    public int count = 2;   // 생상할 발판의 개수

    public float timeBetSpawnMin = 1.25f;  // 발판 생성 간격 최소값
    public float timeBetSpawnMax = 2.5f;  // 발판 생성 간격 최대값
    private float timeBetSpawn;  // 다음 배치까지의 시간 간격

    public float yMin = -3.5f;   // 배치할 위치의 최소 y값
    public float yMax = 1.5f;    // 배치할 위치의 최소 y값
    private float xPos = 20f;  // 배치할 위치의 x값
    private GameObject[] platforms;  // 생성된 발판들을 저장할 배열
    private int currentIndex = 0;  // 현재 발판의 인덱스
    private Vector2 poolPosition = new Vector2(0, 25f);  // 발판이 풀링될 위치
    private float lastSpawnTime;  // 마지막 발판 생성 시간
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // count만큼의 공가을 가지는 새로운 발판 배열 생성
        platforms = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            // 랜덤하게 긴 플랫폼 또는 짧은 플랫폼 선택
            GameObject selectedPrefab = (Random.value < longPlatformChance) ? longPlatformPrefab : shortPlatformPrefab;
            
            // 선택된 프리팹을 원본으로 새 발판을 poolPosition 위치에 복제 생성
            // 생성된 발판을 platform 배열에 할당
            platforms[i] = Instantiate(selectedPrefab, poolPosition, Quaternion.identity);
        }

        // 마지막 배치 시점 초기화
        lastSpawnTime = 0f;

        // 다음번 배치까지의 시간 간격을 0으로 초기화
        timeBetSpawn = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (GameManager.instance.isGameOver)
        // {
        //     return;  // 게임이 끝나면 발판 생성 중지
        // }

        if (Time.time >= lastSpawnTime + timeBetSpawn)
        {
            // 기록된 마지막 배치 시점을 현재 시점으로 갱신
            lastSpawnTime = Time.time;

            // 다음 배치까지의 시간 간격을 timeBetSpawnMin과 timeBetSpawnMax 사이의 랜덤 값으로 설정
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

            // 배치할 위치의 높이를 yMin과 yMax 사이의 랜덤 값으로 설정
            float yPos = Random.Range(yMin, yMax);

            // 사용할 현재 순번의 발판 게임 오브젝트를 비활성화하고 즉시 다시 활성화
            // 이때 발판의 Platform 컴포넌트의 OnEnable 메서드가 실행됨
            platforms[currentIndex].SetActive(false);
            platforms[currentIndex].SetActive(true);

            // 현재 순번의 발판을 화면 오른쪽으로 재배치
            platforms[currentIndex].transform.position = new Vector2(xPos, yPos);

            // 아이템 스폰
            if (ItemSpawnerFixed.Instance != null)
            {
                // 플랫폼 위치와 크기 정보 전달
                Vector2 platformPos = platforms[currentIndex].transform.position;
                Collider2D platformCol = platforms[currentIndex].GetComponent<Collider2D>();
                Vector2 platformSize = platformCol ? platformCol.bounds.size : Vector2.one;
                
                ItemSpawnerFixed.Instance.SpawnItemsOnPlatform(platformPos, platformSize, platforms[currentIndex]);
            }

            // 순번 넘기기
            currentIndex++;

            // 마지막 순번에 도달했다면 순번을 리셋
            if (currentIndex >= count)
            {
                currentIndex = 0;
            }
        }
    }
}
