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
        // options
        TemplateContainer options = root.Q<TemplateContainer>("Options");
        Button options_Back = options.Q<Button>("Back");
        bool[] optionsLeftClickedCreated = new bool[GameState.maxPlayer];
        bool[] optionsRightClickedCreated = new bool[GameState.maxPlayer];
        TextField options_Path = options.Q<TextField>("Path");
        TextField options_Delay = options.Q<TextField>("Delay");
        MicrophoneData[] microphones = new MicrophoneData[GameState.maxPlayer];
        // set functionality of all buttons
        // main menu
        mainMenu_Play.clicked += () =>
        {
            SceneManager.LoadScene("ChooseSong");
        };
        mainMenu_Server.clicked += () =>
        {
            // start online microphone server if not running
            if (!serverStarted)
            {
                serverStarted = true;
                System.Threading.Tasks.Task.Run(() => StartServer());
            }
        };
        mainMenu_Playerprofiles.clicked += () =>
        {
            SceneManager.LoadScene("PlayerProfiles");
        };
        mainMenu_Options.clicked += () =>
        {
            mainMenu.visible = false;
            options.visible = true;
            // load config
            options_Path.value = GameState.settings.absolutePathToSongs;
            options_Delay.value = GameState.settings.microphoneDelayInSeconds.ToString();
            microphones = new MicrophoneData[GameState.maxPlayer];
            // load microphones
            for (int i = 0; i < GameState.maxPlayer; i++)
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
                if (microphones[i].EqualsWithoutChannel(new MicrophoneData()))
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
                    options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Left").clicked += () => OptionsLeftClicked(options, microphones, iCopy);
                    optionsLeftClickedCreated[iCopy] = true;
                }
                if (!optionsRightClickedCreated[iCopy])
                {
                    options.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Right").clicked += () => OptionsRightClicked(options, microphones, iCopy);
                    optionsRightClickedCreated[iCopy] = true;
                }
            }
        };
        mainMenu_Exit.clicked += () =>
                {
                    Application.Quit();
                };
        // options
        options_Back.clicked += () =>
        {
            mainMenu.visible = true;
            options.visible = false;
            // save config
            Settings settings = new(options_Path.value, float.Parse(options_Delay.value.Replace(".", ",")), microphones);
            json = JsonUtility.ToJson(settings);
            File.WriteAllText("config.json", json);
            // update setting
            GameState.settings = settings;
        };
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

    void OptionsLeftClicked(TemplateContainer options, MicrophoneData[] microphones, int playerId)
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

    void OptionsRightClicked(TemplateContainer options, MicrophoneData[] microphones, int playerId)
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
