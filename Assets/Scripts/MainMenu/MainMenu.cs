using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using Classes;

public class MainMenu : MonoBehaviour
{
    bool serverStarted = false;

    void OnEnable()
    {
        // load settings
        string json = File.ReadAllText("config.json");
        GameState.settings = JsonUtility.FromJson<Settings>(json);
        // load playerProfiles
        json = File.ReadAllText("playerProfiles.json");
        if (GameState.profiles.Count == 0)
        {
            GameState.profiles.AddRange(JsonUtility.FromJson<JsonPlayerProfiles>(json).playerProfiles);
        }
        // Loading song list
        if (!GameState.songsLoaded)
        {
            GameState.songs = new();
            if (Directory.Exists(GameState.settings.absolutePathToSongs))
            {
                SearchDirectory(GameState.settings.absolutePathToSongs);
            }
            GameState.songs.Sort();
            GameState.songsLoaded = true;
        }
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all Buttons
        Button play = root.Q<Button>("Play");
        Button gameModes = root.Q<Button>("GameModes");
        Button server = root.Q<Button>("Server");
        Button playerprofiles = root.Q<Button>("Playerprofiles");
        Button options = root.Q<Button>("Options");
        Button exit = root.Q<Button>("Exit");
        // set functionality of all buttons
        play.clicked += () =>
        {
            SceneManager.LoadScene("ChooseSong");
        };
        gameModes.clicked += () =>
        {
            SceneManager.LoadScene("GameModeConfig");
        };
        server.clicked += () =>
        {
            // start online microphone server if not running
            if (!serverStarted)
            {
                serverStarted = true;
                System.Threading.Tasks.Task.Run(() => StartServer());
            }
        };
        playerprofiles.clicked += () =>
        {
            SceneManager.LoadScene("PlayerProfiles");
        };
        options.clicked += () =>
        {
            SceneManager.LoadScene("Options");
        };
        exit.clicked += () =>
        {
            Application.Quit();
        };
    }

    // Go through all files and folders
    void SearchDirectory(string path)
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
        string songVideoPath;
        foreach (string file in files)
            // check for song file
            if (file.Contains(".txt"))
            {
                text = File.ReadAllLines(file);
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
                            songTitle = line[7..];
                        }
                        else if (line.StartsWith("#ARTIST:"))
                        {
                            songArtist = line[8..];
                        }
                        else if (line.StartsWith("#MP3:"))
                        {
                            songMusicPath = path + "/" + line[5..];
                        }
                        else if (line.StartsWith("#BPM:"))
                        {
                            bpm = float.Parse(line[5..].Replace(".", ","));
                        }
                        else if (line.StartsWith("#GAP:"))
                        {
                            gap = float.Parse(line[5..].Replace(".", ",")) * 0.001f;
                        }
                        else if (line.StartsWith("#VIDEO:"))
                        {
                            songVideoPath = path + "/" + line[7..];
                        }
                    }
                    currentSong = new SongData(file, songTitle, songArtist, songMusicPath, bpm, gap)
                    {
                        pathToVideo = songVideoPath
                    };
                    GameState.songs.Add(currentSong);
                }
            }
        // Get all directorys
        files = Directory.GetDirectories(path);
        foreach (string dir in files)
        {
            SearchDirectory(dir);
        }
    }

    void StartServer()
    {
        // Create Node command for the server
        System.Diagnostics.Process process = new();
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
        process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(OutputHandler);
        process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(OutputHandler);
        // Start server and handlers
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

    // output Handler for server
    void OutputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
    {
        if (outLine.Data == null)
        {
            return;
        }
        string playerName = outLine.Data[..outLine.Data.IndexOf(':')];
        int index = GameState.onlineMicrophones.FindIndex(element => element.id == playerName);
        if (index == -1)
        {
            // Adding if new microphone
            GameState.onlineMicrophones.Add((playerName, Node.None));
        }
        else
        {
            // Updating Nodes
            GameState.onlineMicrophones[index] = (GameState.onlineMicrophones[index].id, NodeFunctions.GetNodeFromString(outLine.Data[(outLine.Data.IndexOf(':') + 1)..]));
        }
    }
}