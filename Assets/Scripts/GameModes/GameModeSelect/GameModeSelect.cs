using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Classes;
using System.Collections.Generic;

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
                GameState.gameModeSongs = new List<SongData>();
                foreach (SongData song in GameState.songs) 
                {
                    if (song.amountVoices == 1)
                    {
                        GameState.gameModeSongs.Add(song);
                    }
                }
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    GameState.currentVoice[i] = 1;
                }
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
