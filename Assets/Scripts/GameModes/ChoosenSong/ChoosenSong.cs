using Classes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosenSong : MonoBehaviour
{
    Label song;

    void Start()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        song = root.Q<Label>("Song");
        // finding all UI Elements
        Label[] player = new Label[GameState.amountPlayer];
        Button[] reroll = new Button[GameState.amountPlayer];
        DropdownField[] switchPlayer = new DropdownField[GameState.amountPlayer];
        Button[] switchPlayer_Button = new Button[GameState.amountPlayer];
        for (int i = 0; i< GameState.amountPlayer; i++)
        {
            player[i] = root.Q<Label>("Player" + (i + 1).ToString());
            reroll[i] = root.Q<Button>("Reroll" + (i + 1).ToString());
            switchPlayer[i] = root.Q<DropdownField>("Switch" + (i + 1).ToString());
            switchPlayer_Button[i] = root.Q<Button>("SwitchButton" + (i + 1).ToString());
            reroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            reroll[i].visible = true;
            switchPlayer[i].visible = true;
            switchPlayer_Button[i].text = "Switch " + GameState.teams[i].amountSwitches + "x";
            switchPlayer_Button[i].visible = true;
        }
        Button play = root.Q<Button>("Play");
        Button exit = root.Q<Button>("Exit");
        // set up functions
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            int iCopy = i;
            reroll[iCopy].clicked += () =>
            {
                if (GameState.teams[iCopy].amountRerolls > 0)
                {
                    RandomSong();
                    GameState.teams[iCopy].amountRerolls--;
                    reroll[iCopy].text = "Reroll Song " + GameState.teams[iCopy].amountRerolls + "x";
                }
            };
            switchPlayer_Button[iCopy].clicked += () =>
            {
                if (switchPlayer[iCopy].index > -1 && GameState.teams[iCopy].amountSwitches > 0)
                {
                    switchPlayer[iCopy].choices.Add(player[iCopy].text);
                    player[iCopy].text = switchPlayer[iCopy].value;
                    switchPlayer[iCopy].choices.RemoveAt(switchPlayer[iCopy].choices.IndexOf(switchPlayer[iCopy].value));
                    switchPlayer[iCopy].index = -1;
                    GameState.teams[iCopy].amountSwitches--;
                    switchPlayer_Button[iCopy].text = "Switch " + GameState.teams[iCopy].amountSwitches + "x";
                }
            };
        }
        play.clicked += () =>
        {
            int index;
            bool found;
            for (int i = 0; i<GameState.teams.Count; i++)
            {
                index = 0;
                found = false;
                while(!found && index < GameState.teams[i].players.Count)
                {
                    if (GameState.teams[i].players[index].name == player[i].text)
                    {
                        found = true;
                    } else
                    {
                        index++;
                    }
                }
                GameState.currentProfileIndex[i] = GameState.profiles.IndexOf(GameState.teams[i].players[index]);
            }
            SceneManager.LoadScene("GameScene");
        };
        exit.clicked += () =>
        {
            SceneManager.LoadScene("MainMenu");
        };
        // get random song
        RandomSong();
        // get random singer per team
        int index;
        int profileIndex;
        for (int i = 0; i<GameState.teams.Count; i++)
        {
            index = Random.Range(0, GameState.teams[i].players.Count);
            profileIndex = GameState.profiles.IndexOf(GameState.teams[i].players[index]);
            player[i].text = GameState.profiles[profileIndex].name;
            foreach (PlayerProfile p in GameState.teams[i].players)
            {
                if (p.name != player[i].text)
                {
                    switchPlayer[i].choices.Add(p.name);
                }
            }
        }
    }

    void RandomSong()
    {
        int index = Random.Range(0, GameState.gameModeSongs.Count);
        GameState.currentSong = GameState.gameModeSongs[index];
        song.text = GameState.currentSong.artist + ": " + GameState.currentSong.title;
    }
}
