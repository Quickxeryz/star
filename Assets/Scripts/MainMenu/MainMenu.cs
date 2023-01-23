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
        // load settings
        string json = System.IO.File.ReadAllText("config.json");
        GameState.settings = JsonUtility.FromJson<Settings>(json);
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        // main menu 
        TemplateContainer mainMenu = root.Q<TemplateContainer>("MainMenu");
        Button mainMenuPlay = mainMenu.Q<Button>("Play");
        Button mainMenuOptions = mainMenu.Q<Button>("Options");
        Button mainMenuExit = mainMenu.Q<Button>("Exit");
        // choose song
        chooseSong = root.Q<TemplateContainer>("ChooseSong");
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
        mainMenuExit.clicked += () =>
        {
            Application.Quit();
        };
        // choose song
        chooseSongBack.clicked += () =>
        {
            mainMenu.visible = true;
            chooseSong.visible = false;
            // clear song data elements
            for (int i = 1; i <= 10; i++)
            {
                chooseSong.Q<Button>(i.ToString()).visible = false;
            }
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
        if (Directory.Exists(GameState.settings.absolutePathToSongs))
        {
            searchDirectory(GameState.settings.absolutePathToSongs);
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
        SongData currentSong;
        string songTitle = "";
        string songArtist = "";
        string songMusicPath = "";
        float bpm = 0;
        float gap = 0;
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
                    songVideoPath = "";
                    // when file is song file extract data
                    foreach (string line in text)
                    {
                        if (line.StartsWith("#TITLE:"))
                        {
                            songTitle = line.Substring(7);
                        }
                        else if (line.StartsWith("#ARTIST:"))
                        {
                            songArtist = line.Substring(8);
                        }
                        else if (line.StartsWith("#MP3:"))
                        {
                            songMusicPath = path + "\\" + line.Substring(5);
                        }
                        else if (line.StartsWith("#BPM:"))
                        {
                            bpm = float.Parse(line.Substring(5).Replace(".", ","));
                        }
                        else if (line.StartsWith("#GAP:"))
                        {
                            gap = int.Parse(line.Substring(5)) * 0.001f;
                        }
                        else if (line.StartsWith("#VIDEO:"))
                        {
                            songVideoPath = path + "\\" + line.Substring(7);
                        }
                    }
                    currentSong = new SongData(file, songTitle, songArtist, songMusicPath, bpm, gap);
                    currentSong.pathToVideo = songVideoPath;
                    songs.Add(currentSong);
                }
            }
        // Get all directorys
        files = Directory.GetDirectories(path);
        foreach (string dir in files)
        {
            searchDirectory(dir);
        }
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
                GameState.currentSong = (SongData)songs[iCopy];
                SceneManager.LoadScene("GameScene");
            };
        }
    }
}
