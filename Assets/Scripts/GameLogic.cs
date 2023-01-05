using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;

public class GameLogic : MonoBehaviour
{
    enum Kind
    {
        Free,
        Points,
        Golden,
    }

    class SyllableData
    {


        public Kind kind;
        public int appearing;
        public int length;
        public int pitch;
        public string syllable;
        public SyllableData()
        {
            kind = Kind.Free;
            appearing = 0;
            length = 0;
            pitch = 0;
            syllable = "";
        }
    }
    // Mic input
    public MicrophoneInput micIn;
    // Songfile data extraction
    float beatDuration;
    ArrayList songData = new ArrayList();
    int songDataCurrentIndex = 0;
    int songDataNewLineIndex = 0;
    // UI pointer
    VisualElement root;
    Label textLine1;
    Label textLine2;
    VisualElement nodeP1; // node start at H = -25 and downwards with position.top += 25
    // node default value
    const int nodeTopDefault = -25;
    // start time for beat calculation
    DateTime startTime;

    void Start()
    {
        // Getting data from song file
        string[] songFileData = System.IO.File.ReadAllLines(GameState.choosenSongPath);
        SyllableData syllable;
        float bpm = 0;
        string temp;
        foreach (string line in songFileData)
        {
            syllable = new SyllableData();
            switch (line[0])
            {
                // Description
                case '#':
                    // Reading BPM
                    if (line.Contains("BPM"))
                    {
                        bpm = float.Parse(line.Substring(line.IndexOf(':') + 1).Trim());
                    }
                    break;
                // Normal note
                case ':':
                    syllable.kind = Kind.Points;
                    temp = line.Substring(2);
                    syllable.appearing = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.length = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
                    temp = temp.Substring(temp.IndexOf(' ') + 1);
                    syllable.pitch = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
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
                    syllable.pitch = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
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
                    syllable.pitch = int.Parse(temp.Substring(0, temp.IndexOf(' ')));
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
        // Getting UI pointer
        root = GetComponent<UIDocument>().rootVisualElement;
        textLine1 = root.Q<Label>("SongLine1");
        textLine2 = root.Q<Label>("SongLine2");
        // Getting player node arrows
        nodeP1 = root.Q<VisualElement>("NodeP1");
        // beat duration = 60000 ms / BPM
        beatDuration = 60000f / bpm;
        startTime = DateTime.Now;
        //Getting first song lines
        int textCounter = 1;
        string text = "";
        while (textCounter < 3)
        {
            if (songData[songDataNewLineIndex].GetType() == typeof(SyllableData))
            {
                text += ((SyllableData)songData[songDataNewLineIndex]).syllable;
            }
            else
            {
                if (textCounter == 1)
                {
                    textLine1.text = text;
                }
                else
                {
                    textLine2.text = text;
                }
                text = "";
                textCounter++;
            }
            songDataNewLineIndex++;
        }
    }

    void Update()
    {
        // updating nodes, songtext and calculating score
        double time = (DateTime.Now - startTime).TotalMilliseconds;
        bool search = true;
        while (search)
        {
            if (songData[songDataCurrentIndex].GetType() == typeof(SyllableData))
            {

            }
            else
            {
                //Loading new line

            }
        }
        // Updating player node arrow
        nodeP1.style.top = nodeTopDefault + 25 * (int)micIn.node;
    }
}
/*
FORMAT:
#TITLE: Title of the song
#ARTIST: Artist behind the song
#MP3: The name of the MP3 being used for this song. Must have a .mp3 extension included here
#GAP: The amount of time, in milliseconds, before the lyrics start. This allows for any instrumental (or other type of) introduction to the song. It is important to note the number of the first note below. If it is not 0 (which is rare) then the #GAP will be less straightforward. If the lyrics aren’t set to start until 8 beats into the song, but the singing starts straight away, then the #GAP may need to be set to a negative number, to force the lyrics to start early.
#BPM: Beats per minute. This signifies the rate at which the text should display. Put simply, fast songs have a higher BPM, slow songs have a lower BPM. To complicate it slightly, the BPM can be upped for slower songs as long as more beats are added in the main body of the song below. If the BPM of a song is high then it generally means a good, smooth .txt file with more attention to subtle changes in tone. But if that means nothing to you, then you don’t need to worry about this tag. If it is a good .txt file, then it won’t need changing.
#GENRE: The genre of the song. As UltraStar has a ‘sort by genre’ option, it’s a useful tag to use. That, and the search option uses the word(s) in the #GENRE tag when you’re on the song selection screen, so you can automatically find all ‘rock’ songs, for example, if you use this tag.
#EDITION: Typically refers to the SingStar edition, if applicable, that the .txt file is taken from. For organisational purposes, it’s good to leave this tag in.
#COVER: Typically the single/album art appropriate for the song, to be displayed on the song selection screen. This is not necessary but it does brighten up the look of the game (and makes certain songs identifiable when not selected). This must be in .jpg format and the .jpg extension must be displayed here.
#VIDEO: The name of the video file used for this song. Must have the file extension included out of the many types of video file that UltraStar accepts.
#BACKGROUND: If you don’t have a video file, then you may prefer to have a background image displayed instead of a plain background or visualization. This must be in .jpg format and should have the .jpg extension attached. If the song is set to have a #VIDEO file and is linked in properly, then this tag is disregarded. If the .txt is set to have a #VIDEO but the video is not linked in properly for whatever reason, then the game will automatically display the background image.
#RELATIVE: This is an unusual tag that I will talk about later. It is simply set to YES or NO. If it is set to YES, then it specifies a particular format of .txt file that functions in a different way to a typical .txt file. If the tag is absent, or is set to NO, then the .txt file functions as the others do. It is essential for this tag to be applied on a relative .txt file (these are rare. If you find one on USDB then the tag will be readily applied anyway).
First col
: Regular note
* Golden note
F Freestyle syllable
– Line break (separates lyrics into suitable lines).
Line breaks are different to other types of row, in that they consist of a hyphen ( – ) and either one or two numbers. If it contains one number, it determines the beat at which the previous line will disappear. For example, in the first line of the song above, the ‘Teenage dreams’ line disappears as soon as it’s been sung, on beat 12. If the line break contains 2 numbers, the first number determines when the first line disappears, and the second determines when the next line will appear. There is no example of this type of line above, as it’s a fast moving song with no proper breaks from singing – line breaks containing two numbers are generally for songs with a large instrumental break in them. Two numbers aren’t at all necessary, however, as the game automatically puts the next line up when it is approaching – it’s only if you want to control when it happens that you need to worry about the ‘second’ number.
Second col
appearing of syllable
Third col
length of syllable
Fourth col
pitch (0 = c1 (negative possible))
Fifth col
text 
*/