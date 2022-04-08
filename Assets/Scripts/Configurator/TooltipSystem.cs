using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem current;

    public Tooltip tooltip;

    //This script controls whether the tooltip is active or not active. 

    void Awake()
    {
        current = this;
    }

    public static void Show(string text)
    {
        current.tooltip.gameObject.SetActive(true);
        current.tooltip.SetText(text);
    }

    public static void Hide()
    {
        current.tooltip.gameObject.SetActive(false);
    }
}
