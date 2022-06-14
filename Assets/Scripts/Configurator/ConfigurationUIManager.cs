using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

public class ConfigurationUIManager : MonoBehaviour
{
    // List of all files currently in the StreamingAssets directory
    private FileInfo[] files;

    // File Information
    public ExperimentContainer ExpContainer;

    public UndoRedo UndoRedo;

    // String representation of the file currently being edited
    private string currentFile;

    // GameObjects for the various UI components
    public GameObject ConfirmationPopup, FileDropdown, BlockView, DirtyText, FileSaveDialog, ParametersDialog;

    public GameObject BlockTab, PropertyTab, ExperimentTab;

    // When true, the user has made a modification to the JSON without saving.
    // We use this to let the user know in the UI they have unsaved changes.
    private bool dirty;

    // Reserved filename for JSON file that contains information for type checking
    public const string MASTER_JSON_FILENAME = "experiment_parameters.json";

    public const string EXPERIMENT_PARAMETERS = "default_parameters.json";

    public GameObject SwapModeButton;

    public Dropdown ParametersDropdown;

    public TipManager tipManager;

    // Tracks whether a file has been opened or not
    public bool fileOpen = false;

    /// <summary>
    /// Public accessor for "dirty" variable. When the variable is modified
    /// We also let the user know via updating the UI
    /// </summary>
    public bool Dirty
    {
        get => dirty;
        set
        {
            dirty = value;
            UpdateDirtyText();
        }
    }

    // Key-value map containing all the default values for each parameter type currently supported.
    // This can be modified via the UI or in the json file
    private Dictionary<string, object> masterParameters;

    private Dictionary<string, object> defaultParameters;

    private Dictionary<string, object> currentParameters = new Dictionary<string, object>();

    private Dictionary<string, object> globalParameters;

    // The zero based index representing which block is currently selected by the user to modify
    public int CurrentSelectedBlock;

    // Determines if the block editor or property editor should be displayed
    private bool enableBlockPanel = true;

    // Property Panel Objects
    public Text PropertyNameText, PropertyValueText;
    public InputField PropertyNameInput, PropertyValueInput;
    public GameObject BlockPanel, PropertyPanel;
    public GameObject BlockPropertiesButton;
    public Dropdown PropertyDropdown;

    public string blockParamsButtonText, expParamsButtonText;

    // Start is called before the first frame update
    void Start()
    {
        // Assign callback for file save dialog prompt
        FileSaveDialog.GetComponent<FileSavePopup>().Callback += SaveAs;

        

        string path = Application.dataPath + "/StreamingAssets/Parameters/experiment_parameters.json";
        if (File.Exists(path))
        {
            masterParameters = (Dictionary<string, object>)MiniJSON.Json.Deserialize(File.ReadAllText(
                path));
        }
        else
        {
            Debug.LogWarning("Master JSON does not exist.");
        }

        path = Application.dataPath + "/StreamingAssets/Parameters/default_parameters.json";
        if (File.Exists(path))
        {
            defaultParameters = (Dictionary<string, object>)MiniJSON.Json.Deserialize(File.ReadAllText(
                path));
        }
        else
        {
            Debug.LogWarning("Default Parameters JSON does not exist.");
        }

        path = Application.dataPath + "/StreamingAssets/Parameters/global_parameters.json";
        if (File.Exists(path))
        {
            globalParameters = (Dictionary<string, object>)MiniJSON.Json.Deserialize(File.ReadAllText(
                path));
        }
        else
        {
            Debug.LogWarning("Global Parameters JSON does not exist.");
        }

        GetFiles();
    }

