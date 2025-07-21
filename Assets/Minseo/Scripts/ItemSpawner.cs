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
                Debug.Log($"장애물 발견: {child.name} on platform {platform.name}");
                return child;
            }
        }
        Debug.Log($"장애물 없음: platform {platform.name}");
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
                Debug.Log($"아이템 제거: {child.name}");
            }
        }
    }

    // 메인 스폰 메서드
    public void SpawnItemsOnPlatform(Vector2 platformPosition, Vector2 platformSize, GameObject platform)
    {
        ClearItemsFromPlatform(platform);

        if (platform == null) return;
        if (platformSize.x < minPlatformWidthForItems) return;

        platformCountThisStage++;

        Transform activeObstacle = FindActiveObstacleOnPlatform(platform);

        if (activeObstacle == null)
        {
            Debug.Log($"일직선 배치: {platform.name}");
            SpawnItemsLine(platform, platformPosition, platformSize);
        }
        else
        {
            Debug.Log($"곡선 배치: {platform.name}");
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
            Debug.LogWarning("사용 가능한 위치가 없어서 일직선 배치로 변경");
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

        // 알약 배치 (햇빛과 겹치지 않게, 최소 2개 위치 필요)
        int pillIdx = -1;
        if (pillSpawnedThisStage < maxPillsPerStage && pillPool != null && usablePositions.Count >= 2)
        {
            if (sunlightIdx != -1)
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
            else
            {
                pillIdx = Random.Range(0, usablePositions.Count);
            }
            
            if (pillIdx != -1)
            {
                SpawnPill(usablePositions[pillIdx], platform);
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
        pill.transform.SetParent(parentPlatform.transform, false);
        pill.transform.localPosition = localPosition;
        pill.SetActive(true);
        pillSpawnedThisStage++;
        pillIndex = (pillIndex + 1) % pillPoolCount;
    }

    void SpawnSunlight(Vector2 localPosition, GameObject parentPlatform)
    {
        if (sunlightPool == null || sunlightPool.Length == 0) return;

        GameObject sunlight = sunlightPool[sunlightIndex];
        sunlight.transform.SetParent(parentPlatform.transform, false);
        sunlight.transform.localPosition = localPosition;
        sunlight.SetActive(true);
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
