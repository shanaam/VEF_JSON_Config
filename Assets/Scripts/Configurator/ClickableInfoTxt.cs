using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class ClickableInfoTxt : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject InfoText;
    //public BlockPanel bp;

    public UnityEvent<int> OnClick = new UnityEvent<int>();

    public UnityEvent<int> OnHover = new UnityEvent<int>();

    public bool hovered = true;


    /// <summary>
    /// Executes when user clicks on a ui object that this script is attached to. 
    /// If the mouse clicks on a TMP link, then invoke the OnClick event.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        var text = InfoText.GetComponent<TextMeshProUGUI>();
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            if (linkIndex > -1)
            {
                var linkInfo = text.textInfo.linkInfo[linkIndex];
                var linkId = linkInfo.GetLinkID();


                OnClick.Invoke(int.Parse(linkId));
                //bp.OnClickOption(int.Parse(linkId));
            }
        }

    }

    void Update()
    {
        /// If the mouse hovers on a TMP link, then invoke the OnHover event.
        if (hovered)
        {
            var text = InfoText.GetComponent<TextMeshProUGUI>();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            if (linkIndex > -1)
            {
                var linkInfo = text.textInfo.linkInfo[linkIndex];
                var linkId = linkInfo.GetLinkID();


                OnHover.Invoke(int.Parse(linkId));
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        OnHover.Invoke(-1);
    }
}
