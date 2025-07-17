using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab; // 물방울 아이템
    public GameObject pillPrefab; // 알약 아이템 (무적)
    
    [Header("아이템 스폰 확률")]
    [Range(0f, 1f)]
    public float waterDropSpawnChance = 0.7f; // 물방울 스폰 확률 (70%)
    [Range(0f, 1f)]
    public float pillSpawnChance = 0.1f; // 알약 스폰 확률 (10%)
    
    [Header("아이템 위치 설정")]
    public float itemHeightOffset = 1f; // 플랫폼 위 높이 오프셋
    public float itemSpacing = 1.5f; // 아이템 간 거리
    public float minPlatformWidthForItems = 2f; // 아이템 스폰 최소 플랫폼 너비
    
    [Header("물방울 풀")]
    public int waterDropPoolCount = 15; // 물방울 풀 개수
    
    [Header("알약 설정")]
    public int pillPoolCount = 5; // 알약 풀 개수
    
    [Header("현재 스테이지")]
    public int currentStage = 1; // 1~4
    
    // 오브젝트 풀
    private GameObject[] waterDropPool;
    private GameObject[] pillPool;
    
    // 인덱스 관리
    private int waterDropIndex = 0;
    private int pillIndex = 0;
    
    // 스테이지별 알약 스폰 카운트
    private int pillSpawnedThisStage = 0;
    private int maxPillsPerStage = 3;
    
    private Vector2 poolPosition = new Vector2(0, 25f); // 풀링 위치
    
    // 싱글톤 패턴 (다른 스크립트에서 접근용)
    public static ItemSpawner Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializePools();
    }
    
    void InitializePools()
    {
        // 물방울 풀 초기화
        if (waterDropPrefab != null)
        {
            waterDropPool = new GameObject[waterDropPoolCount];
            for (int i = 0; i < waterDropPoolCount; i++)
            {
                waterDropPool[i] = Instantiate(waterDropPrefab, poolPosition, Quaternion.identity);
                waterDropPool[i].SetActive(false);
            }
        }
        
        // 알약 풀 초기화
        if (pillPrefab != null)
        {
            pillPool = new GameObject[pillPoolCount];
            for (int i = 0; i < pillPoolCount; i++)
            {
                pillPool[i] = Instantiate(pillPrefab, poolPosition, Quaternion.identity);
                pillPool[i].SetActive(false);
            }
        }
    }
    
    // 플랫폼이 생성될 때 호출되는 메서드 (PlatformSpawner에서 호출)
    public void SpawnItemsOnPlatform(Vector2 platformPosition, Vector2 platformSize)
    {
        // 플랫폼이 너무 작으면 아이템 스폰 안함
        if (platformSize.x < minPlatformWidthForItems) return;
        
        // 플랫폼 너비에 따라 스폰할 아이템 개수 계산
        int maxItems = Mathf.FloorToInt(platformSize.x / itemSpacing);
        maxItems = Mathf.Clamp(maxItems, 1, 5); // 최소 1개, 최대 5개
        
        // 플랫폼의 왼쪽 끝 위치 계산
        float leftEdge = platformPosition.x - (platformSize.x / 2f);
        float itemY = platformPosition.y + (platformSize.y / 2f) + itemHeightOffset;
        
        // 각 위치에 확률적으로 아이템 스폰
        for (int i = 0; i < maxItems; i++)
        {
            // 아이템 X 위치 계산 (플랫폼을 균등하게 나눔)
            float itemX = leftEdge + ((platformSize.x / (maxItems + 1)) * (i + 1));
            Vector2 itemPosition = new Vector2(itemX, itemY);
            
            // 확률에 따라 아이템 스폰
            float randomValue = Random.Range(0f, 1f);
            
            if (randomValue < pillSpawnChance && pillSpawnedThisStage < maxPillsPerStage)
            {
                // 알약 스폰 (우선순위가 높음)
                SpawnPillAtPosition(itemPosition);
            }
            else if (randomValue < waterDropSpawnChance + pillSpawnChance)
            {
                // 물방울 스폰
                SpawnWaterDropAtPosition(itemPosition);
            }
            // 그 외에는 해당 위치에 아이템 스폰 안함
        }
    }
    
    // 기존 메서드도 유지 (호환성을 위해)
    public void SpawnItemOnPlatform(Vector2 platformPosition, Vector2 platformSize)
    {
        SpawnItemsOnPlatform(platformPosition, platformSize);
    }
    
    void SpawnWaterDropAtPosition(Vector2 position)
    {
        if (waterDropPool == null || waterDropPool.Length == 0) return;
        
        GameObject waterDrop = waterDropPool[waterDropIndex];
        
        waterDrop.transform.position = position;
        waterDrop.SetActive(false);
        waterDrop.SetActive(true);
        
        // 인덱스 순환
        waterDropIndex++;
        if (waterDropIndex >= waterDropPoolCount)
        {
            waterDropIndex = 0;
        }
        
        Debug.Log($"물방울 스폰 at {position}");
    }
    
    void SpawnPillAtPosition(Vector2 position)
    {
        if (pillPool == null || pillPool.Length == 0) return;
        
        GameObject pill = pillPool[pillIndex];
        
        pill.transform.position = position;
        pill.SetActive(false);
        pill.SetActive(true);
        
        // 카운트 증가
        pillSpawnedThisStage++;
        
        // 인덱스 순환
        pillIndex++;
        if (pillIndex >= pillPoolCount)
        {
            pillIndex = 0;
        }
        
        Debug.Log($"스테이지 {currentStage}: 알약 {pillSpawnedThisStage}/{maxPillsPerStage} 스폰 at {position}");
    }
    
    // 스테이지 변경 시 호출
    public void ChangeStage(int newStage)
    {
        if (newStage >= 1 && newStage <= 4)
        {
            currentStage = newStage;
            pillSpawnedThisStage = 0; // 알약 카운트 리셋
            Debug.Log($"스테이지 {currentStage}로 변경: 알약 카운트 리셋");
        }
    }
    
    // 게임 재시작 시 호출
    public void RestartGame()
    {
        currentStage = 1;
        pillSpawnedThisStage = 0;
        waterDropIndex = 0;
        pillIndex = 0;
        
        // 모든 아이템 비활성화
        if (waterDropPool != null)
        {
            foreach (GameObject item in waterDropPool)
            {
                if (item != null) item.SetActive(false);
            }
        }
        
        if (pillPool != null)
        {
            foreach (GameObject item in pillPool)
            {
                if (item != null) item.SetActive(false);
            }
        }
    }
    
    // 테스트용 메서드들
    [ContextMenu("테스트: 랜덤 위치에 물방울 스폰")]
    public void TestSpawnWaterDrop()
    {
        Vector2 testPos = new Vector2(Random.Range(5f, 15f), Random.Range(-2f, 2f));
        SpawnWaterDropAtPosition(testPos);
    }
    
    [ContextMenu("테스트: 랜덤 위치에 알약 스폰")]
    public void TestSpawnPill()
    {
        if (pillSpawnedThisStage < maxPillsPerStage)
        {
            Vector2 testPos = new Vector2(Random.Range(5f, 15f), Random.Range(-2f, 2f));
            SpawnPillAtPosition(testPos);
        }
        else
        {
            Debug.Log("이미 스테이지당 최대 알약 개수에 도달했습니다!");
        }
    }
    
    [ContextMenu("테스트: 가상 플랫폼에 아이템 스폰")]
    public void TestSpawnOnPlatform()
    {
        Vector2 platformPos = new Vector2(10f, 0f);
        Vector2 platformSize = new Vector2(3f, 1f);
        SpawnItemOnPlatform(platformPos, platformSize);
    }
}
