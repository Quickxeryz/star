using Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameLogic : MonoBehaviour
{
    enum Kind
    {
        Free,
        Normal,
        Golden,
        LineBreak,
        LineBreakExcact,
        Item
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

        public SyllableData Clone()
        {
            SyllableData syllDat = new()
            {
                kind = kind,
                appearing = appearing,
                length = length,
                node = node,
                syllable = syllable
            };
            return syllDat;

        }
    }

    class TextObject 
    {
        public GameObject obj;
        public TextMeshProUGUI textMesh;
        public bool isSecondHalf;

        public TextObject(GameObject obj, TextMeshProUGUI textMesh, bool isSecondHalf)
        {
            this.obj = obj;
            this.textMesh = textMesh;
            this.isSecondHalf = isSecondHalf;
        }

        public TextObject Clone()
        {
            GameObject currentObject = Instantiate(obj);
            currentObject.transform.SetParent(obj.transform.parent);
            TextMeshProUGUI currentObjectTM = currentObject.GetComponent<TextMeshProUGUI>();
            return new TextObject(currentObject, currentObjectTM, isSecondHalf);
        }
    }

    // size of node shower in bpm
    public const int NODESHOWER_SIZE = 67;
    // list of functions to be executed x frames later
    public List<(int, Action)> executeLater = new();
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
    List<SyllableData>[] trimedSongData;
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
    int currentBeat;
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
    VisualElement[] nameBoxes;
    Label[] playerLabels;
    VisualElement[] pointsBoxes;
    VisualElement[] swapBoxes;
    VisualElement[] swapBoxesAnimation;
    Label[] swapLabels;
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
    double[] pointsPerBeat;
    readonly int[] lastTimeStamps = new int[GameState.amountPlayer];
    readonly double[] points = new double[GameState.amountPlayer];
    // middle values of nodes for together game mode
    readonly Node[,] middleNodes = { { Node.C, Node.C, Node.CH, Node.CH, Node.D, Node.D, Node.DH, Node.A, Node.AH, Node.AH, Node.B, Node.B }, { Node.C, Node.CH, Node.CH, Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.AH, Node.B, Node.B, Node.C }, { Node.CH, Node.CH, Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.B, Node.C, Node.C }, { Node.CH, Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.C, Node.C, Node.CH }, { Node.D, Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.CH, Node.CH }, { Node.D, Node.DH, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.D }, { Node.A, Node.DH, Node.E, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH }, { Node.A, Node.AH, Node.E, Node.F, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH, Node.A }, { Node.AH, Node.AH, Node.B, Node.F, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH, Node.A, Node.A }, { Node.AH, Node.B, Node.B, Node.FH, Node.FH, Node.G, Node.G, Node.GH, Node.GH, Node.A, Node.A, Node.AH }, { Node.B, Node.B, Node.C, Node.C, Node.G, Node.G, Node.GH, Node.GH, Node.A, Node.A, Node.AH, Node.AH }, { Node.B, Node.C, Node.C, Node.CH, Node.CH, Node.GH, Node.GH, Node.A, Node.A, Node.AH, Node.AH, Node.B } };
    // team game mode variables
    int amountPlayerChanges = 0;
    int swapTime = 30;
    List<int>[] playerNotSung = new List<int>[GameState.amountPlayer];
    PlayerProfile[] singerProfiles;
    int[] nextProfileIndex;
    PlayerProfile[] nextProfiles;
    int[] oldProfiles;
    // item game mode variables
    List<(int, Node)>[] itemBeats = new List<(int, Node)>[GameState.amountPlayer];
    VisualElement[] itemBoxes;
    (int, Node) itemNodeToHit = (-1, Node.None);
    readonly int[] itemLastTimeStamps = new int[GameState.amountPlayer];
    int itemNodeLength = 1;
    bool[] canSeeNodes = new bool[GameState.amountPlayer];
    bool[] canSeeArrow = new bool[GameState.amountPlayer];
    double[] effectEndTime = new double[GameState.amountPlayer];
    const int ITEM_NODELENGTH = 1;

    void Start()
    {
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            canSeeArrow[i] = true;
            if (GameState.showNodes) {
                canSeeNodes[i] = true;
            }
            lastTimeStamps[i] = -1;
        }
        if (!GameState.useAudio)
        {
            AudioListener.volume = 0;
        }
        // init songData index
        songDataCurrentIndex = new int[GameState.currentSong.amountVoices];
        songDataNewLineIndex = new int[GameState.currentSong.amountVoices];
        // init array sizes
        startBeatLine = new int[GameState.currentSong.amountVoices];
        endBeatLine = new int[GameState.currentSong.amountVoices];
        beatSumLine = new int[GameState.currentSong.amountVoices];
        pointsPerBeat = new double[GameState.currentSong.amountVoices];
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
        int lastBeat = int.MinValue;
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
        VisualElement nodeBox;
        int index;
        // show voices
        if (voices.Count > 1)
        {
            TextObject voiceNumber; 
            if (GameState.currentSong.singer != null && GameState.currentSong.singer.Length > voices[1])
            {
                voiceNumber = CreateSyllabel(GameState.currentSong.singer[voices[1]]);
            } else
            {
                voiceNumber = CreateSyllabel(voices[1].ToString());
            }
            voiceNumber.obj.transform.localPosition = new Vector3(-450f, 275f, 0f);
            if (GameState.currentSong.singer != null && GameState.currentSong.singer.Length > voices[0])
            {
                voiceNumber = CreateSyllabel(GameState.currentSong.singer[voices[0]]);
            }
            else
            {
                voiceNumber = CreateSyllabel(voices[0].ToString());
            }
            voiceNumber.obj.transform.localPosition = new Vector3(-450f, -700f, 0f);
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
        singerProfiles = new PlayerProfile[GameState.amountPlayer];        
        nameBoxes = new VisualElement[GameState.amountPlayer];
        pointsBoxes = new VisualElement[GameState.amountPlayer];
        playerLabels = new Label[GameState.amountPlayer];
        swapBoxes = new VisualElement[GameState.amountPlayer];
        swapBoxesAnimation = new VisualElement[GameState.amountPlayer];
        swapLabels = new Label[GameState.amountPlayer];
        if (GameState.settings.useNewNodeEngine && GameState.currentGameMode == GameMode.Item)
        {
            itemBoxes = new VisualElement[GameState.amountPlayer];
        }
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            playerLabels[i] = roots[i].Q<Label>("Name");
            nameBoxes[i] = roots[i].Q<VisualElement>("NameBox");
            pointsBoxes[i] = roots[i].Q<VisualElement>("PointsBox");
            if (GameState.currentGameMode == GameMode.Team)
            {
                swapBoxes[i] = roots[i].Q<VisualElement>("SwapBox");
                swapBoxesAnimation[i] = roots[i].Q<VisualElement>("Animation");
                swapLabels[i] = roots[i].Q<Label>("Swap");
                swapBoxes[i].visible = true;
            }
            singerProfiles[i] = GameState.profiles[GameState.currentProfileIndex[i]];
            color = GameState.profiles[GameState.currentProfileIndex[i]].color;
            if (GameState.currentGameMode == GameMode.Together)
            {
                playerLabels[i].text = GameState.profiles[GameState.currentProfileIndex[i]].name + " and " + GameState.profiles[GameState.currentSecondProfileIndex[i]].name;
            }
            else
            {
                playerLabels[i].text = GameState.profiles[GameState.currentProfileIndex[i]].name;

            }
            nameBoxes[i].style.unityBackgroundImageTintColor = new Color(color.r / 255, color.g / 255, color.b / 255);
            pointsBoxes[i].style.unityBackgroundImageTintColor = new Color(color.r / 255, color.g / 255, color.b / 255);
            // change name and points color depending on player color
            if (color.r * 0.299 + color.g * 0.587 + color.b * 0.114 > 186) 
            {
                playerLabels[i].style.color = new Color(0f, 0f, 0f);
                pointsTexts[i].style.color = new Color(0f, 0f, 0f);
            } else
            {
                playerLabels[i].style.color = new Color(1f, 1f, 1f);
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
                        if (GameState.showText)
                        {
                            if (i == 0)
                            {
                                textLine2Bottom = CreateSyllabel(text);
                                textLine2Bottom.obj.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, -700f, 0f);
                                textLine2Bottom.textMesh.ForceMeshUpdate();
                            }
                            if (i > 0 || (voices.Count == 1 && GameState.amountPlayer > 1))
                            {
                                textLine2Top = CreateSyllabel(text);
                                textLine2Top.obj.transform.localPosition = new Vector3(500f - textLine2Top.textMesh.preferredWidth / 2, 275f, 0f);
                                textLine2Top.textMesh.ForceMeshUpdate();
                            }
                        }
                    }
                    text = "";
                    textCounter++;
                }
                songDataNewLineIndex[voices[i]]++;
            }
            startBeatLine[voices[i]] = songData[voices[i]][0].appearing;
            beatSumLine[voices[i]] = endBeatLine[voices[i]] - startBeatLine[voices[i]];
        }
        float currentPercent;
        int beatSum;
        trimedSongData = new List<SyllableData>[voices.Count];
        foreach (int v in voices)
        {
            // init node boxes
            if (GameState.settings.useNewNodeEngine) {
                trimedSongData[v] = new();
                // create node boxes for every songdata
                foreach (SyllableData sD in songData[v])
                {
                    if (sD.kind != Kind.LineBreak && sD.kind != Kind.LineBreakExcact)
                    {
                        trimedSongData[v].Add(sD.Clone());
                        for (int j = 0; j < GameState.amountPlayer; j++)
                        {
                            if (GameState.currentVoice[j] == v)
                            {
                                if (canSeeNodes[j])
                                {
                                    nodeBox = new VisualElement();
                                    nodeBox.AddToClassList("nodeBox");
                                    nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sD.node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                    nodeBox.style.left = Length.Percent(0);
                                    nodeBox.style.width = Length.Percent(0);
                                    nodeBoxes[j].Add(nodeBox);                                    
                                }
                            }
                        }
                    }
                }
            }
            else  
            {
                index = 0;
                while (songData[v][index].kind != Kind.LineBreak && songData[v][index].kind != Kind.LineBreakExcact)
                {
                    currentPercent = ((songData[v][index].appearing - startBeatLine[v]) * 100) / beatSumLine[v];
                    for (int j = 0; j < GameState.amountPlayer; j++)
                    {
                        if (GameState.currentVoice[j] == v)
                        {
                            if (canSeeNodes[j])
                            {
                                nodeBox = new VisualElement();
                                nodeBox.AddToClassList("nodeBox");
                                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[v][index].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
                                nodeBox.style.left = Length.Percent(currentPercent);
                                nodeBox.style.width = Length.Percent(((songData[v][index].appearing + songData[v][index].length - startBeatLine[v]) * 100) / beatSumLine[v] - currentPercent);
                                nodeBoxes[j].Add(nodeBox);
                            }
                        }
                    }
                    index++;
                }
            }
            // calculating points per beat
            beatSum = 0;
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
            pointsPerBeat[v] = 10000.0 / (double)beatSum;
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
                GeneralFunctions.WriteErrorLog("Error getting audio from " + GameState.currentSong.pathToMusic + "; Maybe the path to the mp3 or video is not found or the txt file isn't written in utf-8!");                
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
        if (GameState.currentGameMode == GameMode.Team)
        {
            oldProfiles = new int[GameState.amountPlayer];
            nextProfileIndex = new int[GameState.amountPlayer];
            // set singer
            int randomIndex;
            for (int i = 0; i < GameState.teams.Count; i++)
            {
                oldProfiles[i] = GameState.currentProfileIndex[i];
                // first singer
                playerNotSung[i] = new();
                for (int j = 0; j < GameState.teams[i].players.Count; j++)
                {
                    playerNotSung[i].Add(j);
                }
                randomIndex = Random.Range(0, playerNotSung[i].Count);
                GameState.currentProfileIndex[i] = GameState.profiles.IndexOf(GameState.teams[i].players[playerNotSung[i][randomIndex]]);
                singerProfiles[i] = GameState.profiles[GameState.currentProfileIndex[i]];
                playerLabels[i].text = singerProfiles[i].name;
                // change colors depending on player color
                nameBoxes[i].style.unityBackgroundImageTintColor = new Color(singerProfiles[i].color.r / 255, singerProfiles[i].color.g / 255, singerProfiles[i].color.b / 255);
                pointsBoxes[i].style.unityBackgroundImageTintColor = new Color(singerProfiles[i].color.r / 255, singerProfiles[i].color.g / 255, singerProfiles[i].color.b / 255);
                if (singerProfiles[i].color.r * 0.299 + singerProfiles[i].color.g * 0.587 + singerProfiles[i].color.b * 0.114 > 186)
                {
                    playerLabels[i].style.color = new Color(0f, 0f, 0f);
                    pointsTexts[i].style.color = new Color(0f, 0f, 0f);
                }
                else
                {
                    playerLabels[i].style.color = new Color(1f, 1f, 1f);
                    pointsTexts[i].style.color = new Color(1f, 1f, 1f);
                }
                playerNotSung[i].RemoveAt(randomIndex);
            }
            nextProfiles = new PlayerProfile[GameState.amountPlayer];
            NextSinger();
            microphoneInput.Init();
        }
        if (GameState.settings.useNewNodeEngine)
        {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                nodeArrows[i].style.left = Length.Percent(100 - NODESHOWER_SIZE - 2);
            }
            if (GameState.currentGameMode == GameMode.Item)
            {
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    itemBoxes[i] = roots[i].Q<VisualElement>("ItemBox");
                    itemLastTimeStamps[i] = -1;
                }
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
        int index = 0;
        while (index < executeLater.Count)
        {
            executeLater[index] = (executeLater[index].Item1 - 1, executeLater[index].Item2);
            if (executeLater[index].Item1 < 1)
            {
                executeLater[index].Item2();
                executeLater.RemoveAt(index);
            } else
            {
                index++;
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
                if (GameState.currentGameMode == GameMode.Team)
                {
                    // init swap time
                    if (songLength < 90.0)
                    {
                        swapTime = (int)(songLength / 3.0);
                    }
                    // init swap boxes
                    for (int j = 0; j < GameState.teams.Count; j++)
                    {
                        int jCopy = j;
                        executeLater.Add((1,() => {
                            swapBoxes[jCopy].style.left = nameBoxes[jCopy].resolvedStyle.left + nameBoxes[jCopy].resolvedStyle.width;
                        }));
                    }
                }
                if (GameState.currentGameMode == GameMode.Item) 
                {
                    // init item times
                    int itemSpawnrate = 30;
                    for (int j = 0; j < GameState.amountPlayer; j++)
                    {
                        itemBeats[j] = new();
                    }
                    Node randomNode;
                    int randomTime;
                    VisualElement nodeBox;
                    for (int i = 0; i < (int)(songLength / itemSpawnrate); i++)
                    {
                        randomNode = (Node)Random.Range(0, 12);
                        randomTime = Random.Range(0, itemSpawnrate + 1);
                        for (int j = 0; j < GameState.amountPlayer; j++)
                        {
                            itemBeats[j].Add(((int)Math.Ceiling(((randomTime + itemSpawnrate * i) / 60.0) * 4.0 * GameState.currentSong.bpm), randomNode));
                        }
                    }
                    if (GameState.settings.useNewNodeEngine)
                    {
                        for (int i = 0; i < (int)(songLength / itemSpawnrate); i++)
                        {
                            for (int j = 0; j < GameState.amountPlayer; j++)                            
                            {
                                nodeBox = new VisualElement();
                                nodeBox.AddToClassList("nodeBox");
                                nodeBox.style.unityBackgroundImageTintColor = new Color(1f - singerProfiles[j].color.r / 255f, 1f - singerProfiles[j].color.g / 255f, 1f - singerProfiles[j].color.b / 255f);
                                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)itemBeats[j][i].Item2) * 100) / nodeTextureHeight - nodeHeightOffset);
                                nodeBox.style.left = Length.Percent(0);
                                nodeBox.style.width = Length.Percent(0);
                                itemBoxes[j].Add(nodeBox);
                            }
                        }
                    }
                }
                currentBeat = (int)(((-GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap) / 60.0) * 4.0 * GameState.currentSong.bpm);
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
                double playerTime = songPlayer.GetTime();
                if (playerTime > 0)
                {
                    songPlayer.started = true;
                }
                double songPercent = (playerTime * 100.0) / songLength;
                // update timeline
                currentTimePointerBottom.anchoredPosition = new Vector3((float)(10.0 + (1895.0 * songPercent) / 100.0), -317.0f, 0f);
                currentTimePointerTop.anchoredPosition = new Vector3((float)(10.0 + (1895.0 * songPercent) / 100.0), 317.0f, 0f);
                // calculate sing time
                double currentTime = playerTime - GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap;
                // calculating current beat: Beatnumber = (Time in sec / 60 sec) * 4 * BPM
                if (currentBeat <= (currentTime / 60.0) * 4.0 * GameState.currentSong.bpm)
                {
                    currentBeat++;
                }
                // updating songtext
                if (GameState.showText) 
                {
                    DyeTextLine(currentBeat, voices[0], textLine1Bottom, syllablesLine1Bottom, -600);
                    if (voices.Count == 1 && GameState.amountPlayer > 1)
                    {
                        // reser textline
                        foreach (TextObject currObject in textLine1Top)
                        {
                            Destroy(currObject.obj, 0.0f);
                        }
                        textLine1Top.Clear();
                        // clone textline
                        TextObject currentTextObject;
                        foreach (TextObject t in textLine1Bottom)
                        {
                            currentTextObject = new(Instantiate(t.obj, t.obj.transform.parent), t.textMesh, t.isSecondHalf);
                            textLine1Top.Add(currentTextObject);
                        }
                        SetUpTextLineXY(textLine1Top, 175f);
                    } else if (voices.Count > 1)
                    {
                        DyeTextLine(currentBeat, voices[1], textLine1Top, syllablesLine1Top, 175f);
                    }
                }
                // update when to start shower
                if (!GameState.settings.useNewNodeEngine)
                {
                    if (currentBeat < startBeatLine[voices[0]])
                    {
                        if (textLine1Bottom.Count > 0)
                        {
                            UpdateWhenToStartRect(currentBeat, voices[0], textLine1Bottom, whenToStartBottom, whenToStartBottomRectTransform, -375f);
                        }
                    }
                    else
                    {
                        whenToStartBottomRectTransform.sizeDelta = new Vector2(0f, 100f);                      
                    }
                    if (GameState.amountPlayer > 1)
                    {
                        int i = 0;
                        if (voices.Count > 1)
                        {
                            i = 1;
                        }
                        if (currentBeat < startBeatLine[voices[i]])
                        {
                            if (textLine1Top.Count > 0)
                            {
                                UpdateWhenToStartRect(currentBeat, voices[i], textLine1Top, whenToStartTop, whenToStartTopRectTransform, 375f);
                            }
                        }
                        else
                        {
                            whenToStartTopRectTransform.sizeDelta = new Vector2(0f, 100f);
                        }
                    }
                }
                // updating nodes and calculating score
                VisualElement nodeBox;
                float startPercent;
                float percent;
                for (int i = 0; i < voices.Count; i++)
                {
                    if (songDataCurrentIndex[voices[i]] < songData[voices[i]].Count)
                    {
                        BackgroundSize backgroundSize = new()
                        {
                            x = new Length(Math.Min((float)((playerTime - swapTime * amountPlayerChanges) * 100) / swapTime, 100f), LengthUnit.Percent),
                            y = new Length(100f, LengthUnit.Percent)
                        };
                        Length leftArrowPercent = Length.Percent(((currentBeat - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]] - nodeArrowWidth);
                        for (int j = 0; j < GameState.amountPlayer; j++)
                        {
                            // update player node arrow:
                            if (GameState.currentVoice[j] == voices[i])
                            {
                                if (!GameState.settings.useNewNodeEngine)
                                {
                                    if (leftArrowPercent.value < 0)
                                    {
                                        leftArrowPercent = Length.Percent(0);
                                    }
                                    nodeArrows[j].style.left = leftArrowPercent;
                                }
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
                                if (GameState.currentGameMode == GameMode.Team)
                                {
                                    // update animation for player swaps 
                                    swapBoxesAnimation[j].style.backgroundSize = backgroundSize;
                                }
                            }
                        }
                        // update node boxes
                        if (GameState.settings.useNewNodeEngine)
                        {
                            index = 0;
                            while (index < trimedSongData[voices[i]].Count)
                            {
                                if (100 - NODESHOWER_SIZE + (float)((trimedSongData[voices[i]][index].appearing + trimedSongData[voices[i]][index].length - currentBeat) * 100) / (float)NODESHOWER_SIZE <= 0)
                                {
                                    trimedSongData[voices[i]].RemoveAt(index);
                                    for (int j = 0; j < GameState.amountPlayer; j++)
                                    {
                                        if (GameState.currentVoice[j] == voices[i])
                                        {
                                            nodeBoxes[j].RemoveAt(index);
                                        }
                                    }
                                }
                                else
                                {
                                    if (trimedSongData[voices[i]][index].appearing > currentBeat + NODESHOWER_SIZE)
                                    {
                                        index = trimedSongData[voices[i]].Count;
                                    }
                                    else
                                    {
                                        for (int j = 0; j < GameState.amountPlayer; j++)
                                        {
                                            if (GameState.currentVoice[j] == voices[i])
                                            {
                                                if (canSeeNodes[j])
                                                {
                                                    if (trimedSongData[voices[i]][index].appearing < currentBeat)
                                                    {
                                                        // startpercent = 0;
                                                        // percent = 0;
                                                        startPercent = 100 - NODESHOWER_SIZE + (float)((trimedSongData[voices[i]][index].appearing - currentBeat) * 100) / (float)NODESHOWER_SIZE;
                                                        percent = 100 - NODESHOWER_SIZE + (float)((trimedSongData[voices[i]][index].appearing + trimedSongData[voices[i]][index].length - currentBeat) * 100) / (float)NODESHOWER_SIZE;
                                                    }
                                                    else
                                                    {
                                                        startPercent = 100 - NODESHOWER_SIZE + (float)((trimedSongData[voices[i]][index].appearing - currentBeat) * 100) / (float)NODESHOWER_SIZE;
                                                        percent = 100 - NODESHOWER_SIZE + (float)((trimedSongData[voices[i]][index].appearing + trimedSongData[voices[i]][index].length - currentBeat) * 100) / (float)NODESHOWER_SIZE;
                                                    }
                                                    nodeBoxes[j][index].style.left = Length.Percent(startPercent);
                                                    if (percent > 100)
                                                    {
                                                        percent = 100;
                                                    }
                                                    nodeBoxes[j][index].style.width = Length.Percent(percent - startPercent);
                                                    nodeBoxes[j][index].visible = true;
                                                } else
                                                {
                                                    nodeBoxes[j][index].visible = false;
                                                }
                                            }
                                        }
                                        index++;
                                    }
                                }
                            }
                            if (GameState.currentGameMode == GameMode.Item)
                            {
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    index = 0;
                                    while (index < itemBeats[j].Count)
                                    {
                                        if (currentBeat > itemBeats[j][index].Item1 + ITEM_NODELENGTH)
                                        {
                                            itemBeats[j].RemoveAt(index);
                                            if (GameState.currentVoice[j] == voices[i])
                                            {
                                                itemBoxes[j].RemoveAt(index);
                                            }
                                        }
                                        else
                                        {
                                            if (itemBeats[j][index].Item1 > currentBeat + NODESHOWER_SIZE)
                                            {
                                                index = itemBeats[j].Count;
                                            }
                                            else
                                            {
                                                if (GameState.currentVoice[j] == voices[i])
                                                {
                                                    if (canSeeNodes[j])
                                                    {
                                                        if (itemBeats[j][index].Item1 < currentBeat)
                                                        {
                                                            startPercent = 0f;
                                                            percent = 0f;
                                                        }
                                                        else
                                                        {
                                                            startPercent = 100 - NODESHOWER_SIZE + (float)((itemBeats[j][index].Item1 - currentBeat) * 100) / (float)NODESHOWER_SIZE;
                                                            percent = 100 - NODESHOWER_SIZE + (float)((itemBeats[j][index].Item1 + ITEM_NODELENGTH - currentBeat) * 100) / (float)NODESHOWER_SIZE;
                                                        }
                                                        itemBoxes[j][index].style.left = Length.Percent(startPercent);
                                                        if (percent > 100)
                                                        {
                                                            percent = 100;
                                                        }
                                                        itemBoxes[j][index].style.width = Length.Percent(percent - startPercent);
                                                        itemBoxes[j][index].visible = true;
                                                    }
                                                    else
                                                    {
                                                        itemBoxes[j][index].visible = false;
                                                    }
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (songData[voices[i]][songDataCurrentIndex[voices[i]]].kind != Kind.LineBreak && songData[voices[i]][songDataCurrentIndex[voices[i]]].kind != Kind.LineBreakExcact)
                        {
                            // Manage node hits
                            if (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing <= currentBeat && currentBeat < (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing + songData[voices[i]][songDataCurrentIndex[voices[i]]].length))
                            {
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (GameState.currentVoice[j] == voices[i])
                                    {
                                        if (currentBeat != lastTimeStamps[j])
                                        {
                                            if (GameState.currentGameMode == GameMode.Together)
                                            {
                                                if (microphoneInput.nodes[j] != Node.None && microphoneInput.nodes[j + GameState.amountPlayer] != Node.None && HitNode(MiddleNode(microphoneInput.nodes[j], microphoneInput.nodes[j + GameState.amountPlayer]), songData[voices[i]][songDataCurrentIndex[voices[i]]].node, GameState.profiles[GameState.currentSecondProfileIndex[j]]))
                                                {
                                                    NodeHit(currentBeat, voices[i], j, Length.Percent(((nodeTextureDistance * (int)MiddleNode(microphoneInput.nodes[j], microphoneInput.nodes[j + GameState.amountPlayer])) * 100) / nodeTextureHeight - nodeHeightOffset), false);
                                                    // updating ui elements
                                                    pointsTexts[j].text = ((int)Math.Ceiling(points[j])).ToString();
                                                    // set actual beat as handled
                                                    lastTimeStamps[j] = currentBeat;
                                                }
                                            }
                                            else
                                            {
                                                if (microphoneInput.nodes[j] != Node.None && HitNode(microphoneInput.nodes[j], songData[voices[i]][songDataCurrentIndex[voices[i]]].node, GameState.profiles[GameState.currentProfileIndex[j]]))
                                                {
                                                    NodeHit(currentBeat, voices[i], j, Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[j]) * 100) / nodeTextureHeight - nodeHeightOffset), false);
                                                }
                                                // updating ui elements
                                                pointsTexts[j].text = ((int)Math.Ceiling(points[j])).ToString();
                                                // set actual beat as handled
                                                lastTimeStamps[j] = currentBeat;
                                            }
                                        }
                                    }
                                }
                            }
                            if (GameState.settings.useNewNodeEngine && GameState.currentGameMode == GameMode.Item)
                            {
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (currentBeat != lastTimeStamps[j])
                                    {
                                        if (itemBeats[j].Count > 0 
                                            && itemBeats[j][0].Item1 <= currentBeat
                                            && currentBeat < itemBeats[j][0].Item1 + ITEM_NODELENGTH
                                            && microphoneInput.nodes[j] != Node.None 
                                            && HitNode(microphoneInput.nodes[j], itemBeats[j][0].Item2, GameState.profiles[GameState.currentProfileIndex[j]])
                                        )
                                        {
                                            int randomChange = Random.Range(0, 2);
                                            for (int hittedPlayer = 0; hittedPlayer < j; hittedPlayer++)
                                            {
                                                SetNegativeEffect(hittedPlayer, playerTime, randomChange);
                                            }
                                            for (int hittedPlayer = j + 1; hittedPlayer < GameState.amountPlayer; hittedPlayer++)
                                            {
                                                SetNegativeEffect(hittedPlayer, playerTime, randomChange);
                                            }
                                        }
                                        lastTimeStamps[j] = currentBeat;                                            
                                    }
                                }
                            }
                            if (GameState.currentGameMode == GameMode.Item)
                            {
                                List<int> playerHit = new();
                                // check for item hit
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (GameState.currentVoice[j] == voices[i])
                                    {
                                        if (currentBeat != itemLastTimeStamps[j])
                                        {
                                            if (currentBeat - 1 >= itemNodeToHit.Item1 && currentBeat - 1 < itemNodeToHit.Item1 + itemNodeLength)
                                            {
                                                if (microphoneInput.nodes[j] != Node.None && HitNode(microphoneInput.nodes[j], itemNodeToHit.Item2, GameState.profiles[GameState.currentProfileIndex[j]]))
                                                {
                                                    playerHit.Add(j);
                                                    NodeHit(currentBeat, voices[i], j, Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[j]) * 100) / nodeTextureHeight - nodeHeightOffset), true);
                                                    // set actual beat as handled
                                                    itemLastTimeStamps[j] = currentBeat;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (playerHit.Count > 0)
                                {
                                    int randomChange = Random.Range(0, 2);
                                    if (playerHit.Count > 1)
                                    {
                                        for (int j = 0; j < GameState.amountPlayer; j++)
                                        {
                                            SetNegativeEffect(j, playerTime, randomChange);
                                        }
                                    } else
                                    {
                                        for (int j = 0; j < playerHit[0]; j++)
                                        {
                                            SetNegativeEffect(j, playerTime, randomChange);
                                        }
                                        for (int j = playerHit[0] + 1; j < GameState.amountPlayer; j++)
                                        {
                                            SetNegativeEffect(j, playerTime, randomChange);
                                        }
                                    }
                                }
                            }
                            // Update song data index
                            if (currentBeat + 1 >= (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing + songData[voices[i]][songDataCurrentIndex[voices[i]]].length))
                            {
                                songDataCurrentIndex[voices[i]]++;
                            }
                        }
                        else
                        {
                            if (songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing > currentBeat)
                            {
                                return;
                            }
                            int nodesNewLineIndex;
                            if (i == 0)
                            {
                                SetNextTextLine(voices[i], ref syllablesLine1Bottom, ref syllablesLine2Bottom, ref textLine2Bottom, -700f);
                            }
                            if (i > 0)
                            {
                                SetNextTextLine(voices[i], ref syllablesLine1Top, ref syllablesLine2Top, ref textLine2Top, 275f);
                            }
                            if (GameState.showText)
                            {
                                if (voices.Count == 1 && GameState.amountPlayer > 1)
                                {
                                    // updating for more player and same voices
                                    syllablesLine1Top = syllablesLine1Bottom;
                                    if (songDataNewLineIndex[voices[i]] - 1 <= songData[voices[i]].Count)
                                    {
                                        syllablesLine2Top = syllablesLine2Bottom;
                                        Destroy(textLine2Top.obj);
                                        textLine2Top = textLine2Bottom.Clone();
                                        textLine2Top.obj.transform.localPosition = new Vector3(500f - textLine2Top.textMesh.preferredWidth / 2, 275f, 0f);
                                        textLine2Top.textMesh.ForceMeshUpdate();
                                    }
                                    else
                                    {
                                        textLine2Top.textMesh.text = "";
                                        textLine2Top.textMesh.ForceMeshUpdate();
                                    }
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
                            // update node boxes                            
                            if (!GameState.settings.useNewNodeEngine)
                            {
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (GameState.currentVoice[j] == voices[i])
                                    {
                                        nodeBoxes[j].Clear();
                                    }
                                }
                                while (nodesNewLineIndex < songData[voices[i]].Count && songData[voices[i]][nodesNewLineIndex].kind != Kind.LineBreak && songData[voices[i]][nodesNewLineIndex].kind != Kind.LineBreakExcact)
                                {
                                    percent = ((songData[voices[i]][nodesNewLineIndex].appearing - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]];
                                    for (int j = 0; j < GameState.amountPlayer; j++)
                                    {
                                        if (GameState.currentVoice[j] == voices[i])
                                        {
                                            if (canSeeNodes[j])
                                            {
                                                nodeBox = new VisualElement();
                                                nodeBox.AddToClassList("nodeBox");
                                                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[voices[i]][nodesNewLineIndex].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                                nodeBox.style.left = Length.Percent(percent);
                                                nodeBox.style.width = Length.Percent(((songData[voices[i]][nodesNewLineIndex].appearing + songData[voices[i]][nodesNewLineIndex].length - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]] - percent);
                                                nodeBoxes[j].Add(nodeBox);
                                            }
                                        }
                                    }
                                    nodesNewLineIndex++;
                                }
                            }                   
                            if (GameState.currentGameMode == GameMode.Team)
                            {
                                if (playerTime > swapTime * (amountPlayerChanges + 1))
                                {
                                    // changing singer
                                    for (int j = 0; j < GameState.teams.Count; j++)
                                    {
                                        singerProfiles[j] = nextProfiles[j];
                                        GameState.currentProfileIndex[j] = nextProfileIndex[j];
                                        playerLabels[j].text = singerProfiles[j].name;
                                        // change colors depending on player color
                                        nameBoxes[j].style.unityBackgroundImageTintColor = new Color(singerProfiles[j].color.r / 255, singerProfiles[j].color.g / 255, singerProfiles[j].color.b / 255);
                                        pointsBoxes[j].style.unityBackgroundImageTintColor = new Color(singerProfiles[j].color.r / 255, singerProfiles[j].color.g / 255, singerProfiles[j].color.b / 255);
                                        if (nextProfiles[j].color.r * 0.299 + singerProfiles[j].color.g * 0.587 + singerProfiles[j].color.b * 0.114 > 186)
                                        {
                                            playerLabels[j].style.color = new Color(0f, 0f, 0f);
                                            pointsTexts[j].style.color = new Color(0f, 0f, 0f);
                                        }
                                        else
                                        {
                                            playerLabels[j].style.color = new Color(1f, 1f, 1f);
                                            pointsTexts[j].style.color = new Color(1f, 1f, 1f);
                                        }
                                    }
                                    if (swapTime * (amountPlayerChanges + 2) < songLength)
                                    {
                                        NextSinger();
                                    }
                                    else
                                    {
                                        for (int j = 0; j < GameState.teams.Count; j++)
                                        {
                                            swapBoxes[j].visible = false;
                                        }
                                    }
                                    amountPlayerChanges++;
                                    microphoneInput.Init();                                    
                                }
                            }
                            if (GameState.currentGameMode == GameMode.Item)
                            {
                                if (!GameState.settings.useNewNodeEngine)
                                {
                                    // add items
                                    for (int j = 0; j < GameState.amountPlayer; j++)
                                    {
                                        if (itemBeats[j].Count > 0)
                                        {
                                            if (startBeatLine[voices[i]] - itemBeats[j][0].Item1 >= 0 || itemBeats[j][0].Item1 - (startBeatLine[voices[i]] + beatSumLine[voices[i]]) <= 0)
                                            {
                                                percent = ((itemBeats[j][0].Item1 - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]];
                                                Color color = new(1f - singerProfiles[j].color.r / 255f, 1f - singerProfiles[j].color.g / 255f, 1f - singerProfiles[j].color.b / 255f);
                                                if (GameState.currentVoice[j] == voices[i])
                                                {
                                                    nodeBox = new VisualElement();
                                                    nodeBox.AddToClassList("nodeBox");
                                                    nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[voices[i]][nodesNewLineIndex].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                                    nodeBox.style.left = Length.Percent(percent);
                                                    nodeBox.style.width = Length.Percent(((itemBeats[j][0].Item1 + itemNodeLength - startBeatLine[voices[i]]) * 100) / beatSumLine[voices[i]] - percent);
                                                    nodeBox.style.unityBackgroundImageTintColor = color;
                                                    nodeBoxes[j].Add(nodeBox);
                                                }
                                                itemNodeToHit = itemBeats[j][0];
                                                itemBeats[j].RemoveAt(0);
                                            }
                                        }
                                    }
                                }
                                // manage negative effects
                                for (int j = 0; j < GameState.amountPlayer; j++)
                                {
                                    if (effectEndTime[j] < playerTime)
                                    {
                                        canSeeNodes[j] = true;
                                        canSeeArrow[j] = true;
                                    }
                                    if (canSeeArrow[j])
                                    {
                                        nodeArrows[j].visible = true;
                                    }
                                    else
                                    {
                                        nodeArrows[j].visible = false;
                                    }
                                }                                
                            }
                            songDataCurrentIndex[voices[i]]++;
                            // handle syllable coming at the same beat as line break
                            if (currentBeat == songData[voices[i]][songDataCurrentIndex[voices[i]]].appearing)
                            {
                                currentBeat--;
                            }
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
                }
            }
            else
            {
                // reset sing settings
                if (GameState.currentGameMode == GameMode.Random)
                {
                    GameState.showNodes = true;
                    GameState.showText = true;
                    GameState.useAudio = true;
                    AudioListener.volume = 1;
                }               
                // reset profiles
                if (GameState.currentGameMode == GameMode.Team)
                {
                    for (int i = 0; i < GameState.amountPlayer; i++)
                    {
                        GameState.currentProfileIndex[i] = oldProfiles[i];
                    }
                }
                // finalize points
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    GameState.profiles[GameState.currentProfileIndex[i]].points = (int)Math.Ceiling(points[i]);
                }
                SceneManager.LoadScene("SongEnd");
            }
        }
    }

    private void SetUpTextLineXY(List<TextObject> textLine, float textLineY)
    {
        // calculate needed width
        float renderedWidth = 0;
        foreach (TextObject to in textLine)
        {
            if (!to.isSecondHalf)
            {
                renderedWidth += to.textMesh.preferredWidth;
            }
        }
        // set position of text elements
        textLine[0].obj.transform.localPosition = new Vector3(500f - renderedWidth / 2, textLineY, 0f);
        TextObject beforeObject = textLine[0];
        beforeObject.textMesh.ForceMeshUpdate();
        foreach (TextObject to in textLine.Skip(1))
        {
            if (to.isSecondHalf)
            {
                to.obj.transform.localPosition = new Vector3(beforeObject.obj.transform.localPosition.x, beforeObject.obj.transform.localPosition.y, beforeObject.obj.transform.localPosition.z);
            }
            else
            {
                to.obj.transform.localPosition = new Vector3(beforeObject.obj.transform.localPosition.x + beforeObject.textMesh.preferredWidth, beforeObject.obj.transform.localPosition.y, beforeObject.obj.transform.localPosition.z);
            }
            to.textMesh.ForceMeshUpdate();
            beforeObject = to;
        }
    }

    private void DyeTextLine(int currentTimeStamp, int voiceNumber, List<TextObject> textLine, List<SyllableData> syllablesLine, float textLineY)
    {
        // reset song text
        string text = "";
        string textToSing = "";
        string textCurrentSing = "";
        string textSung = "";
        bool currentIsGolden = false;
        foreach (TextObject currObject in textLine)
        {
            Destroy(currObject.obj, 0.0f);
        }
        textLine.Clear();
        // Making syllable colored
        foreach (SyllableData s in syllablesLine)
        {
            // if alredy sung
            if (s.appearing < songData[voiceNumber][songDataCurrentIndex[voiceNumber]].appearing)
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
            else if (s.appearing > songData[voiceNumber][songDataCurrentIndex[voiceNumber]].appearing)
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
            CreateSyllabelToList(textLine, textSung);
        }
        if (textCurrentSing != "")
        {
            float currentSyllablePercent = ((float)(currentTimeStamp - songData[voiceNumber][songDataCurrentIndex[voiceNumber]].appearing)) / songData[voiceNumber][songDataCurrentIndex[voiceNumber]].length;
            if (currentSyllablePercent < 1f)
            {
                CreateCurrentSyllabel(textLine, textCurrentSing, currentIsGolden, currentSyllablePercent);
            }
            else
            {
                if (currentIsGolden)
                {
                    CreateSyllabelToList(textLine, colorGoldenSung + textCurrentSing + "</color>");
                }
                else
                {
                    CreateSyllabelToList(textLine, colorSung + textCurrentSing + "</color>");
                }
            }
        }
        if (textToSing != "")
        {
            CreateSyllabelToList(textLine, textToSing);
        }
        SetUpTextLineXY(textLine, textLineY);
    }

    private void NodeHit(int timeStamp, int voiceNumber, int playerNumber, Length nodeBoxTop, bool isItemMode)
    {
        Kind toHit;
        if (isItemMode)
        {
            toHit = Kind.Item;
        }
        else
        {
            toHit = songData[voiceNumber][songDataCurrentIndex[voiceNumber]].kind;
        }
        if (!GameState.settings.useNewNodeEngine)
        {
            // creating new node box
            float currentPercent = ((timeStamp - startBeatLine[voiceNumber]) * 100) / beatSumLine[voiceNumber];
            VisualElement nodeBox = new();
            nodeBox.AddToClassList("nodeBox");
            nodeBox.style.top = nodeBoxTop;
            nodeBox.style.left = Length.Percent(currentPercent);
            nodeBox.style.width = Length.Percent((((timeStamp - startBeatLine[voiceNumber] + 1) * 100) / beatSumLine[voiceNumber]) - currentPercent);
            Color color = new(singerProfiles[playerNumber].color.r / 255f, singerProfiles[playerNumber].color.g / 255f, singerProfiles[playerNumber].color.b / 255f);
            // setting node box color            
            switch (toHit)
            {
                case Kind.Normal:
                case Kind.Item:
                    nodeBox.style.unityBackgroundImageTintColor = color;
                    break;
                case Kind.Golden:
                    nodeBox.style.unityBackgroundImageTintColor = new Color(1f - color.r, 1f - color.g, 1f - color.b);
                    break;
                case Kind.Free:
                    nodeBox.style.unityBackgroundImageTintColor = new Color(color.r, color.g, color.b, 0.5f);
                    break;
            }
            nodeBoxes[playerNumber].Add(nodeBox);
        }
        // update points
        switch (toHit)
        {
            case Kind.Normal:
                points[playerNumber] += pointsPerBeat[voiceNumber];
                break;
            case Kind.Golden:
                points[playerNumber] += pointsPerBeat[voiceNumber] * 2;
                break;
        }
    }

    private void UpdateWhenToStartRect(int currentTimeStamp, int voiceNumber, List<TextObject> textLine, GameObject rect, RectTransform rectTransform, float posY)
    {
        if (GameState.showText)
        {
            float startX = -945f;
            float endX = textLine[0].obj.transform.localPosition.x - 500f;
            double startBeat;
            if (songDataCurrentIndex[voiceNumber] > 0)
            {
                startBeat = songData[voiceNumber][songDataCurrentIndex[voiceNumber] - 1].appearing;
            }
            // song with start gap
            else
            {
                startBeat = ((-GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap) / 60.0) * 4.0 * GameState.currentSong.bpm;
            }
            double percent = 100 - ((songData[voiceNumber][songDataCurrentIndex[voiceNumber]].appearing - currentTimeStamp) * 100) / (songData[voiceNumber][songDataCurrentIndex[voiceNumber]].appearing - startBeat);
            double posX = startX + ((endX - startX) * (percent)) / 100;
            rectTransform.sizeDelta = new Vector2(10f, 100f);
            rect.transform.localPosition = new Vector3((float)posX, posY, 0f);
        }
    }

    private void SetNextTextLine(int voiceNumber, ref List<SyllableData> currentSyllables, ref List<SyllableData> nextSyllables, ref TextObject textLine, float posY)
    {
        currentSyllables = new();
        foreach (SyllableData sD in nextSyllables)
        {
            currentSyllables.Add(sD.Clone());
        }
        // Calculating next line data
        nextSyllables = new();
        int nodesNewLineIndex = songDataNewLineIndex[voiceNumber];
        if (songDataNewLineIndex[voiceNumber] < songData[voiceNumber].Count)
        {
            string text = "";
            while (songDataNewLineIndex[voiceNumber] < songData[voiceNumber].Count && songData[voiceNumber][songDataNewLineIndex[voiceNumber]].kind != Kind.LineBreak && songData[voiceNumber][songDataNewLineIndex[voiceNumber]].kind != Kind.LineBreakExcact)
            {
                if (GameState.showText)
                {
                    // adding text based on kind of syllable
                    switch (songData[voiceNumber][songDataNewLineIndex[voiceNumber]].kind)
                    {
                        case Kind.Normal:
                            text += songData[voiceNumber][songDataNewLineIndex[voiceNumber]].syllable;
                            break;
                        case Kind.Free:
                            text += "<i>" + songData[voiceNumber][songDataNewLineIndex[voiceNumber]].syllable + "</i>";
                            break;
                        case Kind.Golden:
                            text += colorGoldenToSing + songData[voiceNumber][songDataNewLineIndex[voiceNumber]].syllable + "</color>";
                            break;
                    }
                    nextSyllables.Add(songData[voiceNumber][songDataNewLineIndex[voiceNumber]]);
                }
                songDataNewLineIndex[voiceNumber]++;
            }
            if (GameState.showText)
            {
                Destroy(textLine.obj);
                textLine = CreateSyllabel(text);
                textLine.obj.transform.localPosition = new Vector3(500f - textLine.textMesh.preferredWidth / 2, posY, 0f);
                textLine.textMesh.ForceMeshUpdate();
            }
            endBeatLine[voiceNumber] = songData[voiceNumber][nodesNewLineIndex - 1].appearing;
            songDataNewLineIndex[voiceNumber]++;
        }
        else
        {
            if (GameState.showText)
            {
                textLine.textMesh.text = "";
                textLine.textMesh.ForceMeshUpdate();
            }
            endBeatLine[voiceNumber] = songData[voiceNumber][nodesNewLineIndex - 2].appearing + songData[voiceNumber][nodesNewLineIndex - 2].length;
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
        return middleNodes[(int)first, (int)second];
    }

    private void NextSinger()
    {
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            if (playerNotSung[i].Count == 0)
            {
                for (int p = 0; p < GameState.teams[i].players.Count; p++)
                {
                    playerNotSung[i].Add(p);
                }
            }
            int randomIndex = Random.Range(0, playerNotSung[i].Count);
            nextProfileIndex[i] = GameState.profiles.IndexOf(GameState.teams[i].players[playerNotSung[i][randomIndex]]);
            nextProfiles[i] = GameState.profiles[nextProfileIndex[i]];
            swapLabels[i].text = " next: " + nextProfiles[i].name;
            swapBoxesAnimation[i].style.unityBackgroundImageTintColor = new Color(nextProfiles[i].color.r / 255, nextProfiles[i].color.g / 255, nextProfiles[i].color.b / 255);
            if (nextProfiles[i].color.r * 0.299 + nextProfiles[i].color.g * 0.587 + nextProfiles[i].color.b * 0.114 > 186)
            {
                swapLabels[i].style.color = new Color(0f, 0f, 0f);
                swapBoxes[i].style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f);
            }
            else
            {
                swapLabels[i].style.color = new Color(1f, 1f, 1f);
                swapBoxes[i].style.unityBackgroundImageTintColor = new Color(0f, 0f, 0f);
            }
            playerNotSung[i].RemoveAt(randomIndex);
            int iCopy = i;
            executeLater.Add((1, () =>
            {
                swapBoxes[iCopy].style.left = nameBoxes[iCopy].resolvedStyle.left + nameBoxes[iCopy].resolvedStyle.width;
            }));
        }
    }

    private void SetNegativeEffect(int playernumber, double currentTime, int change)
    {
        switch (change)
        {
            case 0:
                canSeeNodes[playernumber] = false;
                break;
            case 1:
                canSeeArrow[playernumber] = false;
                break;
        }
        effectEndTime[playernumber] = currentTime + 10;
    }
}