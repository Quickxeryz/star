using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using Classes;

public class PlayerProfiles : MonoBehaviour
{
    int currentProfileIndex = 0;

    void OnEnable()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        Button back = root.Q<Button>("Back");
        Label label = root.Q<Label>("Player");
        TextField nameInput = root.Q<TextField>("Name");
        Button leftButton = root.Q<Button>("Left");
        Button rightButton = root.Q<Button>("Right");
        Button newButton = root.Q<Button>("New");
        Button deleteButton = root.Q<Button>("Delete");
        // set functionality of all buttons
        leftButton.clicked += () =>
        {
            if (currentProfileIndex > 0)
            {
                // save data
                GameState.profiles[currentProfileIndex].name = nameInput.value;
                // change viewed profile
                currentProfileIndex--;
                label.text = GameState.profiles[currentProfileIndex].name;
                nameInput.value = GameState.profiles[currentProfileIndex].name;
            }
        };
        rightButton.clicked += () =>
        {
            if (currentProfileIndex < GameState.profiles.Count - 1)
            {
                // save data
                GameState.profiles[currentProfileIndex].name = nameInput.value;
                // change viewed profile
                currentProfileIndex++;
                label.text = GameState.profiles[currentProfileIndex].name;
                nameInput.value = GameState.profiles[currentProfileIndex].name;
            }
        };
        newButton.clicked += () =>
        {
            // save data
            GameState.profiles[currentProfileIndex].name = nameInput.value;
            // add new profile
            GameState.profiles.Add(new PlayerProfile("Name"));
            // change view to new profile
            currentProfileIndex = GameState.profiles.Count - 1;
            label.text = GameState.profiles[currentProfileIndex].name;
            nameInput.value = GameState.profiles[currentProfileIndex].name;
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
                label.text = GameState.profiles[currentProfileIndex].name;
                nameInput.value = GameState.profiles[currentProfileIndex].name;
            }
            else
            {
                label.text = "Name";
                nameInput.value = "Name";
            }
        };
        back.clicked += () =>
        {
            // saving profiles
            GameState.profiles[currentProfileIndex].name = nameInput.value;
            JsonPlayerProfiles profilesToJson = new(GameState.profiles.ToArray());
            string json = JsonUtility.ToJson(profilesToJson);
            File.WriteAllText("playerProfiles.json", json);
            SceneManager.LoadScene("MainMenu");
        };
        // show profile 1
        label.text = GameState.profiles[0].name;
        nameInput.value = GameState.profiles[0].name;
    }
}