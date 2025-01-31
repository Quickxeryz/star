using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Classes;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameModeConfig : MonoBehaviour
{
    void Start()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all UI Elements        
        TemplateContainer amountRounds = root.Q<TemplateContainer>("AmountRounds");
        GroupBox amountRounds_TextBox = amountRounds.Q<GroupBox>("TextBox");
        Button amountRounds_Left = amountRounds.Q<Button>("Left");
        Button amountRounds_Right = amountRounds.Q<Button>("Right");
        TemplateContainer amountRerolls = root.Q<TemplateContainer>("AmountRerolls");
        GroupBox amountRerolls_TextBox = amountRerolls.Q<GroupBox>("TextBox");
        Button amountRerolls_Left = amountRerolls.Q<Button>("Left");
        Button amountRerolls_Right = amountRerolls.Q<Button>("Right");
        TemplateContainer amountSwitches = root.Q<TemplateContainer>("AmountSwitches");
        GroupBox amountSwitches_TextBox = amountSwitches.Q<GroupBox>("TextBox");
        Button amountSwitches_Left = amountSwitches.Q<Button>("Left");
        Button amountSwitches_Right = amountSwitches.Q<Button>("Right");
        TemplateContainer amountTeams = root.Q<TemplateContainer>("AmountTeams");
        GroupBox amountTeams_TextBox = amountTeams.Q<GroupBox>("TextBox");
        Button amountTeams_Left = amountTeams.Q<Button>("Left");
        Button amountTeams_Right = amountTeams.Q<Button>("Right");
        Button randomizeTeamMember = root.Q<Button>("Randomize");
        DropdownField player = root.Q<DropdownField>("Player");
        Button[] addTeam = new Button[GameState.maxPlayer];
        for (int i = 0; i < addTeam.Length; i++)
        {
            addTeam[i] = root.Q<Button>("AddTeam" + (i + 1).ToString());
        }
        DropdownField[] team = new DropdownField[GameState.maxPlayer];
        for (int i = 0; i < team.Length; i++)
        {
            team[i] = root.Q<DropdownField>("Team" + (i + 1).ToString());
        }
        Button[] removePlayerTeam = new Button[GameState.maxPlayer];
        for (int i = 0; i < removePlayerTeam.Length; i++)
        {
            removePlayerTeam[i] = root.Q<Button>("RemovePlayerTeam" + (i + 1).ToString());
        }
        Label warning = root.Q<Label>("Warning");
        Button continuee = root.Q<Button>("Continue");
        Button back = root.Q<Button>("Back");
        // set functionality of all buttons
        amountRounds_Left.clicked += () =>
        {
            if (Int32.Parse(amountRounds_TextBox.text) > 1)
            {
                amountRounds_TextBox.text = (Int32.Parse(amountRounds_TextBox.text) - 1).ToString();
            }
        };
        amountRounds_Right.clicked += () =>
        {
            amountRounds_TextBox.text = (Int32.Parse(amountRounds_TextBox.text) + 1).ToString();
        };
        amountRerolls_Left.clicked += () =>
        {
            if (Int32.Parse(amountRerolls_TextBox.text) > 0)
            {
                amountRerolls_TextBox.text = (Int32.Parse(amountRerolls_TextBox.text) - 1).ToString();
            }
        };
        amountRerolls_Right.clicked += () =>
        {
            amountRerolls_TextBox.text = (Int32.Parse(amountRerolls_TextBox.text) + 1).ToString();
        };
        amountSwitches_Left.clicked += () =>
        {
            if (Int32.Parse(amountSwitches_TextBox.text) > 0)
            {
                amountSwitches_TextBox.text = (Int32.Parse(amountSwitches_TextBox.text) - 1).ToString();
            }
        };
        amountSwitches_Right.clicked += () =>
        {
            amountSwitches_TextBox.text = (Int32.Parse(amountSwitches_TextBox.text) + 1).ToString();
        };
        amountTeams_Left.clicked += () =>
        {
            // set visibility
            int teams = Int32.Parse(amountTeams_TextBox.text);
            if (teams > 2)
            {
                amountTeams_TextBox.text = (teams - 1).ToString();
                addTeam[teams - 1].visible = false;
                team[teams - 1].visible = false;
                removePlayerTeam[teams - 1].visible = false;
            }
            // delete player from teams
            int teamNumber = team[teams - 1].choices.Count;
            for (int i = 0; i < teamNumber; i++)
            {
                player.choices.Add(team[teams - 1].choices[0]);
                team[teams - 1].choices.RemoveAt(0);
            }
            team[teams - 1].index = -1;
        };
        amountTeams_Right.clicked += () =>
        {
            int teams = Int32.Parse(amountTeams_TextBox.text);
            if (teams < 6)
            {
                amountTeams_TextBox.text = (teams + 1).ToString();
                addTeam[teams].visible = true;
                team[teams].visible = true;
                removePlayerTeam[teams].visible = true;
            }
        };
        randomizeTeamMember.clicked += () =>
        {
            // get all player
            List<string> player = new List<string>();
            foreach (DropdownField t in team)
            {
                foreach (string p in t.choices) 
                {
                    player.Add(p);
                }
            }
            // remove team entrys
            foreach (DropdownField t in team)
            {
                t.choices.Clear();
            }
            // split player to teams
            int amountTeams = Int32.Parse(amountTeams_TextBox.text);            
            int amountPeopleInTeam = player.Count / amountTeams;
            int randomIndex;
            for (int i = 0; i < amountTeams - 1; i++) 
            {
                for (int j = 0; j < amountPeopleInTeam; j++)
                {
                    randomIndex = Random.Range(0, player.Count);
                    team[i].choices.Add(player[randomIndex]);
                    player.RemoveAt(randomIndex);
                }
            }
            while (player.Count > 0)
            {
                team[amountTeams - 1].choices.Add(player[0]);
                player.RemoveAt(0);
            }
        };
        for (int i = 0; i < GameState.maxPlayer; i++)
        {
            int iCopy = i;
            addTeam[iCopy].clicked += () =>
            {
                if (player.index > -1)
                {
                    team[iCopy].choices.Add(player.value);
                    player.choices.RemoveAt(player.choices.IndexOf(player.value));
                    player.index = -1;
                }
            };
            removePlayerTeam[iCopy].clicked += () =>
            {
                if (team[iCopy].index > -1)
                {
                    player.choices.Add(team[iCopy].value);
                    team[iCopy].choices.RemoveAt(team[iCopy].choices.IndexOf(team[iCopy].value));
                    team[iCopy].index = -1;
                }
            };
        }
        continuee.clicked += () =>
        {
            GameState.roundsLeft = Int32.Parse(amountRounds_TextBox.text);
            GameState.teams = new();
            // adding teams
            for(int i = 0; i< Int32.Parse(amountTeams_TextBox.text); i++)
            {
                GameState.teams.Add(new Team((i + 1).ToString()));
            }
            // rerolls and switches
            GameState.refillRerolls = root.Q<Toggle>("RefillRerolls").value;
            GameState.refillSwitches = root.Q<Toggle>("RefillSwitches").value;
            GameState.amountRerolls = Int32.Parse(amountRerolls_TextBox.text);
            GameState.amountSwitches = Int32.Parse(amountSwitches_TextBox.text);
            bool found;
            int help = 0;
            for (int i = 0; i < Int32.Parse(amountTeams_TextBox.text); i++)
            {
                // adding player of teams
                // print warning message if a team is empty
                if (team[i].choices.Count==0)
                {
                    warning.text = "<color=red>Team "+(i+1).ToString()+" is empty!</color>";
                    return;
                }
                // add player
                for (int j = 0; j < team[i].choices.Count; j++)
                {
                    found = false;
                    help = 0;
                    while (!found && help<GameState.profiles.Count)
                    {
                        if (GameState.profiles[help].name==team[i].choices[j])
                        {
                            found = true;
                        }
                        else
                        {
                            help++;
                        }
                    }
                    GameState.teams[i].players.Add(GameState.profiles[help]);
                }
                // add rerolls and switches
                GameState.teams[i].amountRerolls = Int32.Parse(amountRerolls_TextBox.text);
                GameState.teams[i].amountSwitches = Int32.Parse(amountSwitches_TextBox.text);
            }
            GameState.amountPlayer = GameState.teams.Count;
            for(int i = 0; i<GameState.amountPlayer; i++)
            {
                // resetting team points
                GameState.teams[i].points = 0;
                // init random singer list
                GameState.teams[i].playersNotSung = new List<PlayerProfile>();
                foreach (PlayerProfile p in GameState.teams[i].players) {
                    GameState.teams[i].playersNotSung.Add(p);
                }
            }
            // loading new scene
            SceneManager.LoadScene("GameModeSelect");
        };
        back.clicked += () =>
        {
            SceneManager.LoadScene("MainMenu");
        };
        // init
        amountRounds_TextBox.text = "1";
        amountRerolls_TextBox.text = "0";
        amountSwitches_TextBox.text = "0";
        amountTeams_TextBox.text = "2";
        for(int i = 0; i<2; i++)
        {
            addTeam[i].visible = true;
            team[i].visible = true;
            removePlayerTeam[i].visible = true;
        }
        foreach (PlayerProfile profile in GameState.profiles)
        {
            player.choices.Add(profile.name);
        }
    }
}
