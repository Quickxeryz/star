using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosenSong : MonoBehaviour
{
    void Start()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all UI Elements
        Label song = root.Q<Label>("Song");
        Label[] player = new Label[GameState.amountPlayer];
        for (int i = 0; i< GameState.amountPlayer; i++)
        {
            player[i] = root.Q<Label>("Player"+(i+1).ToString());
        }
        Button play = root.Q<Button>("Play");
        Button exit = root.Q<Button>("Exit");
        // set up functions
        play.clicked += () =>
        {
            SceneManager.LoadScene("GameScene");
        };
        exit.clicked += () =>
        {
            SceneManager.LoadScene("MainMenu");
        };
        // get random song
        int index = Random.Range(0,GameState.songs.Count);
        GameState.currentSong = GameState.songs[index];
        song.text = GameState.currentSong.artist + ": " + GameState.currentSong.title;
        // get random singer per team
        int profileIndex;
        for (int i = 0; i<GameState.teams.Count; i++)
        {
            index = Random.Range(0, GameState.teams[i].Count);
            profileIndex = GameState.profiles.IndexOf(GameState.teams[i][index]);
            GameState.currentProfileIndex[i] = profileIndex;
            player[i].text = GameState.profiles[profileIndex].name;
        }
    }
}
