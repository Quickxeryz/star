using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        // main menu 
        TemplateContainer mainMenu = root.Q<TemplateContainer>("MainMenu");
        Button mainMenuPlay = mainMenu.Q<Button>("Play");
        Button mainMenuOptions = mainMenu.Q<Button>("Options");
        // choose song
        TemplateContainer chooseSong = root.Q<TemplateContainer>("ChooseSong");
        Button chooseSongPlay = chooseSong.Q<Button>("Play");
        Button chooseSongBack = chooseSong.Q<Button>("Back");
        // options
        TemplateContainer options = root.Q<TemplateContainer>("Options");
        Button optionsBack = options.Q<Button>("Back");
        // set functionality of all buttons
        // main menu
        mainMenuPlay.clicked += () =>
        {
            mainMenu.visible = false;
            chooseSong.visible = true;
        };
        mainMenuOptions.clicked += () =>
        {
            mainMenu.visible = false;
            options.visible = true;
        };
        // choose song
        chooseSongPlay.clicked += () =>
        {
            SceneManager.LoadScene("GameScene");
        };
        chooseSongBack.clicked += () =>
        {
            mainMenu.visible = true;
            chooseSong.visible = false;
        };
        // options
        optionsBack.clicked += () =>
        {
            mainMenu.visible = true;
            options.visible = false;
        };
    }
}
