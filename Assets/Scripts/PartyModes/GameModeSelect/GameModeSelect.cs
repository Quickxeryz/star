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
        Button duet = root.Q<Button>("Duet");
        Button miau = root.Q<Button>("Meow");
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
                together.text = "Together: Every team needs at least 2 people for this gamemode!";
            }
        };
        duet.clicked += () =>
        {
            bool ok = true;
            // check for max amount singer
            if (GameState.amountPlayer * 2 > GameState.maxPlayer)
            {
                ok = false;
                duet.text = "Duet: The amount of teams must be <= 3!";
            }
            if (ok) 
            {
                // check for amount team member
                foreach (Team t in GameState.teams)
                {
                    if (t.players.Count < 2)
                    {
                        ok = false;
                        duet.text = "Duet: Every team needs at least 2 people for this gamemode!";
                    }
                }
            }
            if (ok)
            {
                GameState.amountPlayer = GameState.amountPlayer * 2;
                if (GameState.songsLoaded)
                {
                    GameState.currentPartyMode = PartyMode.Duet;
                    GameState.currentGameMode = GameMode.Duet;
                    GameState.partyModeSongs = new List<SongData>();
                    // exclude only main singer songs
                    foreach (SongData song in GameState.songs)
                    {
                        if (song.amountVoices > 1)
                        {
                            GameState.partyModeSongs.Add(song);
                        }
                    }
                    // set voice to first
                    for (int i = 0; i < GameState.amountPlayer; i += 2)
                    {
                        GameState.currentVoice[i] = 1;
                    }
                    // set voice to second
                    for (int i = 1; i < GameState.amountPlayer; i += 2)
                    {
                        GameState.currentVoice[i] = 2;
                    }
                    SceneManager.LoadScene("ChoosenSong");
                }
            }
        };
        miau.clicked += () =>
        {
            if (GameState.songsLoaded)
            {
                GameState.currentPartyMode = PartyMode.Meow;
                GameState.currentGameMode = GameMode.Meow;
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
