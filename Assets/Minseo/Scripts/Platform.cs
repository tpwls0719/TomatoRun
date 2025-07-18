using UnityEngine;
using System.Collections.Generic;

public class Platform : MonoBehaviour
{
    [Header("장애물 설정")]
    public GameObject obstaclePrefab;       // 프리팹 할당
    [Range(0f,1f)] public float spawnChance = 0.3f;
    public float heightOffset = 1f;         // 플랫폼 위 높이

    [HideInInspector]
    public List<Collider2D> obstacles = new List<Collider2D>();
    
    private bool stepped = false; // 플레이어 캐릭터가 밟았었는가

    void OnEnable()
    {
        // 밟힘 상태를 리셋
        stepped = false;
        
        // 기존 장애물들 정리
        ClearObstacles();
        
        // 새 장애물 스폰
        SpawnObstacle();
    }

    void ClearObstacles()
    {
        // 기존 장애물들 제거
        foreach (var obstacle in obstacles)
        {
            if (obstacle != null && obstacle.gameObject != null)
            {
                DestroyImmediate(obstacle.gameObject);
            }
        }
        obstacles.Clear();
    }

    void SpawnObstacle()
    {
        // 장애물 프리팹이 없다면 Resources에서 자동 로드
        if (obstaclePrefab == null)
        {
            obstaclePrefab = Resources.Load<GameObject>("Stage1_Obstacle01_0");
            if (obstaclePrefab == null)
            {
                Debug.LogWarning("[Platform] 장애물 프리팹을 찾을 수 없습니다.");
                return;
            }
        }

        if (Random.value > spawnChance) return;

        // 인스턴스화
        GameObject ob = Instantiate(obstaclePrefab, transform);
        ob.transform.localPosition = new Vector3(0, heightOffset, 0);
        ob.transform.localScale = Vector3.one * 0.3f;  // 크기 조정

        // Collider2D 저장
        var col = ob.GetComponent<Collider2D>();
        if (col != null) 
        {
            obstacles.Add(col);
            Debug.Log($"[Platform] 장애물 스폰됨 - 위치: {ob.transform.localPosition}, 스케일: {ob.transform.localScale}");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 상대방의 태그가 Player && 이전에 플레이어가 밟지 않았다면
        if (collision.collider.tag=="Player" && !stepped)
        {
            // 플레이어가 밟았음을 기록
            stepped = true;

            // GameManager의 점수 증가 메서드 호출
            //GameManager.instance.AddScore(1);
        }
    }
}
