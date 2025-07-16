using UnityEngine;

//게임 오브젝트를 계속 좌측으로 스크롤하는 스크립트
public class ScrollingObject : MonoBehaviour
{
    public float speed = 100f; // 스크롤 속도

    private void Update()
    {
        /*if(!GameManager.instance.isGameOver)
        {*/
            //게임 오버 상태가 아니라면
            //스크롤 속도에 따라 좌측으로 이동
        
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        //}

    }
}
