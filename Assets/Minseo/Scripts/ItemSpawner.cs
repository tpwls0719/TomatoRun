using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;  // 물방울
    public GameObject pillPrefab;       // 알약

    [Header("스폰 확률")]
    [Range(0f,1f)] public float waterDropSpawnChance = 0.7f;
    [Range(0f,1f)] public float pillSpawnChance      = 0.1f;

    [Header("위치 설정")]
    public float itemHeightOffset = 0.5f; // 플랫폼 위로 올리는 높이
    public float itemSpacing      = 1.5f; // 슬롯당 간격
    public float minPlatformWidthForItems = 2f;

    [Header("풀 크기")]
    public int waterDropPoolCount = 20;
    public int pillPoolCount      = 5;

    [Header("스테이지 설정")]
    public int currentStage     = 1;
    public int maxPillsPerStage = 2;
    private int pillSpawnedThisStage = 0;

    // 오브젝트 풀
    GameObject[] waterDropPool;
    GameObject[] pillPool;
    int waterDropIndex = 0;
    int pillIndex      = 0;

    public static ItemSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance==null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 풀 초기화
        waterDropPool = new GameObject[waterDropPoolCount];
        for(int i=0;i<waterDropPoolCount;i++){
            var go = Instantiate(waterDropPrefab);
            go.SetActive(false);
            waterDropPool[i] = go;
        }
        pillPool = new GameObject[pillPoolCount];
        for(int i=0;i<pillPoolCount;i++){
            var go = Instantiate(pillPrefab);
            go.SetActive(false);
            pillPool[i] = go;
        }
    }

    // PlatformSpawner에서 호출
    public void SpawnItemsOnPlatform(GameObject platform)
    {
        // 1) 콜라이더로 실제 bounds 구하기
        var col = platform.GetComponent<Collider2D>();
        if (col==null) return;
        Vector2 min = col.bounds.min;
        Vector2 max = col.bounds.max;
        float width = max.x - min.x;
        if (width < minPlatformWidthForItems) return;

        // 2) 플랫폼 위 장애물 리스트
        var obstacles = FindObstaclesOnPlatform(platform);

        // 3) 장애물 유무 따라 분기
        if (obstacles.Count > 0)
            SpawnCookieRunStyle(platform, obstacles, min, max);
        else
            SpawnFullPlatformStyle(platform, min, max, width);
    }

    // Platform.cs 컴포넌트에서 직접 장애물 리스트 가져오기
    List<Collider2D> FindObstaclesOnPlatform(GameObject platform)
    {
        var platformComponent = platform.GetComponent<Platform>();
        if (platformComponent != null)
        {
            Debug.Log($"[ItemSpawner] Platform.cs에서 장애물 {platformComponent.obstacles.Count}개 읽어옴");
            return platformComponent.obstacles;
        }
        
        Debug.LogWarning("[ItemSpawner] Platform 컴포넌트가 없어서 빈 리스트 반환");
        return new List<Collider2D>();
    }

    // ───── 쿠키런 스타일 배치 ─────
    void SpawnCookieRunStyle(GameObject platform,
                             List<Collider2D> obstacles,
                             Vector2 min, Vector2 max)
    {
        // A) 장애물 바로 위에 1개씩
        foreach(var obs in obstacles)
        {
            Vector2 worldTop = new Vector2(
                obs.bounds.center.x,
                obs.bounds.max.y + itemHeightOffset
            );
            if (!IsOverlappingAnyObstacle(worldTop, obstacles))
                SpawnSingleItemWorld(worldTop, platform);
        }

        // B) 남은 공간 슬롯으로 나눠서 배치
        float totalWidth = max.x - min.x;
        int slots = Mathf.Clamp(Mathf.FloorToInt(totalWidth / itemSpacing), 1, 5);
        float slotW = totalWidth / (slots + 1);

        for(int i=0;i<slots;i++)
        {
            Vector2 worldPos = new Vector2(
                min.x + slotW*(i+1),
                max.y + itemHeightOffset
            );
            if (IsOverlappingAnyObstacle(worldPos, obstacles)) 
                continue;
            SpawnSingleItemWorld(worldPos, platform);
        }
    }

    // ───── 장애물 없을 때 전체 균등 배치 ─────
    void SpawnFullPlatformStyle(GameObject platform,
                                Vector2 min, Vector2 max, float width)
    {
        int count = Mathf.Clamp(Mathf.FloorToInt(width / itemSpacing), 1, 5);
        float slotW = width / (count + 1);

        for(int i=0;i<count;i++)
        {
            Vector2 worldPos = new Vector2(
                min.x + slotW*(i+1),
                max.y + itemHeightOffset
            );
            SpawnSingleItemWorld(worldPos, platform);
        }
    }

    // 월드 좌표가 any 장애물 bounds 안에 있는지 체크
    bool IsOverlappingAnyObstacle(Vector2 worldPos, List<Collider2D> obstacles)
    {
        return obstacles.Any(o => o.bounds.Contains(worldPos));
    }

    // ───── 실제 스폰 ─────
    void SpawnSingleItemWorld(Vector2 worldPos, GameObject platform)
    {
        float r = Random.value;
        if (r < pillSpawnChance && pillSpawnedThisStage < maxPillsPerStage)
        {
            SpawnPillWorld(worldPos, platform);
            pillSpawnedThisStage++;
        }
        else if (r < pillSpawnChance + waterDropSpawnChance)
        {
            SpawnWaterWorld(worldPos, platform);
        }
    }

    void SpawnWaterWorld(Vector2 worldPos, GameObject platform)
    {
        var drop = waterDropPool[waterDropIndex];
        drop.SetActive(false);
        drop.transform.position = worldPos;
        drop.transform.SetParent(platform.transform);  // worldPositionStays = true
        drop.SetActive(true);

        waterDropIndex = (waterDropIndex + 1) % waterDropPoolCount;
    }

    void SpawnPillWorld(Vector2 worldPos, GameObject platform)
    {
        var pill = pillPool[pillIndex];
        pill.SetActive(false);
        pill.transform.position = worldPos;
        pill.transform.SetParent(platform.transform);
        pill.SetActive(true);

        pillIndex = (pillIndex + 1) % pillPoolCount;
    }

    // 스테이지 변경 시 알약 리셋
    public void ChangeStage(int newStage)
    {
        currentStage = newStage;
        pillSpawnedThisStage = 0;
    }

    // 게임 재시작 시 인덱스 & 풀 초기화
    public void RestartGame()
    {
        pillSpawnedThisStage = 0;
        waterDropIndex = pillIndex = 0;
        foreach(var w in waterDropPool) if (w) w.SetActive(false);
        foreach(var p in pillPool)      if (p) p.SetActive(false);
    }
}
