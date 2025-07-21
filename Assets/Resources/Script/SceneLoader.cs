using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!

public class SceneLoader : MonoBehaviour
{
    // 버튼의 OnClick() 이벤트에서 호출할 함수
    // 반드시 public으로 선언해야 합니다.
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}