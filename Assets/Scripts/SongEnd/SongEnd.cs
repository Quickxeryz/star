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
            GameState.player[0].points = 0;
            GameState.player[1].points = 0;
            GameState.player[2].points = 0;
            GameState.player[3].points = 0;
            GameState.player[4].points = 0;
            GameState.player[5].points = 0;
            SceneManager.LoadScene("MainMenu");
        };
        // show placements
        // sort after points
        Player[] player = new Player[GameState.amountPlayer];
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            player[i] = GameState.player[i];
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
