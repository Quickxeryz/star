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
            SceneManager.LoadScene("MainMenu");
        };
        // show placements
        // sort after points
        PlayerProfile[] player = new PlayerProfile[GameState.amountPlayer];
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            player[i] = GameState.currentPlayer[i];
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
