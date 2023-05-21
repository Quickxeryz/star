using UnityEngine;
using Classes;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    public static Settings settings;
    public static SongData currentSong;
    public static Difficulty difficulty = Difficulty.Easy;
    public static int amountPlayer = 1;
    public static PlayerProfile[] currentPlayer = new PlayerProfile[] { new PlayerProfile("P1"), new PlayerProfile("P2"), new PlayerProfile("P3"), new PlayerProfile("P4"), new PlayerProfile("P5"), new PlayerProfile("P6") };
    public static List<(string id, Node node)> onlineMicrophones = new List<(string, Node)>();
    public static PlayerProfile[] profiles;
}