using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using Classes;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.Video;

public class ChooseSong : MonoBehaviour
{
    // ui
    VisualElement root;
    List<SongData> currentSongs;
    DateTime lastTimePressed;
    // song preview
    new AudioSource audio;
    VideoPlayer videoPlayer;

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
        TemplateContainer gameMode = root.Q<TemplateContainer>("GameMode");
        GroupBox gameMode_TextBox = gameMode.Q<GroupBox>("TextBox");
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
                GameState.currentSong = currentSongs[GameState.lastSongIndex + iCopy - 1];
                bool found = false;
                int j = 0;
                string path = currentSongs[GameState.lastSongIndex + iCopy - 1].path;
                while (!found && j<GameState.songs.Count)
                {
                    if (GameState.songs[j].path == path)
                    {
                        GameState.lastSongIndex = j;
                        found = true;
                    }
                    j++;
                }
                for (j = 0; j < GameState.currentVoice.Length; j++)
                {
                    GameState.currentVoice[j] = 1;
                }
                switch (GameState.currentGameMode) { 
                    case GameMode.Classic:
                        SceneManager.LoadScene("GameScene");
                        break;
                    case GameMode.Duet:
                        SceneManager.LoadScene("ChooseVoice");
                        break;
                    case GameMode.Together:
                        SceneManager.LoadScene("ChoosePartner");
                        break;
                }
            };
            root.Q<Button>(iCopy.ToString()).RegisterCallback<MouseEnterEvent, int>(PlaySong, iCopy);
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
                    currentSongs = new List<SongData>(GameState.partyModeSongs); ;
                }
                else
                {
                    GameState.lastSongIndex = 0;
                    currentSongs.Clear();
                    foreach (SongData song in GameState.partyModeSongs)
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
        gameMode.Q<Button>("Left").clicked += () => {
            int gameModeNumber = (int)GameState.currentGameMode;
            if (gameModeNumber > 0)
            {
                gameModeNumber -= 1;
                GameState.currentGameMode = (GameMode)gameModeNumber;
                gameMode_TextBox.text = GameModeFunctions.GameModeToString(GameState.currentGameMode);
                SetUpSongList();
            }            
        };
        gameMode.Q<Button>("Right").clicked += () => {
            int gameModeNumber = (int)GameState.currentGameMode;
            if (gameModeNumber < 2)
            {
                gameModeNumber += 1;
                GameState.currentGameMode = (GameMode)gameModeNumber;
                gameMode_TextBox.text = GameModeFunctions.GameModeToString(GameState.currentGameMode);
                SetUpSongList();
            }
        };
        back.clicked += () =>
        {
            SceneManager.LoadScene("MainMenu");
        };
        if(GameState.currentGameMode == GameMode.None)
        {
            GameState.currentGameMode = GameMode.Classic;
        }
        GameState.currentPartyMode = PartyMode.ChooseSong;
        SetUpSongList();
        gameMode_TextBox.text = GameModeFunctions.GameModeToString(GameState.currentGameMode);
        // showing song list
        currentSongs = new List<SongData>(GameState.songs); ;
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
        // init songpreview
        GameObject player = GameObject.Find("Player");
        audio = player.AddComponent<AudioSource>();
        videoPlayer = player.AddComponent<VideoPlayer>();
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

    void SetUpSongList()
    {
        GameState.lastSongIndex = 0;
        currentSongs = new List<SongData>();
        switch (GameState.currentGameMode)
        {
            case GameMode.Classic:
            case GameMode.Together:
                // all songs
                foreach (SongData song in GameState.songs)
                {
                    currentSongs.Add(song);
                }
                break;
            case GameMode.Duet:
                // exclude only main singer songs
                foreach (SongData song in GameState.songs)
                {
                    if (song.amountVoices > 1)
                    {
                        currentSongs.Add(song);
                    }
                }
                break;
        }
        UpdateSongList();
    }

    void PlaySong(MouseEnterEvent evt, int id)
    {
        // using audio for sound
        if (currentSongs[GameState.lastSongIndex + id - 1].pathToMusic != "" && currentSongs[GameState.lastSongIndex + id - 1].pathToMusic != currentSongs[GameState.lastSongIndex + id - 1].pathToVideo)
        {
            videoPlayer.Pause();
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + currentSongs[GameState.lastSongIndex + id - 1].pathToMusic, AudioType.MPEG);
            req.SendWebRequest();
            while (!req.isDone)
            {
                Thread.Sleep(100);
            }
            audio.clip = DownloadHandlerAudioClip.GetContent(req);
            audio.Play();
        } else // using video for sound
        {
            audio.Pause();
            videoPlayer.url = currentSongs[GameState.lastSongIndex + id - 1].pathToVideo;
            videoPlayer.Play();
        }
    }
}
