using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public class VideoPlayer : MonoBehaviour
{
    public UnityEngine.Video.VideoClip videoClip;
    UnityEngine.Video.VideoPlayer videoPlayer;
    RenderTexture renderTexture;
    UnityEngine.UI.RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        // Create new texture with video size
        renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 24);
        // Create VideoPlayer with clip and attach it to camera
        GameObject camera = GameObject.Find("MainCamera");
        videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.clip = videoClip;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.Play();
        // Set texture to image
        image = GetComponent<UnityEngine.UI.RawImage>();
        image.texture = renderTexture;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
