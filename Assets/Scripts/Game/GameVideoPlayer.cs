using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public class GameVideoPlayer : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer;
    RenderTexture renderTexture;
    UnityEngine.UI.RawImage image;

    void Awake()
    {
        if (GameState.currentSong.pathToVideo != "")
        {
            // Create VideoPlayer with clip and attach it to camera
            GameObject camera = GameObject.Find("MainCamera");
            videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.url = GameState.currentSong.pathToVideo;
            videoPlayer.targetTexture = renderTexture;
            // Create new texture with video size
            renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 24);
            // Set texture to image
            image = GetComponent<UnityEngine.UI.RawImage>();
            image.texture = renderTexture;
        }
    }
}
