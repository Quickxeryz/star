using UnityEngine;
using Classes;

public class GameState : MonoBehaviour
{
    public static string choosenVideoPath = "C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets\\Disney's Eiskönigin 2 - Wo noch niemand war\\Disney's Eiskönigin 2 - Wo noch niemand war.mp4";
    public static string choosenSongPath = "C:\\Users\\Maurice\\Documents\\GitHub\\star\\Assets\\TestAssets\\Disney's Eiskönigin 2 - Wo noch niemand war\\Disney's Eiskönigin 2 - Wo noch niemand war.txt";
    public static Difficulty difficulty = Difficulty.Easy;
    // mic input delay in sec
    public static float micDelay = 0.0f;
    public static string namePlayer1 = "Maurice";
    public static string namePlayer2 = "P2";
    public static string namePlayer3 = "P3";
    public static string namePlayer4 = "P4";
}