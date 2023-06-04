using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
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
    public MicrophoneInput microphoneInput;
    // Video
    public GameVideoPlayer video;
    // song player
    SongPlayer songPlayer;
    bool songPlayerNotSet = true;
    bool notLoadedMP3 = true;
    // Songfile data extraction
    ArrayList songData = new ArrayList(); // Todo: writing own class with better performance
                                          // syllables data
    ArrayList syllablesLine1 = new ArrayList();
    ArrayList syllablesLine2 = new ArrayList();
    // node line data
    ArrayList[] nodesLines1 = new ArrayList[GameState.amountPlayer];
    ArrayList[] nodesLines2 = new ArrayList[GameState.amountPlayer];
    // songData index pointer
    int songDataCurrentIndex = 0;
    int songDataNewLineIndex = 0;
    // beat pointer
    int startBeatLine1 = 0;
    int startBeatLine2 = 0;
    int endBeatLine2 = 0;
    // UI pointer
    Label textLine1Bottom;
    Label textLine2Bottom;
    Label textLine1Top;
    Label textLine2Top;

    VisualElement[] nodeArrows = new VisualElement[GameState.amountPlayer];
    VisualElement[] nodeBoxes = new VisualElement[GameState.amountPlayer];
    Label[] pointsTexts = new Label[GameState.amountPlayer];
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
    int[] lastBeats = new int[GameState.amountPlayer];
    float[] points = new float[GameState.amountPlayer];

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
                    syllable.node = NodeFunctions.getNodeFromInt(int.Parse(temp.Substring(0, temp.IndexOf(' '))));
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
                    syllable.node = NodeFunctions.getNodeFromInt(int.Parse(temp.Substring(0, temp.IndexOf(' '))));
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
                    syllable.node = NodeFunctions.getNodeFromInt(int.Parse(temp.Substring(0, temp.IndexOf(' '))));
                    syllable.syllable = temp.Substring(temp.IndexOf(' ') + 1);
                    songData.Add(syllable);
                    break;
                // Line break
                case '-': // TODO Does only works for "- newLineTime" and not for "- deleteLineTime newLineTime"
                    temp = line.TrimEnd();
                    // Handle "- newLineTime" and "- deleteLineTime newLineTime"
                    if (temp.IndexOf(' ') == temp.LastIndexOf(' '))
                    {
                        songData.Add(int.Parse(temp.Substring(2)));
                    }
                    else
                    {
                        songData.Add(int.Parse(temp.Substring(temp.LastIndexOf(' ') + 1)));
                    }
                    break;
                default:
                    break;
            }
        }
        // set ui screen
        VisualElement r = GetComponent<UIDocument>().rootVisualElement;
        TemplateContainer root = new TemplateContainer();
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
        textLine1Bottom = currentContainer.Q<Label>("SongLine1");
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
            roots[i].Q<Label>("Name").text = GameState.currentPlayer[i].name;
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
                    textLine1Bottom.text = text;
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
            textLine1Top.text = textLine1Bottom.text;
            textLine2Top.text = textLine2Bottom.text;
        }
        // setting nodes of first line
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            nodesLines1[i] = new ArrayList();
        }
        int index = 0;
        beatSumLine1 = startBeatLine2;
        float currentPercent = 0f;
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
                nodeBox.style.width = Length.Percent(((sData.appearing + sData.length) * 100) / beatSumLine1 - currentPercent);
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
            nodesLines2[i] = new ArrayList();
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
        pointsPerBeat = 10000f / (float)beatSum;
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
                    StartCoroutine(loadAudioFile());
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
            if ((!songPlayer.currentPlayerIsAudioSource && (songPlayer.isPlaying() || songPlayer.getTime() == 0)) || (songPlayer.currentPlayerIsAudioSource == true && songPlayer.isPlaying()))
            {
                double currentTime = songPlayer.getTime() - GameState.settings.microphoneDelayInSeconds - GameState.currentSong.gap;
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
                        textLine1Bottom.text = text;
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

                                    if (microphoneInput.nodes[i] != Node.None && hitNode(microphoneInput.nodes[i], sData.node))
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
                        textLine1Bottom.text = textLine2Bottom.text;
                        if (GameState.amountPlayer > 1)
                        {
                            textLine1Top.text = textLine2Top.text;
                        }
                        syllablesLine1 = (ArrayList)syllablesLine2;
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            nodesLines1[i] = (ArrayList)nodesLines2[i];
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
                        syllablesLine2 = new ArrayList();
                        for (int i = 0; i < GameState.amountPlayer; i++)
                        {
                            nodesLines2[i] = new ArrayList();
                        }
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
                    for (int i = 0; i < GameState.amountPlayer; i++)
                    {
                        nodeArrows[i].style.left = Length.Percent(((currentBeat + GameState.currentSong.gap - startBeatLine1) * 100) / beatSumLine1 - nodeArrowWidth);
                    }
                }
                else
                {
                    // Updating player node arrow
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
                    GameState.currentPlayer[i].points = (int)System.Math.Ceiling(points[i]);
                }
                SceneManager.LoadScene("SongEnd");
            }
        }
    }

    private IEnumerator loadAudioFile()
    {
        // setting up audio source object
        GameObject camera = GameObject.Find("MainCamera");
        AudioSource audio = camera.AddComponent<AudioSource>();
        // get audio file per request
        UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + GameState.currentSong.pathToMusic, AudioType.MPEG);
        yield return req.SendWebRequest();
        audio.clip = DownloadHandlerAudioClip.GetContent(req);
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