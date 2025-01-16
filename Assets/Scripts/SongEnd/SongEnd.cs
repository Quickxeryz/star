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
            switch (GameState.currentPartyMode)
            {
                case PartyMode.ChooseSong:
                    SceneManager.LoadScene("ChooseSong");
                    break;
                case PartyMode.Classic:
                case PartyMode.Together:
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
                case PartyMode.None:
                    SceneManager.LoadScene("MainMenu");
                    break;
            }
        };
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            player[i] = GameState.profiles[GameState.currentProfileIndex[i]];
        }
        PlayerProfile[] partner = new PlayerProfile[GameState.amountPlayer];
        if (GameState.currentGameMode == GameMode.Together) {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                partner[i] = GameState.profiles[GameState.currentSecondProfileIndex[i]];
            }
        }
        // sort after points
        if (GameState.currentGameMode == GameMode.Together)
        {
            Array.Sort(player, partner);
        }
        Array.Sort(player);
        // print amount playing people with highest number
        Label currentPlace;
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            currentPlace = root.Q<Label>((i + 1).ToString());
            if (GameState.currentGameMode == GameMode.Together)
            {
                currentPlace.text = "Place " + (i + 1).ToString() + ": " + player[i].name + " and " + partner[i].name + " with " + player[i].points.ToString() + " Points.";
            }
            else
            {
                currentPlace.text = "Place " + (i + 1).ToString() + ": " + player[i].name + " with " + player[i].points.ToString() + " Points.";
            }
        }
    }
}
