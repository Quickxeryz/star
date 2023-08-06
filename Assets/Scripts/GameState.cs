using UnityEngine;
using Classes;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public const int maxPlayer = 6;
    public static GameMode currentGameMode = GameMode.None;
    public static int lastSongIndex;
    public static Settings settings;
    public static SongData currentSong;
    public static int amountPlayer = 1;
    public static int[] currentProfileIndex = { 0, 0, 0, 0, 0, 0 };
    public static List<(string id, Node node)> onlineMicrophones = new();
    public static List<PlayerProfile> profiles = new();
}