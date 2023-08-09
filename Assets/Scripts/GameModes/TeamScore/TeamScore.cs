using Classes;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TeamScore : MonoBehaviour
{
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        PlayerProfile[] teams = new PlayerProfile[GameState.teams.Count];
        Button continue_ = root.Q<Button>("Continue");
        // set button functionality
        continue_.clicked += () =>
        {
            SceneManager.LoadScene("GameModeConfig");
        };
        // sort after points
        int[] teamPoints = new int[GameState.teams.Count];
        for (int i = 0; i < GameState.teams.Count; i++)
        {
            teamPoints[i] = GameState.teamPoints[i];
        }
        Array.Sort(teamPoints);
        // print amount playing people with highest number
        Label currentPlace;
        int lastPointsEqual = 0;
        int currentLastPointsEqual;
        int j;
        bool found;
        for (int i = GameState.teams.Count-1; i>=0; i--)
        {
            if (i<GameState.teams.Count-1 && teamPoints[i] == teamPoints[i+1])
            {
                lastPointsEqual++;
            } else
            {
                lastPointsEqual = 0;
            }
            currentLastPointsEqual = lastPointsEqual;
            j = 0;
            found = false;
            while (!found && j < teamPoints.Length)
            {
                if (teamPoints[i] == GameState.teamPoints[j])
                {
                    if (currentLastPointsEqual == 0) 
                    {
                        found = true;
                    }
                    else
                    {
                        currentLastPointsEqual--;
                        j++;
                    }
                } else
                {
                    j++;
                }          
            }
            currentPlace = root.Q<Label>((teamPoints.Length-i).ToString());
            currentPlace.text = "Place " + (i + 1).ToString() + ": Team " + (j+1).ToString() + " with " + teamPoints[i] + " Points.";
        }
    }
}
