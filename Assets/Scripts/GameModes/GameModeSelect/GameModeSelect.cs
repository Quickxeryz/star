using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Classes;

public class GameModeSelect : MonoBehaviour
{
    Label songsLoaded;

    void Start()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all UI Elements
        songsLoaded = root.Q<Label>("SongsLoaded");
        Button classic = root.Q<Button>("Classic");
        Button back = root.Q<Button>("Back");
        // set up functions
        classic.clicked += () =>
        {
            if (GameState.songsLoaded)
            {
                GameState.currentGameMode = GameMode.Classic;
                SceneManager.LoadScene("ChoosenSong");
            }
        };
        back.clicked += () =>
        {
            SceneManager.LoadScene("GameModeConfig");
        };
        // init
        if (GameState.songsLoaded)
        {
            songsLoaded.text = "Songs loaded :)";
        }
        else
        {
            songsLoaded.text = "Loading songs ...";
        }
    }

    private void Update()
    {
        if (GameState.songsLoaded)
        {
            songsLoaded.text = "Songs loaded :)";
        }
    }
}
