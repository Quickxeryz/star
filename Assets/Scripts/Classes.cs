using System;
using System.Collections;
using System.Runtime.ConstrainedExecution;

namespace Classes
{
    public enum GameMode
    {
        None,
        ChooseSong
    }

    public enum Node
    {
        C = 11,
        CH = 10,
        D = 9,
        DH = 8,
        E = 7,
        F = 6,
        FH = 5,
        G = 4,
        GH = 3,
        A = 2,
        AH = 1,
        B = 0,
        None = -1
    }

    public static class NodeFunctions
    {
        public static Node GetNodeFromString(string nodeString)
        {
            Node node = nodeString switch
            {
                "C" => Node.C,
                "CH" => Node.CH,
                "D" => Node.D,
                "DH" => Node.DH,
                "E" => Node.E,
                "F" => Node.F,
                "FH" => Node.FH,
                "G" => Node.G,
                "GH" => Node.GH,
                "A" => Node.A,
                "AH" => Node.AH,
                "B" => Node.B,
                _ => Node.None,
            };
            return node;
        }

        public static Node GetNodeFromInt(int nodeNumber)
        {
            while (nodeNumber < 0)
            {
                nodeNumber += 12;
            }
            nodeNumber %= 12;
            Node node = nodeNumber switch
            {
                0 => Node.C,
                1 => Node.CH,
                2 => Node.D,
                3 => Node.DH,
                4 => Node.E,
                5 => Node.F,
                6 => Node.FH,
                7 => Node.G,
                8 => Node.GH,
                9 => Node.A,
                10 => Node.AH,
                11 => Node.B,
                _ => Node.None,
            };
            return node;
        }
    }

    public enum Difficulty
    {
        Easy = 2,
        Normal = 1,
        Hard = 0
    }

    public class DifficultyFunctions
    {
        public static Difficulty StringToDifficulty(string difficulty)
        {
            return difficulty switch
            {
                "Easy" => Difficulty.Easy,
                "Normal" => Difficulty.Normal,
                "Hard" => Difficulty.Hard,
                _ => Difficulty.Easy,
            };
        }
    }

    [System.Serializable]
    public class PlayerProfile : IComparable
    {
        public string name;
        public int points = 0;
        public Difficulty difficulty = Difficulty.Easy;

        public PlayerProfile(string name)
        {
            this.name = name;
        }

        public int CompareTo(object obj)
        {
            PlayerProfile Temp = (PlayerProfile)obj;
            if (points < Temp.points)
                return 1;
            if (points > Temp.points)
                return -1;
            else
                return 0;
        }
    }

    // json wrapper class to read PlayerProfile[]
    public class JsonPlayerProfiles
    {
        public PlayerProfile[] playerProfiles;
        public JsonPlayerProfiles(PlayerProfile[] playerProfiles)
        {
            this.playerProfiles = playerProfiles;
        }
    }

    public class SongData : IComparable
    {
        public string path;
        public string title;
        public string artist;
        public string pathToMusic;
        public float bpm = 0f;
        public float gap = 0f;
        public string pathToVideo = "";

        public SongData(string path, string title, string artist, string pathToMusic, float bpm, float gap)
        {
            this.path = path;
            this.title = title;
            this.artist = artist;
            this.pathToMusic = pathToMusic;
            this.bpm = bpm;
            this.gap = gap;
        }

        public int CompareTo(object obj)
        {
            SongData otherSong= (SongData)obj;
            if (artist.CompareTo(otherSong.artist) == 0)
            {
                return title.CompareTo(otherSong.title);
            }
            return artist.CompareTo(otherSong.artist);
        }
    }

    public class SongPlayer
    {
        public UnityEngine.AudioSource audioSource;
        public UnityEngine.Video.VideoPlayer videoPlayer;
        public bool currentPlayerIsAudioSource;

        public SongPlayer(UnityEngine.AudioSource audioSource)
        {
            this.audioSource = audioSource;
            currentPlayerIsAudioSource = true;
        }

        public SongPlayer(UnityEngine.Video.VideoPlayer videoPlayer)
        {
            this.videoPlayer = videoPlayer;
            currentPlayerIsAudioSource = false;
        }

        public bool IsPlaying()
        {
            if (currentPlayerIsAudioSource)
            {
                return audioSource.isPlaying;
            }
            else
            {
                return videoPlayer.isPlaying;
            }
        }

        public double GetTime()
        {
            if (currentPlayerIsAudioSource)
            {
                return audioSource.time;
            }
            else
            {
                return videoPlayer.time;
            }
        }
    }

    [System.Serializable]
    public class MicrophoneData
    {
        public string name;
        public int index;
        public int channel;
        public bool isOnline;

        public MicrophoneData()
        {
            name = "";
            index = 0;
            channel = 0;
            isOnline = false;
        }

        public MicrophoneData(string name, int index, int channel, bool isOnline)
        {
            this.name = name;
            this.index = index;
            this.channel = channel;
            this.isOnline = isOnline;
        }

        public bool EqualsWithoutChannel(MicrophoneData mD)
        {
            return (name == mD.name && index == mD.index);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public string absolutePathToSongs;
        public float microphoneDelayInSeconds;
        public MicrophoneData[] microphoneInput;

        public Settings(string path, float delayInSeconds, MicrophoneData[] microphoneInput)
        {
            absolutePathToSongs = path;
            microphoneDelayInSeconds = delayInSeconds;
            this.microphoneInput = microphoneInput;
        }
    }
}
