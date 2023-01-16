using UnityEngine;
using Classes;

public class GameState : MonoBehaviour
{
    public static string choosenVideoPath = "C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets\\Disney's Eiskönigin 2 - Wo noch niemand war\\Disney's Eiskönigin 2 - Wo noch niemand war.mp4";
    public static string choosenSongPath = "C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets\\Disney's Eiskönigin 2 - Wo noch niemand war\\Disney's Eiskönigin 2 - Wo noch niemand war.txt";
    public static Difficulty difficulty = Difficulty.Easy;
    // mic input delay in sec
    public static float micDelay = 0.3f;
    public static int amountPlayer = 1;
    public static Player player1 = new Player("P1");
    public static Player player2 = new Player("P2");
    public static Player player3 = new Player("P3");
    public static Player player4 = new Player("P4");
}