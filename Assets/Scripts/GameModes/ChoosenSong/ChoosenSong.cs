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
        for (int i = 0; i< GameState.amountPlayer; i++)
        {
            player[i] = root.Q<Label>("Player" + (i + 1).ToString());
            reroll[i] = root.Q<Button>("Reroll" + (i + 1).ToString());
            reroll[i].text = "Reroll Song " + GameState.teams[i].amountRerolls + "x";
            reroll[i].visible = true;
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
        }
        play.clicked += () =>
        {
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
            GameState.currentProfileIndex[i] = profileIndex;
            player[i].text = GameState.profiles[profileIndex].name;
        }
    }

    void RandomSong()
    {
        int index = Random.Range(0, GameState.songs.Count);
        GameState.currentSong = GameState.songs[index];
        song.text = GameState.currentSong.artist + ": " + GameState.currentSong.title;
    }
}
