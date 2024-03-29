﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BlockPanel : MonoBehaviour
{
    public GameObject PropertySelectionDropdown;
    public GameObject BlockParameterText;
    public GameObject TextInputField;
    public GameObject DropdownInputField;
    public GameObject BlockParameterValue;
    public GameObject UIManager;
    public GameObject BlockInfoText;

    private int index = -1;
    public string selectedParameter;
    public string hoveredParameter;

    private ConfigurationUIManager uiManager;

    public void Start()
    {
        uiManager = UIManager.GetComponent<ConfigurationUIManager>();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Populates dropdown
    public void Populate(int index)
    {
        this.index = index;

        BlockInfoText.GetComponent<TextMeshProUGUI>().text = "Block Properties:\n\n";

        int i = 0;
        List<string> options = new List<string>();
        foreach (KeyValuePair<string, object> kp in uiManager.ExpContainer.Data)
        {
            if (kp.Key.StartsWith("per_block"))
            {
                options.Add(kp.Key);
                i++;
            }
        }
        PropertySelectionDropdown.GetComponent<Dropdown>().ClearOptions();
        PropertySelectionDropdown.GetComponent<Dropdown>().AddOptions(options);

        UpdateBlockPropertyText();

        if (options.Count > 0)
        {
            OnClickOption(0);
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void OnClickOption(int option)
    {
        //Debug.Log(option);
        selectedParameter = PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text;
        //Debug.Log(selectedParameter);
        BlockParameterText.GetComponent<Text>().text = selectedParameter;
        BlockParameterValue.GetComponent<Text>().text = "Value: " +
                                                        (uiManager.ExpContainer.Data[selectedParameter] as List<object>)[index];

        if (uiManager.ExpContainer.GetDefaultValue(
            PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text) is IList)
        {
            TextInputField.SetActive(false);
            DropdownInputField.SetActive(true);

            // Set up options for dropdown
            List<object> list = uiManager.ExpContainer.GetDefaultValue(
                PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text) as List<object>;

            List<string> newList = new List<string>();

            // First option is blank
            newList.Add("");

            foreach (object o in list)
            {
                newList.Add((string)o);
            }

            DropdownInputField.GetComponent<Dropdown>().ClearOptions();
            DropdownInputField.GetComponent<Dropdown>().AddOptions(newList);
        }
        else
        {
            TextInputField.SetActive(true);
            TextInputField.GetComponent<InputField>().text = "";
            DropdownInputField.SetActive(false);
        }

        UpdateBlockPropertyText();
    }

    public void OnHoverOption(int option)
    {
        if (option < 0)
        {
            hoveredParameter = string.Empty;
        }
        else
        {
            hoveredParameter = PropertySelectionDropdown.GetComponent<Dropdown>().options[option].text;
        }
       
        UpdateBlockPropertyText();
    }

    public void OnInputFinishEdit(string text)
    {
        if (index == -1 || text.Length == 0) return;

        UndoRedo.instance.Backup();

        object obj = uiManager.ExpContainer.ConvertToCorrectType(text);

        bool isCorrectType = false;

        switch(uiManager.ExpContainer.GetDefaultValue(selectedParameter))
        {
            case "":
                isCorrectType = true;
                break;

            case 0:
                if (obj.GetType().IsInstanceOfType(0))
                    isCorrectType = true;
                break;

            case false:
                if (obj.GetType().IsInstanceOfType(false))
                    isCorrectType = true;
                break;

            case 0.0f:
                if (obj.GetType().IsInstanceOfType(0) || obj.GetType().IsInstanceOfType(0.0f))
                    isCorrectType = true;

                break;
            default:
                break;
        }


        if (isCorrectType)
        {
            BlockParameterValue.GetComponent<Text>().text = "Value: " + text;

            ConfigurationBlockManager blockManager = uiManager.BlockView.GetComponent<ConfigurationBlockManager>();
            foreach (GameObject g in blockManager.SelectedBlocks)
            {
                ((List<object>)uiManager.ExpContainer.Data[selectedParameter])[g.GetComponent<BlockComponent>().BlockID] = obj;
            }
            UpdateBlockPropertyText();
            uiManager.Dirty = true;
        }
        else
        {
            uiManager.ConfirmationPopup.GetComponent<ConfirmationPopup>().ShowPopup(
                "The input type does not match the correct type for this property.", null);
        }
    }

    public void OnDropdownFinishEdit(int option)
    {
        // If user selected blank, don't edit the parameter
        if (option == 0) return;

        UndoRedo.instance.Backup();

        BlockParameterValue.GetComponent<Text>().text = "Value: " + 
            DropdownInputField.GetComponent<Dropdown>().options[option].text;

        ConfigurationBlockManager blockManager = uiManager.BlockView.GetComponent<ConfigurationBlockManager>();

        foreach (GameObject g in blockManager.SelectedBlocks)
        {
            ((List<object>)uiManager.ExpContainer.Data[selectedParameter])[g.GetComponent<BlockComponent>().BlockID] =
                DropdownInputField.GetComponent<Dropdown>().options[option].text;
        }

        uiManager.BlockView.GetComponent<ConfigurationBlockManager>().ReadjustBlocks();
        UpdateBlockPropertyText();

        uiManager.Dirty = true;
    }

    private void UpdateBlockPropertyText()
    {
        BlockInfoText.GetComponent<TextMeshProUGUI>().text = "Block Properties:\n\n";

        int i = 0;
        foreach (KeyValuePair<string, object> kp in uiManager.ExpContainer.Data)
        {
            if (kp.Key.StartsWith("per_block"))
            {
                if (kp.Key.Equals(selectedParameter))
                    BlockInfoText.GetComponent<TextMeshProUGUI>().text += 
                        "<color=\"black\"><mark><u><link=\"" + i + "\">" + kp.Key + "</color></mark></u> : " + (kp.Value as List<object>)[index] + "</link>\n";
                else if (kp.Key.Equals(hoveredParameter))
                    BlockInfoText.GetComponent<TextMeshProUGUI>().text +=
                        "<color=\"grey\"></mark><u><link=\"" + i + "\">" + kp.Key + "</color></mark></u> : " + (kp.Value as List<object>)[index] + "</link>\n";
                else
                    BlockInfoText.GetComponent<TextMeshProUGUI>().text += 
                        "<u><link=\"" + i + "\">" + kp.Key + "</u> : " + (kp.Value as List<object>)[index] + "</link>\n";
                i++;
            }
        }
    }
}
