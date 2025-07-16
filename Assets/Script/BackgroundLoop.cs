using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    private float width; // 배경의 너비

    private void Awake() //start() 메서드보다 먼저 호출
    {
        //BoxCollider2D 컴포넌트를 가져와 배경의 너비를 설정
        BoxCollider2D backgroundCollider = GetComponent<BoxCollider2D>();
        width = backgroundCollider.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        //현재 위치가 원점에서 왼쪽으로 width 이상 이동했을때 위치를 리셋
        if (transform.position.x <= -width)
        {
            Reposition();
        }

    }

    //위치를 리셋하는 메서드
    private void Reposition()
    {
        //현재 위치에서 오른쪽으로 가로길이 *2 만큼 이동
        Vector2 offset = new Vector2(width * 2f, 0f);
        transform.position = (Vector2)transform.position + offset;
    }
}
