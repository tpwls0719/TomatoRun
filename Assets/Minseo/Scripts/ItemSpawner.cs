using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;
    public GameObject pillPrefab;
    public GameObject sunlightPrefab;

    [Header("스폰 확률 (0~1)")]
    [Range(0f,1f)] public float pillSpawnChance     = 0.3f;
    [Range(0f,1f)] public float sunlightSpawnChance = 0.2f;

    [Header("스폰 높이 오프셋")]
    public float itemHeightOffset         = 0.4f;
    public float minPlatformWidthForItems = 4.0f;

    [Header("풀 크기")]
    public int waterDropPoolCount = 90;
    public int pillPoolCount      = 10;
    public int sunlightPoolCount  = 4;

    [Header("스테이지당 최대 알약 개수")]
    public int maxPillsPerStage    = 5;
    [Header("스테이지당 최대 햇빛 개수")]
    public int maxSunlightPerStage = 1;

    [Header("플랫폼 카운트")]
    public int platformsPerStage = 10;

    [Header("시간 기반 스테이지")]
    public int totalGameStages   = 4;
    private float stageTimeInterval = 37.5f; // 150초 / 4

    [Header("레이어 마스크")]
    public LayerMask obstacleLayerMask;

    public static ItemSpawner Instance { get; private set; }

    int currentStage = 1;
    int platformCountThisStage;
    int pillSpawnedThisStage;
    int sunlightSpawnedThisStage;

    GameObject[] waterDropPool;
    GameObject[] pillPool;
    GameObject[] sunlightPool;

    int waterCursor, pillCursor, sunCursor;

    void Awake()
    {
        if (Instance == null) { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        InitPools();
    }

    void InitPools()
    {
        waterDropPool  = CreatePool(waterDropPrefab, waterDropPoolCount);
        pillPool       = CreatePool(pillPrefab,      pillPoolCount);
        sunlightPool   = CreatePool(sunlightPrefab,  sunlightPoolCount);
    }

    GameObject[] CreatePool(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return null;
        var arr = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            arr[i] = Instantiate(prefab, Vector3.one * 9999, Quaternion.identity);
            arr[i].SetActive(false);
        }
        return arr;
    }

    GameObject GetInactiveWaterDrop()
    {
        if (waterDropPool == null) return null;
        int len = waterDropPool.Length;
        for (int i = 0; i < len; i++)
        {
            int idx = (waterCursor + i) % len;
            if (!waterDropPool[idx].activeSelf)
            {
                waterCursor = (idx + 1) % len;
                return waterDropPool[idx];
            }
        }
        return null;
    }

    GameObject Pull(ref GameObject[] pool, ref int cursor, GameObject prefab, int growBy = 5)
    {
        if (pool == null) return null;
        int len = pool.Length;
        for (int i = 0; i < len; i++)
        {
            int idx = (cursor + i) % len;
            if (pool[idx] != null && !pool[idx].activeSelf)
            {
                cursor = (idx + 1) % len;
                Debug.Log($"풀에서 오브젝트 재사용: {prefab.name}, 인덱스: {idx}");
                return pool[idx];
            }
        }
        
        // 모두 사용 중이면 풀 확장
        Debug.Log($"풀 확장 중: {prefab.name}, 기존 크기: {len}, 확장: {growBy}");
        int old = len;
        System.Array.Resize(ref pool, len + growBy);
        for (int i = 0; i < growBy; i++)
        {
            pool[old + i] = Instantiate(prefab, Vector3.one * 9999, Quaternion.identity);
            pool[old + i].SetActive(false);
            Debug.Log($"새 오브젝트 생성: {prefab.name}_{old + i}");
        }
        cursor = (old + 1) % pool.Length;
        return pool[old];
    }

    /// <summary>
    /// 플레이어가 알약을 먹을 때 UIManager.CollectPill()에서 호출.
    /// 스폰 제한 카운터를 하나 줄여 줘서
    /// "먹은 만큼 다시 스폰할 수 있도록" 만든 메서드.
    /// </summary>
    public void OnPillCollected()
    {
        if (pillSpawnedThisStage > 0)
            pillSpawnedThisStage--;
    }

    public void SpawnItemsOnPlatform(Vector2 pos, Vector2 size, GameObject platform)
    {
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null && !gm.IsGameActive()) return;
        if (platform == null || size.x < minPlatformWidthForItems) return;

        // 이전 아이템 전부 비활성화
        ClearItems(platform);

        // 스테이지 카운트
        if (++platformCountThisStage > platformsPerStage)
        {
            platformCountThisStage      = 1;
            pillSpawnedThisStage        = 0;
            sunlightSpawnedThisStage    = 0;
            currentStage++;
        }

        var obs = FindActiveObstacle(platform);
        if (obs == null) SpawnLine(platform, size);
        else            SpawnCurve(platform, obs, size);
    }

    Transform FindActiveObstacle(GameObject plat)
    {
        foreach (var c in plat.GetComponentsInChildren<Transform>())
            if (c.CompareTag("Hit") && c.gameObject.activeInHierarchy)
                return c;
        return null;
    }

    void ClearItems(GameObject plat)
    {
        foreach (var c in plat.GetComponentsInChildren<Transform>())
        {
            if (c.CompareTag("Pill") || c.CompareTag("Sunlight") || c.CompareTag("Water"))
            {
                c.SetParent(null);
                c.gameObject.SetActive(false);
            }
        }
    }

    void SpawnLine(GameObject plat, Vector2 size)
    {
        float w = size.x, y = size.y / 2 + itemHeightOffset;
        int cnt = Mathf.Clamp(Mathf.FloorToInt(w / 3f), 3, 5);

        var posList = new List<Vector2>();
        for (int i = 0; i < cnt; i++)
            posList.Add(GetLinePos(w, cnt, i, y));

        // 햇빛
        int sunIdx = -1;
        if (CanSpawnSunlight() && Random.value < sunlightSpawnChance)
        {
            sunIdx = Random.Range(0, posList.Count);
            SpawnSun(posList[sunIdx], plat);
        }

        // 알약
        int pillIdx = -1;
        if (pillSpawnedThisStage < maxPillsPerStage && Random.value < pillSpawnChance)
        {
            pillIdx = RandExcept(posList.Count, sunIdx);
            SpawnPill(posList[pillIdx], plat);
        }

        // 물방울
        for (int i = 0; i < posList.Count; i++)
        {
            if (i == sunIdx || i == pillIdx) continue;
            var drop = GetInactiveWaterDrop();
            if (drop == null) continue;
            drop.transform.SetParent(plat.transform, false);
            drop.transform.localPosition = posList[i];
            drop.SetActive(true);
        }
    }

    Vector2 GetLinePos(float w, int c, int idx, float y)
    {
        float m = w * 0.15f, avail = w - m * 2;
        float step = (c == 1 ? 0 : avail / (c - 1));
        return new Vector2(-avail / 2 + step * idx, y);
    }

    void SpawnCurve(GameObject plat, Transform obs, Vector2 size)
    {
        float w = size.x, h = size.y, r = w * 0.45f;
        int cnt = 8;
        var all = new List<Vector2>();
        for (int i = 0; i < cnt; i++)
        {
            float t = i / (cnt - 1f),
                  ang = Mathf.Lerp(-75, 75, t) * Mathf.Deg2Rad;
            all.Add(new Vector2(Mathf.Sin(ang) * r,
                                (h / 2) + itemHeightOffset + Mathf.Cos(ang) * r * 0.7f - 1.6f));
        }

        var use = new List<Vector2>();
        foreach (var p in all)
        {
            var wp = plat.transform.TransformPoint(p);
            if (!Physics2D.OverlapCircle(wp, 0.8f, obstacleLayerMask))
                use.Add(p);
        }

        if (use.Count == 0)
        {
            SpawnLine(plat, size);
            return;
        }

        int sunIdx = -1;
        if (CanSpawnSunlight() && Random.value < sunlightSpawnChance)
        {
            sunIdx = Random.Range(0, use.Count);
            SpawnSun(use[sunIdx], plat);
        }

        int pillIdx = -1;
        if (pillSpawnedThisStage < maxPillsPerStage && Random.value < pillSpawnChance)
        {
            pillIdx = RandExcept(use.Count, sunIdx);
            SpawnPill(use[pillIdx], plat);
        }

        for (int i = 0; i < use.Count; i++)
        {
            if (i == sunIdx || i == pillIdx) continue;
            var drop = GetInactiveWaterDrop();
            if (drop == null) continue;
            drop.transform.SetParent(plat.transform, false);
            drop.transform.localPosition = use[i];
            drop.SetActive(true);
        }
    }

    void SpawnPill(Vector2 loc, GameObject plat)
    {
        Debug.Log($"알약 스폰 시도: 현재 스테이지 {currentStage}, 이미 스폰된 알약: {pillSpawnedThisStage}/{maxPillsPerStage}");
        
        var o = Pull(ref pillPool, ref pillCursor, pillPrefab, 3);
        if (!o) 
        {
            Debug.LogError("알약 스폰 실패: 풀에서 오브젝트를 가져올 수 없음");
            return;
        }
        
        o.transform.SetParent(plat.transform, false);
        o.transform.localPosition = loc;
        o.SetActive(true);
        pillSpawnedThisStage++;
        
        Debug.Log($"알약 스폰 성공: {o.name}, 위치: {loc}, 현재 스폰 수: {pillSpawnedThisStage}");
    }

    void SpawnSun(Vector2 loc, GameObject plat)
    {
        var o = Pull(ref sunlightPool, ref sunCursor, sunlightPrefab, 2);
        if (!o) return;
        o.transform.SetParent(plat.transform, false);
        o.transform.localPosition = loc;
        o.SetActive(true);
        sunlightSpawnedThisStage++;
    }

    bool CanSpawnSunlight()
    {
        var ui = FindFirstObjectByType<UIManager>();
        if (ui == null) return false;
        float t = ui.GameTime;
        for (int st = 1; st <= totalGameStages; st++)
        {
            float end = st * stageTimeInterval;
            if (t >= end - 10f && t <= end)
            {
                if (currentStage != st)
                {
                    currentStage = st;
                    sunlightSpawnedThisStage = 0;
                }
                return sunlightSpawnedThisStage < maxSunlightPerStage;
            }
        }
        return false;
    }

    int RandExcept(int size, int ex)
    {
        if (size <= 1) return 0;
        int i;
        do { i = Random.Range(0, size); } while (i == ex);
        return i;
    }

    public void RestartGame()
    {
        currentStage = 1;
        platformCountThisStage = 0;
        pillSpawnedThisStage = 0;
        sunlightSpawnedThisStage = 0;
        waterCursor = pillCursor = sunCursor = 0;
        ResetPool(waterDropPool);
        ResetPool(pillPool);
        ResetPool(sunlightPool);
    }

    void ResetPool(GameObject[] pool)
    {
        if (pool == null) return;
        
        int resetCount = 0;
        foreach (var o in pool)
        {
            if (o != null)
            {
                // 부모 관계 해제
                o.transform.SetParent(null);
                // 위치 초기화
                o.transform.position = Vector3.one * 9999;
                // 비활성화
                o.SetActive(false);
                resetCount++;
            }
        }
        Debug.Log($"풀 리셋 완료: {resetCount}개 오브젝트 리셋됨");
    }
}
