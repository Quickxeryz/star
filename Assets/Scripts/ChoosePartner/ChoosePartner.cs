using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChoosePartner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // get UI elements
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button play = root.Q<Button>("Play");
        Button back = root.Q<Button>("Back");
        TemplateContainer[] partnerX = new TemplateContainer[GameState.amountPlayer];
        for (int i = 0; i < partnerX.Length; i++)
        {
            partnerX[i] = root.Q<TemplateContainer>("Partner" + (i + 1).ToString());
        }
        Label[] playerX_Label = new Label[GameState.amountPlayer];
        for (int i = 0; i < playerX_Label.Length; i++)
        {
            playerX_Label[i] = root.Q<Label>("Player" + (i + 1).ToString() + "Text");
        }
        GroupBox[] partnerX_TextBox = new GroupBox[GameState.amountPlayer];
        for (int i = 0; i < partnerX_TextBox.Length; i++)
        {
            partnerX_TextBox[i] = partnerX[i].Q<GroupBox>("TextBox");
        }
        Button[] partnerX_Left = new Button[GameState.amountPlayer];
        for (int i = 0; i < partnerX_Left.Length; i++)
        {
            partnerX_Left[i] = partnerX[i].Q<Button>("Left");
        }
        Button[] partnerX_Right = new Button[GameState.amountPlayer];
        for (int i = 0; i < partnerX_Right.Length; i++)
        {
            partnerX_Right[i] = partnerX[i].Q<Button>("Right");
        }
        // set functionality of buttons
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            int iCopy = i;
            partnerX_Left[iCopy].clicked += () =>
            {
                if (GameState.currentSecondProfileIndex[iCopy] > 0)
                {
                    GameState.currentSecondProfileIndex[iCopy]--;
                    partnerX_TextBox[iCopy].text = GameState.profiles[GameState.currentSecondProfileIndex[iCopy]].name;
                }
            };
            partnerX_Right[iCopy].clicked += () =>
            {
                if (GameState.currentSecondProfileIndex[iCopy] < GameState.profiles.Count - 1)
                {
                    GameState.currentSecondProfileIndex[iCopy]++;
                    partnerX_TextBox[iCopy].text = GameState.profiles[GameState.currentSecondProfileIndex[iCopy]].name;
                }
            };
        }
        play.clicked += () =>
        {
            SceneManager.LoadScene("GameScene");
        };
        back.clicked += () =>
        {
            SceneManager.LoadScene("ChooseSong");
        };
        // init text
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            playerX_Label[i].text = GameState.profiles[GameState.currentProfileIndex[i]].name;
            if (GameState.currentSecondProfileIndex[i] < GameState.profiles.Count)
            {
                partnerX_TextBox[i].text = GameState.profiles[GameState.currentSecondProfileIndex[i]].name;
            }
            else
            {
                partnerX_TextBox[i].text = GameState.profiles[0].name;
                GameState.currentSecondProfileIndex[i] = 0;
            }
        }
        // set visibility of player settings
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            playerX_Label[i].visible = true;
            partnerX[i].visible = true;
        }
    }
}
