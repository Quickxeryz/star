using UnityEngine;
using UnityEngine.UIElements;
public class GameLogicPlayer1 : MonoBehaviour
{
    public MicrophoneInput micIn;
    VisualElement nodeP1; // node start at H = -25 and downwards with position.top += 25
    const int nodeTopDefoult = -25;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        nodeP1 = root.Q<VisualElement>("NodeP1");
    }

    void Update()
    {
        nodeP1.style.top = nodeTopDefoult + 25 * (int)micIn.node;
    }
}
