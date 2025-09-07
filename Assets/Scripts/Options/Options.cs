using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using Classes;

public class Options : MonoBehaviour
{
    VisualElement root;

    void OnEnable()
    {
        // UI
        root = GetComponent<UIDocument>().rootVisualElement;
        Button back = root.Q<Button>("Back");
        bool[] leftClickedCreated = new bool[GameState.maxPlayer];
        bool[] rightClickedCreated = new bool[GameState.maxPlayer];
        TextField path = root.Q<TextField>("Path");
        TextField delay = root.Q<TextField>("Delay");
        Toggle useNewNodeEngine = root.Q<Toggle>("NewNodeEngineToggle");
        MicrophoneData[] microphones = new MicrophoneData[GameState.maxPlayer];
        // load config
        path.value = GameState.settings.absolutePathToSongs;
        delay.value = GameState.settings.microphoneDelayInSeconds.ToString();
        microphones = new MicrophoneData[GameState.maxPlayer];
        useNewNodeEngine.value = GameState.settings.useNewNodeEngine;
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
                root.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = "Online Microphone: " + microphones[iCopy].name.ToString();
            }
            else
            {
                root.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[iCopy].name.ToString() + ", Channel; " + microphones[iCopy].channel.ToString();
            }
            if (!leftClickedCreated[iCopy])
            {
                root.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Left").clicked += () => OptionsLeftClicked(microphones, iCopy);
                leftClickedCreated[iCopy] = true;
            }
            if (!rightClickedCreated[iCopy])
            {
                root.Q<TemplateContainer>("Microphone" + (iCopy + 1).ToString()).Q<Button>("Right").clicked += () => OptionsRightClicked(microphones, iCopy);
                rightClickedCreated[iCopy] = true;
            }
        }
        // set up buttons
        back.clicked += () =>
        {
            // reload songs if new path
            if (path.value != GameState.settings.absolutePathToSongs)
            {
                GameState.songsLoaded = false;
            }
            // save config
            Settings settings = new(path.value, float.Parse(delay.value.Replace(".", ",")), microphones, useNewNodeEngine.value);
            string json = JsonUtility.ToJson(settings);
            File.WriteAllText("config.json", json);
            // update setting
            GameState.settings = settings;
            SceneManager.LoadScene("MainMenu");
        };
    }

    void OptionsLeftClicked(MicrophoneData[] microphones, int playerId)
    {
        if (microphones[playerId].channel == 1)
        {
            microphones[playerId].channel -= 1;
            microphones[playerId].isOnline = false;
            root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
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
                    root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
                }
                else
                {
                    microphones[playerId].name = GameState.onlineMicrophones[microphones[playerId].index - NAudio.Wave.WaveInEvent.DeviceCount].id;
                    microphones[playerId].isOnline = true;
                    root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Online microphone: " + microphones[playerId].name.ToString();
                }
            }
        }
    }

    void OptionsRightClicked(MicrophoneData[] microphones, int playerId)
    {
        if (microphones[playerId].channel == 0)
        {
            if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount)
            {
                microphones[playerId].channel += 1;
                microphones[playerId].isOnline = false;
                root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
            }
            else if (microphones[playerId].index < NAudio.Wave.WaveInEvent.DeviceCount + GameState.onlineMicrophones.Count - 1)
            {
                microphones[playerId].index += 1;
                microphones[playerId].name = GameState.onlineMicrophones[microphones[playerId].index - NAudio.Wave.WaveInEvent.DeviceCount].id;
                microphones[playerId].isOnline = true;
                root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Online microphone: " + microphones[playerId].name.ToString();
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
                    root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Microphone: " + microphones[playerId].name.ToString() + ", Channel: " + microphones[playerId].channel.ToString();
                }
                else
                {
                    microphones[playerId].name = GameState.onlineMicrophones[microphones[playerId].index - NAudio.Wave.WaveInEvent.DeviceCount].id;
                    microphones[playerId].isOnline = true;
                    root.Q<TemplateContainer>("Microphone" + (playerId + 1).ToString()).Q<Label>("Text").text = "Online microphone: " + microphones[playerId].name.ToString();
                }
            }
        }
    }
}
