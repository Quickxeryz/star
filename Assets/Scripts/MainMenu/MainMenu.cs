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
            SceneManager.LoadScene("Options");
        };
        mainMenu_Exit.clicked += () =>
        {
            Application.Quit();
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
}