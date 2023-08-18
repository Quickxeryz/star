using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using Classes;

public class SongEnd : MonoBehaviour
{
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        PlayerProfile[] player = new PlayerProfile[GameState.amountPlayer];
        Button continue_ = root.Q<Button>("Continue");
        // set button functionality
        continue_.clicked += () =>
        {
            switch (GameState.currentGameMode)
            {
                case GameMode.ChooseSong:
                    SceneManager.LoadScene("ChooseSong");
                    break;
                case GameMode.Classic:
                    // calculate team points
                    int x;
                    int y;
                    bool found;
                    for (int i = 0; i < GameState.amountPlayer; i++)
                    {
                        found = false;
                        x = 0;
                        while((!found) && x < GameState.teams.Count)
                        {
                            y = 0;
                            while ((!found) && y < GameState.teams[x].players.Count)
                            {
                                if (GameState.teams[x].players[y] == player[i])
                                {
                                    GameState.teams[x].points += GameState.amountPlayer - i - 1;
                                    found = true;
                                }
                                y++;
                            }
                            x++;
                        }
                    }
                    // choose next screen
                    GameState.roundsLeft--;
                    if (GameState.roundsLeft == 0)
                    {
                        SceneManager.LoadScene("TeamScore");
                        break;
                    } else
                    {
                        SceneManager.LoadScene("ChoosenSong");
                        break;
                    }
                case GameMode.None:
                    SceneManager.LoadScene("MainMenu");
                    break;
            }
        };
        // sort after points
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            player[i] = GameState.profiles[GameState.currentProfileIndex[i]];
        }
        Array.Sort(player);
        // print amount playing people with highest number
        Label currentPlace;
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            currentPlace = root.Q<Label>((i + 1).ToString());
            currentPlace.text = "Place " + (i + 1).ToString() + ": " + player[i].name + " with " + player[i].points.ToString() + " Points.";
        }
    }
}
