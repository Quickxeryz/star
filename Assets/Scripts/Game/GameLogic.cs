using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
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
    // Mic input
    public MicrophoneInput micInP1;
    public MicrophoneInput micInP2;
    public MicrophoneInput micInP3;
    public MicrophoneInput micInP4;
    // Video
    public VideoPlayer video;
    // Songfile data extraction
    ArrayList songData = new ArrayList(); // Todo: writing own class with better performance
    // syllables data
    ArrayList syllablesLine1 = new ArrayList();
    ArrayList syllablesLine2 = new ArrayList();
    // node line data
    ArrayList nodesLine1P1 = new ArrayList();
    ArrayList nodesLine2P1 = new ArrayList();
    ArrayList nodesLine1P2 = new ArrayList();
    ArrayList nodesLine2P2 = new ArrayList();
    ArrayList nodesLine1P3 = new ArrayList();
    ArrayList nodesLine2P3 = new ArrayList();
    ArrayList nodesLine1P4 = new ArrayList();
    ArrayList nodesLine2P4 = new ArrayList();
    // songData index pointer
    int songDataCurrentIndex = 0;
    int songDataNewLineIndex = 0;
    // beat pointer
    int startBeatLine1 = 0;
    int startBeatLine2 = 0;
    int endBeatLine2 = 0;
    // UI pointer
    Label textLine1;
    Label textLine2;
    VisualElement nodeP1;
    VisualElement nodeP2;
    VisualElement nodeP3;
    VisualElement nodeP4;
    VisualElement nodeBoxP1;
    Label pointsTextP1;
    Label pointsTextP2;
    Label pointsTextP3;
    Label pointsTextP4;
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
    int lastBeat;
    float pointsP1 = 0;
    float pointsP2 = 0;
    float pointsP3 = 0;
    float pointsP4 = 0;

    void Start()
    {
        // Getting data from song file
        string[] songFileData = System.IO.File.ReadAllLines(GameState.currentSong.path);
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
                    temp = line.Substring(2);
                    syllable.appearing = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.length = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.node = NodeFunctions.getNode(int.Parse(temp.Substring(0, temp.IndexOf(' '))));
                    syllable.syllable = temp.Substring(temp.IndexOf(' ') + 1);
                    songData.Add(syllable);
                    break;
                // Golden note
                case '*':
                    syllable.kind = Kind.Golden;
                    temp = line.Substring(2);
                    syllable.appearing = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.length = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.node = NodeFunctions.getNode(int.Parse(temp.Substring(0, temp.IndexOf(' '))));
                    syllable.syllable = temp.Substring(temp.IndexOf(' ') + 1);
                    songData.Add(syllable);
                    break;
                // Freestyle syllable
                case 'F':
                    syllable.kind = Kind.Free;
                    temp = line.Substring(2);
                    syllable.appearing = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.length = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.node = NodeFunctions.getNode(int.Parse(temp.Substring(0, temp.IndexOf(' '))));
                    syllable.syllable = temp.Substring(temp.IndexOf(' ') + 1);
                    songData.Add(syllable);
                    break;
                // Line break
                case '-':
                    songData.Add(int.Parse(line.Substring(2)));
                    break;
                default:
                    break;
            }
        }
        // get UI pointer
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        textLine1 = root.Q<Label>("SongLine1");
        textLine2 = root.Q<Label>("SongLine2");
        // get ui player data pointer 
        VisualElement rootP1 = root.Q<VisualElement>("PlayerNodeBoxP1");
        nodeBoxP1 = rootP1.Q<VisualElement>("NodeBox");
        pointsTextP1 = rootP1.Q<Label>("Points");
        // setting player name
        rootP1.Q<Label>("Name").text = GameState.player1.name;
        // Getting player node arrow
        nodeP1 = rootP1.Q<VisualElement>("Node");
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
                    textLine1.text = text;
                    // Setting beatEnd for node shower
                    startBeatLine2 = (int)songData[songDataNewLineIndex];
                }
                else
                {
                    textLine2.text = text;
                    // Setting beatEnd for node shower
                    beatEnd2 = (int)songData[songDataNewLineIndex];
                }
                text = "";
                textCounter++;
            }
            songDataNewLineIndex++;
        }
        // setting nodes of first line
        int i = 0;
        beatSumLine1 = startBeatLine2;
        float currentPercent = 0f;
        while (songData[i].GetType() == typeof(SyllableData))
        {
            sData = (SyllableData)songData[i];
            currentPercent = (sData.appearing * 100) / beatSumLine1;
            nodeBox = new VisualElement();
            nodeBox.AddToClassList("nodeBox");
            nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sData.node) * 100) / nodeTextureHeight - nodeHeightOffset);
            // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
            nodeBox.style.left = Length.Percent(currentPercent);
            nodeBox.style.width = Length.Percent(((sData.appearing + sData.length) * 100) / beatSumLine1 - currentPercent);
            nodesLine1P1.Add(nodeBox);
            i++;
        }
        foreach (VisualElement element in nodesLine1P1)
        {
            nodeBoxP1.Add(element);
        }
        // setting nodes of second line
        beatSumLine2 = beatEnd2 - beatSumLine1;
        i++;
        while (songData[i].GetType() == typeof(SyllableData))
        {
            sData = (SyllableData)songData[i];
            currentPercent = ((sData.appearing - startBeatLine2) * 100) / beatSumLine2;
            nodeBox = new VisualElement();
            nodeBox.AddToClassList("nodeBox");
            // node image location = (pitch in image * 100)/image height - node block offset
            nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sData.node) * 100) / nodeTextureHeight - nodeHeightOffset);
            // beatNumber/100 % = startbeat/x -> x in % = (startbeat*100)/beatNumber
            nodeBox.style.left = Length.Percent(currentPercent);
            nodeBox.style.width = Length.Percent(((sData.appearing + sData.length - startBeatLine2) * 100) / beatSumLine2 - currentPercent);
            nodesLine2P1.Add(nodeBox);
            i++;
        }
        endBeatLine2 = beatEnd2;
        // calculating points per beat
        int beatSum = 0;
        SyllableData currentSyllable;
        // getting sum of beats (golden notes double)
        for (i = 0; i < songData.Count; i++)
        {
            if (songData[i].GetType() == typeof(SyllableData))
            {
                currentSyllable = (SyllableData)songData[i];
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
        pointsPerBeat = 10000f / (float)beatSum;
    }

    void Update()
    {
        if (video.videoPlayer.isPlaying || video.videoPlayer.time == 0)
        {
            double currentTime = video.videoPlayer.time - GameState.micDelay - GameState.currentSong.gap;
            // calculating current beat: Beatnumber = (Time in sec / 60 sec) * 4 * BPM - GAP
            int currentBeat = (int)System.Math.Ceiling((currentTime / 60.0) * 4.0 * GameState.currentSong.bpm - GameState.currentSong.gap);
            // updating nodes, songtext and calculating score
            string text = "";
            SyllableData sData;
            VisualElement nodeBox;
            float currentPercent;
            if (songDataCurrentIndex < songData.Count)
            {
                if (songData[songDataCurrentIndex].GetType() == typeof(SyllableData))
                {
                    sData = (SyllableData)songData[songDataCurrentIndex];
                    text = "";
                    // Making syllable colored
                    foreach (SyllableData s in syllablesLine1)
                    {
                        if (s.appearing < sData.appearing)
                        {
                            switch (s.kind)
                            {
                                case Kind.Normal:
                                    text += "<color=#0000ffff>" + s.syllable + "</color>";
                                    break;
                                case Kind.Free:
                                    text += "<i><color=#0000ffff>" + s.syllable + "</color></i>";
                                    break;
                                case Kind.Golden:
                                    text += "<color=#ff00ffff>" + s.syllable + "</color>";
                                    break;
                            }
                        }
                        else
                        {
                            switch (s.kind)
                            {
                                case Kind.Normal:
                                    text += s.syllable;
                                    break;
                                case Kind.Free:
                                    text += "<i>" + s.syllable + "</i>";
                                    break;
                                case Kind.Golden:
                                    text += "<color=#ffff00ff>" + s.syllable + "</color>";
                                    break;
                            }
                        }
                    }
                    textLine1.text = text;
                    // Time in sec = Beatnumber / BPM / 4 * 60 sec
                    if (sData.appearing / GameState.currentSong.bpm / 4 * 60 <= currentTime && (sData.appearing + sData.length) / GameState.currentSong.bpm / 4 * 60 >= currentTime)
                    {
                        // calculating score and updating UI
                        if (currentBeat != lastBeat)
                        {
                            if (micInP1.node != Node.None && hitNode(micInP1.node, sData.node))
                            {
                                // creating new node box
                                currentPercent = ((currentBeat - 1 - startBeatLine1) * 100) / beatSumLine1;
                                nodeBox = new VisualElement();
                                nodeBox.AddToClassList("nodeBox");
                                nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)micInP1.node) * 100) / nodeTextureHeight - nodeHeightOffset);
                                nodeBox.style.left = Length.Percent(currentPercent);
                                nodeBox.style.width = Length.Percent((((currentBeat - startBeatLine1) * 100) / beatSumLine1) - currentPercent);
                                // updating score and setting node box color
                                switch (sData.kind)
                                {
                                    case Kind.Normal:
                                        pointsP1 += pointsPerBeat;
                                        nodeBox.style.unityBackgroundImageTintColor = new StyleColor(new Color(0, 0, 1, 1));
                                        break;
                                    case Kind.Golden:
                                        pointsP1 += pointsPerBeat * 2;
                                        nodeBox.style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 0, 1, 1));
                                        break;
                                    case Kind.Free:
                                        nodeBox.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 1));
                                        break;
                                }
                                // updating ui elements
                                pointsTextP1.text = ((int)System.Math.Ceiling(pointsP1)).ToString();
                                nodeBoxP1.Add(nodeBox);
                                // set actual beat as handled
                                lastBeat = currentBeat;
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
                    textLine1.text = textLine2.text;
                    syllablesLine1 = (ArrayList)syllablesLine2;
                    nodesLine1P1 = (ArrayList)nodesLine2P1;
                    beatSumLine1 = beatSumLine2;
                    startBeatLine1 = startBeatLine2;
                    nodeBoxP1.Clear();
                    foreach (VisualElement element in nodesLine1P1)
                    {
                        nodeBoxP1.Add(element);
                    }
                    // Calculating next line data
                    syllablesLine2 = new ArrayList();
                    nodesLine2P1 = new ArrayList();
                    int nodesNewLineIndex = songDataNewLineIndex;
                    int beatEnd = 0;
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
                        textLine2.text = text;
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
                            nodeBox = new VisualElement();
                            nodeBox.AddToClassList("nodeBox");
                            nodeBox.style.top = Length.Percent(((nodeTextureDistance * (int)sData.node) * 100) / nodeTextureHeight - nodeHeightOffset);
                            nodeBox.style.left = Length.Percent(currentPercent);
                            nodeBox.style.width = Length.Percent(((sData.appearing + sData.length - endBeatLine2) * 100) / beatSumLine2 - currentPercent);
                            nodesLine2P1.Add(nodeBox);
                            nodesNewLineIndex++;
                        }
                        endBeatLine2 = beatEnd;
                        songDataNewLineIndex++;
                    }
                    else
                    {
                        textLine2.text = "";
                    }
                    startBeatLine1 = (int)songData[songDataCurrentIndex];
                    songDataCurrentIndex++;
                }
                // Updating player node arro:
                nodeP1.style.left = Length.Percent(((currentBeat - startBeatLine1) * 100) / beatSumLine1 - nodeArrowWidth);
            }
            else
            {
                // Updating player node arrow
                nodeP1.style.left = 0;
            }
            // Updating player node arrow
            if (micInP1.node != Node.None)
            {
                nodeP1.style.top = Length.Percent(((nodeTextureDistance * (int)micInP1.node) * 100) / nodeTextureHeight - nodeHeightOffset);
            }
            else
            {
                nodeP1.style.top = Length.Percent(((nodeTextureDistance * 13) * 100) / nodeTextureHeight - nodeHeightOffset);
            }
        }
        else
        {
            GameState.player1.points = (int)System.Math.Ceiling(pointsP1);
            GameState.player2.points = (int)System.Math.Ceiling(pointsP2);
            GameState.player3.points = (int)System.Math.Ceiling(pointsP3);
            GameState.player4.points = (int)System.Math.Ceiling(pointsP4);
            SceneManager.LoadScene("SongEnd");
        }
    }

    // checks if sung node hits reference node
    private bool hitNode(Node sung, Node toHit)
    {
        // get distance between node enums
        int distance = 0;
        if (sung > toHit)
        {
            distance = (int)sung - (int)toHit;
        }
        else
        {
            distance = (int)toHit - (int)sung;
        }
        // check if hit
        if (distance <= (int)GameState.difficulty || distance >= 12 - (int)GameState.difficulty)
        {
            return true;
        }
        return false;
    }
}