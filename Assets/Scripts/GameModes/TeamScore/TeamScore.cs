using Classes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TeamScore : MonoBehaviour
{
    void OnEnable()
    {
        // get ui elements
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        PlayerProfile[] teams = new PlayerProfile[GameState.teams.Count];
        Button continue_ = root.Q<Button>("Continue");
        // set button functionality
        continue_.clicked += () =>
        {
            SceneManager.LoadScene("GameModeConfig");
        };
        // print amount playing people with highest number
        Label currentPlace;
        GameState.teams.Sort();
        for (int i = 0; i < GameState.teams.Count; i++)
        {
            currentPlace = root.Q<Label>((i + 1).ToString());
            currentPlace.text = "Place " + (i + 1).ToString() + ": " + GameState.teams[i].name + " with " + GameState.teams[i].points.ToString() + " Points.";
        }
    }
}
