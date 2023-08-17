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
    readonly ArrayList songData = new(); // Todo: writing own class with better performance
    // syllables data
    List<SyllableData> syllablesLine1 = new();
    List<SyllableData> syllablesLine2 = new();
    // node line data
    readonly List<VisualElement>[] nodesLines1 = new List<VisualElement>[GameState.amountPlayer];
    readonly List<VisualElement>[] nodesLines2 = new List<VisualElement>[GameState.amountPlayer];
    // songData index
    int songDataCurrentIndex = 0;
    int songDataNewLineIndex = 0;
    // beat data
    int startBeatLine1 = 0;
    int startBeatLine2 = 0;
    int endBeatLine2 = 0;
    // UI data
    Vector2 sizeDelta = new Vector2(1000f, 500f);
    // UI pointer
    List<TextObject> textLine1Bottom = new();
    //Label textLine1Bottom;
    Label textLine2Bottom;
    Label textLine1Top;
    Label textLine2Top;
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
    // current line beat sum
    int beatSumLine1 = 0;
    int beatSumLine2 = 0;
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
                case '-': // TODO Does only works for "- newLineTime" and not for "- deleteLineTime newLineTime"
                    temp = line.TrimEnd();
                    // Handle "- newLineTime" and "- deleteLineTime newLineTime"
                    if (temp.IndexOf(' ') == temp.LastIndexOf(' '))
                    {
                        songData.Add(int.Parse(temp[2..]));
                    }
                    else
                    {
                        songData.Add(int.Parse(temp[(temp.LastIndexOf(' ') + 1)..]));
                    }
                    break;
                default:
                    break;
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
        TemplateContainer currentContainer = root.Q<TemplateContainer>("SongLinesBottom");
        //textLine1Bottom = currentContainer.Q<Label>("SongLine1");
        textLine2Bottom = currentContainer.Q<Label>("SongLine2");
        if (GameState.amountPlayer > 1)
        {
            currentContainer = root.Q<TemplateContainer>("SongLinesTop");
            textLine1Top = currentContainer.Q<Label>("SongLine1");
            textLine2Top = currentContainer.Q<Label>("SongLine2");
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
        SyllableData sData;
        int beatEnd2 = 0;
        while (textCounter < 3)
        {
            if (songData[songDataNewLineIndex].GetType() == typeof(SyllableData))
            {
                sData = (SyllableData)songData[songDataNewLineIndex];
                // Combininig text based on kind of syllable
                switch (sData.kind)
                {
                    case Kind.Normal:
                        text += sData.syllable;
                        break;
                    case Kind.Free:
                        text += "<i>" + sData.syllable + "</i>";
                        break;
                    case Kind.Golden:
                        text += "<color=#ffff00ff>" + sData.syllable + "</color>";
                        break;
                }
                // Setting syllables of first line
                if (textCounter == 1)
                {
                    syllablesLine1.Add(sData);
                }
                // Setting syllables of second line
                else if (textCounter == 2)
                {
                    syllablesLine2.Add(sData);
                }
            }
            else
            {
                if (textCounter == 1)
                {
                    // Setting beatEnd for node shower
                    startBeatLine2 = (int)songData[songDataNewLineIndex];
                }
                else
                {
                    textLine2Bottom.text = text;
                    // Setting beatEnd for node shower
                    beatEnd2 = (int)songData[songDataNewLineIndex];
                }
                text = "";
                textCounter++;
            }
            songDataNewLineIndex++;
        }
        if (GameState.amountPlayer > 1)
        {
            //textLine1Top.text = textLine1Bottom.text;
            textLine2Top.text = textLine2Bottom.text;
        }
        // setting nodes of first line
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            nodesLines1[i] = new();
        }
        int index = 0;
        beatSumLine1 = startBeatLine2;
        float currentPercent;
        while (songData[index].GetType() == typeof(SyllableData))
        {
            sData = (SyllableData)songData[index];
            currentPercent = (sData.appearing * 100) / beatSumLine1;
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                nodeBox = new VisualElement();
                nodeBox.AddToClassList("nodeBox");
                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sData.node) * 100) / nodeTextureHeight - nodeHeightOffset);
                // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
                nodeBox.style.left = Length.Percent(currentPercent);
                nodeBox.style.width = Length.Percent((sData.appearing + sData.length) * 100 / beatSumLine1 - currentPercent);
                nodesLines1[i].Add(nodeBox);
            }
            index++;
        }
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            foreach (VisualElement element in nodesLines1[i])
            {
                nodeBoxes[i].Add(element);
            }
        }
        // setting nodes of second line
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            nodesLines2[i] = new();
        }
        beatSumLine2 = beatEnd2 - beatSumLine1;
        index++;
        while (songData[index].GetType() == typeof(SyllableData))
        {
            sData = (SyllableData)songData[index];
            currentPercent = ((sData.appearing - startBeatLine2) * 100) / beatSumLine2;
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                nodeBox = new VisualElement();
                nodeBox.AddToClassList("nodeBox");
                // node image location = (pitch in image * 100)/image height - node block offset
                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sData.node) * 100) / nodeTextureHeight - nodeHeightOffset);
                // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
                nodeBox.style.left = Length.Percent(currentPercent);
                nodeBox.style.width = Length.Percent(((sData.appearing + sData.length - startBeatLine2) * 100) / beatSumLine2 - currentPercent);
                nodesLines2[i].Add(nodeBox);
            }
            index++;
        }
        endBeatLine2 = beatEnd2;
        // calculating points per beat
        int beatSum = 0;
        SyllableData currentSyllable;
        // getting sum of beats (golden notes double)
        for (index = 0; index < songData.Count; index++)
        {
            if (songData[index].GetType() == typeof(SyllableData))
            {
                currentSyllable = (SyllableData)songData[index];
                // handling different nodes 
                switch (currentSyllable.kind)
                {
                    case Kind.Normal:
                        beatSum += currentSyllable.length;
                        break;
                    case Kind.Golden:
                        beatSum += currentSyllable.length * 2;
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
        }
        else
        {
            if ((!songPlayer.currentPlayerIsAudioSource && (songPlayer.IsPlaying() || songPlayer.GetTime() == 0)) || (songPlayer.currentPlayerIsAudioSource && songPlayer.IsPlaying()))
            {
                double currentTime = songPlayer.GetTime() - GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap;
                // calculating current beat: Beatnumber = (Time in sec / 60 sec) * 4 * BPM - GAP
                int currentBeat = (int)Math.Ceiling((currentTime / 60.0) * 4.0 * GameState.currentSong.bpm);
                // updating nodes, songtext and calculating score
                SyllableData sData;
                VisualElement nodeBox;
                float currentPercent;
                string text = "";
                if (songDataCurrentIndex < songData.Count)
                {
                    if (songData[songDataCurrentIndex].GetType() == typeof(SyllableData))
                    {
                        sData = (SyllableData)songData[songDataCurrentIndex];
                        // reset song text
                        string textToSing = "";
                        string textCurrentSing = "";
                        string textSung = "";
                        bool currentIsGolden = false;
                        foreach (TextObject currObject in textLine1Bottom)
                        {
                            Destroy(currObject.gameObject, 0.0f);
                        }
                        textLine1Bottom.Clear();
                        // Making syllable colored
                        foreach (SyllableData s in syllablesLine1)
                        {
                            // if alredy sung
                            if (s.appearing < sData.appearing)
                            {
                                switch (s.kind)
                                {
                                    case Kind.Normal:
                                        text += "<color=#0000ffff>" + s.syllable + "</color>";
                                        textSung += "<color=#0000ffff>" + s.syllable + "</color>";
                                        break;
                                    case Kind.Free:
                                        text += "<i><color=#0000ffff>" + s.syllable + "</color></i>";
                                        textSung += "<i><color=#0000ffff>" + s.syllable + "</color></i>";
                                        break;
                                    case Kind.Golden:
                                        text += "<color=#ff00ffff>" + s.syllable + "</color>";
                                        textSung += "<color=#ff00ffff>" + s.syllable + "</color>";
                                        break;
                                }
                            } 
                            // if has to sing
                            else if (s.appearing > sData.appearing)
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
                                        text += "<color=#ffff00ff>" + s.syllable + "</color>";
                                        textToSing += "<color=#ffff00ff>" + s.syllable + "</color>";
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
                                        text += "<color=#ffff00ff>" + s.syllable + "</color>";
                                        textCurrentSing += s.syllable;
                                        currentIsGolden = true;
                                        break;
                                }
                            }
                        }
                        // render text
                        if (textSung != "")
                        {
                            CreateSyllabel(textLine1Bottom, textSung);
                        }
                        if (textCurrentSing != "")
                        {
                            float currentSyllablePercent = ((float)(currentBeat - sData.appearing)) / sData.length;
                            if (currentSyllablePercent < 1f)
                            {
                                CreateCurrentSyllabel(textLine1Bottom, textCurrentSing, currentIsGolden, currentSyllablePercent);
                            }
                            else
                            {
                                if (currentIsGolden)
                                {
                                    CreateSyllabel(textLine1Bottom, "<color=#ffff00ff>" + textCurrentSing + "</color>");
                                }
                                else
                                {
                                    CreateSyllabel(textLine1Bottom, "<color=#0000ffff>" + textCurrentSing + "</color>");
                                }
                            }
                        }
                        if (textToSing != "")
                        {
                            CreateSyllabel(textLine1Bottom, textToSing);
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
                            textLine1Top.text = text;
                        }
                        // Time in sec = Beatnumber / BPM / 4 * 60 sec
                        if (sData.appearing / GameState.currentSong.bpm / 4 * 60 <= currentTime && (sData.appearing + sData.length) / GameState.currentSong.bpm / 4 * 60 >= currentTime)
                        {
                            // calculating score and updating UI
                            for (int i = 0; i < GameState.amountPlayer; i++)
                            {
                                if (currentBeat != lastBeats[i])
                                {

                                    if (microphoneInput.nodes[i] != Node.None && HitNode(microphoneInput.nodes[i], sData.node, GameState.profiles[GameState.currentProfileIndex[i]]))
                                    {
                                        // creating new node box
                                        currentPercent = ((currentBeat - 1 - startBeatLine1) * 100) / beatSumLine1;
                                        nodeBox = new VisualElement();
                                        nodeBox.AddToClassList("nodeBox");
                                        nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)microphoneInput.nodes[i]) * 100) / nodeTextureHeight - nodeHeightOffset);
                                        nodeBox.style.left = Length.Percent(currentPercent);
                                        nodeBox.style.width = Length.Percent((((currentBeat - startBeatLine1) * 100) / beatSumLine1) - currentPercent);
                                        // updating score and setting node box color
                                        switch (sData.kind)
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
                        else
                        {
                            if (currentTime > (sData.appearing + sData.length) / GameState.currentSong.bpm / 4 * 60)
                            {
                                songDataCurrentIndex++;
                            }
                        }
                    }
                    else
                    {
                        // Setting next line data to current line data
                        //textLine1Bottom.text = textLine2Bottom.text;
                        if (GameState.amountPlayer > 1)
                        {
                            textLine1Top.text = textLine2Top.text;
                        }
                        syllablesLine1 = syllablesLine2;
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            nodesLines1[i] = nodesLines2[i];
                        }
                        beatSumLine1 = beatSumLine2;
                        startBeatLine1 = startBeatLine2;
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            nodeBoxes[i].Clear();
                        }
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            foreach (VisualElement element in nodesLines1[i])
                            {
                                nodeBoxes[i].Add(element);
                            }
                        }
                        // Calculating next line data
                        syllablesLine2 = new();
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            nodesLines2[i] = new();
                        }
                        int nodesNewLineIndex = songDataNewLineIndex;
                        int beatEnd;
                        if (songDataNewLineIndex < songData.Count)
                        {
                            text = "";
                            while (songDataNewLineIndex < songData.Count && songData[songDataNewLineIndex].GetType() == typeof(SyllableData))
                            {
                                sData = (SyllableData)songData[songDataNewLineIndex];
                                // adding text based on kind of syllable
                                switch (sData.kind)
                                {
                                    case Kind.Normal:
                                        text += sData.syllable;
                                        break;
                                    case Kind.Free:
                                        text += "<i>" + sData.syllable + "</i>";
                                        break;
                                    case Kind.Golden:
                                        text += "<color=#ffff00ff>" + sData.syllable + "</color>";
                                        break;
                                }
                                syllablesLine2.Add(sData);
                                songDataNewLineIndex++;
                            }
                            textLine2Bottom.text = text;
                            if (GameState.amountPlayer > 1)
                            {
                                textLine2Top.text = text;
                            }
                            // calculating node line data
                            if (songDataNewLineIndex < songData.Count)
                            {
                                beatEnd = (int)songData[songDataNewLineIndex];
                            }
                            else
                            {
                                sData = (SyllableData)songData[songDataNewLineIndex - 1];
                                beatEnd = sData.appearing + sData.length;
                            }
                            beatSumLine2 = beatEnd - endBeatLine2;
                            while (nodesNewLineIndex < songData.Count && songData[nodesNewLineIndex].GetType() == typeof(SyllableData))
                            {
                                sData = (SyllableData)songData[nodesNewLineIndex];
                                currentPercent = ((sData.appearing - endBeatLine2) * 100) / beatSumLine2;
                                for (int i = 0; i < GameState.amountPlayer; i++)
                                {
                                    nodeBox = new VisualElement();
                                    nodeBox.AddToClassList("nodeBox");
                                    nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sData.node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                    nodeBox.style.left = Length.Percent(currentPercent);
                                    nodeBox.style.width = Length.Percent(((sData.appearing + sData.length - endBeatLine2) * 100) / beatSumLine2 - currentPercent);
                                    nodesLines2[i].Add(nodeBox);
                                }
                                nodesNewLineIndex++;
                            }
                            endBeatLine2 = beatEnd;
                            songDataNewLineIndex++;
                        }
                        else
                        {
                            textLine2Bottom.text = "";
                            if (GameState.amountPlayer > 1)
                            {
                                textLine2Top.text = "";
                            }
                        }
                        startBeatLine1 = (int)songData[songDataCurrentIndex];
                        songDataCurrentIndex++;
                    }
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
                    GameState.profiles[GameState.currentProfileIndex[i]].points = (int)System.Math.Ceiling(points[i]);
                }
                SceneManager.LoadScene("SongEnd");
            }
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

    private void CreateSyllabel(List<TextObject> objects, String text)
    {
        // create object
        GameObject currentObject = new("TM");
        currentObject.transform.parent = gameObject.transform;
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
        objectLeft.transform.parent = gameObject.transform;
        RectMask2D maskLeft = objectLeft.AddComponent<RectMask2D>();
        maskLeft.rectTransform.sizeDelta = sizeDelta;
        GameObject subObjectLeft = new("TM");
        subObjectLeft.transform.parent = objectLeft.transform;
        TextMeshProUGUI wordLeft = subObjectLeft.AddComponent<TextMeshProUGUI>();
        if (isGolden)
        {
            wordLeft.text = "<color=#ffff00ff>"+text;
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
        objectRight.transform.parent = gameObject.transform;
        RectMask2D maskRight = objectRight.AddComponent<RectMask2D>();
        maskRight.rectTransform.sizeDelta = sizeDelta;
        GameObject subObjectRight = new("TM");
        subObjectRight.transform.parent = objectRight.transform;
        TextMeshProUGUI wordRight = subObjectRight.AddComponent<TextMeshProUGUI>();
        if (isGolden)
        {
            wordRight.text = "<color=#ff00ffff>" + text;
        }
        else
        {
            wordRight.text = "<color=#0000ffff>" + text;
        }
        wordRight.fontSize = 60;
        wordRight.enableWordWrapping = false;
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
}