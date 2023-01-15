using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public class VideoPlayer : MonoBehaviour
{
    [HideInInspector]
    public UnityEngine.Video.VideoPlayer videoPlayer;
    RenderTexture renderTexture;
    UnityEngine.UI.RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        // Create VideoPlayer with clip and attach it to camera
        GameObject camera = GameObject.Find("MainCamera");
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.url = GameState.choosenVideoPath;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.Play();
        // Create new texture with video size
        renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 24);
        // Set texture to image
        image = GetComponent<UnityEngine.UI.RawImage>();
        image.texture = renderTexture;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
