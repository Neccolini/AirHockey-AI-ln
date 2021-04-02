using UnityEngine;

/*
ゲームの終了の仕方がわからなかったのでググって出てきたコードをコピペしてクラス化
*/
public class QuitClass : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
    }
    public void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Quit();
    }
}