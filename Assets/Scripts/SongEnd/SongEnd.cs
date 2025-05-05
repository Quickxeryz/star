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
        PlayerProfile[] partner;
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
                case PartyMode.Duet:
                    // calculate team points
                    int x;
                    int y;
                    bool found;
                    int last_points = 0;
                    for (int i = 0; i < player.Length; i++)
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
                                    // handle draw
                                    if (i > 0 && player[i - 1].points == player[i].points)
                                    {
                                        GameState.teams[x].points += last_points;
                                    }
                                    else
                                    {
                                        GameState.teams[x].points += player.Length - i - 1;
                                        last_points = player.Length - i - 1;
                                    }
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
        if (GameState.currentPartyMode == PartyMode.Duet)
        {
            player = new PlayerProfile[GameState.amountPlayer / 2];
            for (int i = 0; i < GameState.amountPlayer; i += 2)
            {
                player[i / 2] = GameState.profiles[GameState.currentProfileIndex[i]];
            }
            partner = new PlayerProfile[GameState.amountPlayer / 2];
            for (int i = 1; i < GameState.amountPlayer; i += 2)
            {
                partner[i / 2] = GameState.profiles[GameState.currentProfileIndex[i]];
            }
            // add points together
            for (int i = 0; i < player.Length; i++)
            {
                player[i].points += partner[i].points;
            }
        } else
        {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                player[i] = GameState.profiles[GameState.currentProfileIndex[i]];
            }
            partner = new PlayerProfile[GameState.amountPlayer];
            if (GameState.currentGameMode == GameMode.Together)
            {
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    partner[i] = GameState.profiles[GameState.currentSecondProfileIndex[i]];
                }
            }
        }
        // sort after points
        if (GameState.currentGameMode == GameMode.Together
        || GameState.currentGameMode == GameMode.Duet)
        {
            Array.Sort(player, partner);
        }
        Array.Sort(player);
        // print amount playing people with highest number
        Label currentPlace;        
        if (GameState.currentPartyMode == PartyMode.Duet)
        {
            for (int i = 0; i < player.Length; i++)
            {
                currentPlace = root.Q<Label>((i + 1).ToString());
                currentPlace.text = "Place " + (i + 1).ToString() + ": " + player[i].name + " and " + partner[i].name + " with " + player[i].points.ToString() + " Points.";
            }            
        } else
        {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                currentPlace = root.Q<Label>((i + 1).ToString());
                switch (GameState.currentPartyMode)
                {
                    case PartyMode.Classic:
                        currentPlace.text = "Place " + (i + 1).ToString() + ": " + player[i].name + " with " + player[i].points.ToString() + " Points.";
                        break;
                    case PartyMode.Together:
                        currentPlace.text = "Place " + (i + 1).ToString() + ": " + player[i].name + " and " + partner[i].name + " with " + player[i].points.ToString() + " Points.";
                        break;
                }
            }
        }
    }
}
