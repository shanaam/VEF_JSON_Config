using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI content;

    //Tooltip that follows the mouse and displays context relevant information. 


    private void OnEnable()
    {
        //Prevents tooltip from being stuck for 1 frame after appearing
        transform.position = Input.mousePosition;
    }

    void Update()
    {
        transform.position = Input.mousePosition;
    }

    public void SetText(string text)
    {
        // Note: Content is null for reason. This line is a workaround to find the text object at runtime.
        GetComponentInChildren<TextMeshProUGUI>().text = text;
        //content.text = text;
    }
}
