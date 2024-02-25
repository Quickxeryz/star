using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Classes
{
    public enum GameMode
    {
        None,
        ChooseSong,
        Classic
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
        public bool useOnlineMic = false;
        public string onlineMicName = "";
        public Color color = new(0f, 0f, 255f);

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

    public class Team : IComparable
    {
        public List<PlayerProfile> players;
        public string name = "";
        public int points = 0;
        public int amountRerolls = 0;
        public int amountSwitches = 0;

        public Team(string name)
        {
            this.name = name;
            players = new();
        }
        public int CompareTo(object obj)
        {
            Team Temp = (Team)obj;
            if (points < Temp.points)
                return 1;
            if (points > Temp.points)
                return -1;
            else
                return 0;
        }
    }

    public class SongData : IComparable
    {
        public string path;
        public string title;
        public string artist;
        public string pathToMusic;
        public float bpm = 0f;
        /// <summary>
        /// Gap of the song in seconds
        /// </summary>
        public float gap = 0f;
        public string pathToVideo = "";
        public int amountVoices;

        public SongData(string path, string title, string artist, string pathToMusic, float bpm, float gap, int amountVoices)
        {
            this.path = path;
            this.title = title;
            this.artist = artist;
            this.pathToMusic = pathToMusic;
            this.bpm = bpm;
            this.gap = gap;
            this.amountVoices = amountVoices;
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
        public AudioSource audioSource;
        public VideoPlayer videoPlayer;
        public bool currentPlayerIsAudioSource;

        public SongPlayer(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            currentPlayerIsAudioSource = true;
        }

        public SongPlayer(VideoPlayer videoPlayer)
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

        /// <summary>
        /// Returns the running time of the song in seconds
        /// </summary>
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

        public double GetLength()
        {
            if (currentPlayerIsAudioSource)
            {
                return audioSource.clip.length;
            }
            else
            {
                return videoPlayer.frameCount / videoPlayer.frameRate;
            }
        }

        public bool IsPrepared()
        {
            if (currentPlayerIsAudioSource)
            {
                return true;
            }
            else
            {
                return videoPlayer.isPrepared;
            }
        }

        public bool HasFinished()
        {
            if (currentPlayerIsAudioSource)
            {
                return audioSource.clip.length - audioSource.time < 0;
            }
            else
            {
                return (videoPlayer.frameCount / videoPlayer.frameRate) - (videoPlayer.time + 1) < 0;
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
