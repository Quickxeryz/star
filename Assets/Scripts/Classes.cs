using System;

namespace Classes
{
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
        public static Node getNode(int nodeNumber)
        {
            Node node;
            while (nodeNumber < 0)
            {
                nodeNumber += 12;
            }
            nodeNumber = nodeNumber % 12;
            switch (nodeNumber)
            {
                case 0:
                    node = Node.C;
                    break;
                case 1:
                    node = Node.CH;
                    break;
                case 2:
                    node = Node.D;
                    break;
                case 3:
                    node = Node.DH;
                    break;
                case 4:
                    node = Node.E;
                    break;
                case 5:
                    node = Node.F;
                    break;
                case 6:
                    node = Node.FH;
                    break;
                case 7:
                    node = Node.G;
                    break;
                case 8:
                    node = Node.GH;
                    break;
                case 9:
                    node = Node.A;
                    break;
                case 10:
                    node = Node.AH;
                    break;
                case 11:
                    node = Node.B;
                    break;
                default:
                    node = Node.None;
                    break;
            }
            return node;
        }
    }

    public enum Difficulty
    {
        Easy = 2,
        Normal = 1,
        Hard = 0
    }

    public class Player : IComparable
    {
        public string name;
        public int points = 0;

        public Player(string name)
        {
            this.name = name;
        }

        public int CompareTo(object obj)
        {
            Player Temp = (Player)obj;
            if (this.points < Temp.points)
                return 1;
            if (this.points > Temp.points)
                return -1;
            else
                return 0;
        }
    }

    public class SongData
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

        public bool isPlaying()
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

        public double getTime()
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

    public class Settings
    {
        public string absolutePathToSongs;
        public float microphoneDelayInSeconds;

        public Settings(string path, float delayInSeconds)
        {
            absolutePathToSongs = path;
            microphoneDelayInSeconds = delayInSeconds;
        }
    }
}
