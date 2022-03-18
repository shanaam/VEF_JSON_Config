using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    public int NumActivePopups { get; private set; }

    public GameObject PopupPrefab;

    private Vector3 PopUpPosition = new Vector3(0, -300, 0);

    public float PopUpTime = 5f;

    public GameObject PopUpLayout;

    public enum MessageType
    {
        Positive,
        Negative,
        Neutral
    }

    readonly static Dictionary<MessageType, Color> colors = new Dictionary<MessageType, Color>()
    {
        {MessageType.Positive, new Color(0.5f, 1, 0.5f)},
        {MessageType.Negative, new Color(1f, 0.5f, 0.5f)},
        {MessageType.Neutral, new Color(0.5f, 0.5f, 0.5f)}
    };

    public void ShowPopup(string message, MessageType type = MessageType.Neutral)
    {
        NumActivePopups++;
        GameObject Popup = Instantiate(PopupPrefab, PopUpLayout.transform);
        //Popup.GetComponent<RectTransform>().anchoredPosition = PopUpPosition + Vector3.up * (NumActivePopups - 1) * 50;
        Popup.GetComponentInChildren<TextMeshProUGUI>().text = message;
        Popup.GetComponentInChildren<Image>().color = colors[type];
        Destroy(Popup, PopUpTime);
        StartCoroutine(DecrementPopups());
    }

    IEnumerator DecrementPopups()
    {
        yield return new WaitForSeconds(PopUpTime);
        NumActivePopups--;
    }
}
