using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Classes;

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

    // Mic input
    public MicrophoneInput microphoneInput;
    // Video
    public GameVideoPlayer video;
    // song player
    SongPlayer songPlayer;
    bool songPlayerNotSet = true;
    bool notLoadedMP3 = true;
    // Songfile data extraction
    readonly List<SyllableData> songData = new ();
    // syllables data
    List<SyllableData> syllablesLine1 = new();
    List<SyllableData> syllablesLine2 = new();
    // songData index
    int songDataCurrentIndex = 0;
    int songDataNewLineIndex = 0;
    // beat data
    int startBeatLine1 = 0;
    int endBeatLine1 = 0;
    int beatSumLine1 = 0;
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
    // half size of node arrow and blocks texture in %
    const int nodeHeightOffset = 5;
    // difference to next node in node texture in pixel
    const int nodeTextureDistance = 5;
    // length of node texture in pixel
    const int nodeTextureHeight = 64;
    // width of node arrow in percent
    const int nodeArrowWidth = 2;
    // score calculating variables 
    float pointsPerBeat;
    readonly int[] lastBeats = new int[GameState.amountPlayer];
    readonly float[] points = new float[GameState.amountPlayer];

    void Start()
    {
        // Getting data from song file
        string[] songFileData = File.ReadAllLines(GameState.currentSong.path);
        SyllableData syllable;
        string temp;
        foreach (string line in songFileData)
        {
            if (line.Length > 0) 
            {
                syllable = new SyllableData();
                switch (line[0])
                {
                    // Normal note
                    case ':':
                        syllable.kind = Kind.Normal;
                        temp = line[2..];
                        syllable.appearing = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.length = int.Parse(temp[..temp.IndexOf(' ')]);
                        temp = temp[(temp.IndexOf(' ') + 1)..];
                        syllable.node = NodeFunctions.GetNodeFromInt(int.Parse(temp[..temp.IndexOf(' ')]));
                        syllable.syllable = temp[(temp.IndexOf(' ') + 1)..];
                        songData.Add(syllable);
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
                        syllable.syllable = temp[(temp.IndexOf(' ') + 1)..];
                        songData.Add(syllable);
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
                        syllable.syllable = temp[(temp.IndexOf(' ') + 1)..];
                        songData.Add(syllable);
                        break;
                    // Line break
                    case '-':
                        temp = line.TrimEnd();
                        // Handle "- newLineTime" and "- deleteLineTime newLineTime"
                        if (temp.IndexOf(' ') == temp.LastIndexOf(' '))
                        {
                            syllable.kind = Kind.LineBreak;
                            syllable.appearing = int.Parse(temp[2..]);
                            songData.Add(syllable);
                        }
                        else
                        {
                            syllable.kind = Kind.LineBreakExcact;
                            temp = line[2..];
                            syllable.appearing = int.Parse(temp[..temp.IndexOf(' ')]);
                            temp = temp[(temp.IndexOf(' ') + 1)..];
                            syllable.length = int.Parse(temp[(temp.IndexOf(' ') + 1)..]) - syllable.appearing;
                            songData.Add(syllable);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        // set ui screen
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
        // get UI pointer
        if (GameState.amountPlayer > 1)
        {
            gameObject.transform.Find("BackgroundTop").gameObject.SetActive(true);
        }
        // get ui player data pointer 
        VisualElement[] roots = new VisualElement[GameState.amountPlayer];
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            roots[i] = root.Q<VisualElement>("PlayerNodeBoxP" + (i + 1).ToString());
            nodeBoxes[i] = roots[i].Q<VisualElement>("NodeBox");
            pointsTexts[i] = roots[i].Q<Label>("Points");
        }
        // setting player name
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            roots[i].Q<Label>("Name").text = GameState.profiles[GameState.currentProfileIndex[i]].name;
        }
        // Getting player node arrow
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            nodeArrows[i] = roots[i].Q<VisualElement>("Node");
        }
        //Getting first song lines
        int textCounter = 1;
        string text = "";
        VisualElement nodeBox;
        while (textCounter < 3)
        {
            if (songData[songDataNewLineIndex].kind != Kind.LineBreak && songData[songDataNewLineIndex].kind != Kind.LineBreakExcact)
            {
                // Combininig text based on kind of syllable
                switch (songData[songDataNewLineIndex].kind)
                {
                    case Kind.Normal:
                        text += songData[songDataNewLineIndex].syllable;
                        break;
                    case Kind.Free:
                        text += "<i>" + songData[songDataNewLineIndex].syllable + "</i>";
                        break;
                    case Kind.Golden:
                        text += colorGoldenToSing + songData[songDataNewLineIndex].syllable + "</color>";
                        break;
                }
                // Setting syllables of first line
                if (textCounter == 1)
                {
                    syllablesLine1.Add(songData[songDataNewLineIndex]);
                }
                // Setting syllables of second line
                else if (textCounter == 2)
                {
                    syllablesLine2.Add(songData[songDataNewLineIndex]);
                }
            }
            else
            {
                if (textCounter == 1)
                {
                    // Setting beatEnd for node shower
                    endBeatLine1 = songData[songDataNewLineIndex].appearing;
                }
                else
                {
                    textLine2Bottom = CreateSyllabel(text);
                    textLine2Bottom.gameObject.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, -700f, 0f);
                    textLine2Bottom.textMesh.ForceMeshUpdate();
                }
                text = "";
                textCounter++;
            }
            songDataNewLineIndex++;
        }
        if (GameState.amountPlayer > 1)
        {
            textLine2Top = CreateSyllabel(textLine2Bottom.textMesh.text);
            textLine2Top.gameObject.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, 275f, 0f);
            textLine2Top.textMesh.ForceMeshUpdate();
        }
        int index = 0;
        beatSumLine1 = endBeatLine1 - startBeatLine1;
        float currentPercent;
        while (songData[index].kind != Kind.LineBreak && songData[index].kind != Kind.LineBreakExcact)
        {
            currentPercent = (songData[index].appearing * 100) / beatSumLine1;
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                nodeBox = new VisualElement();
                nodeBox.AddToClassList("nodeBox");
                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[index].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
                nodeBox.style.left = Length.Percent(currentPercent);
                nodeBox.style.width = Length.Percent((songData[index].appearing + songData[index].length) * 100 / beatSumLine1 - currentPercent);
                nodeBoxes[i].Add(nodeBox);
            }
            index++;
        }
        // calculating points per beat
        int beatSum = 0;
        // getting sum of beats (golden notes double)
        for (index = 0; index < songData.Count; index++)
        {
            if (songData[index].kind != Kind.LineBreak && songData[index].kind != Kind.LineBreakExcact)
            {
                // handling different nodes 
                switch (songData[index].kind)
                {
                    case Kind.Normal:
                        beatSum += songData[index].length;
                        break;
                    case Kind.Golden:
                        beatSum += songData[index].length * 2;
                        break;
                    default:
                        break;
                }
            }
        }
        pointsPerBeat = 10000f / beatSum;
    }

    void Update()
    {
        // set song player if not done
        if (songPlayerNotSet)
        {
            if (GameState.currentSong.pathToMusic != "" && GameState.currentSong.pathToMusic != GameState.currentSong.pathToVideo)
            {
                // using audiofile for sound
                if (notLoadedMP3)
                {
                    StartCoroutine(LoadAudioFile());
                    notLoadedMP3 = false;
                }
            }
            else
            {
                // using video for sound
                if (video.videoPlayer != null)
                {
                    songPlayer = new SongPlayer(video.videoPlayer);
                    video.videoPlayer.Play();
                    songPlayerNotSet = false;
                }
            }
            return;
        }
        if ((!songPlayer.currentPlayerIsAudioSource && (songPlayer.IsPlaying() || songPlayer.GetTime() == 0)) || (songPlayer.currentPlayerIsAudioSource && songPlayer.IsPlaying()))
        {
            double currentTime = songPlayer.GetTime() - GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap;
            // calculating current beat: Beatnumber = (Time in sec / 60 sec) * 4 * BPM - GAP
            int currentBeat = (int)Math.Ceiling((currentTime / 60.0) * 4.0 * GameState.currentSong.bpm);
            // updating nodes, songtext and calculating score
            VisualElement nodeBox;
            float currentPercent;
            string text = "";
            if (songDataCurrentIndex < songData.Count)
            {
                // Updating player node arrow:
                Length leftArrowPercent = Length.Percent(((currentBeat - startBeatLine1) * 100) / beatSumLine1 - nodeArrowWidth); ;
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    if (leftArrowPercent.value < 0)
                    {
                        leftArrowPercent = Length.Percent(0);
                    }
                    nodeArrows[i].style.left = leftArrowPercent;
                }
                if (songData[songDataCurrentIndex].kind != Kind.LineBreak && songData[songDataCurrentIndex].kind != Kind.LineBreakExcact)
                {
                    // reset song text
                    string textToSing = "";
                    string textCurrentSing = "";
                    string textSung = "";
                    bool currentIsGolden = false;
                    foreach (TextObject currObject in textLine1Bottom)
                    {
                        Destroy(currObject.gameObject, 0.0f);
                    }
                    foreach (TextObject currObject in textLine1Top)
                    {
                        Destroy(currObject.gameObject, 0.0f);
                    }
                    textLine1Bottom.Clear();
                    textLine1Top.Clear();
                    // Making syllable colored
                    foreach (SyllableData s in syllablesLine1)
                    {
                        // if alredy sung
                        if (s.appearing < songData[songDataCurrentIndex].appearing)
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
                        else if (s.appearing > songData[songDataCurrentIndex].appearing)
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
                        float currentSyllablePercent = ((float)(currentBeat - songData[songDataCurrentIndex].appearing)) / songData[songDataCurrentIndex].length;
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
                    textLine1Bottom[0].gameObject.transform.localPosition = new Vector3(500f - renderedWidth/2, -600f, 0f);
                    TextObject beforeObject = textLine1Bottom[0];
                    beforeObject.textMesh.ForceMeshUpdate();
                    foreach (TextObject to in textLine1Bottom.Skip(1))
                    {
                        if (to.isSecondHalf)
                        {
                            to.gameObject.transform.localPosition = new Vector3(beforeObject.gameObject.transform.localPosition.x, beforeObject.gameObject.transform.localPosition.y, beforeObject.gameObject.transform.localPosition.z);                               
                        } else
                        {
                            to.gameObject.transform.localPosition = new Vector3(beforeObject.gameObject.transform.localPosition.x + beforeObject.textMesh.preferredWidth, beforeObject.gameObject.transform.localPosition.y, beforeObject.gameObject.transform.localPosition.z);
                        }
                        to.textMesh.ForceMeshUpdate();
                        beforeObject = to;
                    }
                    if (GameState.amountPlayer > 1)
                    {
                        CloneSyllableWithY(textLine1Bottom, textLine1Top, 175f);
                    }
                    // Time in sec = Beatnumber / BPM / 4 * 60 sec
                    if (songData[songDataCurrentIndex].appearing / GameState.currentSong.bpm / 4 * 60 <= currentTime && (songData[songDataCurrentIndex].appearing + songData[songDataCurrentIndex].length) / GameState.currentSong.bpm / 4 * 60 >= currentTime)
                    {
                        // calculating score and updating UI
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            if (currentBeat != lastBeats[i])
                            {
                                if (microphoneInput.nodes[i] != Node.None && HitNode(microphoneInput.nodes[i], songData[songDataCurrentIndex].node, GameState.profiles[GameState.currentProfileIndex[i]]))
                                {
                                    // creating new node box
                                    currentPercent = ((currentBeat - 1 - startBeatLine1) * 100) / beatSumLine1;
                                    nodeBox = new VisualElement();
                                    nodeBox.AddToClassList("nodeBox");
                                    nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[i]) * 100) / nodeTextureHeight - nodeHeightOffset);
                                    nodeBox.style.left = Length.Percent(currentPercent);
                                    nodeBox.style.width = Length.Percent((((currentBeat - startBeatLine1) * 100) / beatSumLine1) - currentPercent);
                                    // updating score and setting node box color
                                    switch (songData[songDataCurrentIndex].kind)
                                    {
                                        case Kind.Normal:
                                            points[i] += pointsPerBeat;
                                            nodeBox.style.unityBackgroundImageTintColor = new StyleColor(new Color(0, 0, 1, 1));
                                            break;
                                        case Kind.Golden:
                                            points[i] += pointsPerBeat * 2;
                                            nodeBox.style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 0, 1, 1));
                                            break;
                                        case Kind.Free:
                                            nodeBox.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 1));
                                            break;
                                    }
                                    // updating ui elements
                                    pointsTexts[i].text = ((int)System.Math.Ceiling(points[i])).ToString();
                                    nodeBoxes[i].Add(nodeBox);
                                    // set actual beat as handled
                                    lastBeats[i] = currentBeat;
                                }
                            }
                        }
                    }
                    // time with no nodes
                    else
                    {
                        if (currentTime > (songData[songDataCurrentIndex].appearing + songData[songDataCurrentIndex].length) / GameState.currentSong.bpm / 4 * 60)
                        {
                            songDataCurrentIndex++;
                        }
                    }
                }
                else
                {
                    if (songData[songDataCurrentIndex].appearing > currentBeat)
                    {
                        return;
                    }
                    syllablesLine1 = syllablesLine2;
                    // Calculating next line data
                    syllablesLine2 = new();
                    int nodesNewLineIndex = songDataNewLineIndex;
                    if (songDataNewLineIndex < songData.Count)
                    {
                        text = "";
                        while (songDataNewLineIndex < songData.Count && songData[songDataNewLineIndex].kind != Kind.LineBreak && songData[songDataNewLineIndex].kind != Kind.LineBreakExcact)
                        {
                            // adding text based on kind of syllable
                            switch (songData[songDataNewLineIndex].kind)
                            {
                                case Kind.Normal:
                                    text += songData[songDataNewLineIndex].syllable;
                                    break;
                                case Kind.Free:
                                    text += "<i>" + songData[songDataNewLineIndex].syllable + "</i>";
                                    break;
                                case Kind.Golden:
                                    text += colorGoldenToSing + songData[songDataNewLineIndex].syllable + "</color>";
                                    break;
                            }
                            syllablesLine2.Add(songData[songDataNewLineIndex]);
                            songDataNewLineIndex++;
                        }
                        Destroy(textLine2Bottom.gameObject);
                        textLine2Bottom = CreateSyllabel(text);
                        textLine2Bottom.gameObject.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, -700f, 0f);
                        textLine2Bottom.textMesh.ForceMeshUpdate();
                        if (GameState.amountPlayer > 1)
                        {
                            Destroy(textLine2Top.gameObject);
                            textLine2Top = CreateSyllabel(text);
                            textLine2Top.gameObject.transform.localPosition = new Vector3(500f - textLine2Bottom.textMesh.preferredWidth / 2, 275f, 0f);
                            textLine2Top.textMesh.ForceMeshUpdate();
                        }
                        endBeatLine1 = songData[nodesNewLineIndex - 1].appearing;
                        songDataNewLineIndex++;
                    }
                    else
                    {
                        textLine2Bottom.textMesh.text = "";
                        textLine2Bottom.textMesh.ForceMeshUpdate();
                        if (GameState.amountPlayer > 1)
                        {
                            textLine2Top.textMesh.text = "";
                            textLine2Top.textMesh.ForceMeshUpdate();
                        }
                        endBeatLine1 = songData[nodesNewLineIndex - 2].appearing + songData[nodesNewLineIndex - 2].length;
                    }
                    // calculating beat data
                    if (songData[songDataCurrentIndex].kind == Kind.LineBreak)
                    {
                        startBeatLine1 = songData[songDataCurrentIndex].appearing;
                    }
                    // must be kind LineBreakExcact
                    else
                    {
                        startBeatLine1 = songData[songDataCurrentIndex].appearing + songData[songDataCurrentIndex].length;
                    }
                    
                    beatSumLine1 = endBeatLine1 - startBeatLine1;
                    nodesNewLineIndex = songDataCurrentIndex + 1;
                    // calculating node line data
                    for (int i = 0; i < GameState.amountPlayer; i++)
                    {
                        nodeBoxes[i].Clear();
                    }
                    while (nodesNewLineIndex < songData.Count && songData[nodesNewLineIndex].kind != Kind.LineBreak && songData[nodesNewLineIndex].kind != Kind.LineBreakExcact)
                    {
                        currentPercent = ((songData[nodesNewLineIndex].appearing - startBeatLine1) * 100) / beatSumLine1;
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            nodeBox = new VisualElement();
                            nodeBox.AddToClassList("nodeBox");
                            nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)songData[nodesNewLineIndex].node) * 100) / nodeTextureHeight - nodeHeightOffset);
                            nodeBox.style.left = Length.Percent(currentPercent);
                            nodeBox.style.width = Length.Percent(((songData[nodesNewLineIndex].appearing + songData[nodesNewLineIndex].length - startBeatLine1) * 100) / beatSumLine1 - currentPercent);
                            nodeBoxes[i].Add(nodeBox);
                        }
                        nodesNewLineIndex++;
                    }
                    songDataCurrentIndex++;
                }
            }
            else
            {
                // reset player node arrow to start
                for (int i = 0; i < GameState.amountPlayer; i++)
                {
                    nodeArrows[i].style.left = 0;
                }
            }
            // Updating player node arrows
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                if (microphoneInput.nodes[i] != Node.None)
                {
                    nodeArrows[i].style.top = Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[i]) * 100) / nodeTextureHeight - nodeHeightOffset);
                }
                else
                {
                    nodeArrows[i].style.top = Length.Percent(((nodeTextureDistance * 13) * 100) / nodeTextureHeight - nodeHeightOffset);
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

    private IEnumerator LoadAudioFile()
    {
        // setting up audio source object
        GameObject camera = GameObject.Find("MainCamera");
        AudioSource audio = camera.AddComponent<AudioSource>();
        // get audio file per request
        UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + GameState.currentSong.pathToMusic, AudioType.MPEG);
        yield return req.SendWebRequest();
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
        songPlayerNotSet = false;
    }

    private TextObject CreateSyllabel(String text)
    {
        // create object
        GameObject currentObject = new("TM");
        currentObject.transform.SetParent(gameObject.transform);
        // set up text mesh
        TextMeshProUGUI currentObjectTM = currentObject.AddComponent<TextMeshProUGUI>();
        currentObjectTM.text = text;
        currentObjectTM.rectTransform.sizeDelta = sizeDelta;
        currentObjectTM.fontSize = 60;
        currentObjectTM.enableWordWrapping = false;
        currentObjectTM.ForceMeshUpdate();
        return new TextObject(currentObject, currentObjectTM, false);
    }

    private void CreateSyllabelToList(List<TextObject> objects, String text)
    {
        // create object
        GameObject currentObject = new("TM");
        currentObject.transform.SetParent(gameObject.transform);
        // set up text mesh
        TextMeshProUGUI currentObjectTM = currentObject.AddComponent<TextMeshProUGUI>();
        currentObjectTM.text = text;
        currentObjectTM.rectTransform.sizeDelta = sizeDelta;
        currentObjectTM.fontSize = 60;
        currentObjectTM.enableWordWrapping = false;
        currentObjectTM.ForceMeshUpdate();
        objects.Add(new TextObject(currentObject, currentObjectTM, false));
    }

    private void CreateCurrentSyllabel(List<TextObject> objects, String text, bool isGolden, float currentPercantage)
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
            wordLeft.text = colorGoldenToSing +text;
        } else
        {
            wordLeft.text = text;
        }
        wordLeft.fontSize = 60;
        wordLeft.enableWordWrapping = false;
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
        if (isGolden)
        {
            wordRight.text = colorGoldenSung + text;
        }
        else
        {
            wordRight.text = colorSung + text;
        }
        wordRight.fontSize = 60;
        wordRight.enableWordWrapping = false;
        wordRight.transform.localPosition = new Vector3(0f, 0f, 0f);
        wordRight.rectTransform.sizeDelta = sizeDelta;
        wordRight.ForceMeshUpdate();
        maskRight.padding = new Vector4(0f, 0f, sizeDelta.x - wordLeft.preferredWidth + (wordLeft.preferredWidth * (1f - currentPercantage))); 
        objects.Add(new TextObject(objectRight, wordRight, true));
    }

    private void CloneSyllableWithY(List<TextObject> list, List<TextObject> clone, float newYCoordinate)
    {
        GameObject currentObj;
        TextMeshProUGUI currentTm;
        RectMask2D currentMask;
        foreach(TextObject to in list)
        {
            currentMask = to.gameObject.GetComponent<RectMask2D>();
            currentObj = Instantiate(to.gameObject);
            currentObj.transform.SetParent(gameObject.transform);
            currentObj.transform.localPosition = new Vector3(to.gameObject.transform.localPosition.x, newYCoordinate, to.gameObject.transform.localPosition.z);
            if (currentMask == null)
            {
                currentTm = currentObj.GetComponent<TextMeshProUGUI>();
                
            } else
            {
                currentTm = currentObj.transform.Find("TM").GetComponent<TextMeshProUGUI>();
            }
            currentTm.ForceMeshUpdate();
            clone.Add(new TextObject(currentObj, currentTm, to.isSecondHalf));
        }
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
}