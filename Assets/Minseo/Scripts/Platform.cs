using UnityEngine;

public class Platform : MonoBehaviour
{
    public GameObject[] obstacles; // 장애물 컴포넌트
    private bool stepped = false; // 플레이어 캐릭터가 밟았었는가

    // 컴포넌트가 활성활될 때마다 매번 실행되는 메서드
    private void OnEnable()
    {
        // 밟힘 상태를 리셋
        stepped = false;

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
        }
    }
}
