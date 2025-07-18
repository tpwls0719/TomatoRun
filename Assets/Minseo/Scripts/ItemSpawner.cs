using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;
    public GameObject pillPrefab;

    [Header("아이템 스폰 확률")]
    [Range(0f, 1f)]
    public float waterDropSpawnChance = 0.7f;
    [Range(0f, 1f)]
    public float pillSpawnChance = 0.1f;

    [Header("위치 설정")]
    public float itemHeightOffset = 1f;
    public float itemSpacing = 1.5f;
    public float minPlatformWidthForItems = 2f;

    [Header("풀 개수")]
    public int waterDropPoolCount = 15;
    public int pillPoolCount = 5;

    [Header("스테이지 설정")]
    public int currentStage = 1;
    private int pillSpawnedThisStage = 0;
    private int maxPillsPerStage = 3;

    private GameObject[] waterDropPool;
    private GameObject[] pillPool;
    private int waterDropIndex = 0;
    private int pillIndex = 0;

    public static ItemSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializePools();
    }

    void InitializePools()
    {
        // 물방울 풀 생성
        waterDropPool = new GameObject[waterDropPoolCount];
        for (int i = 0; i < waterDropPoolCount; i++)
        {
            waterDropPool[i] = Instantiate(waterDropPrefab);
            waterDropPool[i].transform.position = new Vector3(1000f, 1000f, 0f); // 화면 밖으로
            waterDropPool[i].SetActive(false);
        }

        // 알약 풀 생성
        if (pillPrefab != null)
        {
            pillPool = new GameObject[pillPoolCount];
            for (int i = 0; i < pillPoolCount; i++)
            {
                pillPool[i] = Instantiate(pillPrefab);
                pillPool[i].transform.position = new Vector3(1000f, 1000f, 0f); // 화면 밖으로
                pillPool[i].SetActive(false);
            }
        }
        
        Debug.Log($"[ItemSpawner] 풀 초기화 완료 - 물방울: {waterDropPoolCount}개, 알약: {(pillPool?.Length ?? 0)}개");
    }

    public void SpawnItemsOnPlatform(Vector2 platformPosition, Vector2 platformSize, GameObject platform)
    {
        Debug.Log($"[ItemSpawner] 스폰 요청 - 플랫폼: {platform.name}, 크기: {platformSize}");
        
        if (platformSize.x < minPlatformWidthForItems) 
        {
            Debug.Log($"[ItemSpawner] 플랫폼이 너무 작음 - 최소 크기: {minPlatformWidthForItems}");
            return;
        }

        // 플랫폼에 있는 장애물 찾기
        Transform obstacle = FindObstacleOnPlatform(platform);
        
        if (obstacle != null)
        {
            Debug.Log($"[ItemSpawner] 장애물 발견: {obstacle.name} - 쿠키런 스타일 배치");
            // 쿠키런 스타일: 장애물 위와 주변에 아이템 배치
            SpawnItemsCookieRunStyle(platform, obstacle, platformSize);
        }
        else
        {
            Debug.Log("[ItemSpawner] 장애물 없음 - 전체 배치");
            // 기존 방식: 플랫폼 전체에 균등 배치
            SpawnItemsOnFullPlatform(platform, platformSize);
        }
    }
    
    Transform FindObstacleOnPlatform(GameObject platform)
    {
        // 플랫폼의 자식 중에서 장애물 찾기
        for (int i = 0; i < platform.transform.childCount; i++)
        {
            Transform child = platform.transform.GetChild(i);
            if (child.name.Contains("Obstacle") || child.CompareTag("Obstacle"))
            {
                return child;
            }
        }
        return null;
    }
    
    void SpawnItemsCookieRunStyle(GameObject platform, Transform obstacle, Vector2 platformSize)
    {
        Collider2D obstacleCol = obstacle.GetComponent<Collider2D>();
        if (obstacleCol == null) return;
        
        float obstacleWidth = obstacleCol.bounds.size.x;
        float obstacleHeight = obstacleCol.bounds.size.y;
        float obstacleX = obstacle.localPosition.x;
        float obstacleY = obstacle.localPosition.y;
        
        Debug.Log($"[배치] 장애물 정보 - 위치: ({obstacleX}, {obstacleY}), 크기: {obstacleWidth}x{obstacleHeight}");
        
        // 1. 장애물 위에 아이템 배치 (쿠키런 스타일)
        Vector2 topPosition = new Vector2(obstacleX, obstacleY + (obstacleHeight / 2f) + 0.5f);
        Debug.Log($"[배치] 장애물 위 아이템 위치: {topPosition}");
        SpawnSingleItem(topPosition, platform);
        
        // 2. 플랫폼 좌측에 아이템 2개
        float leftX1 = -(platformSize.x / 2f) + 1f;
        float leftX2 = -(platformSize.x / 2f) + 2.5f;
        float platformY = (platformSize.y / 2f) + itemHeightOffset;
        
        if (leftX2 < obstacleX - (obstacleWidth / 2f) - 0.5f)
        {
            SpawnSingleItem(new Vector2(leftX1, platformY), platform);
            SpawnSingleItem(new Vector2(leftX2, platformY), platform);
            Debug.Log($"[배치] 좌측 아이템 - ({leftX1}, {platformY}), ({leftX2}, {platformY})");
        }
        
        // 3. 플랫폼 우측에 아이템 2개
        float rightX1 = (platformSize.x / 2f) - 2.5f;
        float rightX2 = (platformSize.x / 2f) - 1f;
        
        if (rightX1 > obstacleX + (obstacleWidth / 2f) + 0.5f)
        {
            SpawnSingleItem(new Vector2(rightX1, platformY), platform);
            SpawnSingleItem(new Vector2(rightX2, platformY), platform);
            Debug.Log($"[배치] 우측 아이템 - ({rightX1}, {platformY}), ({rightX2}, {platformY})");
        }
    }
    
    void SpawnItemsOnFullPlatform(GameObject platform, Vector2 platformSize)
    {
        int maxItems = Mathf.FloorToInt(platformSize.x / itemSpacing);
        maxItems = Mathf.Clamp(maxItems, 1, 5);

        float itemY = (platformSize.y / 2f) + itemHeightOffset;

        for (int i = 0; i < maxItems; i++)
        {
            float itemLocalX = -(platformSize.x / 2f) + ((platformSize.x / (maxItems + 1)) * (i + 1));
            Vector2 itemLocalPosition = new Vector2(itemLocalX, itemY);
            SpawnSingleItem(itemLocalPosition, platform);
        }
    }
    
    void SpawnSingleItem(Vector2 localPosition, GameObject parentPlatform)
    {
        float rand = Random.value;

        if (rand < pillSpawnChance && pillSpawnedThisStage < maxPillsPerStage)
        {
            SpawnPillAtPosition(localPosition, parentPlatform);
        }
        else if (rand < pillSpawnChance + waterDropSpawnChance)
        {
            SpawnWaterDropAtPosition(localPosition, parentPlatform);
        }
    }

    void SpawnWaterDropAtPosition(Vector2 localPosition, GameObject parentPlatform)
    {
        if (waterDropPool == null || waterDropPool.Length == 0) return;

        GameObject drop = waterDropPool[waterDropIndex];

        // 부모 설정과 위치 조정
        drop.transform.SetParent(parentPlatform.transform, false);
        drop.transform.localPosition = localPosition;
        
        // 활성화
        drop.SetActive(true);
        
        Debug.Log($"[ItemSpawner] 물방울 스폰됨 - 위치: {localPosition}, 부모: {parentPlatform.name}");

        waterDropIndex = (waterDropIndex + 1) % waterDropPoolCount;
    }

    void SpawnPillAtPosition(Vector2 localPosition, GameObject parentPlatform)
    {
        if (pillPool == null || pillPool.Length == 0) return;

        GameObject pill = pillPool[pillIndex];

        // 부모 설정과 위치 조정
        pill.transform.SetParent(parentPlatform.transform, false);
        pill.transform.localPosition = localPosition;
        
        // 활성화
        pill.SetActive(true);
        
        Debug.Log($"[ItemSpawner] 알약 스폰됨 - 위치: {localPosition}, 부모: {parentPlatform.name}");

        pillSpawnedThisStage++;
        pillIndex = (pillIndex + 1) % pillPoolCount;
    }

    public void ChangeStage(int newStage)
    {
        if (newStage >= 1 && newStage <= 4)
        {
            currentStage = newStage;
            pillSpawnedThisStage = 0;
            Debug.Log($"[스테이지 변경] 현재 스테이지: {currentStage}, 알약 리셋됨");
        }
    }

    public void RestartGame()
    {
        currentStage = 1;
        pillSpawnedThisStage = 0;
        waterDropIndex = 0;
        pillIndex = 0;

        foreach (GameObject obj in waterDropPool)
            if (obj != null) obj.SetActive(false);

        foreach (GameObject obj in pillPool)
            if (obj != null) obj.SetActive(false);
    }
}
