using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Classes;
using System.Threading;

public class GameLogic : MonoBehaviour
{
    enum Kind
    {
        Free,
        Normal,
        Golden,
        LineBreak,
        LineBreakExcact
    }

    class SyllableData
    {
        public Kind kind;
        public int appearing;
        public int length;
        public Node node;
        public string syllable;
        public SyllableData()
        {
            kind = Kind.Free;
            appearing = 0;
            length = 0;
            node = Node.C;
            syllable = "";
        }

        public string To_string()
        {
            return "kind: " + this.kind.ToString() + "; appearing: " + this.appearing.ToString() + "; length: " + this.length.ToString() + "; node" + this.node.ToString() + "; syllable: " + this.syllable.ToString();
        }
    }

    class TextObject 
    {
        public GameObject gameObject;
        public TextMeshProUGUI textMesh;
        public bool isSecondHalf;

        public TextObject(GameObject gameObject, TextMeshProUGUI textMesh, bool isSecondHalf)
        {
            this.gameObject = gameObject;
            this.textMesh = textMesh;
            this.isSecondHalf = isSecondHalf;
        }
    }

    // mic input
    public MicrophoneInput microphoneInput;
    // video
    public GameVideoPlayer video;
    // pause menu
    DateTime lastTimePressed = DateTime.Now;
    bool isPaused = false;
    // song player
    SongPlayer songPlayer;
    double songLength = 0;
    // timeLine
    bool timeLineSet = false;
    // songfile data extraction
    List<SyllableData>[] songData;
    // amount voices
    List<int> voices = new();
    // syllables data
    List<SyllableData> syllablesLine1Bottom = new();
    List<SyllableData> syllablesLine2Bottom = new();
    List<SyllableData> syllablesLine1Top = new();
    List<SyllableData> syllablesLine2Top = new();
    // songData index
    int[] songDataCurrentIndex;
    int[] songDataNewLineIndex;
    // beat data
    int[] startBeatLine;
    int[] endBeatLine;
    int[] beatSumLine;
    // colors 
    const string colorSung = "<color=#0000ffff>";
    const string colorGoldenToSing = "<color=#ffff00ff>";
    const string colorGoldenSung = "<color=#ff00ffff>";
    // UI data
    Vector2 sizeDelta = new(1000f, 500f);
    // UI pointer
    readonly List<TextObject> textLine1Bottom = new();
    TextObject textLine2Bottom;
    readonly List<TextObject> textLine1Top = new();
    TextObject textLine2Top;
    readonly VisualElement[] nodeArrows = new VisualElement[GameState.amountPlayer];
    readonly VisualElement[] nodeBoxes = new VisualElement[GameState.amountPlayer];
    readonly Label[] pointsTexts = new Label[GameState.amountPlayer];
    GameObject whenToStartBottom;
    RectTransform whenToStartBottomRectTransform;
    GameObject whenToStartTop;
    RectTransform whenToStartTopRectTransform;
    RectTransform currentTimePointerBottom;
    RectTransform currentTimePointerTop;
    // half size of node arrow and blocks texture in %
    const int nodeHeightOffset = 5;
    // difference to next node in node texture in pixel
    const int nodeTextureDistance = 5;
    // length of node texture in pixel
    const int nodeTextureHeight = 64;
    // width of node arrow in percent
    const int nodeArrowWidth = 2;
    // score calculating variables 
    float[] pointsPerBeat;
    readonly int[] lastTimeStamps = new int[GameState.amountPlayer];
    readonly float[] points = new float[GameState.amountPlayer];
    // middle values of nodes for together game mode
    readonly Node[,] middle_nodes = { { Node.C, Node.C, Node.CH, Node.CH, Node.D, Node.D, Node.DH, Node.A, Node.AH, Node.AH, Node.B, Node.B }, { Node.C, Node.CH, Node.CH, Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.AH, Node.B, Node.B, Node.C }, { Node.CH, Node.CH, Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.B, Node.C, Node.C }, { Node.CH, Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.C, Node.C, Node.CH }, { Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.CH, Node.CH }, { Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.D }, { Node.A, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH }, { Node.A, Node.AH, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH, Node.A }, { Node.AH, Node.AH, Node.B, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH, Node.A, Node.A }, { Node.AH, Node.B, Node.B, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH, Node.A, Node.A, Node.AH }, { Node.B, Node.B, Node.C, Node.C, Node.G, Node.G, Node.GH, Node.GH, Node.A, Node.A, Node.AH, Node.AH }, { Node.B, Node.C, Node.C, Node.CH, Node.CH, Node.GH, Node.GH, Node.A, Node.A, Node.AH, Node.AH, Node.B } };

