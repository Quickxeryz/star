using UnityEngine;
using Classes;

public class GameState : MonoBehaviour
{
    // path with / or \\
    public static string songFolderPath = "C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets";
    public static SongData currentSong = new SongData("C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets\\Disney's Eiskönigin 2 - Wo noch niemand war\\Disney's Eiskönigin 2 - Wo noch niemand war.txt", "Wo war", "Elsa", "C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets\\Disney's Eiskönigin 2 - Wo noch niemand war\\Disney's Eiskönigin 2 - Wo noch niemand war.mp4", 247.08f, 0);
    public static Difficulty difficulty = Difficulty.Easy;
    // mic input delay in sec
    public static float micDelay = 0.3f;
    public static int amountPlayer = 1;
    public static Player player1 = new Player("P1");
    public static Player player2 = new Player("P2");
    public static Player player3 = new Player("P3");
    public static Player player4 = new Player("P4");
}