using UnityEngine;

public class Platform : MonoBehaviour
{
    public GameObject[] obstacles; // 장애물 컴포넌트
    private bool stepped = false; // 플레이어 캐릭터가 밟았었는가
    public float speed = 5f; // 플랫폼 이동 속도
    private float leftBorder; // 화면 왼쪽 경계

    // 컴포넌트가 활성활될 때마다 매번 실행되는 메서드
    private void OnEnable()
    {
        // 밟힘 상태를 리셋
        stepped = false;
        
        // 화면 왼쪽 경계 계산 (카메라 기준 왼쪽 경계 + 플랫폼이 완전히 사라질 여유)
        leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(-0.5f, 0, 0)).x;

        // 장애물의 개수만큼 루프
        for (int i = 0; i < obstacles.Length; i++)
        {
            // 현재 순번의 장애물을 1/3의 확률로 활성화
            if (Random.Range(0, 3) == 0)
            {
                obstacles[i].SetActive(true);
            }
            else
            {
                obstacles[i].SetActive(false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 상대방의 태그가 Plaer && 이전에 플레이어가 밟지 않았다면
        if (collision.collider.tag=="Player" && !stepped)
        {
            // 플레이어가 밟았음을 기록
            stepped = true;

            // GameManager의 점수 증가 메서드 호출
            //GameManager.instance.AddScore(1);
            
            // UIManager를 통해 점수 추가
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(1);
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // 왼쪽으로 일정 속도로 이동
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        
        // 플랫폼의 너비 구하기
        float platformWidth = 0f;
        if (GetComponent<Renderer>() != null)
        {
            platformWidth = GetComponent<Renderer>().bounds.size.x;
        }
        else if (GetComponent<Collider2D>() != null)
        {
            platformWidth = GetComponent<Collider2D>().bounds.size.x;
        }
        
        // 플랫폼이 완전히 화면 밖으로 나갔을 때만 비활성화
        // 플랫폼의 오른쪽 끝이 화면 왼쪽을 벗어났는지 확인
        if (transform.position.x + (platformWidth / 2) < leftBorder)
        {
            gameObject.SetActive(false);
        }
    }
}
