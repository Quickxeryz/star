using UnityEngine;
using Classes;

public class GameState : MonoBehaviour
{
    public static Settings settings;
    public static SongData currentSong;
    public static Difficulty difficulty = Difficulty.Easy;
    public static int amountPlayer = 1;
    public static Player player1 = new Player("P1");
    public static Player player2 = new Player("P2");
    public static Player player3 = new Player("P3");
    public static Player player4 = new Player("P4");
}