using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using System.Collections;
using Classes;

public class MainMenu : MonoBehaviour
{
    // choose song
    public int currentLabel = 0;
    TemplateContainer chooseSong;
    bool inChooseSong;
    ArrayList songs;

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
            updateSongList();
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
        songs = new ArrayList();
        if (Directory.Exists(GameState.songFolderPath))
        {
            searchDirectory(GameState.songFolderPath);
        }
    }

    void Update()
    {
        if (inChooseSong)
        {
            // check for mouse wheel
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if (currentLabel < songs.Count - 10)
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
        string songTitle = "";
        string songAuthor = "";
        string songVideoPath = "";
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
                        if (line.StartsWith("#TITLE:"))
                        {
                            songTitle = line.Substring(7);
                        }
                        else if (line.StartsWith("#ARTIST:"))
                        {
                            songAuthor = line.Substring(8);
                        }
                        else if (line.StartsWith("#VIDEO:"))
                        {
                            songVideoPath = path + "\\" + line.Substring(7);
                        }
                    }
                    songs.Add(new SongData(songTitle, songAuthor, file, songVideoPath));
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
            chooseSong.Q<Button>(i.ToString()).visible = false;
        }
        // set song data to items
        Button songButton;
        for (int i = currentLabel; itemCounter <= 10 && i < songs.Count; i++)
        {
            int iCopy = i;
            songButton = chooseSong.Q<Button>(itemCounter.ToString());
            songButton.text = ((SongData)songs[i]).title;
            songButton.visible = true;
            itemCounter++;
            // set Button function
            songButton.clicked += () =>
            {
                GameState.choosenSongPath = ((SongData)songs[iCopy]).pathToSong;
                GameState.choosenVideoPath = ((SongData)songs[iCopy]).pathToVideo;
                SceneManager.LoadScene("GameScene");
            };
        }
    }
}