    void Start()
    {
        // init songData index
        songDataCurrentIndex = new int[GameState.currentSong.amountVoices];
        songDataNewLineIndex = new int[GameState.currentSong.amountVoices];
        // init array sizes
        startBeatLine = new int[GameState.currentSong.amountVoices];
        endBeatLine = new int[GameState.currentSong.amountVoices];
        beatSumLine = new int[GameState.currentSong.amountVoices];
        pointsPerBeat = new float[GameState.currentSong.amountVoices];
        songData = new List<SyllableData>[GameState.currentSong.amountVoices];
        // init songData
        for (int i = 0; i < GameState.currentSong.amountVoices; i++)
        {
            songData[i] = new();
        }
        // Getting data from song file
        string[] songFileData = File.ReadAllLines(GameState.currentSong.path);
        SyllableData syllable;
        string temp;
        bool needSpace = false;
        int lastBeat = 0;
        int currentVoice = 0;
        foreach (string line in songFileData)
        {
            if (line.Length > 0)
            {
                syllable = new SyllableData();
                switch (line[0])
                {
                    // Normal note
                    case ':':
                        // getting syllable info
                        syllable.kind = Kind.Normal;
                        temp = line[2..];
                        syllable.appearing = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.length = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.node = NodeFunctions.GetNodeFromInt(int.Parse(temp[..temp.IndexOf(' ')]));
                        // changing white space from end to the beginning of the next syllable for text mesh
                        if (needSpace)
                        {
                            syllable.syllable = " ";
                            needSpace = false;
                        }
                        syllable.syllable += temp[(temp.IndexOf(' ') + 1)..];
                        if (syllable.syllable[^1] == ' ')
                        {
                            syllable.syllable = syllable.syllable[..^1];
                            needSpace = true;
                        }
                        // checking if switched to next voice
                        if (lastBeat > syllable.appearing)
                        {
                            currentVoice++;
                        }
                        lastBeat = syllable.appearing;
                        songData[currentVoice].Add(syllable);
                        break;
                    // Golden note
                    case '*':
                        syllable.kind = Kind.Golden;
                        temp = line[2..];
                        syllable.appearing = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.length = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.node = NodeFunctions.GetNodeFromInt(int.Parse(temp[..temp.IndexOf(' ')]));
                        // changing white space from end to the beginning of the next syllable for text mesh
                        if (needSpace)
                        {
                            syllable.syllable = " ";
                            needSpace = false;
                        }
                        syllable.syllable += temp[(temp.IndexOf(' ') + 1)..];
                        if (syllable.syllable[^1] == ' ')
                        {
                            syllable.syllable = syllable.syllable[..^1];
                            needSpace = true;
                        }
                        // checking if switched to next voice
                        if (lastBeat > syllable.appearing)
                        {
                            currentVoice++;
                        }
                        lastBeat = syllable.appearing;
                        songData[currentVoice].Add(syllable);
                        break;
                    // Freestyle syllable
                    case 'F':
                        syllable.kind = Kind.Free;
                        temp = line[2..];
                        syllable.appearing = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.length = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.node = NodeFunctions.GetNodeFromInt(int.Parse(temp[..temp.IndexOf(' ')]));
                        // changing white space from end to the beginning of the next syllable for text mesh
                        if (needSpace)
                        {
                            syllable.syllable = " ";
                            needSpace = false;
                        }
                        syllable.syllable += temp[(temp.IndexOf(' ') + 1)..];
                        if (syllable.syllable[^1] == ' ')
                        {
                            syllable.syllable = syllable.syllable[..^1];
                            needSpace = true;
                        }
                        // checking if switched to next voice
                        if (lastBeat > syllable.appearing)
                        {
                            currentVoice++;
                        }
                        lastBeat = syllable.appearing;
                        songData[currentVoice].Add(syllable);
                        break;
                    // Line break
                    case '-':
                        temp = line.TrimEnd();
                        // Handle "- newLineTime" and "- deleteLineTime newLineTime"
                        if (temp.IndexOf(' ') == temp.LastIndexOf(' '))
                        {
                            syllable.kind = Kind.LineBreak;
                            syllable.appearing = int.Parse(temp[2..]);
                            // checking if switched to next voice
                            if (lastBeat > syllable.appearing)
                            {
                                currentVoice++;
                            }
                            lastBeat = syllable.appearing;
                            songData[currentVoice].Add(syllable);
                        }
                        else
                        {
                            syllable.kind = Kind.LineBreakExcact;
                            temp = line[2..];
                            syllable.appearing = int.Parse(temp[..temp.IndexOf(' ')]);
                            temp = temp[(temp.IndexOf(' ') + 1)..];
                            syllable.length = int.Parse(temp[(temp.IndexOf(' ') + 1)..]) - syllable.appearing;
                            // checking if switched to next voice
                            if (lastBeat > syllable.appearing)
                            {
                                currentVoice++;
                            }
                            lastBeat = syllable.appearing;
                            songData[currentVoice].Add(syllable);
                        }
                        needSpace = false;
                        break;
                    default:
                        break;
                }
            }
        }
        // Collect used voices
        foreach (int v in GameState.currentVoice)
        {
            if (v != -1 && voices.IndexOf(v) == -1)
            {
                voices.Insert(0, v);
            }
        }
        // Swap text
        if (GameState.currentGameMode == GameMode.Meow)
        {
            bool start_syllable = true;
            for (int i = 0; i < songData.Length; i++)
            {
                for (int j = 0; j < songData[i].Count; j++)
                {
                    if (songData[i][j].kind != Kind.LineBreak && songData[i][j].kind != Kind.LineBreakExcact)
                    {
                        if (songData[i][j].syllable.StartsWith(' '))
                        {
                            if (songData[i][j].syllable.EndsWith(' '))
                            {
                                songData[i][j].syllable = " Meow ";
                                start_syllable = true;
                            }
                            else
                            {
                                if (j + 1 == songData[i].Count || (j + 1 < songData[i].Count && (songData[i][j + 1].syllable.StartsWith(' ') || songData[i][j + 1].kind == Kind.LineBreak || songData[i][j + 1].kind == Kind.LineBreakExcact)))
                                {
                                    songData[i][j].syllable = " Meow";
                                    start_syllable = true;
                                }
                                else
                                {
                                    songData[i][j].syllable = " Me";
                                    start_syllable = false;
                                }
                            }
                        }
                        else
                        {
                            if (songData[i][j].syllable.EndsWith(' '))
                            {
                                if (start_syllable)
                                {
                                    songData[i][j].syllable = " Meow ";
                                }
                                else
                                {
                                    songData[i][j].syllable = "-ow ";
                                    start_syllable = true;
                                }
                            }
                            else
                            {
                                if (j + 1 == songData[i].Count || (j + 1 < songData[i].Count && (songData[i][j + 1].syllable.StartsWith(' ') || songData[i][j + 1].kind == Kind.LineBreak || songData[i][j + 1].kind == Kind.LineBreakExcact)))
                                {
                                    if (start_syllable)
                                    {
                                        songData[i][j].syllable = " Meow";
                                    }
                                    else
                                    {
                                        songData[i][j].syllable = "-ow";
                                        start_syllable = true;
                                    }
                                }
                                else
                                {
                                    if (start_syllable)
                                    {
                                        songData[i][j].syllable = " Me";
                                        start_syllable = false;
                                    }
                                    else
                                    {
                                        songData[i][j].syllable = "-ow";
                                        start_syllable = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // Set ui screen
        VisualElement r = GetComponent<UIDocument>().rootVisualElement;
        TemplateContainer root = new();
        switch (GameState.amountPlayer)
        {
            case 1:
                root = r.Q<TemplateContainer>("GameViewP1");
                root.visible = true;
                break;
            case 2:
                root = r.Q<TemplateContainer>("GameViewP2");
                root.visible = true;
                break;
            case 3:
                root = r.Q<TemplateContainer>("GameViewP3");
                root.visible = true;
                break;
            case 4:
                root = r.Q<TemplateContainer>("GameViewP4");
                root.visible = true;
                break;
            case 5:
                root = r.Q<TemplateContainer>("GameViewP5");
                root.visible = true;
                break;
            case 6:
                root = r.Q<TemplateContainer>("GameViewP6");
                root.visible = true;
                break;
        }
        // Get UI pointer
        whenToStartBottom = gameObject.transform.Find("WhenToSingBottom").gameObject;
        whenToStartBottomRectTransform = whenToStartBottom.GetComponent<RectTransform>();
        currentTimePointerBottom = gameObject.transform.Find("CurrentTimeBottom").GetComponent<RectTransform>();
        currentTimePointerTop = gameObject.transform.Find("CurrentTimeTop").GetComponent<RectTransform>();
        if (GameState.amountPlayer > 1 || voices.Count > 1)
        {
            gameObject.transform.Find("BackgroundTop").gameObject.SetActive(true);
            whenToStartTop = gameObject.transform.Find("WhenToSingTop").gameObject;
            whenToStartTop.SetActive(true);
            whenToStartTopRectTransform = whenToStartTop.GetComponent<RectTransform>();
            gameObject.transform.Find("TimeLineTop").gameObject.SetActive(true);
            gameObject.transform.Find("CurrentTimeTop").gameObject.SetActive(true);
        }
        // Get ui player data pointer 
        VisualElement[] roots = new VisualElement[GameState.amountPlayer];
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            roots[i] = root.Q<VisualElement>("PlayerNodeBoxP" + (i + 1).ToString());
            nodeBoxes[i] = roots[i].Q<VisualElement>("NodeBox");
            pointsTexts[i] = roots[i].Q<Label>("Points");
        }
        // Setting player name and get node arrows
        Color color;
        Label name;
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            name = roots[i].Q<Label>("Name");
            color = GameState.profiles[GameState.currentProfileIndex[i]].color;
            if (GameState.currentGameMode == GameMode.Together)
            {
                name.text = GameState.profiles[GameState.currentProfileIndex[i]].name + " and " + GameState.profiles[GameState.currentSecondProfileIndex[i]].name;
            }
            else
            {
                name.text = GameState.profiles[GameState.currentProfileIndex[i]].name;

            }
            roots[i].Q<VisualElement>("NameBox").style.unityBackgroundImageTintColor = new Color(color.r/255, color.g/255, color.b/255);
            roots[i].Q<VisualElement>("PointsBox").style.unityBackgroundImageTintColor = new Color(color.r / 255, color.g / 255, color.b / 255);
            // change name and points color depending on player color
            if (color.r * 0.299 + color.g * 0.587 + color.b * 0.114 > 186) 
            {
                name.style.color = new Color(0f, 0f, 0f);
                pointsTexts[i].style.color = new Color(0f, 0f, 0f);
            } else
            {
                name.style.color = new Color(1f, 1f, 1f);
                pointsTexts[i].style.color = new Color(1f, 1f, 1f);
            }
            nodeArrows[i] = roots[i].Q<VisualElement>("Node");
        }
        // Getting first song lines
        int textCounter;
        string text;        
        // Set first lines
        for (int i = 0; i < voices.Count; i++)
        {
            textCounter = 1;
            text = "";
            while (textCounter < 3)
            {
                if (songData[voices[i]][songDataNewLineIndex[voices[i]]].kind != Kind.LineBreak && songData[voices[i]][songDataNewLineIndex[voices[i]]].kind != Kind.LineBreakExcact)
                {
                    // Combininig text based on kind of syllable
                    switch (songData[voices[i]][songDataNewLineIndex[voices[i]]].kind)
                    {
                        case Kind.Normal:
                            text += songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable;
                            break;
                        case Kind.Free:
                            text += "<i>" + songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable + "</i>";
                            break;
                        case Kind.Golden:
                            text += colorGoldenToSing + songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable + "</color>";
                            break;
                    }
                    // Setting syllables of first line
                    if (textCounter == 1)
                    {
                        if (i == 0)
                        {
                            syllablesLine1Bottom.Add(songData[voices[i]][songDataNewLineIndex[voices[i]]]);
                        }
                        if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1)) 
                        {
                            syllablesLine1Top.Add(songData[voices[i]][songDataNewLineIndex[voices[i]]]);                             
                        }
                    }
                    // Setting syllables of second line
                    else if (textCounter == 2)
                    {
                        if (i == 0)
                        {
                            syllablesLine2Bottom.Add(songData[voices[i]][songDataNewLineIndex[voices[i]]]);
                        }
                        if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1))
                        {
                            syllablesLine2Top.Add(songData[voices[i]][songDataNewLineIndex[voices[i]]]);
                        }
                    }
                }
                else
                {
                    if (textCounter == 1)
                    {
                        // Setting beatEnd for node shower
                        endBeatLine[voices[i]] = songData[voices[i]][songDataNewLineIndex[voices[i]]].appearing;                        
                    }
                    else
                    {
                        if (i == 0)
                        {
                            textLine2Bottom = CreateSyllabel(text);
                            textLine2Bottom.gameObject.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, -700f, 0f);
                            textLine2Bottom.textMesh.ForceMeshUpdate();
                        }
                        if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1))
                        {
                            textLine2Top = CreateSyllabel(text);
                            textLine2Top.gameObject.transform.localPosition = new Vector3(500f - textLine2Top.textMesh.preferredWidth / 2, 275f, 0f);
                            textLine2Top.textMesh.ForceMeshUpdate();
                        }
                    }
                    text = "";
                    textCounter++;
                }
                songDataNewLineIndex[voices[i]]++;
            }
            beatSumLine[voices[i]] = endBeatLine[voices[i]] - startBeatLine[voices[i]];
        }
        int index;
        float currentPercent;
        int beatSum;
        VisualElement nodeBox;
        foreach (int v in voices)
        {
            index = 0;
            while (songData[v][index].kind != Kind.LineBreak && songData[v][index].kind != Kind.LineBreakExcact)
            {
                currentPercent = (songData[v][index].appearing * 100) / beatSumLine[v];
                for (int j = 0; j < GameState.amountPlayer; j++)
                {
                    if (GameState.currentVoice[j] == v)
                    {
                        nodeBox = new VisualElement();
                        nodeBox.AddToClassList("nodeBox");
                        nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[v][index].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                        // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
                        nodeBox.style.left = Length.Percent(currentPercent);
                        nodeBox.style.width = Length.Percent((songData[v][index].appearing + songData[v][index].length) * 100 / beatSumLine[v] - currentPercent);
                        nodeBoxes[j].Add(nodeBox);
                    } 
                }
                index++;
            }
            // calculating points per beat
            beatSum = 0;
            // getting sum of beats (golden notes double)
            for (index = 0; index < songData[v].Count; index++)
            {
                if (songData[v][index].kind != Kind.LineBreak && songData[v][index].kind != Kind.LineBreakExcact)
                {
                    // handling different nodes 
                    switch (songData[v][index].kind)
                    {
                        case Kind.Normal:
                            beatSum += songData[v][index].length;
                            break;
                        case Kind.Golden:
                            beatSum += songData[v][index].length * 2;
                            break;
                        default:
                            break;
                    }
                }
            }
            pointsPerBeat[v] = 10000f / beatSum;
        }
        // set song player
        if (GameState.currentSong.pathToMusic != "" && GameState.currentSong.pathToMusic != GameState.currentSong.pathToVideo)
        {
            // using audiofile for sound
            GameObject camera = GameObject.Find("MainCamera");
            AudioSource audio = camera.AddComponent<AudioSource>();
            UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + GameState.currentSong.pathToMusic, AudioType.MPEG);
            req.SendWebRequest();
            while (!req.isDone)
            {
                Thread.Sleep(100);
            }
            try
            {
                audio.clip = DownloadHandlerAudioClip.GetContent(req);
            }
            catch (Exception)
            {
                StreamWriter file = new("errors.log", true);
                file.WriteLine("Error getting audio from " + GameState.currentSong.pathToMusic + "; Maybe the path to the mp3 or video is not found or the txt file isn't written in utf-8!");
                file.Close();
                SceneManager.LoadScene("MainMenu");
            }
            songPlayer = new SongPlayer(audio);
            // play audio and video 
            audio.Play();
            if (GameState.currentSong.pathToVideo != "")
            {
                video.videoPlayer.SetDirectAudioMute(0, true);
                video.videoPlayer.Play();
            }
            songLength = songPlayer.GetLength();
        }
        else
        {
            // using video for sound
            if (video.videoPlayer != null)
            {
                songPlayer = new SongPlayer(video.videoPlayer);
                video.videoPlayer.Play();
                video.videoPlayer.prepareCompleted += (eventVideoPlayer) =>
                {
                    songLength = songPlayer.GetLength();
                };
            }
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            if (DateTime.Now.Subtract(lastTimePressed).TotalMilliseconds > 500)
            {
                if(isPaused)
                {
                    isPaused = false;
                    video.Unpause();
                    songPlayer.Unpause();
                } else
                {
                    isPaused = true;
                    video.Pause();
                    songPlayer.Pause();
                }
                lastTimePressed = DateTime.Now;
            }
        }
        if (!timeLineSet) {
            // set nodes for timeline of first voice
            if (songPlayer.IsPrepared())
            {
                GameObject currentGameObject;
                RectTransform currentRectTransform;
                UnityEngine.UI.Image currentImage;
                double beatInSec;
                double beatPercentStart;
                double beatPercentEnd;
                double gapTimeInBeats = (GameState.currentSong.gap / 60.0) * 4.0 * GameState.currentSong.bpm;
                for (int i = 0; i < voices.Count; i++)
                {
                    foreach (SyllableData s in songData[voices[i]])
                    {
                        if (s.kind != Kind.LineBreak && s.kind != Kind.LineBreakExcact)
                        {
                            currentGameObject = new GameObject("TimeLineObject");
                            currentGameObject.transform.parent = gameObject.transform;
                            // set up rect transform
                            currentRectTransform = currentGameObject.AddComponent<RectTransform>();
                            beatInSec = (15 * (s.appearing + gapTimeInBeats)) / GameState.currentSong.bpm;
                            beatPercentStart = (beatInSec * 100.0) / songLength;
                            if (i == 0) 
                            {
                                currentRectTransform.anchoredPosition = new Vector3((float)(15.0 + (1895.0 * beatPercentStart) / 100.0), -317.0f, 0f);
                            }
                            if (i > 0)
                            {
                                currentRectTransform.anchoredPosition = new Vector3((float)(15.0 + (1895.0 * beatPercentStart) / 100.0), 317.0f, 0f);
                            }
                            beatPercentEnd = ((15 * (s.appearing + s.length + gapTimeInBeats)) / GameState.currentSong.bpm) * 100 / songLength;
                            currentRectTransform.sizeDelta = new Vector2((float)(15.0 + (1895.0 * beatPercentEnd) / 100.0 - currentRectTransform.anchoredPosition.x + 2.5), 10f);
                            currentRectTransform.pivot = new Vector2(0, 0.5f);
                            // set anchor to middle left
                            currentRectTransform.anchorMin = new Vector2(0, 0.5f);
                            currentRectTransform.anchorMax = new Vector2(0, 0.5f);
                            // set up image
                            currentImage = currentGameObject.AddComponent<UnityEngine.UI.Image>();
                            switch (s.kind)
                            {
                                case Kind.Free:
                                    currentImage.color = Color.gray;
                                    break;
                                case Kind.Normal:
                                    currentImage.color = Color.blue;
                                    break;
                                case Kind.Golden:
                                    currentImage.color = Color.yellow;
                                    break;
                            }
                            if (voices.Count == 1 && GameState.amountPlayer > 1)
                            {
                                currentGameObject = Instantiate(currentGameObject);
                                currentGameObject.transform.SetParent(gameObject.transform);
                                currentGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3((float)(15.0 + (1895.0 * beatPercentStart) / 100.0), 317.0f, 0f);
                            }
                        }
                    }
                }
                currentTimePointerBottom.SetAsLastSibling();
                currentTimePointerTop.SetAsLastSibling();
                timeLineSet = true;
            } else
            {
                return;
            }
        }
        if (!isPaused)
        {
            // if song not ended
            if (!songPlayer.HasFinished())
            {
                if (songPlayer.GetTime() > 0)
                {
                    songPlayer.started = true;
                }
                double songPercent = (songPlayer.GetTime() * 100.0) / songLength;
                // update timeline
                currentTimePointerBottom.anchoredPosition = new Vector3((float)(10.0 + (1895.0 * songPercent) / 100.0), -317.0f, 0f);
                currentTimePointerTop.anchoredPosition = new Vector3((float)(10.0 + (1895.0 * songPercent) / 100.0), 317.0f, 0f);
                // calculate sing time
                double currentTime = songPlayer.GetTime() - GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap;
                // calculating current beat: Beatnumber = (Time in sec / 60 sec) * 4 * BPM
                int currentTimeStamp = (int)Math.Ceiling((currentTime / 60.0) * 4.0 * GameState.currentSong.bpm);
                // updating nodes, songtext and calculating score
                VisualElement nodeBox;
                float currentPercent;
                string text;
                string textToSing;
                string textCurrentSing;
                string textSung;
                bool currentIsGolden;
                for (int i = 0; i < voices.Count; i++)
                {
                    if (songDataCurrentIndex[voices[i]] < songData[voices[i]].Count)
                    {
                        // Updating player node arrow:
                        Length leftArrowPercent = Length.Percent(((currentTimeStamp - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]] - nodeArrowWidth);
                        for (int j = 0; j < GameState.amountPlayer; j++)
                        {
                            if (GameState.currentVoice[j] == voices[i])
                            {
                                if (leftArrowPercent.value < 0)
                                {
                                    leftArrowPercent = Length.Percent(0);
                                }
                                nodeArrows[j].style.left = leftArrowPercent;
                            }
                        }
                        if (songData[voices[i]][songDataCurrentIndex[voices[i]]].kind != Kind.LineBreak && songData[voices[i]][songDataCurrentIndex[voices[i]]].kind != Kind.LineBreakExcact)
                        {
                            if (i == 0)
                            {
                                // reset song text
                                text = "";
                                textToSing = "";
                                textCurrentSing = "";
                                textSung = "";
                                currentIsGolden = false;
                                foreach (TextObject currObject in textLine1Bottom)
                                {
                                    Destroy(currObject.gameObject, 0.0f);
                                }
                                textLine1Bottom.Clear();
                                string test = "";
                                foreach (SyllableData t in syllablesLine1Bottom)
                                {
                                    test += t.syllable;
                                }
                                // Making syllable colored
                                foreach (SyllableData s in syllablesLine1Bottom)
                                {
                                    // if alredy sung
                                    if (s.appearing < songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)
                                    {
                                        switch (s.kind)
                                        {
                                            case Kind.Normal:
                                                text += colorSung + s.syllable + "</color>";
                                                textSung += colorSung + s.syllable + "</color>";
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + colorSung + s.syllable + "</color></i>";
                                                textSung += "<i>" + colorSung + s.syllable + "</color></i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenSung + s.syllable + "</color>";
                                                textSung += colorGoldenSung + s.syllable + "</color>";
                                                break;
                                        }
                                    }
                                    // if has to sing
                                    else if (s.appearing > songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)
                                    {
                                        switch (s.kind)
                                        {
                                            case Kind.Normal:
                                                text += s.syllable;
                                                textToSing += s.syllable;
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + s.syllable + "</i>";
                                                textToSing += "<i>" + s.syllable + "</i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenToSing + s.syllable + "</color>";
                                                textToSing += colorGoldenToSing + s.syllable + "</color>";
                                                break;
                                        }
                                    }
                                    // current node
                                    else
                                    {
                                        switch (s.kind)
                                        {
                                            case Kind.Normal:
                                                text += s.syllable;
                                                textCurrentSing += s.syllable;
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + s.syllable + "</i>";
                                                textCurrentSing += "<i>" + s.syllable + "</i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenToSing + s.syllable + "</color>";
                                                textCurrentSing += s.syllable;
                                                currentIsGolden = true;
                                                break;
                                        }
                                    }
                                }
                                // render text
                                if (textSung != "")
                                {
                                    CreateSyllabelToList(textLine1Bottom, textSung);
                                }
                                if (textCurrentSing != "")
                                {
                                    float currentSyllablePercent = ((float)(currentTimeStamp - songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)) / songData[voices[i]][songDataCurrentIndex[voices[i]]].length;
                                    if (currentSyllablePercent < 1f)
                                    {
                                        CreateCurrentSyllabel(textLine1Bottom, textCurrentSing, currentIsGolden, currentSyllablePercent);
                                    }
                                    else
                                    {
                                        if (currentIsGolden)
                                        {
                                            CreateSyllabelToList(textLine1Bottom, colorGoldenSung + textCurrentSing + "</color>");
                                        }
                                        else
                                        {
                                            CreateSyllabelToList(textLine1Bottom, colorSung + textCurrentSing + "</color>");
                                        }
                                    }
                                }
                                if (textToSing != "")
                                {
                                    CreateSyllabelToList(textLine1Bottom, textToSing);
                                }
                                // calculate needed width
                                float renderedWidth = 0;
                                foreach (TextObject to in textLine1Bottom)
                                {
                                    if (!to.isSecondHalf)
                                    {
                                        renderedWidth += to.textMesh.preferredWidth;
                                    }
                                }
                                // set position of text elements
                                textLine1Bottom[0].gameObject.transform.localPosition = new Vector3(500f - renderedWidth / 2, -600f, 0f);
                                TextObject beforeObject = textLine1Bottom[0];
                                beforeObject.textMesh.ForceMeshUpdate();
                                foreach (TextObject to in textLine1Bottom.Skip(1))
                                {
                                    if (to.isSecondHalf)
                                    {
                                        to.gameObject.transform.localPosition = new Vector3(beforeObject.gameObject.transform.localPosition.x, beforeObject.gameObject.transform.localPosition.y, beforeObject.gameObject.transform.localPosition.z);
                                    }
                                    else
                                    {
                                        to.gameObject.transform.localPosition = new Vector3(beforeObject.gameObject.transform.localPosition.x + beforeObject.textMesh.preferredWidth, beforeObject.gameObject.transform.localPosition.y, beforeObject.gameObject.transform.localPosition.z);
                                    }
                                    to.textMesh.ForceMeshUpdate();
                                    beforeObject = to;
                                }
                            }
                            if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1))
                            {
                                foreach (TextObject currObject in textLine1Top)
                                {
                                    Destroy(currObject.gameObject, 0.0f);
                                }
                                textLine1Top.Clear();
                                // reset song text
                                text = "";
                                textToSing = "";
                                textCurrentSing = "";
                                textSung = "";
                                currentIsGolden = false;
                                // Making syllable colored
                                foreach (SyllableData s in syllablesLine1Top)
                                {
                                    // if alredy sung
                                    if (s.appearing < songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)
                                    {
                                        switch (s.kind)
                                        {
                                            case Kind.Normal:
                                                text += colorSung + s.syllable + "</color>";
                                                textSung += colorSung + s.syllable + "</color>";
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + colorSung + s.syllable + "</color></i>";
                                                textSung += "<i>" + colorSung + s.syllable + "</color></i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenSung + s.syllable + "</color>";
                                                textSung += colorGoldenSung + s.syllable + "</color>";
                                                break;
                                        }
                                    }
                                    // if has to sing
                                    else if (s.appearing > songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)
                                    {
                                        switch (s.kind)
                                        {
                                            case Kind.Normal:
                                                text += s.syllable;
                                                textToSing += s.syllable;
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + s.syllable + "</i>";
                                                textToSing += "<i>" + s.syllable + "</i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenToSing + s.syllable + "</color>";
                                                textToSing += colorGoldenToSing + s.syllable + "</color>";
                                                break;
                                        }
                                    }
                                    // current node
                                    else
                                    {
                                        switch (s.kind)
                                        {
                                            case Kind.Normal:
                                                text += s.syllable;
                                                textCurrentSing += s.syllable;
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + s.syllable + "</i>";
                                                textCurrentSing += "<i>" + s.syllable + "</i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenToSing + s.syllable + "</color>";
                                                textCurrentSing += s.syllable;
                                                currentIsGolden = true;
                                                break;
                                        }
                                    }
                                }
                                // render text                                
                                if (textSung != "")
                                {
                                    CreateSyllabelToList(textLine1Top, textSung);
                                }
                                if (textCurrentSing != "")
                                {
                                    float currentSyllablePercent = ((float)(currentTimeStamp - songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)) / songData[voices[i]][songDataCurrentIndex[voices[i]]].length;
                                    if (currentSyllablePercent < 1f)
                                    {
                                        CreateCurrentSyllabel(textLine1Top, textCurrentSing, currentIsGolden, currentSyllablePercent);
                                    }
                                    else
                                    {
                                        if (currentIsGolden)
                                        {
                                            CreateSyllabelToList(textLine1Top, colorGoldenSung + textCurrentSing + "</color>");
                                        }
                                        else
                                        {
                                            CreateSyllabelToList(textLine1Top, colorSung + textCurrentSing + "</color>");
                                        }
                                    }
                                }
                                if (textToSing != "")
                                {
                                    CreateSyllabelToList(textLine1Top, textToSing);
                                }
                                // calculate needed width
                                float renderedWidth = 0;
                                foreach (TextObject to in textLine1Top)
                                {
                                    if (!to.isSecondHalf)
                                    {
                                        renderedWidth += to.textMesh.preferredWidth;
                                    }
                                }
                                // set position of text elements
                                textLine1Top[0].gameObject.transform.localPosition = new Vector3(500f - renderedWidth / 2, 175f, 0f);
                                TextObject beforeObject = textLine1Top[0];
                                beforeObject.textMesh.ForceMeshUpdate();
                                foreach (TextObject to in textLine1Top.Skip(1))
                                {
                                    if (to.isSecondHalf)
                                    {
                                        to.gameObject.transform.localPosition = new Vector3(beforeObject.gameObject.transform.localPosition.x, beforeObject.gameObject.transform.localPosition.y, beforeObject.gameObject.transform.localPosition.z);
                                    }
                                    else
                                    {
                                        to.gameObject.transform.localPosition = new Vector3(beforeObject.gameObject.transform.localPosition.x + beforeObject.textMesh.preferredWidth, beforeObject.gameObject.transform.localPosition.y, beforeObject.gameObject.transform.localPosition.z);
                                    }
                                    to.textMesh.ForceMeshUpdate();
                                    beforeObject = to;
                                }
                            }                           
                            // Time in sec = Beatnumber / BPM / 4 * 60 sec
                            if (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing / GameState.currentSong.bpm / 4 * 60 <= currentTime && (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing + songData[voices[i]][songDataCurrentIndex[voices[i]]].length) / GameState.currentSong.bpm / 4 * 60 >= currentTime)
                            {
                                Color color;
                                // calculating score and updating UI
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (GameState.currentVoice[j] == voices[i])
                                    {
                                        if (currentTimeStamp != lastTimeStamps[j])
                                        {
                                            if (GameState.currentGameMode == GameMode.Together)
                                            {
                                                if (microphoneInput.nodes[j] != Node.None && microphoneInput.nodes[j + GameState.amountPlayer] != Node.None && HitNode(MiddleNode(microphoneInput.nodes[j], microphoneInput.nodes[j + GameState.amountPlayer]), songData[voices[i]][songDataCurrentIndex[voices[i]]].node, GameState.profiles[GameState.currentSecondProfileIndex[j]]))
                                                {
                                                    // creating new node box
                                                    currentPercent = ((currentTimeStamp - 1 - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]];
                                                    nodeBox = new VisualElement();
                                                    nodeBox.AddToClassList("nodeBox");
                                                    nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)MiddleNode(microphoneInput.nodes[j], microphoneInput.nodes[j + GameState.amountPlayer])) * 100) / nodeTextureHeight - nodeHeightOffset);
                                                    nodeBox.style.left = Length.Percent(currentPercent);
                                                    nodeBox.style.width = Length.Percent((((currentTimeStamp - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]]) - currentPercent);
                                                    color = new Color(GameState.profiles[GameState.currentProfileIndex[j]].color.r / 255f, GameState.profiles[GameState.currentProfileIndex[j]].color.g / 255f, GameState.profiles[GameState.currentProfileIndex[j]].color.b / 255f);
                                                    // updating score and setting node box color
                                                    switch (songData[voices[i]][songDataCurrentIndex[voices[i]]].kind)
                                                    {
                                                        case Kind.Normal:
                                                            points[j] += pointsPerBeat[voices[i]];
                                                            nodeBox.style.unityBackgroundImageTintColor = color;
                                                            break;
                                                        case Kind.Golden:
                                                            points[j] += pointsPerBeat[voices[i]] * 2;
                                                            nodeBox.style.unityBackgroundImageTintColor = new Color(1f - color.r, 1f - color.g, 1f - color.b);
                                                            break;
                                                        case Kind.Free:
                                                            nodeBox.style.unityBackgroundImageTintColor = new Color(color.r, color.g, color.b, 0.5f);
                                                            break;
                                                    }
                                                    // updating ui elements
                                                    pointsTexts[j].text = ((int)System.Math.Ceiling(points[j])).ToString();
                                                    nodeBoxes[j].Add(nodeBox);
                                                    // set actual beat as handled
                                                    lastTimeStamps[j] = currentTimeStamp;
                                                }
                                            }
                                            else
                                            {
                                                if (microphoneInput.nodes[j] != Node.None && HitNode(microphoneInput.nodes[j], songData[voices[i]][songDataCurrentIndex[voices[i]]].node, GameState.profiles[GameState.currentProfileIndex[j]]))
                                                {
                                                    // creating new node box
                                                    currentPercent = ((currentTimeStamp - 1 - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]];
                                                    nodeBox = new VisualElement();
                                                    nodeBox.AddToClassList("nodeBox");
                                                    nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[j]) * 100) / nodeTextureHeight - nodeHeightOffset);
                                                    nodeBox.style.left = Length.Percent(currentPercent);
                                                    nodeBox.style.width = Length.Percent((((currentTimeStamp - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]]) - currentPercent);
                                                    color = new Color(GameState.profiles[GameState.currentProfileIndex[j]].color.r / 255f, GameState.profiles[GameState.currentProfileIndex[j]].color.g / 255f, GameState.profiles[GameState.currentProfileIndex[j]].color.b / 255f);
                                                    // updating score and setting node box color
                                                    switch (songData[voices[i]][songDataCurrentIndex[voices[i]]].kind)
                                                    {
                                                        case Kind.Normal:
                                                            points[j] += pointsPerBeat[voices[i]];
                                                            nodeBox.style.unityBackgroundImageTintColor = color;
                                                            break;
                                                        case Kind.Golden:
                                                            points[j] += pointsPerBeat[voices[i]] * 2;
                                                            nodeBox.style.unityBackgroundImageTintColor = new Color(1f - color.r, 1f - color.g, 1f - color.b);
                                                            break;
                                                        case Kind.Free:
                                                            nodeBox.style.unityBackgroundImageTintColor = new Color(color.r, color.g, color.b, 0.5f);
                                                            break;
                                                    }
                                                    // updating ui elements
                                                    pointsTexts[j].text = ((int)System.Math.Ceiling(points[j])).ToString();
                                                    nodeBoxes[j].Add(nodeBox);
                                                    // set actual beat as handled
                                                    lastTimeStamps[j] = currentTimeStamp;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // time with no nodes
                            else
                            {
                                if (currentTime > (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing + songData[voices[i]][songDataCurrentIndex[voices[i]]].length) / GameState.currentSong.bpm / 4 * 60)
                                {
                                    songDataCurrentIndex[voices[i]]++;
                                }
                            }
                            // set up time to start node shower
                            if (currentTimeStamp < startBeatLine[voices[i]])
                            {
                                if (i == 0)
                                {
                                    if (textLine1Bottom.Count > 0)
                                    {
                                        // 500 = textLine1Bottom[0] width / 2
                                        float startX = -945f;
                                        float endX = textLine1Bottom[0].gameObject.transform.localPosition.x - 500f;
                                        double startBeat;
                                        if (songDataCurrentIndex[voices[i]] > 0)
                                        {
                                            startBeat = songData[voices[i]][songDataCurrentIndex[voices[i]] - 1].appearing;
                                        }
                                        // song with start gap
                                        else
                                        {
                                            startBeat = ((-GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap) / 60.0) * 4.0 * GameState.currentSong.bpm;
                                        }
                                        double percent = 100 - ((songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing - currentTimeStamp) * 100) / (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing - startBeat);
                                        double posX = startX + ((endX - startX) * (percent)) / 100;
                                        whenToStartBottomRectTransform.sizeDelta = new Vector2(10f, 100f);
                                        whenToStartBottom.transform.localPosition = new Vector3((float)posX, -375f, 0f);
                                        if (GameState.amountPlayer > 1)
                                        {
                                            if (GameState.currentSong.amountVoices == 1)
                                            {
                                                whenToStartTopRectTransform.sizeDelta = new Vector2(10f, 100f);
                                                whenToStartTop.transform.localPosition = new Vector3((float)posX, 375f, 0f);
                                            }
                                        }
                                    }
                                }
                                if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1))
                                {
                                    if (textLine1Top.Count > 0)
                                    {
                                        // 500 = textLine1Bottom[0] width / 2
                                        float startX = -945f;
                                        float endX = textLine1Top[0].gameObject.transform.localPosition.x - 500f;
                                        double startBeat;
                                        if (songDataCurrentIndex[voices[i]] > 0)
                                        {
                                            startBeat = songData[voices[i]][songDataCurrentIndex[voices[i]] - 1].appearing;
                                        }
                                        // song with start gap
                                        else
                                        {
                                            startBeat = ((-GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap) / 60.0) * 4.0 * GameState.currentSong.bpm;
                                        }
                                        double percent = 100 - ((songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing - currentTimeStamp) * 100) / (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing - startBeat);
                                        double posX = startX + ((endX - startX) * (percent)) / 100;
                                        whenToStartTopRectTransform.sizeDelta = new Vector2(10f, 100f);
                                        whenToStartTop.transform.localPosition = new Vector3((float)posX, 375f, 0f);
                                    }
                                }
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    whenToStartBottomRectTransform.sizeDelta = new Vector2(0f, 100f);
                                    if (GameState.amountPlayer > 1 && GameState.currentSong.amountVoices == 1)
                                    {
                                        whenToStartTopRectTransform.sizeDelta = new Vector2(0f, 100f);
                                    }
                                }
                                if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1))
                                {
                                    whenToStartTopRectTransform.sizeDelta = new Vector2(0f, 100f);
                                }
                            }
                        }
                        else
                        {
                            if (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing > currentTimeStamp)
                            {
                                return;
                            }
                            int nodesNewLineIndex;
                            if (i == 0)
                            {
                                syllablesLine1Bottom = syllablesLine2Bottom;
                                if (voices.Count == 1 && GameState.amountPlayer > 1)
                                {
                                    // updating for more player and same voices
                                    syllablesLine1Top = syllablesLine1Bottom;
                                }
                                // Calculating next line data
                                syllablesLine2Bottom = new();
                                nodesNewLineIndex = songDataNewLineIndex[voices[i]];
                                if (songDataNewLineIndex[voices[i]] < songData[voices[i]].Count)
                                {
                                    text = "";
                                    while (songDataNewLineIndex[voices[i]] < songData[voices[i]].Count && songData[voices[i]][songDataNewLineIndex[voices[i]]].kind != Kind.LineBreak && songData[voices[i]][songDataNewLineIndex[voices[i]]].kind != Kind.LineBreakExcact)
                                    {
                                        // adding text based on kind of syllable
                                        switch (songData[voices[i]][songDataNewLineIndex[voices[i]]].kind)
                                        {
                                            case Kind.Normal:
                                                text += songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable;
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable + "</i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenToSing + songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable + "</color>";
                                                break;
                                        }
                                        syllablesLine2Bottom.Add(songData[voices[i]][songDataNewLineIndex[voices[i]]]);
                                        songDataNewLineIndex[voices[i]]++;
                                    }
                                    Destroy(textLine2Bottom.gameObject);
                                    textLine2Bottom = CreateSyllabel(text);
                                    textLine2Bottom.gameObject.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, -700f, 0f);
                                    textLine2Bottom.textMesh.ForceMeshUpdate();
                                    if (voices.Count == 1 && GameState.amountPlayer > 1)
                                    {
                                        // updating for more player and same voices
                                        syllablesLine2Top = syllablesLine2Bottom;
                                        Destroy(textLine2Top.gameObject);
                                        textLine2Top = CreateSyllabel(text);
                                        textLine2Top.gameObject.transform.localPosition = new Vector3(500f - textLine2Top.textMesh.preferredWidth / 2, 275f, 0f);
                                        textLine2Top.textMesh.ForceMeshUpdate();
                                    }
                                    endBeatLine[voices[i]] = songData[voices[i]][nodesNewLineIndex - 1].appearing;
                                    songDataNewLineIndex[voices[i]]++;
                                }
                                else
                                {
                                    textLine2Bottom.textMesh.text = "";
                                    textLine2Bottom.textMesh.ForceMeshUpdate();
                                    if (voices.Count == 1 && GameState.amountPlayer > 1)
                                    {
                                        textLine2Top.textMesh.text = "";
                                        textLine2Top.textMesh.ForceMeshUpdate();
                                    }
                                    endBeatLine[voices[i]] = songData[voices[i]][nodesNewLineIndex - 2].appearing + songData[voices[i]][nodesNewLineIndex - 2].length;
                                }
                            }
                            if (i > 0)
                            {
                                syllablesLine1Top = syllablesLine2Top;
                                // Calculating next line data
                                syllablesLine2Top = new();
                                nodesNewLineIndex = songDataNewLineIndex[voices[i]];
                                if (songDataNewLineIndex[voices[i]] < songData[voices[i]].Count)
                                {
                                    text = "";
                                    while (songDataNewLineIndex[voices[i]] < songData[voices[i]].Count && songData[voices[i]][songDataNewLineIndex[voices[i]]].kind != Kind.LineBreak && songData[voices[i]][songDataNewLineIndex[voices[i]]].kind != Kind.LineBreakExcact)
                                    {
                                        // adding text based on kind of syllable
                                        switch (songData[voices[i]][songDataNewLineIndex[voices[i]]].kind)
                                        {
                                            case Kind.Normal:
                                                text += songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable;
                                                break;
                                            case Kind.Free:
                                                text += "<i>" + songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable + "</i>";
                                                break;
                                            case Kind.Golden:
                                                text += colorGoldenToSing + songData[voices[i]][songDataNewLineIndex[voices[i]]].syllable + "</color>";
                                                break;
                                        }
                                        syllablesLine2Top.Add(songData[voices[i]][songDataNewLineIndex[voices[i]]]);
                                        songDataNewLineIndex[voices[i]]++;
                                    }
                                    Destroy(textLine2Top.gameObject);
                                    textLine2Top = CreateSyllabel(text);
                                    textLine2Top.gameObject.transform.localPosition = new Vector3(500f - textLine2Top.textMesh.preferredWidth / 2, 275f, 0f);
                                    textLine2Top.textMesh.ForceMeshUpdate();
                                    endBeatLine[voices[i]] = songData[voices[i]][nodesNewLineIndex - 1].appearing;
                                    songDataNewLineIndex[voices[i]]++;
                                }
                                else
                                {
                                    textLine2Top.textMesh.text = "";
                                    textLine2Top.textMesh.ForceMeshUpdate();
                                    endBeatLine[voices[i]] = songData[voices[i]][nodesNewLineIndex - 2].appearing + songData[voices[i]][nodesNewLineIndex - 2].length;
                                }
                            }
                            // calculating beat data
                            if (songData[voices[i]][songDataCurrentIndex[voices[i]]].kind == Kind.LineBreak)
                            {
                                startBeatLine[voices[i]] = songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing;
                            }
                            // must be kind LineBreakExcact
                            else
                            {
                                startBeatLine[voices[i]] = songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing + songData[voices[i]][songDataCurrentIndex[voices[i]]].length;
                            }
                            beatSumLine[voices[i]] = endBeatLine[voices[i]] - startBeatLine[voices[i]];
                            nodesNewLineIndex = songDataCurrentIndex[voices[i]] + 1;
                            // calculating node line data
                            for (int j = 0; j < GameState.amountPlayer; j++)
                            {
                                if (GameState.currentVoice[j] == voices[i])
                                {
                                    nodeBoxes[j].Clear();
                                }
                            }
                            while (nodesNewLineIndex < songData[voices[i]].Count && songData[voices[i]][nodesNewLineIndex].kind != Kind.LineBreak && songData[voices[i]][nodesNewLineIndex].kind != Kind.LineBreakExcact)
                            {
                                currentPercent = ((songData[voices[i]][nodesNewLineIndex].appearing - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]];
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (GameState.currentVoice[j] == voices[i])
                                    {
                                        nodeBox = new VisualElement();
                                        nodeBox.AddToClassList("nodeBox");
                                        nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[voices[i]][nodesNewLineIndex].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                        nodeBox.style.left = Length.Percent(currentPercent);
                                        nodeBox.style.width = Length.Percent(((songData[voices[i]][nodesNewLineIndex].appearing + songData[voices[i]][nodesNewLineIndex].length - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]] - currentPercent);
                                        nodeBoxes[j].Add(nodeBox);
                                    }
                                }
                                nodesNewLineIndex++;
                            }
                            songDataCurrentIndex[voices[i]]++;
                        }
                    }
                    else
                    {
                        // reset player node arrow to start
                        for (int j = 0; j < GameState.amountPlayer; j++)
                        {
                            if (GameState.currentVoice[j] == voices[i])
                            {
                                nodeArrows[j].style.left = 0;
                            }
                        }
                    }
                    // Updating player node arrows
                    for (int j = 0; j < GameState.amountPlayer; j++)
                    {
                        if (GameState.currentGameMode == GameMode.Together)
                        {
                            if (microphoneInput.nodes[j] != Node.None && microphoneInput.nodes[j + GameState.amountPlayer] != Node.None)
                            {
                                nodeArrows[j].style.top = Length.Percent(((nodeTextureDistance * (int)MiddleNode(microphoneInput.nodes[j], microphoneInput.nodes[j + GameState.amountPlayer])) * 100) / nodeTextureHeight - nodeHeightOffset);
                            }
                            else
                            {
                                nodeArrows[j].style.top = Length.Percent(((nodeTextureDistance * 13) * 100) / nodeTextureHeight - nodeHeightOffset);
                            }
                        }
                        else
                        {

                            if (microphoneInput.nodes[j] != Node.None)
                            {
                                nodeArrows[j].style.top = Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[j]) * 100) / nodeTextureHeight - nodeHeightOffset);
                            }
                            else
                            {
                                nodeArrows[j].style.top = Length.Percent(((nodeTextureDistance * 13) * 100) / nodeTextureHeight - nodeHeightOffset);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    GameState.profiles[GameState.currentProfileIndex[i]].points = (int)Math.Ceiling(points[i]);
                }
                SceneManager.LoadScene("SongEnd");
            }
        }
    }

    private TextObject CreateSyllabel(string text)
    {
        // create object
        GameObject currentObject = new("TM");
        currentObject.transform.SetParent(gameObject.transform);
        // set up text mesh
        TextMeshProUGUI currentObjectTM = currentObject.AddComponent<TextMeshProUGUI>();
        currentObjectTM.text = text;
        currentObjectTM.rectTransform.sizeDelta = sizeDelta;
        currentObjectTM.fontSize = 50;
        currentObjectTM.textWrappingMode = TextWrappingModes.NoWrap;
        currentObjectTM.ForceMeshUpdate();
        return new TextObject(currentObject, currentObjectTM, false);
    }

    private void CreateSyllabelToList(List<TextObject> objects, string text)
    {
        // create object
        GameObject currentObject = new("TM");
        currentObject.transform.SetParent(gameObject.transform);
        // set up text mesh
        TextMeshProUGUI currentObjectTM = currentObject.AddComponent<TextMeshProUGUI>();
        currentObjectTM.text = text;
        currentObjectTM.rectTransform.sizeDelta = sizeDelta;
        currentObjectTM.fontSize = 60;
        currentObjectTM.textWrappingMode = TextWrappingModes.NoWrap;
        currentObjectTM.ForceMeshUpdate();
        objects.Add(new TextObject(currentObject, currentObjectTM, false));
    }

    private void CreateCurrentSyllabel(List<TextObject> objects, string text, bool isGolden, float currentPercantage)
    {
        GameObject objectLeft = new("Mask");
        objectLeft.transform.SetParent(gameObject.transform);
        RectMask2D maskLeft = objectLeft.AddComponent<RectMask2D>();
        maskLeft.rectTransform.sizeDelta = sizeDelta;
        GameObject subObjectLeft = new("TM");
        subObjectLeft.transform.SetParent(objectLeft.transform);
        TextMeshProUGUI wordLeft = subObjectLeft.AddComponent<TextMeshProUGUI>();
        if (isGolden)
        {
            wordLeft.text = colorGoldenToSing + text;
        } else
        {
            wordLeft.text = text;
        }
        wordLeft.fontSize = 60;
        wordLeft.textWrappingMode = TextWrappingModes.NoWrap;
        wordLeft.transform.localPosition = new Vector3(0f, 0f, 0f);
        wordLeft.rectTransform.sizeDelta = sizeDelta;
        wordLeft.ForceMeshUpdate();
        maskLeft.padding = new Vector4((wordLeft.preferredWidth * currentPercantage), 0f);
        objects.Add(new TextObject(objectLeft, wordLeft, false));
        GameObject objectRight = new("Mask");
        objectRight.transform.SetParent(gameObject.transform);
        RectMask2D maskRight = objectRight.AddComponent<RectMask2D>();
        maskRight.rectTransform.sizeDelta = sizeDelta;
        GameObject subObjectRight = new("TM");
        subObjectRight.transform.SetParent(objectRight.transform);
        TextMeshProUGUI wordRight = subObjectRight.AddComponent<TextMeshProUGUI>();
        // change space with non breaking space at end for tm
        if (isGolden)
        {
            wordRight.text = colorGoldenSung + text;
        }
        else
        {
            wordRight.text = colorSung + text;
        }
        wordRight.fontSize = 60;
        wordRight.textWrappingMode = TextWrappingModes.NoWrap;
        wordRight.transform.localPosition = new Vector3(0f, 0f, 0f);
        wordRight.rectTransform.sizeDelta = sizeDelta;
        wordRight.ForceMeshUpdate();
        maskRight.padding = new Vector4(0f, 0f, sizeDelta.x - wordLeft.preferredWidth + (wordLeft.preferredWidth * (1f - currentPercantage))); 
        objects.Add(new TextObject(objectRight, wordRight, true));
    }

    // checks if sung node hits reference node
    private bool HitNode(Node sung, Node toHit, PlayerProfile singer)
    {
        // get distance between node enums
        int distance;
        if (sung > toHit)
        {
            distance = (int)sung - (int)toHit;
        }
        else
        {
            distance = (int)toHit - (int)sung;
        }
        // check if hit
        if (distance <= (int)singer.difficulty || distance >= 12 - (int)singer.difficulty)
        {
            return true;
        }
        return false;
    }

    private Node MiddleNode(Node first, Node second)
    {
        return middle_nodes[(int)first, (int)second];
    }
}