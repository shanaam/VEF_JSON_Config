using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class UndoRedo : MonoBehaviour
{
    public List<UndoRedoInfo> UndoList = new List<UndoRedoInfo>();
    public List<UndoRedoInfo> RedoList = new List<UndoRedoInfo>();

    public int stackLimit = 15;

    public ExperimentContainer ExpContainer;
    public ConfigurationUIManager uiManager;
    public PopUp PopUpManager;

    public static UndoRedo instance;

    // States are stored as a UndoRedoInfo struct, containing the expcontainer data and the index of the selected block
    public struct UndoRedoInfo
    {
        public string Data;
        public int SelectedBlock;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Undo shortcut
        if (/*Input.GetKey(KeyCode.LeftControl) &&*/ Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        //Redo shortcut
        else if (/*Input.GetKey(KeyCode.LeftControl) && */(Input.GetKeyDown(KeyCode.Y) || (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Z))))
        {
            Redo();
        }
    }

    /// <summary>
    /// Call this method BEFORE any changes. 
    /// ex: at the beginning of the paste function.
    /// This method makes a copy of the current data, and pushes that to the Undos stack.
    /// </summary>
    public void Backup()
    {
        RedoList.Clear();

        //Debug.Log("State backup and redo stack cleared");

        UndoList.Add(CreateUndoRedoInfo());

        LimitCheck();
    }

    public void Initialize(ConfigurationUIManager uiManager, ExperimentContainer expContainer)
    {
        this.uiManager = uiManager;
        this.ExpContainer = expContainer;
    }

    [ContextMenu("UNDO")]
    private void Undo()
    {
        if (UndoList.Count == 0)
        {
            PopUpManager.ShowPopup("Nothing to Undo.", PopUp.MessageType.Negative);
            return;
        }

        //put current state into redos
        RedoList.Add(CreateUndoRedoInfo());

        //load backup
        var state = UndoList[UndoList.Count - 1];
        UndoList.RemoveAt(UndoList.Count - 1);

        Dictionary<string, object> fileParameters =
             (Dictionary<string, object>)MiniJSON.Json.Deserialize(state.Data);

        uiManager.LoadFile(fileParameters);
        uiManager.BlockPanel.GetComponent<BlockPanel>().Populate(state.SelectedBlock);

        // log unde and redo
        //LogUndoRedo();

        PopUpManager.ShowPopup("Undo!", PopUp.MessageType.Positive);

        LimitCheck();
    }

    [ContextMenu("REDO")]
    private void Redo()
    {
        if (RedoList.Count == 0)
        {
            PopUpManager.ShowPopup("Nothing to Redo.", PopUp.MessageType.Negative);
            return;
        }

        //put current state into undos
        UndoList.Add(CreateUndoRedoInfo());

        var state = RedoList[RedoList.Count - 1];
        RedoList.RemoveAt(RedoList.Count - 1);

        //load from redo
        Dictionary<string, object> fileParameters =
             (Dictionary<string, object>)MiniJSON.Json.Deserialize(state.Data);

        uiManager.LoadFile(fileParameters);
        uiManager.BlockPanel.GetComponent<BlockPanel>().Populate(state.SelectedBlock);

        // log undos and redos
        //LogUndoRedo();

        PopUpManager.ShowPopup("Redo!", PopUp.MessageType.Positive);

        LimitCheck();
    }
/*
    public void LogUndoRedo()
    {
        Debug.Log("UNDOs " + Undos.Count + " REDOs:" + Redos.Count);

        // peek at the last element in the undos stack
        if (Undos.Count > 0)
        {
            Debug.Log("Last element in undos stack: " + Undos.Peek());
        }

        // peek at the last element in the redo stack
        if (Redos.Count > 0)
        {
            Debug.Log("Last redo: " + Redos.Peek());
        }
    }*/

    public void Clear()
    {
        UndoList.Clear();
        RedoList.Clear();
    }

    public void LimitCheck()
    {
        if (UndoList.Count > stackLimit)
            UndoList.RemoveAt(0);

        if (RedoList.Count > stackLimit)
            RedoList.RemoveAt(0);
    }

    public UndoRedoInfo CreateUndoRedoInfo()
    {
        string json = MiniJSON.Json.Serialize(ExpContainer.Data);
        int selected = uiManager.CurrentSelectedBlock;

        UndoRedoInfo i = new UndoRedoInfo();
        i.Data = json;
        i.SelectedBlock = selected;

        return i;
    }
}
