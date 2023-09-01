using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using Classes;

public class PlayerProfiles : MonoBehaviour
{
    int currentProfileIndex = 0;
    Label label;
    TextField nameInput;
    GroupBox difficulty_TextBox;
    Toggle useOnlineMic;
    TextField onlineMicName;
    TextField colorRed;
    TextField colorGreen;
    TextField colorBlue;

    void OnEnable()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        Button back = root.Q<Button>("Back");
        label = root.Q<Label>("Player");
        nameInput = root.Q<TextField>("Name");
        Button leftButton = root.Q<Button>("Left");
        Button rightButton = root.Q<Button>("Right");
        Button newButton = root.Q<Button>("New");
        Button deleteButton = root.Q<Button>("Delete");
        TemplateContainer difficulty = root.Q<TemplateContainer>("Difficulty");
        difficulty_TextBox = difficulty.Q<GroupBox>("TextBox");
        Button difficulty_Left = difficulty.Q<Button>("Left");
        Button difficulty_Right = difficulty.Q<Button>("Right");
        useOnlineMic = root.Q<Toggle>("UseOnlineMic");
        onlineMicName = root.Q<TextField>("OnlineMicName");
        colorRed = root.Q<TextField>("ColorRed");
        colorGreen = root.Q<TextField>("ColorGreen");
        colorBlue = root.Q<TextField>("ColorBlue");
        // set functionality of all buttons
        leftButton.clicked += () =>
        {
            if (currentProfileIndex > 0)
            {
                // save data
                SaveProfile(currentProfileIndex);
                // change viewed profile
                currentProfileIndex--;
                ShowProfile(currentProfileIndex);
            }
        };
        rightButton.clicked += () =>
        {
            if (currentProfileIndex < GameState.profiles.Count - 1)
            {
                // save data
                SaveProfile(currentProfileIndex);
                // change viewed profile
                currentProfileIndex++;
                ShowProfile(currentProfileIndex);
            }
        };
        newButton.clicked += () =>
        {
            // save data
            SaveProfile(currentProfileIndex);
            // add new profile
            GameState.profiles.Add(new PlayerProfile("Name"));
            // change view to new profile
            currentProfileIndex = GameState.profiles.Count - 1;
            ShowProfile(currentProfileIndex);
        };
        deleteButton.clicked += () =>
        {
            // delete profile
            if (GameState.profiles.Count > 1)
            {
                GameState.profiles.RemoveAt(currentProfileIndex);
                // change view to other profile
                if (currentProfileIndex >= GameState.profiles.Count)
                {
                    currentProfileIndex--;
                }
                ShowProfile(currentProfileIndex);
            }
            else
            {
                label.text = "Name";
                nameInput.value = "Name";
                difficulty_TextBox.text = Difficulty.Easy.ToString();
                useOnlineMic.value = false;
                onlineMicName.value = "";
            }
        };
        difficulty_Left.clicked += () =>
        {
            difficulty_TextBox.text = difficulty_TextBox.text switch
            {
                "Easy" => "Easy",
                "Normal" => "Easy",
                "Hard" => "Normal",
                _ => throw new System.NotImplementedException(),
            };
        };
        difficulty_Right.clicked += () =>
        {
            difficulty_TextBox.text = difficulty_TextBox.text switch
            {
                "Easy" => "Normal",
                "Normal" => "Hard",
                "Hard" => "Hard",
                _ => throw new System.NotImplementedException(),
            };
        };
        back.clicked += () =>
        {
            // saving profiles
            SaveProfile(currentProfileIndex);
            JsonPlayerProfiles profilesToJson = new(GameState.profiles.ToArray());
            string json = JsonUtility.ToJson(profilesToJson);
            File.WriteAllText("playerProfiles.json", json);
            SceneManager.LoadScene("MainMenu");
        };
        // show profile 1
        ShowProfile(0);
    }

    private void SaveProfile(int index) 
    {
        GameState.profiles[index].name = nameInput.value;
        GameState.profiles[index].difficulty = DifficultyFunctions.StringToDifficulty(difficulty_TextBox.text);
        GameState.profiles[index].useOnlineMic = useOnlineMic.value;
        GameState.profiles[index].onlineMicName = onlineMicName.value;
        GameState.profiles[index].color = new Color(float.Parse(colorRed.value), float.Parse(colorGreen.value), float.Parse(colorBlue.value));
    }

    private void ShowProfile(int index)
    {
        label.text = GameState.profiles[index].name;
        nameInput.value = GameState.profiles[index].name;
        difficulty_TextBox.text = GameState.profiles[index].difficulty.ToString();
        useOnlineMic.value = GameState.profiles[index].useOnlineMic;
        onlineMicName.value = GameState.profiles[index].onlineMicName;
        colorRed.value = GameState.profiles[index].color.r.ToString();
        colorGreen.value = GameState.profiles[index].color.g.ToString();
        colorBlue.value = GameState.profiles[index].color.b.ToString();
    }
}