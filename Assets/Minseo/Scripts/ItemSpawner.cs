using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    /* ───── 아이템 프리팹 ───── */
    [Header("아이템 프리팹")]
    public GameObject waterDropPrefab;
    public GameObject pillPrefab;
    public GameObject sunlightPrefab;

    /* ───── 스폰 확률 (물방울·알약만) ───── */
    [Header("스폰 확률 (0~1)")]
    [Range(0f, 1f)] public float pillSpawnChance     = 0.3f;
    [Range(0f, 1f)] public float sunlightSpawnChance = 0.2f; // ‼ 햇빛은 고정 타이밍으로만 나오므로 사실상 미사용

    /* ───── 위치·풀·카운트 설정 ───── */
    [Header("스폰 높이 오프셋")]
    public float itemHeightOffset         = 0.4f;
    public float minPlatformWidthForItems = 4.0f;

    [Header("풀 크기")]
    public int waterDropPoolCount = 90;
    public int pillPoolCount      = 10;
    public int sunlightPoolCount  = 4;

    [Header("스테이지당 최대 알약/햇빛")]
    public int maxPillsPerStage    = 5;

    [Header("플랫폼 카운트")]
    public int platformsPerStage = 10;

    /* ───── 시간 기반 햇빛 타이밍 ───── */
    [Header("고정 햇빛 타이밍(초)")]
    // 35, 35+37.5, … — Start()에서 계산
    readonly List<float> sunlightTimes = new();
    int nextSunIdx = 0; // 다음 햇빛 타이밍 인덱스

    /* ───── 기타 ───── */
    [Header("레이어 마스크")]
    public LayerMask obstacleLayerMask;

    public static ItemSpawner Instance { get; private set; }

    int currentStage = 1;
    int platformCountThisStage;
    int pillSpawnedThisStage;

    GameObject[] waterDropPool;
    GameObject[] pillPool;
    GameObject[] sunlightPool;

    int waterCursor, pillCursor, sunCursor;

    /* ───────────────────────────────────────────────────────────── */

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        InitPools();

        // 햇빛 스폰 시각 계산: 35, 72.5, 110, 147.5
        for (int i = 0; i < 4; i++)
            sunlightTimes.Add(30f + 37.5f * i);
    }

    /* ─── 풀 초기화 ─── */
    void InitPools()
    {
        waterDropPool = CreatePool(waterDropPrefab, waterDropPoolCount);
        pillPool      = CreatePool(pillPrefab,      pillPoolCount);
        sunlightPool  = CreatePool(sunlightPrefab,  sunlightPoolCount);
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

    /* ─── 풀에서 비활성 오브젝트 꺼내오기 ─── */
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
                return pool[idx];
            }
        }

        // 꽉 찼다면 풀 확장
        System.Array.Resize(ref pool, len + growBy);
        for (int i = 0; i < growBy; i++)
        {
            pool[len + i] = Instantiate(prefab, Vector3.one * 9999, Quaternion.identity);
            pool[len + i].SetActive(false);
        }
        cursor = (len + 1) % pool.Length;
        return pool[len];
    }

    /* ─── 알약 먹었을 때 호출 → 스폰 제한 해제 ─── */
    public void OnPillCollected()
    {
        if (pillSpawnedThisStage > 0)
            pillSpawnedThisStage--;
    }

    /* ─── 발판마다 아이템 스폰 ─── */
    public void SpawnItemsOnPlatform(Vector2 pos, Vector2 size, GameObject platform)
    {
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null && !gm.IsGameActive()) return;
        if (platform == null || size.x < minPlatformWidthForItems) return;

        // 이전 아이템 비활성화
        ClearItems(platform);

        // 스테이지 플랫폼 카운트
        if (++platformCountThisStage > platformsPerStage)
        {
            platformCountThisStage = 1;
            pillSpawnedThisStage   = 0;
            currentStage++;
        }

        var obs = FindActiveObstacle(platform);
        if (obs == null) SpawnLine(platform, size);
        else             SpawnCurve(platform, obs, size);
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

    /* ─── 직선 패턴 ─── */
    void SpawnLine(GameObject plat, Vector2 size)
    {
        float w = size.x, y = size.y / 2 + itemHeightOffset;
        int cnt = Mathf.Clamp(Mathf.FloorToInt(w / 3f), 3, 5);

        var posList = new List<Vector2>();
        for (int i = 0; i < cnt; i++)
            posList.Add(GetLinePos(w, cnt, i, y));

        // 햇빛
        int sunIdx = -1;
        if (CanSpawnSunlight())
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

    /* ─── 곡선 패턴 ─── */
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
        if (CanSpawnSunlight())
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

    /* ─── 알약/햇빛 스폰 ─── */
    void SpawnPill(Vector2 loc, GameObject plat)
    {
        var o = Pull(ref pillPool, ref pillCursor, pillPrefab, 3);
        if (!o) return;

        o.transform.SetParent(plat.transform, false);
        o.transform.localPosition = loc;
        o.SetActive(true);
        pillSpawnedThisStage++;
    }

    void SpawnSun(Vector2 loc, GameObject plat)
    {
        var o = Pull(ref sunlightPool, ref sunCursor, sunlightPrefab, 2);
        if (!o) return;

        o.transform.SetParent(plat.transform, false);
        o.transform.localPosition = loc;
        o.SetActive(true);

        nextSunIdx++; // 다음 예약 타이밍으로
    }

    /* ─── 햇빛 스폰 가능? ─── */
    bool CanSpawnSunlight()
    {
        if (nextSunIdx >= sunlightTimes.Count) return false; // 예약 끝
        var ui = FindFirstObjectByType<UIManager>();
        if (ui == null) return false;

        return ui.GameTime >= sunlightTimes[nextSunIdx];
    }

    /* ─── 유틸 ─── */
    int RandExcept(int size, int ex)
    {
        if (size <= 1) return 0;
        int i;
        do { i = Random.Range(0, size); } while (i == ex);
        return i;
    }

    /* ─── 재시작 ─── */
    public void RestartGame()
    {
        currentStage = 1;
        platformCountThisStage = 0;
        pillSpawnedThisStage   = 0;

        waterCursor = pillCursor = sunCursor = 0;
        nextSunIdx  = 0;

        ResetPool(waterDropPool);
        ResetPool(pillPool);
        ResetPool(sunlightPool);
    }

    void ResetPool(GameObject[] pool)
    {
        if (pool == null) return;
        foreach (var o in pool)
        {
            if (o != null)
            {
                o.transform.SetParent(null);
                o.transform.position = Vector3.one * 9999;
                o.SetActive(false);
            }
        }
    }
}