    /// <summary>
    /// Updates the list of files in the StreamingAssets folder
    /// </summary>
    void GetFiles()
    {
        DirectoryInfo d = new DirectoryInfo(Application.dataPath + "/StreamingAssets");
        files = d.GetFiles("*.json").Where(
            file => (file.Name != MASTER_JSON_FILENAME && file.Name != EXPERIMENT_PARAMETERS)).ToArray();

        List<Dropdown.OptionData> fileOptions = new List<Dropdown.OptionData>();
        //Add default option because otherwise first option isn't clickable
        fileOptions.Add(new Dropdown.OptionData("Click here to open a file.")); 
        foreach (FileInfo f in files)
        {
            fileOptions.Add(new Dropdown.OptionData(f.Name));
        }

        FileDropdown.GetComponent<Dropdown>().ClearOptions();
        FileDropdown.GetComponent<Dropdown>().options.AddRange(fileOptions);

        //Set text to let the user know what to do (The text from the first option isn't visible)
        FileDropdown.GetComponentInChildren<Text>().text = "Click here to open a file.";
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Callback method for when the user interacts with the popup
    /// </summary>
    /// <param name="accept"></param>
    void OnConfirmationPopup(bool accept)
    {
        // Unsubscribe
        ConfirmationPopup.GetComponent<ConfirmationPopup>().ConfirmCallback -= OnConfirmationPopup;
    }

    /// <summary>
    /// Write all Data to the specified json file name
    /// </summary>
    /// <param name="fileName"></param>
    void SaveFile(string fileName)
    {
        string json = MiniJSON.Json.Serialize(ExpContainer.Data);
        File.WriteAllText(fileName, json);
        currentFile = fileName;
    }

    /// <summary>
    /// Executed when the user presses "Save" on the UI
    /// </summary>
    void Save()
    {
        if (dirty)
        {
            SaveFile(currentFile);
            Dirty = false;
        }
    }

    /// <summary>
    /// Executed when the user presses confirm in the "Save As" popup.
    /// The "Save As" popup allows the user to enter their own file name before saving.
    /// </summary>
    /// <param name="accept"></param>
    void SaveAs(bool accept, string fileName)
    {
        if (accept)
        {
            SaveFile(Application.dataPath + "/StreamingAssets/" + fileName);
            GetFiles();

            Dirty = false;
            FileDropdown.GetComponent<Dropdown>().value = FileDropdown.GetComponent<Dropdown>().options.FindIndex(
                o => o.text == fileName
            );
        }
    }

    /// <summary>
    /// Executes when the user presses "Save As" in the UI
    /// </summary>
    void PromptSaveDialog()
    {
        if (Dirty)
        {
            FileSaveDialog.GetComponent<FileSavePopup>().Container.SetActive(true);
        }
    }

    /// <summary>
    /// Executes when the user selects a file from the dropdown. The file is loaded, parsed, then
    /// the UI is populated with all the parameters provided by the JSON.
    /// </summary>
    /// <param name="index"></param>
    public void OpenFile(int index)
    {
        if (!fileOpen)
        {
            //Remove first default option if first time opening file
            fileOpen = true;
            index--;
            FileDropdown.GetComponent<Dropdown>().options.RemoveAt(0); //remove default option
            FileDropdown.GetComponent<Dropdown>().value--; //fixes visual bug where it looked like the next file in list is selected
        }
 
        currentFile = files[index].FullName;
        

        if (dirty)
        {
            ConfirmationPopup.GetComponent<ConfirmationPopup>().ShowPopup(
                "You have unsaved changes. Are you sure you want to continue?", OnOpenFileConfirm);
        }
        else
        {
            //Debug.Log(index);
            if (index != -1)
            {
                OpenFile(currentFile);
            }
        }

    }

    private void OpenFile(string fileName)
    {
        // Attempt to open file
        Dictionary<string, object> fileParameters =
            (Dictionary<string, object>)MiniJSON.Json.Deserialize(File.ReadAllText(fileName));

        string exp_type = fileParameters["experiment_mode"].ToString();

        CreateParameterList(exp_type);

        foreach (KeyValuePair<string, object> kp in globalParameters)
        {
            if (!fileParameters.ContainsKey(kp.Key))
                fileParameters.Add(kp.Key, kp.Value);
        }

        ExpContainer = new ExperimentContainer(fileParameters, currentParameters);

        HardReset();

        tipManager.SetTip(TipManager.TipType.OpenFile);
    }

    public void LoadFile(Dictionary<string, object> fileParameters)
    {
        string exp_type = fileParameters["experiment_mode"].ToString();

        CreateParameterList(exp_type);

        foreach (KeyValuePair<string, object> kp in globalParameters)
        {
            if (!fileParameters.ContainsKey(kp.Key))
                fileParameters.Add(kp.Key, kp.Value);
        }

        ExpContainer = new ExperimentContainer(fileParameters, currentParameters);

        SoftReset();
    }

    public void OnOpenFileConfirm(bool accept)
    {
        if (accept)
        {
            Dirty = false;
            OpenFile(currentFile);
        }
    }

    public void NewFileParameters()
    {
        if (dirty)
        {
            ConfirmationPopup.GetComponent<ConfirmationPopup>().ShowPopup(
                "You have unsaved changes. Are you sure you want to continue?", OnNewFileConfirm);
        }
        else
        {
            ParametersDropdown.ClearOptions();

            ParametersDropdown.AddOptions(defaultParameters.Keys.ToList());

            ParametersDialog.SetActive(true);
        }
    }

    /// <summary>
    /// Generates a new file with one block and all default values
    /// </summary>
    private void NewFile()
    {
        CreateParameterList(ParametersDropdown.value);

        ParametersDialog.SetActive(false);

        Dictionary<string, object> temp = new Dictionary<string, object>();
        temp.Add("experiment_mode", ParametersDropdown.options[ParametersDropdown.value].text);

        foreach (KeyValuePair<string, object> kp in globalParameters)
        {

            temp.Add(kp.Key, kp.Value);
        }

        ExpContainer = new ExperimentContainer(temp, currentParameters);

        // Initialize dictionary with 1 block and default values
        foreach (KeyValuePair<string, object> kp in currentParameters)
        {
            if (kp.Key.StartsWith("per_block"))
            {
                List<object> list = kp.Value as List<object>;
                ExpContainer.Data[kp.Key] = new List<object>();
                List<object> newList = ExpContainer.Data[kp.Key] as List<object>;
                if (list[0].GetType() == typeof(string))
                {
                    switch ((string)list[0])
                    {
                        case ExperimentContainer.STRING_PROPERTY_ID:
                            newList.Add("");
                            break;
                        case ExperimentContainer.BOOLEAN_PROPERTY_ID:
                            newList.Add(false);
                            break;
                        case ExperimentContainer.INTEGER_PROPERTY_ID:
                            newList.Add(0);
                            break;
                        default:
                            newList.Add(list[0]);
                            break;
                    }
                }
                else
                {
                    newList.Add(list[0]);
                }
            }
        }

        Dirty = true;

        HardReset();

        FileDropdown.GetComponent<Dropdown>().value = 0;

        //Clear dropdown text in case a file was previously open
        FileDropdown.GetComponentInChildren<Text>().text = "";
    }

    /// <summary>
    /// Callback function if the user decides to create a new file without saving
    /// </summary>
    void OnNewFileConfirm(bool accept)
    {
        if (accept)
        {
            Dirty = false;
            NewFileParameters();
        }
    }

    public void ExitApplication()
    {
        if (dirty)
        {
            ConfirmationPopup.GetComponent<ConfirmationPopup>().ShowPopup(
                "You have unsaved changes. Are you sure you want to quit?", OnExitConfirm);
        }
        else
        {
            Application.Quit();
        }
    }

    void OnExitConfirm(bool accept)
    {
        if (accept)
        {
            Dirty = false;
            ExitApplication();
        }
    }

    /// <summary>
    /// Executes when the user clicks on the GameObject representing an individual block.
    /// This populates the UI with input fields that allow the user to modify the "per_block_" parameters for
    /// a particular block.
    /// </summary>
    public void OnClickBlock(GameObject btn)
    {
        if (!BlockView.GetComponent<ConfigurationBlockManager>().Dragged &&
            !Input.GetKeyDown(KeyCode.LeftShift))
        {
            BlockPanel.GetComponent<BlockPanel>().Populate(btn.GetComponent<BlockComponent>().BlockID);
            CurrentSelectedBlock = btn.GetComponent<BlockComponent>().BlockID;


            tipManager.SetTip(TipManager.TipType.SelectBlock);
        }
    }

    /// <summary>
    /// Updates the text shown on the screen depending on the value of dirty
    /// </summary>
    private void UpdateDirtyText()
    {
        DirtyText.SetActive(dirty);
    }


    /// <summary>
    /// Switches between showing the editor for blocks and custom parameters
    /// </summary>
    void SwapPanel()
    {
        enableBlockPanel = !enableBlockPanel;

        BlockPanel.SetActive(enableBlockPanel);
        PropertyPanel.SetActive(!enableBlockPanel);
    }

    // Property Editor Panel
    void OnSelectProperty(int index)
    {
        PropertyNameText.gameObject.SetActive(true);
        PropertyNameText.text =
            "Property Name: " + PropertyDropdown.options[index].text;

        PropertyValueText.gameObject.SetActive(true);
        PropertyValueText.text =
            "Property Value: " + string.Join(", ", ExpContainer.Data[
                PropertyDropdown.options[index].text
            ]);

        //PropertyNameInput.
    }

    /// <summary>
    /// Executes when the user modifies a value in the property
    /// </summary>
    /// <param name="newValue"></param>
    void OnEndPropertyValueEdit(string newValue)
    {
        // Converts the comma separated input into a list of the correct type
        string[] values = newValue.Split(',');

        List<object> newList = new List<object>();
        foreach (string val in values)
        {
            newList.Add(ExpContainer.ConvertToCorrectType(val));
        }

        // Replace the old list
        ExpContainer.Data[PropertyDropdown.options[PropertyDropdown.value].text] = newList;
    }

    /// <summary>
    /// Executes when the user modifies the name of the property
    /// </summary>
    /// <param name="newName"></param>
    void OnEndPropertyNameEdit(string newName)
    {
        // Move old list to new key and delete old key
        ExpContainer.Data[newName] =
            ExpContainer.Data[PropertyDropdown.options[PropertyDropdown.value].text];

        ExpContainer.Data.Remove(PropertyDropdown.options[PropertyDropdown.value].text);
    }

    /// <summary>
    /// Switches between showing the editor for blocks and custom parameters
    /// </summary>
    public void SwapMode()
    {
        if (!fileOpen)
            return;

        BlockTab.SetActive(!BlockTab.activeSelf);
        PropertyTab.SetActive(!PropertyTab.activeSelf);

        if (!BlockTab.activeSelf)
        {
            SwapModeButton.GetComponentInChildren<Text>().text = expParamsButtonText;
        }
        else
        {
            SwapModeButton.GetComponentInChildren<Text>().text = blockParamsButtonText;
        }
    }

    private void CreateParameterList(int dropdownNum)
    {
        CreateParameterList(ParametersDropdown.options[dropdownNum].text);
    }

    /// <summary>
    /// Adds all parameters in masterParameters that are associated with experimentType to currentParameters.
    /// </summary>
    public void CreateParameterList(string experimentType)
    {
        currentParameters = new Dictionary<string, object>();

        List<object> parameters = defaultParameters[experimentType] as List<object>;


        foreach (string parameter in parameters)
        {
            if (masterParameters.ContainsKey(parameter))
            {
                currentParameters.Add(parameter, masterParameters[parameter]);
            }
        }
    }

    /// <summary>
    /// Initializes blockview, blockpanel, and undoredo. Defaults to showing the block tab. 
    /// Called when opening a new file, 
    /// </summary>
    public void HardReset()
    {
        // Default to show the block tab
        BlockTab.SetActive(true);
        PropertyTab.SetActive(false);
        SwapModeButton.GetComponentInChildren<Text>().text = blockParamsButtonText;

        BlockView.GetComponent<ConfigurationBlockManager>().InitializeBlockPrefabs(this, ExpContainer);

        BlockPanel.GetComponent<BlockPanel>().Start();

        UndoRedo.Initialize(this, ExpContainer);
        UndoRedo.Clear();
    }

    //
    // Initializes blockview, blockpanel, and undoredo, without clearing the undo/redo stacks. 
    // Called when undoing/redoing.
    //
    public void SoftReset()
    {
        BlockView.GetComponent<ConfigurationBlockManager>().InitializeBlockPrefabs(this, ExpContainer);

        BlockPanel.GetComponent<BlockPanel>().Start();

        //updates property panel visual, in case that is getting updated.
        PropertyTab.GetComponent<PropertyPanel>().Populate();

        UndoRedo.Initialize(this, ExpContainer);
    }
}
