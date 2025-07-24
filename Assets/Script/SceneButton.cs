using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneTransitionManager.Instance.LoadScene(sceneName);
        });
    }
}
