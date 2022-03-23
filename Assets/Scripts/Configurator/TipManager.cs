using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TipManager : MonoBehaviour
{
    public TextMeshProUGUI text;

    public enum TipType
    {
        Default,
        OpenFile,
        Copy,
        SelectBlock
    }

    readonly static Dictionary<TipType, string> tips = new Dictionary<TipType, string>()
    {
        {TipType.OpenFile, "Click on a Block to select it."},
        {TipType.Copy, "Select a Notch to be able to paste to it."},
        {TipType.SelectBlock, "Shift-click on another block to select multiple blocks at the same time."}
    };

    public void SetTip(TipType tipType = TipType.Default)
    {
        text.text = "Tip: " + tips[tipType];
    }
}
