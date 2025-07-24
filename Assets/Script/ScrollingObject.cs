using UnityEngine;

// 게임 오브젝트를 계속 좌측으로 스크롤하는 스크립트
public class ScrollingObject : MonoBehaviour
{
    public float speed = 6f;        // 기본 스크롤 속도
    private float originalSpeed;      // 원래 속도 저장용

    private void Start()
    {
        originalSpeed = speed;        // 시작 시 기본 속도 저장
    }

    private void Update()
    {
        // 게임 오버 등 조건을 걸고 싶다면 여기에 추가 가능
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    /// <summary>
    /// 속도에 배율을 곱해 일시적으로 속도를 증가시킴
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speed = originalSpeed * multiplier;
    }

    /// <summary>
    /// 속도를 원래대로 복구함
    /// </summary>
    public void ResetSpeed()
    {
        speed = originalSpeed;
    }
}
