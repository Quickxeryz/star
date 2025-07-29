using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ChooseVoice : MonoBehaviour
{
    GroupBox[] playerX_TextBox;

    void Start()
    {
        // get UI elements
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button play = root.Q<Button>("Play");
        Button back = root.Q<Button>("Back");
        TemplateContainer[] playerX = new TemplateContainer[GameState.amountPlayer];
        int[] voices = new int[GameState.amountPlayer];
        for (int i = 0; i < playerX.Length; i++)
        {
            playerX[i] = root.Q<TemplateContainer>("Player" + (i + 1).ToString());
        }
        Label[] playerX_Label = new Label[GameState.amountPlayer];
        for (int i = 0; i < playerX_Label.Length; i++)
        {
            playerX_Label[i] = root.Q<Label>("Player" + (i + 1).ToString() + "Text");
        }
        playerX_TextBox = new GroupBox[GameState.amountPlayer];
        for (int i = 0; i < playerX_TextBox.Length; i++)
        {
            playerX_TextBox[i] = playerX[i].Q<GroupBox>("TextBox");
        }
        Button[] playerX_Left = new Button[GameState.amountPlayer];
        for (int i = 0; i < playerX_Left.Length; i++)
        {
            playerX_Left[i] = playerX[i].Q<Button>("Left");
        }
        Button[] playerX_Right = new Button[GameState.amountPlayer];
        for (int i = 0; i < playerX_Right.Length; i++)
        {
            playerX_Right[i] = playerX[i].Q<Button>("Right");
        }
        // set functionality of buttons
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            int iCopy = i;
            playerX_Left[iCopy].clicked += () =>
            {
                if (voices[iCopy] > 0)
                {
                    voices[iCopy]--;
                    SetSingerName(voices, iCopy);
                }
            };
            playerX_Right[iCopy].clicked += () =>
            {
                if (voices[iCopy] < GameState.currentSong.amountVoices - 1)
                {
                    voices[iCopy]++;
                    SetSingerName(voices, iCopy);
                }
            };
        }
        play.clicked += () =>
        {
            for (int i = 0; i < GameState.amountPlayer; i++)
            {
                GameState.currentVoice[i] = voices[i];
            }
            for (int i = GameState.amountPlayer; i < GameState.currentVoice.Length; i++)
            {
                GameState.currentVoice[i] = -1;
            }
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
            SetSingerName(voices, i);
        }
        // set visibility of player settings
        for (int i = 0; i < GameState.amountPlayer; i++)
        {
            playerX_Label[i].visible = true;
            playerX[i].visible = true;
        }
    }

    private void SetSingerName(int[] voices, int index)
    {
        if (GameState.currentSong.singer.Length > voices[index])
        {
            playerX_TextBox[index].text = GameState.currentSong.singer[voices[index]];
        }
        else
        {
            playerX_TextBox[index].text = voices[index].ToString();
        }
    }
}
