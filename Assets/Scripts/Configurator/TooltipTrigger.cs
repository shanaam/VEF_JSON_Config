using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string content;

    public bool visible; 

    //Place this script on anything you want to have a tooltip for.

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(Show());
        visible = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
        visible = false;
    }

    IEnumerator Show()
    {
        yield return new WaitForSeconds(0.5f);
        if (visible)
        {
            TooltipSystem.Show(content);
        }
    }
}
