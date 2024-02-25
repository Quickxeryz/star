using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Classes;

public class MainMenu : MonoBehaviour
{
    // global vars for song loading
    Label songsLoaded;

    void OnEnable()
    {
        // UI
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        // finding all ui elements
        Button play = root.Q<Button>("Play");
        songsLoaded = root.Q<Label>("SongsLoaded");
        Button gameModes = root.Q<Button>("GameModes");
        Button server = root.Q<Button>("Server");
        Label website = root.Q<Label>("Website");
        Button playerprofiles = root.Q<Button>("Playerprofiles");
        Button options = root.Q<Button>("Options");
        Button exit = root.Q<Button>("Exit");
        // load settings
        string json = File.ReadAllText("config.json");
        GameState.settings = JsonUtility.FromJson<Settings>(json);
        // load playerProfiles
        json = File.ReadAllText("playerProfiles.json");
        if (GameState.profiles.Count == 0)
        {
            GameState.profiles.AddRange(JsonUtility.FromJson<JsonPlayerProfiles>(json).playerProfiles);
        }
        // get ip for server
        if (!GameState.serverStarted)
        {
            Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            GameState.ip = endPoint.Address.ToString();
            // set ip address in server and client file
            string[] text = File.ReadAllLines("Server/client.js");
            text[0] = "const ip = \"" + GameState.ip + "\";";
            File.WriteAllLines("Server/client.js", text);
            text = File.ReadAllLines("Server/server.js");
            text[0] = "const hostname = \"" + GameState.ip + "\";";
            File.WriteAllLines("Server/server.js", text);
        } else
        {
            website.text = "Server available under: https://" + GameState.ip + ":8085";
        }
        // parallel loading songs 
        if (GameState.songsLoaded)
        {
            songsLoaded.text = "Songs loaded :)";
        } else
        {
            songsLoaded.text = "Loading songs ...";
            System.Threading.Tasks.Task.Run(() => LoadSongs());
        }
        // set functionality of all buttons
        play.clicked += () =>
        {
            if (GameState.songsLoaded)
            {
                SceneManager.LoadScene("ChooseSong");
            }
        };
        gameModes.clicked += () =>
        {
            SceneManager.LoadScene("GameModeConfig");
        };
        server.clicked += () =>
        {
            // start online microphone server if not running
            if (!GameState.serverStarted)
            {
                GameState.serverStarted = true;
                website.text = "Server available under: https://" + GameState.ip + ":8085";
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

    private void Update()
    {
        if (GameState.songsLoaded)
        {
            songsLoaded.text = "Songs loaded :)";
        }
    }

    void LoadSongs() 
    {
        GameState.songs = new();
        if (Directory.Exists(GameState.settings.absolutePathToSongs))
        {
            SearchDirectory(GameState.settings.absolutePathToSongs);
        }
        GameState.songs.Sort();
        GameState.songsLoaded = true;
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
        int lastBeat;
        int currentBeat;
        int amountVoices;
        string temp;
        foreach (string file in files)
        {
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
                    amountVoices = 1;
                    lastBeat = 0;
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
                            // reading gap and transforming to seconds
                            gap = float.Parse(line[5..].Replace(".", ",")) * 0.001f;
                        }
                        else if (line.StartsWith("#VIDEO:"))
                        {
                            songVideoPath = path + "/" + line[7..];
                        } else if (line.Length>0)
                        {
                            if (line[0] == ':')
                            {
                                temp = line[2..];
                                currentBeat = int.Parse(temp[..temp.IndexOf(' ')]);
                                if (currentBeat < lastBeat)
                                {
                                    amountVoices++;
                                }
                                lastBeat = currentBeat;
                            }
                        }
                    }
                    currentSong = new SongData(file, songTitle, songArtist, songMusicPath, bpm, gap, amountVoices)
                    {
                        pathToVideo = songVideoPath
                    };
                    GameState.songs.Add(currentSong);
                }
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
                GameState.serverStarted = false;
                process.Kill();
                process.WaitForExit();
            };
        System.AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                GameState.serverStarted = false;
                process.Kill();
                process.WaitForExit();
            };
        System.AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                GameState.serverStarted = false;
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