using UnityEngine;
using Classes;

public class GameState : MonoBehaviour
{
    public static Settings settings;
    public static SongData currentSong;
    public static Difficulty difficulty = Difficulty.Easy;
    public static int amountPlayer = 1;
    public static Player[] player = new Player[] { new Player("P1"), new Player("P2"), new Player("P3"), new Player("P4"), new Player("P5"), new Player("P6") };
}