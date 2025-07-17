using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("스테이지별 플랫폼 프리팹")]
    public GameObject[] stage1Platforms = new GameObject[2];
    public GameObject[] stage2Platforms = new GameObject[2];
    public GameObject[] stage3Platforms = new GameObject[2];
    public GameObject[] stage4Platforms = new GameObject[2];
    
    [Header("스폰 설정")]
    public int maxActivePlatforms = 4; // 화면에 유지할 최대 플랫폼 개수
    public int poolCount = 8; // 각 플랫폼당 풀 개수 (여유분 포함)
    public float timeBetSpawnMin = 1.25f;
    public float timeBetSpawnMax = 2.5f;
    public float yMin = -3.5f;
    public float yMax = 1.5f;
    public float xPos = 20f;
    public float despawnXPos = -15f; // 플랫폼이 사라질 X 위치
    
    [Header("현재 스테이지")]
    public int currentStage = 1; // 1~4
    
    private GameObject[][] platformPools; // 각 스테이지별 플랫폼 풀
    private int[] currentIndices; // 각 플랫폼 타입별 현재 인덱스
    private float timeBetSpawn;
    private float lastSpawnTime;
    private Vector2 poolPosition = new Vector2(0, 25f);
    
    // 활성 플랫폼 관리
    private GameObject[] activePlatforms; // 현재 활성화된 플랫폼들
    private int activePlatformCount = 0; // 현재 활성 플랫폼 개수
    
    void Start()
    {
        InitializePools();
        activePlatforms = new GameObject[maxActivePlatforms];
        lastSpawnTime = 0f;
        timeBetSpawn = 0f;
    }
    
    void InitializePools()
    {
        GameObject[][] allStages = { stage1Platforms, stage2Platforms, stage3Platforms, stage4Platforms };
        
        // 전체 플랫폼 타입 수 계산 (4스테이지 × 2플랫폼 = 8)
        int totalPlatformTypes = 8;
        platformPools = new GameObject[totalPlatformTypes][];
        currentIndices = new int[totalPlatformTypes];
        
        int poolIndex = 0;
        for (int stage = 0; stage < 4; stage++)
        {
            for (int platform = 0; platform < 2; platform++)
            {
                if (allStages[stage][platform] != null)
                {
                    // 각 플랫폼 타입별로 풀 생성
                    platformPools[poolIndex] = new GameObject[poolCount];
                    
                    for (int i = 0; i < poolCount; i++)
                    {
                        platformPools[poolIndex][i] = Instantiate(allStages[stage][platform], poolPosition, Quaternion.identity);
                        platformPools[poolIndex][i].SetActive(false);
                    }
                }
                poolIndex++;
            }
        }
    }

    void Update()
    {
        // 활성 플랫폼들의 위치 확인 및 정리
        CheckAndDeactivatePlatforms();
        
        // 최대 개수보다 적으면 새 플랫폼 스폰
        if (activePlatformCount < maxActivePlatforms && Time.time >= lastSpawnTime + timeBetSpawn)
        {
            SpawnPlatform();
            
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        }
    }
    
    void CheckAndDeactivatePlatforms()
    {
        for (int i = 0; i < activePlatformCount; i++)
        {
            if (activePlatforms[i] != null && activePlatforms[i].activeInHierarchy)
            {
                // 플랫폼이 화면 왼쪽을 벗어나면 비활성화
                if (activePlatforms[i].transform.position.x < despawnXPos)
                {
                    activePlatforms[i].SetActive(false);
                    RemoveFromActiveList(i);
                    i--; // 인덱스 조정
                }
            }
        }
    }
    
    void RemoveFromActiveList(int index)
    {
        // 배열에서 해당 인덱스 제거하고 뒤의 요소들을 앞으로 이동
        for (int i = index; i < activePlatformCount - 1; i++)
        {
            activePlatforms[i] = activePlatforms[i + 1];
        }
        activePlatformCount--;
        activePlatforms[activePlatformCount] = null;
    }
    
    void SpawnPlatform()
    {
        // 최대 개수에 도달했으면 스폰 안함
        if (activePlatformCount >= maxActivePlatforms) return;
        
        // 현재 스테이지의 플랫폼 중 랜덤 선택
        int stageIndex = currentStage - 1; // 0~3
        int platformType = Random.Range(0, 2); // 0 또는 1
        int poolIndex = (stageIndex * 2) + platformType;
        
        if (platformPools[poolIndex] == null) return;
        
        // 현재 인덱스의 플랫폼 가져오기
        GameObject platform = platformPools[poolIndex][currentIndices[poolIndex]];
        
        // 플랫폼 재활성화 및 위치 설정
        platform.SetActive(false);
        platform.SetActive(true);
        
        float yPos = Random.Range(yMin, yMax);
        Vector2 platformPosition = new Vector2(xPos, yPos);
        platform.transform.position = platformPosition;
        
        // 활성 플랫폼 목록에 추가
        activePlatforms[activePlatformCount] = platform;
        activePlatformCount++;
        
        // 플랫폼 크기 가져오기 (Collider 또는 Renderer 기준)
        Vector2 platformSize = GetPlatformSize(platform);
        
        // 아이템 스폰 (플랫폼 길이에 맞게 여러 개)
        if (ItemSpawner.Instance != null)
        {
            ItemSpawner.Instance.SpawnItemsOnPlatform(platformPosition, platformSize);
        }
        
        // 인덱스 순환
        currentIndices[poolIndex]++;
        if (currentIndices[poolIndex] >= poolCount)
        {
            currentIndices[poolIndex] = 0;
        }
        
        Debug.Log($"플랫폼 스폰: 활성 플랫폼 {activePlatformCount}/{maxActivePlatforms}");
    }
    
    Vector2 GetPlatformSize(GameObject platform)
    {
        // Collider2D가 있으면 그것을 기준으로
        Collider2D collider = platform.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.size;
        }
        
        // Renderer가 있으면 그것을 기준으로
        Renderer renderer = platform.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }
        
        // 둘 다 없으면 기본 크기 반환
        return new Vector2(3f, 1f);
    }
    
    public void ChangeStage(int newStage)
    {
        if (newStage >= 1 && newStage <= 4)
        {
            currentStage = newStage;
            Debug.Log($"스테이지 {currentStage}로 변경");
        }
    }
    
    // 게임 재시작 시 호출
    public void RestartGame()
    {
        // 모든 활성 플랫폼 비활성화
        for (int i = 0; i < activePlatformCount; i++)
        {
            if (activePlatforms[i] != null)
            {
                activePlatforms[i].SetActive(false);
            }
        }
        
        // 활성 플랫폼 목록 초기화
        activePlatformCount = 0;
        for (int i = 0; i < maxActivePlatforms; i++)
        {
            activePlatforms[i] = null;
        }
        
        // 스테이지 초기화
        currentStage = 1;
        lastSpawnTime = 0f;
        timeBetSpawn = 0f;
        
        Debug.Log("플랫폼 스포너 재시작");
    }
    
    [ContextMenu("테스트: 플랫폼 즉시 생성")]
    public void TestSpawnPlatform()
    {
        SpawnPlatform();
    }
}
