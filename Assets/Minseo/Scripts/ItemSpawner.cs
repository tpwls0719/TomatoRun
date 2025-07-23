using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;
    public GameObject pillPrefab;
    public GameObject sunlightPrefab;

    [Header("스폰 확률")]
    [Range(0f, 1f)]
    public float waterDropSpawnChance = 0.7f;
    [Range(0f, 1f)]
    public float pillSpawnChance = 0.3f;
    [Range(0f, 1f)]
    public float sunlightSpawnChance = 0.2f;

    [Header("스폰 높이 오프셋")]
    public float itemHeightOffset = 0.4f;  // 픽셀 유닛 변경으로 2배 증가
    public float minPlatformWidthForItems = 4.0f;  // 픽셀 유닛 변경으로 2배 증가

    [Header("풀 개수")]
    public int waterDropPoolCount = 30;
    public int pillPoolCount = 5;
    public int sunlightPoolCount = 3;

    [Header("스테이지 설정")]
    public int currentStage = 1;
    private int pillSpawnedThisStage = 0;
    private int sunlightSpawnedThisStage = 0;
    private int maxPillsPerStage = 3;
    private int maxSunlightPerStage = 1;
    
    [Header("플랫폼 카운터")]
    public int platformsPerStage = 10;
    private int platformCountThisStage = 0;

    private GameObject[] waterDropPool;
    private GameObject[] pillPool;
    private GameObject[] sunlightPool;
    private int waterDropIndex = 0;
    private int pillIndex = 0;
    private int sunlightIndex = 0;

    public LayerMask obstacleLayerMask;

    public static ItemSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePools();
    }

    void InitializePools()
    {
        if (waterDropPrefab != null)
        {
            waterDropPool = new GameObject[waterDropPoolCount];
            for (int i = 0; i < waterDropPoolCount; i++)
            {
                waterDropPool[i] = Instantiate(waterDropPrefab);
                waterDropPool[i].transform.position = new Vector3(1000f, 1000f, 0f);
                waterDropPool[i].SetActive(false);
            }
        }
        if (pillPrefab != null)
        {
            pillPool = new GameObject[pillPoolCount];
            for (int i = 0; i < pillPoolCount; i++)
            {
                pillPool[i] = Instantiate(pillPrefab);
                pillPool[i].transform.position = new Vector3(1000f, 1000f, 0f);
                pillPool[i].SetActive(false);
            }
        }
        if (sunlightPrefab != null)
        {
            sunlightPool = new GameObject[sunlightPoolCount];
            for (int i = 0; i < sunlightPoolCount; i++)
            {
                sunlightPool[i] = Instantiate(sunlightPrefab);
                sunlightPool[i].transform.position = new Vector3(1000f, 1000f, 0f);
                sunlightPool[i].SetActive(false);
            }
        }
    }

    // 활성화된 장애물 찾기
    Transform FindActiveObstacleOnPlatform(GameObject platform)
    {
        Transform[] allChildren = platform.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child != platform.transform && 
                child.CompareTag("Hit") && 
                child.gameObject.activeInHierarchy)
            {
                return child;
            }
        }
        return null;
    }

    // 플랫폼에서 기존 아이템 모두 제거
    void ClearItemsFromPlatform(GameObject platform)
    {
        Transform[] allChildren = platform.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child != platform.transform && 
                (child.CompareTag("Water") || child.CompareTag("Pill") || child.CompareTag("Sunlight")))
            {
                child.gameObject.SetActive(false);
                child.SetParent(null);
            }
        }
    }

    // 메인 스폰 메서드
    public void SpawnItemsOnPlatform(Vector2 platformPosition, Vector2 platformSize, GameObject platform)
    {
        // 게임 오버 상태에서는 스폰 안함
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.isGameOver)
        {
            return;
        }

        // 플랫폼 유효성 검사
        if (platform == null) 
        {
            Debug.LogWarning("ItemSpawner: platform이 null입니다");
            return;
        }
        
        if (platformSize.x < minPlatformWidthForItems) 
        {
            Debug.Log($"ItemSpawner: 플랫폼이 너무 작습니다 ({platformSize.x} < {minPlatformWidthForItems})");
            return;
        }

        // 이미 아이템이 있는 플랫폼인지 확인 (중복 스폰 방지)
        if (platform.transform.childCount > 0)
        {
            bool hasActiveItems = false;
            for (int i = 0; i < platform.transform.childCount; i++)
            {
                Transform child = platform.transform.GetChild(i);
                if (child.gameObject.activeSelf && 
                    (child.CompareTag("Pill") || child.CompareTag("WaterDrop") || child.CompareTag("SunLight")))
                {
                    hasActiveItems = true;
                    break;
                }
            }
            if (hasActiveItems)
            {
                Debug.Log("ItemSpawner: 플랫폼에 이미 활성화된 아이템이 있어서 스킵합니다");
                return;
            }
        }

        ClearItemsFromPlatform(platform);
        platformCountThisStage++;

        // 스테이지 리셋 확인 (10개 플랫폼마다 스테이지 초기화)
        if (platformCountThisStage > platformsPerStage)
        {
            platformCountThisStage = 1;
            pillSpawnedThisStage = 0;
            sunlightSpawnedThisStage = 0;
            currentStage++;
        }

        Transform activeObstacle = FindActiveObstacleOnPlatform(platform);

        if (activeObstacle == null)
        {
            SpawnItemsLine(platform, platformPosition, platformSize);
        }
        else
        {
            SpawnItemsCurve(platform, activeObstacle, platformPosition, platformSize);
        }
    }

    // 장애물 없을 때: 일직선 배치 (개선됨)
    void SpawnItemsLine(GameObject platform, Vector2 platformPosition, Vector2 platformSize)
    {
        float width = platformSize.x;
        float itemY = (platformSize.y / 2f) + itemHeightOffset;
        int itemCount = Mathf.Clamp(Mathf.FloorToInt(width / 3f), 3, 5);  // 간격 조정 (픽셀 유닛 변경으로 두 배로 증가)

        // 배치할 위치들을 먼저 계산
        List<Vector2> positions = new List<Vector2>();
        for (int i = 0; i < itemCount; i++)
        {
            Vector2 pos = GetLineItemPosition(width, itemCount, i, itemY);
            positions.Add(pos);
        }

        // 햇빛 배치 (스테이지 마지막에만)
        int sunlightIdx = -1;
        bool isLastPlatformOfStage = (platformCountThisStage >= platformsPerStage);
        if (isLastPlatformOfStage && sunlightSpawnedThisStage < maxSunlightPerStage && sunlightPool != null)
        {
            sunlightIdx = Random.Range(0, positions.Count);
            SpawnSunlight(positions[sunlightIdx], platform);
        }

        // 알약 배치 (햇빛과 겹치지 않게)
        int pillIdx = -1;
        if (pillSpawnedThisStage < maxPillsPerStage && pillPool != null && positions.Count > 1)
        {
            if (sunlightIdx != -1)
            {
                do {
                    pillIdx = Random.Range(0, positions.Count);
                } while (pillIdx == sunlightIdx);
            }
            else
            {
                pillIdx = Random.Range(0, positions.Count);
            }
            
            SpawnPill(positions[pillIdx], platform);
        }

        // 물방울 배치 (햇빛, 알약 자리 제외)
        for (int i = 0; i < positions.Count; i++)
        {
            if (i == sunlightIdx || i == pillIdx) continue;
            SpawnWaterDrop(positions[i], platform);
        }
    }

    Vector2 GetLineItemPosition(float platformWidth, int count, int index, float y)
    {
        float margin = platformWidth * 0.15f;  // 여백 비율 증가
        float availableWidth = platformWidth - (2 * margin);

        if (count == 1) return new Vector2(0f, y);

        float spacing = availableWidth / (count - 1);
        float startX = -availableWidth / 2f;
        float x = startX + (spacing * index);

        return new Vector2(x, y);
    }

    // 장애물 있을 때: 곡선 배치 (완전히 수정됨)
    void SpawnItemsCurve(GameObject platform, Transform obstacle, Vector2 platformPosition, Vector2 platformSize)
    {
        float width = platformSize.x;
        float height = platformSize.y;
        int itemCount = 8; // 곡선상의 아이템 개수

        float arcStart = -75f;
        float arcEnd = 75f;
        float radius = width * 0.45f; // 반지름은 플랫폼 너비 비율로 계산하므로 그대로 유지

        // 곡선상의 모든 위치 계산 (장애물 더 아래로)
        List<Vector2> allPositions = new List<Vector2>();
        for (int i = 0; i < itemCount; i++)
        {
            float t = i / (float)(itemCount - 1);
            float angle = Mathf.Lerp(arcStart, arcEnd, t) * Mathf.Deg2Rad;
            
            // 장애물을 더 아래로 내리고 곡선을 더 넓게 조정
            Vector2 pos = new Vector2(
                Mathf.Sin(angle) * radius, 
                (height / 2f) + itemHeightOffset + (Mathf.Cos(angle) * radius * 0.7f) - 1.6f  // 픽셀 유닛 변경으로 오프셋 두 배로 증가
            );
            allPositions.Add(pos);
        }

        // 장애물과 겹치지 않는 위치만 필터링 (더 엄격한 검사)
        List<Vector2> usablePositions = new List<Vector2>();
        float checkRadius = 0.8f; // 픽셀 유닛 변경으로 충돌 검사 반경 두 배로 증가
        
        for (int i = 0; i < allPositions.Count; i++)
        {
            Vector2 worldPos = platform.transform.TransformPoint(allPositions[i]);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, checkRadius, obstacleLayerMask);
            if (hit == null)
            {
                usablePositions.Add(allPositions[i]);
            }
        }

        if (usablePositions.Count == 0)
        {
            SpawnItemsLine(platform, platformPosition, platformSize);
            return;
        }

        // 햇빛 배치 (스테이지 마지막에만)
        int sunlightIdx = -1;
        bool isLastPlatformOfStage = (platformCountThisStage >= platformsPerStage);
        if (isLastPlatformOfStage && sunlightSpawnedThisStage < maxSunlightPerStage && sunlightPool != null)
        {
            sunlightIdx = Random.Range(0, usablePositions.Count);
            SpawnSunlight(usablePositions[sunlightIdx], platform);
        }

        // 알약 배치 (조건 완화 - 더 자주 스폰)
        int pillIdx = -1;
        if (pillPool != null && usablePositions.Count >= 1)  // 조건 완화: 최소 1개 위치면 됨
        {
            if (sunlightIdx != -1 && usablePositions.Count > 1)
            {
                // 햇빛과 다른 위치 선택
                List<int> availableIndices = new List<int>();
                for (int i = 0; i < usablePositions.Count; i++)
                {
                    if (i != sunlightIdx) availableIndices.Add(i);
                }
                
                if (availableIndices.Count > 0)
                {
                    pillIdx = availableIndices[Random.Range(0, availableIndices.Count)];
                }
            }
            else if (usablePositions.Count > 0)  // 햇빛 없으면 아무 위치나
            {
                pillIdx = Random.Range(0, usablePositions.Count);
            }
            
            if (pillIdx != -1)
            {
                SpawnPill(usablePositions[pillIdx], platform);
                Debug.Log($"🔴 곡선 배치 알약 스폰 성공! 위치: {pillIdx}");
            }
        }

        // 물방울을 나머지 모든 위치에 배치 (햇빛, 알약 자리 제외)
        for (int i = 0; i < usablePositions.Count; i++)
        {
            if (i == sunlightIdx || i == pillIdx) continue;
            SpawnWaterDrop(usablePositions[i], platform);
        }
    }

    // 아이템 스폰 메서드들
    void SpawnWaterDrop(Vector2 localPosition, GameObject parentPlatform)
    {
        if (waterDropPool == null || waterDropPool.Length == 0) return;

        GameObject drop = waterDropPool[waterDropIndex];
        drop.transform.SetParent(parentPlatform.transform, false);
        drop.transform.localPosition = localPosition;
        drop.SetActive(true);
        waterDropIndex = (waterDropIndex + 1) % waterDropPoolCount;
    }

    void SpawnPill(Vector2 localPosition, GameObject parentPlatform)
    {
        if (pillPool == null || pillPool.Length == 0) return;

        GameObject pill = pillPool[pillIndex];
        if (pill == null) return;

        // 부모에서 분리 후 활성화
        pill.transform.SetParent(null);
        pill.SetActive(true);
        
        // 부모 설정
        pill.transform.SetParent(parentPlatform.transform, false);
        pill.transform.localPosition = localPosition;
        
        // 다시 한번 강제 활성화
        pill.SetActive(true);
        pill.gameObject.SetActive(true);
        
        // 컴포넌트 강제 활성화
        Collider2D pillCollider = pill.GetComponent<Collider2D>();
        if (pillCollider != null) 
        {
            pillCollider.enabled = true;
            pillCollider.isTrigger = true;
        }
        
        SpriteRenderer pillRenderer = pill.GetComponent<SpriteRenderer>();
        if (pillRenderer != null) 
        {
            pillRenderer.enabled = true;
            pillRenderer.color = new Color(1, 1, 1, 1); // 불투명하게
        }
        
        pillSpawnedThisStage++;
        pillIndex = (pillIndex + 1) % pillPoolCount;
    }

    void SpawnSunlight(Vector2 localPosition, GameObject parentPlatform)
    {
        if (sunlightPool == null || sunlightPool.Length == 0) return;

        GameObject sunlight = sunlightPool[sunlightIndex];
        if (sunlight == null) return;

        // 부모에서 분리 후 활성화
        sunlight.transform.SetParent(null);
        sunlight.SetActive(true);
        
        // 부모 설정
        sunlight.transform.SetParent(parentPlatform.transform, false);
        sunlight.transform.localPosition = localPosition;
        
        // 다시 한번 강제 활성화
        sunlight.SetActive(true);
        sunlight.gameObject.SetActive(true);
        
        // 컴포넌트 강제 활성화
        Collider2D sunlightCollider = sunlight.GetComponent<Collider2D>();
        if (sunlightCollider != null) 
        {
            sunlightCollider.enabled = true;
            sunlightCollider.isTrigger = true;
        }
        
        SpriteRenderer sunlightRenderer = sunlight.GetComponent<SpriteRenderer>();
        if (sunlightRenderer != null) 
        {
            sunlightRenderer.enabled = true;
            sunlightRenderer.color = new Color(1, 1, 1, 1); // 불투명하게
        }
        
        sunlightSpawnedThisStage++;
        sunlightIndex = (sunlightIndex + 1) % sunlightPoolCount;
    }

    // 스테이지 관리 메서드들
    public void ChangeStage(int newStage)
    {
        if (newStage >= 1 && newStage <= 4)
        {
            currentStage = newStage;
            pillSpawnedThisStage = 0;
            sunlightSpawnedThisStage = 0;
            platformCountThisStage = 0;
        }
    }

    public void RestartGame()
    {
        currentStage = 1;
        pillSpawnedThisStage = 0;
        sunlightSpawnedThisStage = 0;
        platformCountThisStage = 0;
        waterDropIndex = 0;
        pillIndex = 0;
        sunlightIndex = 0;

        if (waterDropPool != null)
        {
            foreach (GameObject obj in waterDropPool)
                if (obj != null) obj.SetActive(false);
        }
        if (pillPool != null)
        {
            foreach (GameObject obj in pillPool)
                if (obj != null) obj.SetActive(false);
        }
        if (sunlightPool != null)
        {
            foreach (GameObject obj in sunlightPool)
                if (obj != null) obj.SetActive(false);
        }
    }
}