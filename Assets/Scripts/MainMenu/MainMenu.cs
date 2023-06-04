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
    bool serverStarted = false;
    int currentProfileIndex = 0;

    void OnEnable()
    {
        // load settings
        string json = System.IO.File.ReadAllText("config.json");
        GameState.settings = JsonUtility.FromJson<Settings>(json);
        // load playerProfiles
        json = System.IO.File.ReadAllText("playerProfiles.json");
        GameState.profiles = JsonUtility.FromJson<JsonPlayerProfiles>(json).playerProfiles;
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        // main menu 
        TemplateContainer mainMenu = root.Q<TemplateContainer>("MainMenu");
        Button mainMenu_Play = mainMenu.Q<Button>("Play");
        Button mainMenu_Server = mainMenu.Q<Button>("Server");
        Button mainMenu_Playerprofiles = mainMenu.Q<Button>("Playerprofiles");
        Button mainMenu_Options = mainMenu.Q<Button>("Options");
        Button mainMenu_Exit = mainMenu.Q<Button>("Exit");
        // choose song
        chooseSong = root.Q<TemplateContainer>("ChooseSong");
        Button chooseSong_Back = chooseSong.Q<Button>("Back");
        TemplateContainer chooseSong_PlayerAmount = chooseSong.Q<TemplateContainer>("PlayerAmount");
        GroupBox chooseSong_PlayerAmount_TextBox = chooseSong_PlayerAmount.Q<GroupBox>("TextBox");
        Button chooseSong_PlayerAmount_Left = chooseSong_PlayerAmount.Q<Button>("Left");
        Button chooseSong_PlayerAmount_Right = chooseSong_PlayerAmount.Q<Button>("Right");
        // playerProfiles
        TemplateContainer profiles = root.Q<TemplateContainer>("Profiles");
        Button profiles_Back = profiles.Q<Button>("Back");
        Label profiles_Player = profiles.Q<Label>("Player");
        TextField profiles_NameInput = profiles.Q<TextField>("Name");
        Button profiles_LeftButton = profiles.Q<Button>("Left");
        Button profiles_RightButton = profiles.Q<Button>("Right");
        // options
        TemplateContainer options = root.Q<TemplateContainer>("Options");
        Button options_Back = options.Q<Button>("Back");
        bool[] optionsLeftClickedCreated = new bool[maxPlayer];
        bool[] optionsRightClickedCreated = new bool[maxPlayer];
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
        mainMenu_Server.clicked += () =>
        {
            // start online microphone server if not running
            if (!serverStarted)
            {
                serverStarted = true;
                System.Threading.Tasks.Task.Run(() => startServer());
            }
        };
        mainMenu_Playerprofiles.clicked += () =>
        {
            mainMenu.visible = false;
            profiles.visible = true;
            profiles_Player.text = GameState.profiles[0].name;
            profiles_NameInput.value = GameState.profiles[0].name;
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
                // check if mic in naudio mics
                while (j < NAudio.Wave.WaveInEvent.DeviceCount)
                {
                    if (GameState.settings.microphoneInput[i].name == NAudio.Wave.WaveInEvent.GetCapabilities(j).ProductName)
                    {
                        microphones[i].name = NAudio.Wave.WaveInEvent.GetCapabilities(j).ProductName;
                        microphones[i].index = j;
                        microphones[i].channel = GameState.settings.microphoneInput[i].channel;
                        j = NAudio.Wave.WaveInEvent.DeviceCount;
                    }
                    j++;
                }
                // check if mic in online mics
                if (GameState.onlineMicrophones.Exists(element => element.id == GameState.settings.microphoneInput[i].name))
                {
                    int index = GameState.onlineMicrophones.FindIndex(element => element.id == GameState.settings.microphoneInput[i].name);
                    microphones[i].name = GameState.settings.microphoneInput[i].name;
                    microphones[i].index = NAudio.Wave.WaveInEvent.DeviceCount + index;
                    microphones[i].channel = 0;
                    microphones[i].isOnline = true;
                }
                // set default if mic not exists
                if (microphones[i].equalsWithoutChannel(new MicrophoneData()))
                {
                    microphones[i].name = NAudio.Wave.WaveInEvent.GetCapabilities(0).ProductName;
                    microphones[i].index = 0;
                    microphones[i].channel = 0;
                    microphones[i].isOnline = false;
                }
                int iCopy = i;
                // set text of microphone
                if (microphones[iCopy].isOnline)
                {
                    options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = "Online Microphone: " + microphones[iCopy].name.ToString();
                }
                else
                {
                    options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[iCopy].name.ToString() + ", Channel; " + microphones[iCopy].channel.ToString();
                }
                if (!optionsLeftClickedCreated[iCopy])
                {
                    options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Left").clicked += () => optionsLeftClicked(options, microphones, iCopy);
                    optionsLeftClickedCreated[iCopy] = true;
                }
                if (!optionsRightClickedCreated[iCopy])
                {
                    options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Right").clicked += () => optionsRightClicked(options, microphones, iCopy);
                    optionsRightClickedCreated[iCopy] = true;
                }
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
        // playerProfiles
        profiles_LeftButton.clicked += () =>
        {
            if (currentProfileIndex > 0)
            {
                // save data
                GameState.profiles[currentProfileIndex].name = profiles_NameInput.value;
                // change viewed profile
                currentProfileIndex--;
                profiles_Player.text = GameState.profiles[currentProfileIndex].name;
                profiles_NameInput.value = GameState.profiles[currentProfileIndex].name;
            }
        };
        profiles_RightButton.clicked += () =>
        {
            if (currentProfileIndex < GameState.profiles.Length - 1)
            {
                // save data
                GameState.profiles[currentProfileIndex].name = profiles_NameInput.value;
                // change viewed profile
                currentProfileIndex++;
                profiles_Player.text = GameState.profiles[currentProfileIndex].name;
                profiles_NameInput.value = GameState.profiles[currentProfileIndex].name;
            }
        };
        profiles_Back.clicked += () =>
        {
            mainMenu.visible = true;
            profiles.visible = false;
            // saving profiles
            GameState.profiles[currentProfileIndex].name = profiles_NameInput.value;
            JsonPlayerProfiles profilesToJson = new JsonPlayerProfiles(GameState.profiles);
            json = JsonUtility.ToJson(profilesToJson);
            File.WriteAllText("playerProfiles.json", json);
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
                            gap = float.Parse(line.Substring(5).Replace(".", ",")) * 0.001f;
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

    void startServer()
    {
        // Create Node command for the server
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        // Drop Process on exit/
        System.AppDomain.CurrentDomain.DomainUnload += (s, e) =>
            {
                serverStarted = false;
                process.Kill();
                process.WaitForExit();
            };
        System.AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                serverStarted = false;
                process.Kill();
                process.WaitForExit();
            };
        System.AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                serverStarted = false;
                process.Kill();
                process.WaitForExit();
            };
        // Command
        process.StartInfo.FileName = "node";
        process.StartInfo.Arguments = "Server/server.js";
        // Hide Terminal
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        // Set handlers
        process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(outputHandler);
        process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(outputHandler);
        // Start server and handlers
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

    // output Handler for server
    void outputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
    {
        if (outLine.Data == null)
        {
            return;
        }
        string playerName = outLine.Data.Substring(0, outLine.Data.IndexOf(':'));
        int index = GameState.onlineMicrophones.FindIndex(element => element.id == playerName);
        if (index == -1)
        {
            // Adding if new microphone
            GameState.onlineMicrophones.Add((playerName, Node.None));
        }
        else
        {
            // Updating Nodes
            GameState.onlineMicrophones[index] = (GameState.onlineMicrophones[index].id, NodeFunctions.getNodeFromString(outLine.Data.Substring(outLine.Data.IndexOf(':') + 1)));
        }
    }

    void optionsLeftClicked(TemplateContainer options, MicrophoneData[] microphones, int playerId)
    {
        if (microphones[playerId].channel == 1)
        {
            microphones[playerId].channel -= 1;
            microphones[playerId].isOnline = false;
            options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
        }
        else
        {
            if (microphones[playerId].index > 0)
            {
                microphones[playerId].index -= 1;
                if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount)
                {
                    microphones[playerId].name = NAudio.Wave.WaveInEvent.GetCapabilities(microphones[playerId].index).ProductName;
                    microphones[playerId].channel = 1;
                    microphones[playerId].isOnline = false;
                    options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
                }
                else
                {
                    microphones[playerId].name = GameState.onlineMicrophones[microphones[playerId].index - NAudio.Wave.WaveInEvent.DeviceCount].id;
                    microphones[playerId].isOnline = true;
                    options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Online microphone: " + microphones[playerId].name.ToString();
                }
            }
        }
    }

    void optionsRightClicked(TemplateContainer options, MicrophoneData[] microphones, int playerId)
    {
        if (microphones[playerId].channel == 0)
        {
            if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount)
            {
                microphones[playerId].channel += 1;
                microphones[playerId].isOnline = false;
                options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
            }
            else if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount + GameState.onlineMicrophones.Count - 1)
            {
                microphones[playerId].index += 1;
                microphones[playerId].name = GameState.onlineMicrophones[microphones[playerId].index - NAudio.Wave.WaveInEvent.DeviceCount].id;
                microphones[playerId].isOnline = true;
                options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Online microphone: " + microphones[playerId].name.ToString();
            }
        }
        else
        {
            if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount + GameState.onlineMicrophones.Count - 1)
            {
                microphones[playerId].index += 1;
                microphones[playerId].channel = 0;
                if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount)
                {
                    microphones[playerId].name = NAudio.Wave.WaveInEvent.GetCapabilities(microphones[playerId].index).ProductName;
                    microphones[playerId].isOnline = false;
                    options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
                }
                else
                {
                    microphones[playerId].name = GameState.onlineMicrophones[microphones[playerId].index - NAudio.Wave.WaveInEvent.DeviceCount].id;
                    microphones[playerId].isOnline = true;
                    options.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Online microphone: " + microphones[playerId].name.ToString();
                }
            }
        }
    }
}
