using Classes;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class ChoosenSong : MonoBehaviour
{
    Label[] player = new Label[GameState.amountPlayer];
    Button[] reroll = new Button[GameState.amountPlayer];
    DropdownField[] switchPlayer = new DropdownField[GameState.amountPlayer];
    Button[] switchPlayer_Button = new Button[GameState.amountPlayer];
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
        VisualElement root;
        if (GameState.currentPartyMode == PartyMode.Classic)
        {
            Destroy(GameObject.Find("Together"));
            root = GameObject.Find("Classic").GetComponent<UIDocument>().rootVisualElement;
        }
        else
        {
            Destroy(GameObject.Find("Classic"));
            root = GameObject.Find("Together").GetComponent<UIDocument>().rootVisualElement;
        }
        // UI
        song = root.Q<Label>("Song");
        root.Q<Label>("Rounds").text = GameState.roundsLeft.ToString();
        if (GameState.currentPartyMode == PartyMode.Together)
        {
            // finding all UI Elements
            secondPlayer = new Label[GameState.amountPlayer];
            secondReroll = new Button[GameState.amountPlayer];
            secondSwitchPlayer = new DropdownField[GameState.amountPlayer];
            secondSwitchPlayer_Button = new Button[GameState.amountPlayer];
        }
        // finding all UI Elements
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
        }
        if (GameState.currentPartyMode == PartyMode.Together)
        {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
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
            switchPlayer_Button[iCopy].clicked += () =>
            {
                SwitchPlayer(iCopy, true);
            };
        }
        if (GameState.currentPartyMode == PartyMode.Together)
        {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                int iCopy = i;
                secondReroll[iCopy].clicked += () =>
                {
                    Reroll(iCopy);
                };
                secondSwitchPlayer_Button[iCopy].clicked += () =>
                {
                    SwitchPlayer(iCopy, false);
                };
            }
        }
        play.clicked += () =>
        {
            int index;
            bool found;
            for (int i = 0; i < GameState.teams.Count; i++)
            {
                index = 0;
                found = false;
                while (!found && index < GameState.teams[i].players.Count)
                {
                    if (GameState.teams[i].players[index].name == player[i].text)
                    {
                        found = true;
                    }
                    else
                    {
                        index++;
                    }
                }
                GameState.currentProfileIndex[i] = GameState.profiles.IndexOf(GameState.teams[i].players[index]);
            }
            if (GameState.currentPartyMode == PartyMode.Together)
            {
                for (int i = 0; i < GameState.teams.Count; i++)
                {
                    index = 0;
                    found = false;
                    while (!found && index < GameState.teams[i].players.Count)
                    {
                        if (GameState.teams[i].players[index].name == secondPlayer[i].text)
                        {
                            found = true;
                        }
                        else
                        {
                            index++;
                        }
                    }
                    GameState.currentSecondProfileIndex[i] = GameState.profiles.IndexOf(GameState.teams[i].players[index]);
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
        int index = 0;
        for (int i = 0; i < GameState.teams.Count; i++)
        {
            // add singer
            if (GameState.teams[i].playersNotSung.Count == 0)
            {
                foreach (PlayerProfile p in GameState.teams[i].players)
                {
                    GameState.teams[i].playersNotSung.Add(p);
                }
            }
            index = Random.Range(0, GameState.teams[i].playersNotSung.Count);
            player[i].text = GameState.teams[i].playersNotSung[index].name;
            GameState.teams[i].playersNotSung.RemoveAt(index);
        }
        if (GameState.currentPartyMode == PartyMode.Classic)
        {
            for (int i = 0; i < GameState.teams.Count; i++)
            {
                foreach (PlayerProfile p in GameState.teams[i].players)
                {
                    if (p.name != player[i].text)
                    {
                        switchPlayer[i].choices.Add(p.name);
                    }
                }
            }
        }
        else
        {
            bool differentPlayerNotFound = true;
            for (int i = 0; i < GameState.teams.Count; i++)
            {
                // add second singer
                if (GameState.teams[i].playersNotSung.Count == 0)
                {
                    foreach (PlayerProfile p in GameState.teams[i].players)
                    {
                        GameState.teams[i].playersNotSung.Add(p);
                    }
                }
                while (differentPlayerNotFound)
                {
                    index = Random.Range(0, GameState.teams[i].playersNotSung.Count);
                    if (player[i].text != GameState.teams[i].playersNotSung[index].name) {
                        differentPlayerNotFound = false;
                    }
                }
                secondPlayer[i].text = GameState.teams[i].playersNotSung[index].name;
                GameState.teams[i].playersNotSung.RemoveAt(index); 
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
    }

    void Reroll(int i)
    {
        if (GameState.teams[i].amountRerolls > 0)
        {
            RandomSong();
            GameState.teams[i].amountRerolls--;
            reroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            if (GameState.currentPartyMode == PartyMode.Together)
            {
                secondReroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            }
        }
    }

    void SwitchPlayer(int i, bool isMainSinger)
    {
        if (GameState.teams[i].amountSwitches > 0 && ((isMainSinger && switchPlayer[i].index > -1) || (!isMainSinger && secondSwitchPlayer[i].index > -1)))
        {
            int index = 0;
            if (isMainSinger)
            {
                while (index < GameState.teams[i].players.Count)
                {
                    if (player[i].text == GameState.teams[i].players[index].name)
                    {
                        GameState.teams[i].playersNotSung.Add(GameState.teams[i].players[index]);
                        index = GameState.teams[i].players.Count;
                    }
                    index++;
                }
                switchPlayer[i].choices.Add(player[i].text);
                if (GameState.currentPartyMode == PartyMode.Together)
                {
                    secondSwitchPlayer[i].choices.Add(player[i].text);
                }
                player[i].text = switchPlayer[i].value;
                index = 0;
                while (index < GameState.teams[i].playersNotSung.Count)
                {
                    if (player[i].text == GameState.teams[i].playersNotSung[index].name)
                    {
                        GameState.teams[i].playersNotSung.RemoveAt(index);
                        index = GameState.teams[i].players.Count;
                    }
                    index++;
                }
                if (GameState.currentPartyMode == PartyMode.Together)
                {
                    secondSwitchPlayer[i].choices.RemoveAt(switchPlayer[i].choices.IndexOf(switchPlayer[i].value));
                }
                switchPlayer[i].choices.RemoveAt(switchPlayer[i].choices.IndexOf(switchPlayer[i].value));
            }
            else
            {
                while (index < GameState.teams[i].players.Count)
                {
                    if (secondPlayer[i].text == GameState.teams[i].players[index].name)
                    {
                        GameState.teams[i].playersNotSung.Add(GameState.teams[i].players[index]);
                        index = GameState.teams[i].players.Count;
                    }
                    index++;
                }
                switchPlayer[i].choices.Add(secondPlayer[i].text);
                secondSwitchPlayer[i].choices.Add(secondPlayer[i].text);
                secondPlayer[i].text = secondSwitchPlayer[i].value;
                index = 0;
                while (index < GameState.teams[i].playersNotSung.Count)
                {
                    if (secondPlayer[i].text == GameState.teams[i].playersNotSung[index].name)
                    {
                        GameState.teams[i].playersNotSung.RemoveAt(index);
                        index = GameState.teams[i].players.Count;
                    }
                    index++;
                }
                switchPlayer[i].choices.RemoveAt(secondSwitchPlayer[i].choices.IndexOf(secondSwitchPlayer[i].value));
                secondSwitchPlayer[i].choices.RemoveAt(secondSwitchPlayer[i].choices.IndexOf(secondSwitchPlayer[i].value));
            }
            switchPlayer[i].index = -1;
            if (GameState.currentPartyMode == PartyMode.Together)
            {
                secondSwitchPlayer[i].index = -1;
            }
            GameState.teams[i].amountSwitches--;
            switchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
            if (GameState.currentPartyMode == PartyMode.Together)
            {
                secondSwitchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
            }
        }
    }

    void RandomSong()
    {
        int index = Random.Range(0, GameState.partyModeSongs.Count);
        GameState.currentSong = GameState.partyModeSongs[index];
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
