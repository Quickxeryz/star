using UnityEngine;
using Classes;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static GameMode currentGameMode = GameMode.None;
    public static Settings settings;
    public static SongData currentSong;
    public static Difficulty difficulty = Difficulty.Easy;
    public static int amountPlayer = 1;
    public static int[] currentProfileIndex = { 0, 0, 0, 0, 0, 0 };
    public static List<(string id, Node node)> onlineMicrophones = new List<(string, Node)>();
    public static List<PlayerProfile> profiles = new List<PlayerProfile>();
}