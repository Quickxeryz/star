using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.IO;
using Classes;

public class ChooseSong : MonoBehaviour
{
    // UI
    VisualElement root;
    List<SongData> songs;
    List<SongData> currentSongs;
    DateTime lastTimePressed;

    void OnEnable()
    {
        // get UI elements
        root = GetComponent<UIDocument>().rootVisualElement;
        Button back = root.Q<Button>("Back");
        TemplateContainer playerAmount = root.Q<TemplateContainer>("PlayerAmount");
        GroupBox playerAmount_TextBox = playerAmount.Q<GroupBox>("TextBox");
        Button playerAmount_Left = playerAmount.Q<Button>("Left");
        Button playerAmount_Right = playerAmount.Q<Button>("Right");
        TemplateContainer[] playerX = new TemplateContainer[GameState.maxPlayer];
        TextField search = root.Q<TextField>("Search");
        for (int i = 0; i < playerX.Length; i++)
        {
            playerX[i] = root.Q<TemplateContainer>("Player" + (i + 1).ToString());
        }
        Label[] playerX_Label = new Label[GameState.maxPlayer];
        for (int i = 0; i < playerX_Label.Length; i++)
        {
            playerX_Label[i] = root.Q<Label>("Player" + (i + 1).ToString() + "Text");
        }
        GroupBox[] playerX_TextBox = new GroupBox[GameState.maxPlayer];
        for (int i = 0; i < playerX_TextBox.Length; i++)
        {
            playerX_TextBox[i] = playerX[i].Q<GroupBox>("TextBox");
        }
        Button[] playerX_Left = new Button[GameState.maxPlayer];
        for (int i = 0; i < playerX_Left.Length; i++)
        {
            playerX_Left[i] = playerX[i].Q<Button>("Left");
        }
        Button[] playerX_Right = new Button[GameState.maxPlayer];
        for (int i = 0; i < playerX_Right.Length; i++)
        {
            playerX_Right[i] = playerX[i].Q<Button>("Right");
        }
        // set functionality of buttons
        for (int i = 1; i <= 10; i++)
        {
            int iCopy = i;
            root.Q<Button>(iCopy.ToString()).clicked += () =>
            {
                GameState.currentGameMode = GameMode.ChooseSong;
                GameState.currentSong = currentSongs[GameState.lastSongIndex + iCopy - 1];
                bool found = false;
                int j = 0;
                string path = currentSongs[GameState.lastSongIndex + iCopy - 1].path;
                while (!found && j<songs.Count)
                {
                    if (songs[j].path == path)
                    {
                        GameState.lastSongIndex = j;
                        found = true;
                    }
                    j++;
                }
                SceneManager.LoadScene("GameScene");
            };
        }
        playerAmount_Left.clicked += () =>
        {
            if (GameState.amountPlayer > 1)
            {
                // update player number
                GameState.amountPlayer--;
                playerAmount_TextBox.text = GameState.amountPlayer.ToString();
                // make actual player unvisible
                playerX_Label[GameState.amountPlayer].visible = false;
                playerX[GameState.amountPlayer].visible = false;
            }
        };
        playerAmount_Right.clicked += () =>
        {
            if (GameState.amountPlayer < GameState.maxPlayer)
            {
                // make new player visible
                playerX_Label[GameState.amountPlayer].visible = true;
                playerX[GameState.amountPlayer].visible = true;
                // update player number
                GameState.amountPlayer++;
                playerAmount_TextBox.text = GameState.amountPlayer.ToString();
            }
        };
        for (int i = 0; i < GameState.maxPlayer; i++)
        {
            int iCopy = i;
            playerX_Left[iCopy].clicked += () =>
            {
                if (GameState.currentProfileIndex[iCopy] > 0)
                {
                    GameState.currentProfileIndex[iCopy]--;
                    playerX_TextBox[iCopy].text = GameState.profiles[GameState.currentProfileIndex[iCopy]].name;
                }
            };
            playerX_Right[iCopy].clicked += () =>
            {
                if (GameState.currentProfileIndex[iCopy] < GameState.profiles.Count - 1)
                {
                    GameState.currentProfileIndex[iCopy]++;
                    playerX_TextBox[iCopy].text = GameState.profiles[GameState.currentProfileIndex[iCopy]].name;
                }
            };
        }
        search.RegisterValueChangedCallback
        (
            e =>
            {
                string text = e.newValue.ToLower();
                if (text == "")
                {
                    GameState.lastSongIndex = 0;
                    currentSongs = new List<SongData>(songs); ;
                }
                else
                {
                    GameState.lastSongIndex = 0;
                    currentSongs.Clear();
                    foreach (SongData song in songs)
                    {
                        if (song.artist.ToLower().Contains(text))
                        {
                            currentSongs.Add(song);
                        }
                        else if (song.title.ToLower().Contains(text))
                        {
                            currentSongs.Add(song);
                        }
                    }
                }
                UpdateSongList();
            }
        );
        back.clicked += () =>
        {
            SceneManager.LoadScene("MainMenu");
        };
        // Loading song list
        songs = new();
        if (Directory.Exists(GameState.settings.absolutePathToSongs))
        {
            SearchDirectory(GameState.settings.absolutePathToSongs);
        }
        songs.Sort();
        currentSongs = new List<SongData>(songs); ;
        if (GameState.lastSongIndex > currentSongs.Count - 10)
        {
            if (currentSongs.Count >= 10)
            {
                GameState.lastSongIndex = currentSongs.Count - 10;
            } else
            {
                GameState.lastSongIndex = 0;
            }
        }
        UpdateSongList();
        // Load Amount Player 
        playerAmount_TextBox.text = GameState.amountPlayer.ToString();
        for (int i = 0; i < GameState.maxPlayer; i++)
        {
            if (GameState.currentProfileIndex[i] < GameState.profiles.Count)
            {
                playerX_TextBox[i].text = GameState.profiles[GameState.currentProfileIndex[i]].name;
            }
            else
            {
                playerX_TextBox[i].text = GameState.profiles[0].name;
                GameState.currentProfileIndex[i] = 0;
            }
        }
        // set visibility of player settings
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            playerX_Label[i].visible = true;
            playerX[i].visible = true;
        }
        for (int i = GameState.amountPlayer; i < GameState.maxPlayer; i++)
        {
            playerX_Label[i].visible = false;
            playerX[i].visible = false;
        }
    }

    void Update()
    {
        // set up steering for navigating through song list
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (GameState.lastSongIndex > 0)
            {
                GameState.lastSongIndex--;
                UpdateSongList();
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (GameState.lastSongIndex < currentSongs.Count - 10)
            {
                GameState.lastSongIndex++;
                UpdateSongList();
            }
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (DateTime.Now.Subtract(lastTimePressed).TotalMilliseconds > 100)
            {
                if (GameState.lastSongIndex > 0)
                {
                    GameState.lastSongIndex--;
                    UpdateSongList();
                    lastTimePressed = DateTime.Now;
                }
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (DateTime.Now.Subtract(lastTimePressed).TotalMilliseconds > 100)
            {
                if (GameState.lastSongIndex < currentSongs.Count - 10)
                {
                    GameState.lastSongIndex++;
                    UpdateSongList();
                    lastTimePressed = DateTime.Now;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (GameState.lastSongIndex > 10)
            {
                GameState.lastSongIndex-=10;
                UpdateSongList();
            } else
            {
                GameState.lastSongIndex = 0;
                UpdateSongList();
            }
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (currentSongs.Count < 10)
            {
                return;
            }
            if (GameState.lastSongIndex < currentSongs.Count - 20)
            {
                GameState.lastSongIndex += 10;
                UpdateSongList();
            }
            else
            {
                GameState.lastSongIndex = currentSongs.Count - 10;
                UpdateSongList();
            }
        }
    }

    // Go through all files and folders
    void SearchDirectory(string path)
    {
        // Get all files
        string[] files = Directory.GetFiles(path);
        string[] text;
        bool isSong;
        SongData currentSong;
        string songTitle = "";
        string songArtist = "";
        string songMusicPath = "";
        float bpm = 0;
        float gap = 0;
        string songVideoPath;
        foreach (string file in files)
            // check for song file
            if (file.Contains(".txt"))
            {
                text = File.ReadAllLines(file);
                isSong = false;
                foreach (string line in text)
                {
                    if (line.StartsWith("#TITLE"))
                    {
                        isSong = true;
                        break;
                    }
                }
                if (isSong)
                {
                    songVideoPath = "";
                    // when file is song file extract data
                    foreach (string line in text)
                    {
                        if (line.StartsWith("#TITLE:"))
                        {
                            songTitle = line[7..];
                        }
                        else if (line.StartsWith("#ARTIST:"))
                        {
                            songArtist = line[8..];
                        }
                        else if (line.StartsWith("#MP3:"))
                        {
                            songMusicPath = path + "/" + line[5..];
                        }
                        else if (line.StartsWith("#BPM:"))
                        {
                            bpm = float.Parse(line[5..].Replace(".", ","));
                        }
                        else if (line.StartsWith("#GAP:"))
                        {
                            gap = float.Parse(line[5..].Replace(".", ",")) * 0.001f;
                        }
                        else if (line.StartsWith("#VIDEO:"))
                        {
                            songVideoPath = path + "/" + line[7..];
                        }
                    }
                    currentSong = new SongData(file, songTitle, songArtist, songMusicPath, bpm, gap)
                    {
                        pathToVideo = songVideoPath
                    };
                    songs.Add(currentSong);
                }
            }
        // Get all directorys
        files = Directory.GetDirectories(path);
        foreach (string dir in files)
        {
            SearchDirectory(dir);
        }
    }

    void UpdateSongList()
    {
        int itemCounter = 1;
        // clear song data elements
        for (int i = 1; i <= 10; i++)
        {
            root.Q<Button>(i.ToString()).visible = false;
        }
        // set song data to items
        Button songButton;
        for (int i = GameState.lastSongIndex; itemCounter <= 10 && i < currentSongs.Count; i++)
        {
            songButton = root.Q<Button>(itemCounter.ToString());
            songButton.text = (currentSongs[i]).artist + ": "+(currentSongs[i]).title;
            songButton.visible = true;
            itemCounter++;
        }
    }
}