using UnityEngine;
using Classes;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public const int maxPlayer = 6;
    public static List<SongData> songs;
    public static bool songsLoaded = false; 
    public static List<(string id, Node node)> onlineMicrophones = new();
    public static int amountPlayer = 1;
    public static int lastSongIndex;
    public static int[] currentProfileIndex = new int[maxPlayer];
    public static int[] currentSecondProfileIndex = new int[maxPlayer];
    public static int[] currentVoice = new int[maxPlayer];
    public static SongData currentSong;
    public static PartyMode currentPartyMode = PartyMode.None;
    public static GameMode currentGameMode = GameMode.None;
    public static List<SongData> partyModeSongs;
    public static int roundsLeft;
    public static List<Team> teams;
    public static List<int>[] playersPlayed;
    public static bool serverStarted = false;
    public static string ip;
    public static List<PlayerProfile> profiles = new();
    public static Settings settings;
}