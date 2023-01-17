using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    // choose song
    public int currentLabel = 0;
    TemplateContainer chooseSong;
    bool inChooseSong;
    ArrayList songNames;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        // main menu 
        TemplateContainer mainMenu = root.Q<TemplateContainer>("MainMenu");
        Button mainMenuPlay = mainMenu.Q<Button>("Play");
        Button mainMenuOptions = mainMenu.Q<Button>("Options");
        // choose song
        chooseSong = root.Q<TemplateContainer>("ChooseSong");
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
            inChooseSong = true;
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
            inChooseSong = false;
        };
        // options
        optionsBack.clicked += () =>
        {
            mainMenu.visible = true;
            options.visible = false;
        };
        // Loading song list
        songNames = new ArrayList();
        // path with \\ or /
        string path = "C:/ Users / Maurice / Documents / GitHub / star / Assets / TestAssets";
        if (Directory.Exists(path))
        {
            searchDirectory(path);
        }
        // set song data to items
        int itemCounter = 1;
        for (int i = 0; itemCounter <= 10 && i < songNames.Count; i++)
        {
            chooseSong.Q<Label>(itemCounter.ToString()).text = (string)songNames[i];
            itemCounter++;
        }
    }

    void Update()
    {
        if (inChooseSong)
        {
            // check for mouse wheel
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if (currentLabel < songNames.Count - 10)
                {
                    currentLabel++;
                    updateSongList();
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                if (currentLabel > 0)
                {
                    currentLabel--;
                    updateSongList();
                }
            }
        }
    }

    // Go through all files and folders
    void searchDirectory(string path)
    {
        // Get all files
        string[] files = Directory.GetFiles(path);
        string[] text;
        bool isSong;
        foreach (string file in files)
            // check for song file
            if (file.Contains(".txt"))
            {
                text = System.IO.File.ReadAllLines(file);
                isSong = false;
                foreach (string line in text)
                {
                    if (line.StartsWith("#TITLE"))
                    {
                        isSong = true;
                        break;
                    }
                }
                if (isSong)
                {
                    // when file is song file extract data
                    foreach (string line in text)
                    {
                        if (line.StartsWith("#TITLE"))
                        {
                            songNames.Add(line.Substring(7));
                        }
                    }
                }
            }
        // Get all directorys
        files = Directory.GetDirectories(path);
        foreach (string dir in files)
            searchDirectory(dir);
    }

    void updateSongList()
    {
        int itemCounter = 1;
        // clear song data elements
        for (int i = 1; i <= 10; i++)
        {
            chooseSong.Q<Label>(i.ToString()).text = "";
        }
        // set song data to items
        for (int i = currentLabel; itemCounter <= 10 && i < songNames.Count; i++)
        {
            chooseSong.Q<Label>(itemCounter.ToString()).text = (string)songNames[i];
            itemCounter++;
        }
    }
}
