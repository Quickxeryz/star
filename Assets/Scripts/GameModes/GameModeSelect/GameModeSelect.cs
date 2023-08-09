using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Classes;

public class GameModeSelect : MonoBehaviour
{
    void Start()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all UI Elements
        Button classic = root.Q<Button>("Classic");
        Button back = root.Q<Button>("Back");
        // set up functions
        classic.clicked += () =>
        {
            GameState.currentGameMode = GameMode.Classic;
            SceneManager.LoadScene("ChoosenSong");
        };
        back.clicked += () =>
        {
            SceneManager.LoadScene("GameModeConfig");
        };
    }
}
