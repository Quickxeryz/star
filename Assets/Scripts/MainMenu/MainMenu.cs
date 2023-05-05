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
    const int maxPlayer = 6;

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
        Button mainMenu_Play = mainMenu.Q<Button>("Play");
        Button mainMenu_Options = mainMenu.Q<Button>("Options");
        Button mainMenu_Exit = mainMenu.Q<Button>("Exit");
        // choose song
        chooseSong = root.Q<TemplateContainer>("ChooseSong");
        Button chooseSong_Back = chooseSong.Q<Button>("Back");
        TemplateContainer chooseSong_PlayerAmount = chooseSong.Q<TemplateContainer>("PlayerAmount");
        GroupBox chooseSong_PlayerAmount_TextBox = chooseSong_PlayerAmount.Q<GroupBox>("TextBox");
        Button chooseSong_PlayerAmount_Left = chooseSong_PlayerAmount.Q<Button>("Left");
        Button chooseSong_PlayerAmount_Right = chooseSong_PlayerAmount.Q<Button>("Right");
        // options
        TemplateContainer options = root.Q<TemplateContainer>("Options");
        Button options_Back = options.Q<Button>("Back");
        TextField options_Path = options.Q<TextField>("Path");
        TextField options_Delay = options.Q<TextField>("Delay");
        MicrophoneData[] microphones = new MicrophoneData[maxPlayer];
        // set functionality of all buttons
        // main menu
        mainMenu_Play.clicked += () =>
        {
            mainMenu.visible = false;
            updateSongList();
            chooseSong.visible = true;
            inChooseSong = true;
            // Load Amount Player 
            chooseSong_PlayerAmount_TextBox.text = GameState.amountPlayer.ToString();
        };
        mainMenu_Options.clicked += () =>
        {
            mainMenu.visible = false;
            options.visible = true;
            // load config
            options_Path.value = GameState.settings.absolutePathToSongs;
            options_Delay.value = GameState.settings.microphoneDelayInSeconds.ToString();
            microphones = new MicrophoneData[maxPlayer];
            // load microphones
            for (int i = 0; i < maxPlayer; i++)
            {
                int j = 0;
                microphones[i] = new MicrophoneData();
                while (j < NAudio.Wave.WaveInEvent.DeviceCount)
                {
                    if (NAudio.Wave.WaveInEvent.GetCapabilities(j).ProductName == GameState.settings.microphoneInput[i].name)
                    {
                        microphones[i].name = NAudio.Wave.WaveInEvent.GetCapabilities(j).ProductName;
                        microphones[i].index = j;
                        microphones[i].channel = GameState.settings.microphoneInput[i].channel;
                        j = NAudio.Wave.WaveInEvent.DeviceCount;
                    }
                    j++;
                }
                if (microphones[i].equalsWithoutChannel(new MicrophoneData()))
                {
                    microphones[i].name = NAudio.Wave.WaveInEvent.GetCapabilities(0).ProductName;
                    microphones[i].index = 0;
                    microphones[i].channel = 0;
                }
                int iCopy = i;
                // set text of microphone
                options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = microphones[iCopy].name.ToString() + " Channel " + microphones[iCopy].channel.ToString();
                options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Left").clicked += () =>
                {
                    if (microphones[iCopy].channel == 1)
                    {
                        microphones[iCopy].channel -= 1;
                        options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = microphones[iCopy].name.ToString() + " Channel " + microphones[iCopy].channel.ToString();
                    }
                    else
                    {
                        if (microphones[iCopy].index > 0)
                        {
                            microphones[iCopy].index -= 1;
                            microphones[iCopy].name = NAudio.Wave.WaveInEvent.GetCapabilities(microphones[iCopy].index).ProductName;
                            microphones[iCopy].channel = 1;
                            options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = microphones[iCopy].name.ToString() + " Channel " + microphones[iCopy].channel.ToString();
                        }
                    }
                };
                options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Right").clicked += () =>
                {
                    if (microphones[iCopy].channel == 0)
                    {
                        microphones[iCopy].channel += 1;
                        options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = microphones[iCopy].name.ToString() + " Channel " + microphones[iCopy].channel.ToString();
                    }
                    else
                    {
                        if (microphones[iCopy].index < NAudio.Wave.WaveInEvent.DeviceCount - 1)
                        {
                            microphones[iCopy].index += 1;
                            microphones[iCopy].name = NAudio.Wave.WaveInEvent.GetCapabilities(microphones[iCopy].index).ProductName;
                            microphones[iCopy].channel = 0;
                            options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = microphones[iCopy].name.ToString() + " Channel " + microphones[iCopy].channel.ToString();
                        }
                    }
                };
            }
        };
        mainMenu_Exit.clicked += () =>
        {
            Application.Quit();
        };
        // choose song
        chooseSong_Back.clicked += () =>
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
        chooseSong_PlayerAmount_Left.clicked += () =>
        {
            if (GameState.amountPlayer > 1)
            {
                GameState.amountPlayer--;
                chooseSong_PlayerAmount_TextBox.text = GameState.amountPlayer.ToString();
            }
        };
        chooseSong_PlayerAmount_Right.clicked += () =>
        {
            if (GameState.amountPlayer < 6)
            {
                GameState.amountPlayer++;
                chooseSong_PlayerAmount_TextBox.text = GameState.amountPlayer.ToString();
            }
        };
        // options
        options_Back.clicked += () =>
        {
            mainMenu.visible = true;
            options.visible = false;
            // save config
            Settings settings = new Settings(options_Path.value, float.Parse(options_Delay.value.Replace(".", ",")), microphones);
            json = JsonUtility.ToJson(settings);
            File.WriteAllText("config.json", json);
            // update setting
            GameState.settings = settings;
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
