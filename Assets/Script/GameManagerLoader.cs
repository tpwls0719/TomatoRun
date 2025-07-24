using UnityEngine;

public class GameManagerLoader : MonoBehaviour
{
    void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("GameManager");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("GameManager 프리팹 로드 및 생성");
            }
            else
            {
                Debug.LogError("GameManager 프리팹을 Resources 폴더에서 찾을 수 없음");
            }
        }
        else
        {
            Debug.Log("GameManager 인스턴스가 이미 존재함");
        }
    }
}
