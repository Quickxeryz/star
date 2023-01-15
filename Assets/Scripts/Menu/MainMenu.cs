using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button play = root.Q<Button>("Play");
        play.clicked += () => SceneManager.LoadScene("GameScene");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
