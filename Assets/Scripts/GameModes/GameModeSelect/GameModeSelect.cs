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
        Button together = root.Q<Button>("Together");
        Button back = root.Q<Button>("Back");
        // set up functions
        classic.clicked += () =>
        {
            if (GameState.songsLoaded)
            {
                GameState.currentPartyMode = PartyMode.Classic;
                GameState.currentGameMode = GameMode.Classic;
                GameState.partyModeSongs = new List<SongData>();
                // all songs 
                foreach (SongData song in GameState.songs) 
                {
                    GameState.partyModeSongs.Add(song);
                }
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    GameState.currentVoice[i] = 1;
                }
                SceneManager.LoadScene("ChoosenSong");
            }
        };
        together.clicked += () =>
        {
            bool ok = true;
            foreach (Team t in GameState.teams)
            {
                if (t.players.Count < 2)
                {
                    ok = false;
                }
            }
            if (ok) {
                if (GameState.songsLoaded)
                {
                    GameState.currentPartyMode = PartyMode.Together;
                    GameState.currentGameMode = GameMode.Together;
                    GameState.partyModeSongs = new List<SongData>();
                    // all songs
                    foreach (SongData song in GameState.songs)
                    {
                        GameState.partyModeSongs.Add(song);
                    }
                    // set voice to first
                    for (int i = 0; i < GameState.amountPlayer; i++)
                    {
                        GameState.currentVoice[i] = 1;
                    }
                    SceneManager.LoadScene("ChoosenSong");
                } 
            } else
            {
                together.text = "Every Team needs at least 2 people for this Gamemode";
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
