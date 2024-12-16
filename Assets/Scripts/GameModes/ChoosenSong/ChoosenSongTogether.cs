using Classes;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class ChoosenSongTogether : MonoBehaviour
{
    // ui attributes
    Label[] player;
    Button[] reroll;
    DropdownField[] switchPlayer;
    Button[] switchPlayer_Button;
    Label[] secondPlayer;
    Button[] secondReroll;
    DropdownField[] secondSwitchPlayer;
    Button[] secondSwitchPlayer_Button;
    // song attributes
    Label song;
    new AudioSource audio;
    VideoPlayer videoPlayer;

    void Start()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        song = root.Q<Label>("Song");
        // finding all UI Elements
        player = new Label[GameState.amountPlayer];
        reroll = new Button[GameState.amountPlayer];
        switchPlayer = new DropdownField[GameState.amountPlayer];
        switchPlayer_Button = new Button[GameState.amountPlayer];
        secondPlayer = new Label[GameState.amountPlayer];
        secondReroll = new Button[GameState.amountPlayer];
        secondSwitchPlayer = new DropdownField[GameState.amountPlayer];
        secondSwitchPlayer_Button = new Button[GameState.amountPlayer];
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            player[i] = root.Q<Label>("Player" + (i + 1).ToString() + "1");
            reroll[i] = root.Q<Button>("Reroll" + (i + 1).ToString() + "1");
            switchPlayer[i] = root.Q<DropdownField>("Switch" + (i + 1).ToString() + "1");
            switchPlayer_Button[i] = root.Q<Button>("SwitchButton" + (i + 1).ToString() + "1");
            reroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            reroll[i].visible = true;
            switchPlayer[i].visible = true;
            switchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
            switchPlayer_Button[i].visible = true;
            secondPlayer[i] = root.Q<Label>("Player" + (i + 1).ToString() + "2");
            secondReroll[i] = root.Q<Button>("Reroll" + (i + 1).ToString() + "2");
            secondSwitchPlayer[i] = root.Q<DropdownField>("Switch" + (i + 1).ToString() + "2");
            secondSwitchPlayer_Button[i] = root.Q<Button>("SwitchButton" + (i + 1).ToString() + "2");
            secondReroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            secondReroll[i].visible = true;
            secondSwitchPlayer[i].visible = true;
            secondSwitchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
            secondSwitchPlayer_Button[i].visible = true;
        }
        Button play = root.Q<Button>("Play");
        Button exit = root.Q<Button>("Exit");
        // set up functions
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            int iCopy = i;
            reroll[iCopy].clicked += () =>
            {
                Reroll(iCopy);
            };
            secondReroll[iCopy].clicked += () =>
            {
                Reroll(iCopy);
            };
            switchPlayer_Button[iCopy].clicked += () =>
            {
                SwitchPlayer(iCopy, true);
            };
            secondSwitchPlayer_Button[iCopy].clicked += () =>
            {
                SwitchPlayer(iCopy, false);
            };
        }
        play.clicked += () =>
        {
            for (int t = 0; t < GameState.teams.Count; t++)
            {
                for (int p = 0; p < GameState.teams[t].players.Count; p++)
                {
                    if (GameState.teams[t].players[p].name == player[t].text)
                    {
                        GameState.playersPlayed[t][p]--;
                        GameState.currentProfileIndex[t] = GameState.profiles.IndexOf(GameState.teams[t].players[p]);
                    }
                    if (GameState.teams[t].players[p].name == secondPlayer[t].text)
                    {
                        GameState.playersPlayed[t][p]--;
                        GameState.currentSecondProfileIndex[t] = GameState.profiles.IndexOf(GameState.teams[t].players[p]);
                    }
                }
            }
            SceneManager.LoadScene("GameScene");
        };
        exit.clicked += () =>
        {
            SceneManager.LoadScene("MainMenu");
        };
        // init preview
        GameObject music = GameObject.Find("Player");
        audio = music.AddComponent<AudioSource>();
        videoPlayer = music.AddComponent<VideoPlayer>();
        // random song
        RandomSong();
        // random singer
        int index;
        int profileIndex;
        List<int> teamMemberNotSungToOften;
        for (int i = 0; i < GameState.teams.Count; i++)
        {
            teamMemberNotSungToOften = new List<int>();
            // get next singer wich hasn't sung to often
            for (int x = 0; x<GameState.playersPlayed[i].Count; x++)
            {
                if (GameState.playersPlayed[i][x]>0)
                {
                    teamMemberNotSungToOften.Add(x);
                }
            }
            // add first singer
            index = Random.Range(0, teamMemberNotSungToOften.Count);
            index = teamMemberNotSungToOften[index];
            profileIndex = GameState.profiles.IndexOf(GameState.teams[i].players[index]);
            player[i].text = GameState.profiles[profileIndex].name;
            teamMemberNotSungToOften.RemoveAt(index);
            // add second singer
            index = Random.Range(0, teamMemberNotSungToOften.Count);
            index = teamMemberNotSungToOften[index];
            profileIndex = GameState.profiles.IndexOf(GameState.teams[i].players[index]);
            secondPlayer[i].text = GameState.profiles[profileIndex].name;
            foreach (PlayerProfile p in GameState.teams[i].players)
            {
                if (p.name != player[i].text && p.name != secondPlayer[i].text)
                {
                    switchPlayer[i].choices.Add(p.name);
                    secondSwitchPlayer[i].choices.Add(p.name);
                }
            }
        }
    }

    void Reroll(int i)
    {
        if (GameState.teams[i].amountRerolls > 0)
        {
            RandomSong();
            GameState.teams[i].amountRerolls--;
            reroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            secondReroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
        }
    }

    void SwitchPlayer(int i, bool isMainSinger)
    {
        if (GameState.teams[i].amountSwitches > 0 && ((isMainSinger && switchPlayer[i].index > -1) || (!isMainSinger && secondSwitchPlayer[i].index > -1)))
        {
            if (isMainSinger)
            {
                switchPlayer[i].choices.Add(player[i].text);
                secondSwitchPlayer[i].choices.Add(player[i].text);
                player[i].text = switchPlayer[i].value;
                secondSwitchPlayer[i].choices.RemoveAt(switchPlayer[i].choices.IndexOf(switchPlayer[i].value));
                switchPlayer[i].choices.RemoveAt(switchPlayer[i].choices.IndexOf(switchPlayer[i].value));
            }
            else
            {
                switchPlayer[i].choices.Add(secondPlayer[i].text);
                secondSwitchPlayer[i].choices.Add(secondPlayer[i].text);
                secondPlayer[i].text = secondSwitchPlayer[i].value;
                switchPlayer[i].choices.RemoveAt(secondSwitchPlayer[i].choices.IndexOf(secondSwitchPlayer[i].value));
                secondSwitchPlayer[i].choices.RemoveAt(secondSwitchPlayer[i].choices.IndexOf(secondSwitchPlayer[i].value));
            }
            switchPlayer[i].index = -1;
            secondSwitchPlayer[i].index = -1;
            GameState.teams[i].amountSwitches--;
            switchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
            secondSwitchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
        }
    }

    void RandomSong()
    {
        int index = Random.Range(0, GameState.gameModeSongs.Count);
        GameState.currentSong = GameState.gameModeSongs[index];
        song.text = GameState.currentSong.artist + ": " + GameState.currentSong.title;
        // using audio for sound
        if (GameState.currentSong.pathToMusic != "" && GameState.currentSong.pathToMusic != GameState.currentSong.pathToVideo)
        {
            videoPlayer.Pause();
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + GameState.currentSong.pathToMusic, AudioType.MPEG);
            req.SendWebRequest();
            while (!req.isDone)
            {
                Thread.Sleep(100);
            }
            audio.clip = DownloadHandlerAudioClip.GetContent(req);
            audio.Play();
        }
        else // using video for sound
        {
            audio.Pause();
            videoPlayer.url = GameState.currentSong.pathToVideo;
            videoPlayer.Play();
        }
    }
}
