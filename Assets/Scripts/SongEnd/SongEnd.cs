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
        // set button functionality
        Button continue_ = root.Q<Button>("Continue");
        continue_.clicked += () =>
        {
            GameState.player1.points = 0;
            GameState.player2.points = 0;
            GameState.player3.points = 0;
            GameState.player4.points = 0;
            SceneManager.LoadScene("MainMenu");
        };
        // show placements
        // sort after points
        Player[] player = { GameState.player1, GameState.player2, GameState.player3, GameState.player4 };
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
