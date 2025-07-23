using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
     private float minX, maxX, minY, maxY;

    void Start()
    {
        Camera cam = Camera.main;

        // 카메라 기준 화면 크기 계산
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // 화면 경계 계산 (카메라 위치가 (0,0)이라 단순히 절반값만 사용)
        minX = -halfWidth;
        maxX = halfWidth;
        minY = -halfHeight;
        maxY = halfHeight;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
