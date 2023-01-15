using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class SongEnd : MonoBehaviour
{
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button continue_ = root.Q<Button>("Continue");
        continue_.clicked += () =>
        {
            GameState.pointsPlayer1 = 0;
            GameState.pointsPlayer2 = 0;
            GameState.pointsPlayer3 = 0;
            GameState.pointsPlayer4 = 0;
            SceneManager.LoadScene("MainMenu");
        };
    }
}
