using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Classes
{
    public enum PartyMode
    {
        None = -1,
        RandomGameMode = 0,
        Duet = 1,
        ChooseSong = 2,
        Classic = 3,       
        Meow = 4,
        Team = 5,
        Item = 6,
        Random = 7,
        Together = 8,
    }

    public static class PartyModeFunctions
    {
        public static void SetUpNextGameMode()
        {
            if (GameState.currentGameMode == GameMode.Duet)
            {
                GameState.amountPlayer /= 2;
            }
            bool ok = true;
            // check if duet is playable
            if (GameState.amountPlayer * 2 > GameState.maxPlayer)
            {
                ok = false;
            }
            if (ok)
            {
                // check for amount team member
                foreach (Team t in GameState.teams)
                {
                    if (t.players.Count < 2)
                    {
                        ok = false;
                    }
                }
            }
            int min = 1;
            if (!ok)
            {
                min = 2;
            }
            ok = true;
            // check if together is playable
            foreach (Team t in GameState.teams)
            {
                if (t.players.Count < 2)
                {
                    ok = false;
                }
                foreach (PlayerProfile p in t.players)
                {
                    if (p.useOnlineMic == false)
                    {
                        ok = false;
                    }
                }
            }
            int max = 9;
            if (!ok)
            {
                max = 8;
            }
            GameState.currentGameMode = (GameMode)UnityEngine.Random.Range(min, max);
            GameModeFunctions.SetUpGameMode();
        }
    }

    public enum GameMode
    {
        None = -1,
        Classic = 0,
        Duet = 1,
        Together = 2,
        Meow = 3,
        Team = 4,
        Item = 5,
        Random = 6,
    }

    public static class GameModeFunctions
    {
        public static GameMode StringToGameMode(string str)
        {
            switch (str)
            {
                case "Classic":
                    return GameMode.Classic;
                case "Duet":
                    return GameMode.Duet;
                case "Together":
                    return GameMode.Together;
                case "Meow":
                    return GameMode.Meow;
                case "Team":
                    return GameMode.Team;
                case "Item":
                    return GameMode.Team;
                case "Random":
                    return GameMode.Random;
                default:
                    return GameMode.None;
            }
        }

        public static string GameModeToString(GameMode gm)
        {
            switch (gm)
            {
                case GameMode.None:
                    return "None";
                case GameMode.Classic:
                    return "Classic";
                case GameMode.Duet:
                    return "Duet";
                case GameMode.Together:
                    return "Together";
                case GameMode.Meow:
                    return "Meow";
                case GameMode.Team:
                    return "Team";
                case GameMode.Item:
                    return "Item";
                case GameMode.Random:
                    return "Random";
                default:
                    return "ERROR";
            }
        }

        public static void SetUpGameMode()
        {
            GameState.partyModeSongs = new List<SongData>();
            switch (GameState.currentGameMode)
            {
                case GameMode.Classic:
                case GameMode.Together:
                case GameMode.Meow:
                case GameMode.Team:
                case GameMode.Item:
                case GameMode.Random:
                    // all songs 
                    foreach (SongData song in GameState.songs)
                    {
                        GameState.partyModeSongs.Add(song);
                    }
                    // update voices
                    for (int i = 0; i < GameState.amountPlayer; i++)
                    {
                        GameState.currentVoice[i] = 0;
                    }
                    break;
                case GameMode.Duet:
                    GameState.amountPlayer *= 2;
                    // exclude only main singer songs
                    foreach (SongData song in GameState.songs)
                    {
                        if (song.amountVoices > 1)
                        {
                            GameState.partyModeSongs.Add(song);
                        }
                    }
                    // update voices
                    for (int i = 0; i < GameState.amountPlayer; i += 2)
                    {
                        GameState.currentVoice[i] = 0;
                    }
                    for (int i = 1; i < GameState.amountPlayer; i += 2)
                    {
                        GameState.currentVoice[i] = 1;
                    }
                    break;
            }
            for (int i = GameState.amountPlayer; i < GameState.currentVoice.Length; i++)
            {
                GameState.currentVoice[i] = -1;
            }
        }
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
        public List<PlayerProfile> players = new();
        public string name = "";
        public int points = 0;
        public int amountRerolls = 0;
        public int amountSwitches = 0;
        public List<PlayerProfile> playersNotSung = new();

        public Team(string name)
        {
            this.name = name;            
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
        
        public string TeamToString()
        {
            string res = "";
            res += "Name: " + this.name + "\n";
            res += "Player: ";
            foreach (PlayerProfile p in this.players)
            {
                res += p.name + " ";
            }
            res += "\n";
            res += "Points: " + this.points.ToString() + "\n";
            res += "Amount rerolls: " + this.amountRerolls.ToString() + "\n";
            res += "Amount switches: " + this.amountSwitches.ToString() + "\n";
            res += "Players not sung: ";
            foreach (PlayerProfile p in this.playersNotSung)
            {
                res += p.name + " ";
            }
            return res;
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
        public string[] singer = null;

        public SongData(string path, string title, string artist, string pathToMusic, float bpm, float gap, int amountVoices, string[] singer)
        {
            this.path = path;
            this.title = title;
            this.artist = artist;
            this.pathToMusic = pathToMusic;
            this.bpm = bpm;
            this.gap = gap;
            this.amountVoices = amountVoices;
            if (singer != null)
            {
                this.singer = singer;
            }
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
        public bool started = false;

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
                return audioSource.clip.length - (audioSource.time + 0.1) <= 0 || (started && audioSource.time == 0);
            }
            else
            {
                return (videoPlayer.frameCount / videoPlayer.frameRate) - (videoPlayer.time + 0.1) <= 0 || (started && videoPlayer.time == 0);
            }
        }

        public void Pause() {
            if (currentPlayerIsAudioSource)
            {
                audioSource.Pause();
            }
            else
            {
                videoPlayer.Pause();
            }
        }
        public void Unpause() {
            if (currentPlayerIsAudioSource)
            {
                audioSource.UnPause();
            }
            else
            {
                videoPlayer.Play();
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
