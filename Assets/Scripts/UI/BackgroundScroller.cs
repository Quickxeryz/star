using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private RawImage img;
    [SerializeField] private float speed = 1;
    [SerializeField] private float xMultiplyer = 1;
    [SerializeField] private float yMultiplyer = 1;
    private float x;
    private float y;

    void Start()
    {
        x = speed * 0.01f;
        y = speed * 0.01f;
    }
    
    void Update()
    {
        img.uvRect = new Rect(img.uvRect.position + new Vector2(x * xMultiplyer, y * yMultiplyer) * Time.deltaTime, img.uvRect.size);    
    }
}
