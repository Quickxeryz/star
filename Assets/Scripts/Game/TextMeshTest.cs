using System;
using TMPro;
using UnityEngine;

public class Textmeshtest : MonoBehaviour
{
    TextMeshPro word1TM;
    MeshRenderer word1Ren;
    DateTime lastTimePressed = DateTime.Now;
    Vector2 offset = new Vector2(1f, 0f);

    void Start()
    {
        // create object
        GameObject word1 = new("Word1");
        // set up text mesh
        word1TM = word1.AddComponent<TextMeshPro>();
        word1TM.text = "example";
        word1TM.fontSize = 14;
        word1TM.rectTransform.position = new Vector3(0f, -5f, 0f);
        word1TM.horizontalMapping = TextureMappingOptions.Line;
        // set up materials
        Material[] word1Mats = gameObject.GetComponent<MeshRenderer>().materials;
        word1Ren = word1.GetComponent<MeshRenderer>();
        word1Ren.materials = word1Mats;
        // create object 2
        GameObject word2 = new("Word2");
        // set up text mesh
        TextMeshPro word2TM = word2.AddComponent<TextMeshPro>();
        word2TM.text = "something";
        word2TM.fontSize = 14;
        word2TM.horizontalMapping = TextureMappingOptions.Line;
        // calculate word 2 start
        word1TM.ForceMeshUpdate();
        word2TM.rectTransform.position = new Vector3(word1TM.rectTransform.position.x + word1TM.renderedWidth, -5f, 0);
        // set up materials
        Material[] word2Mats = gameObject.GetComponent<MeshRenderer>().materials;
        MeshRenderer word2Ren = word2.GetComponent<MeshRenderer>();
        word2Ren.materials = word2Mats;
        word2Ren.materials[1].SetTextureOffset("_FaceTex", new Vector2(0f, 0f));
    }

    void Update()
    {
        if (DateTime.Now.Subtract(lastTimePressed).TotalMilliseconds > 100)
        {
            if(offset.x <= 0f)
            {
                offset.x = 1f;
            } else
            {
                offset.x -= 0.01f;
            }
            word1Ren.materials[1].SetTextureOffset("_FaceTex", offset);
            lastTimePressed = DateTime.Now;
        }
    }
}
