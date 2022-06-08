using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class UndoRedo : MonoBehaviour
{
    public Stack<UndoRedoInfo> Undos = new Stack<UndoRedoInfo>();
    public Stack<UndoRedoInfo> Redos = new Stack<UndoRedoInfo>();

    public ExperimentContainer ExpContainer;
    public ConfigurationUIManager uiManager;
    public PopUp PopUpManager;

    public static UndoRedo instance;

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
            UndoRedo.instance.Undo();
        }
        //Redo shortcut
        else if (/*Input.GetKey(KeyCode.LeftControl) && */(Input.GetKeyDown(KeyCode.Y) || (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Z))))
        {
            UndoRedo.instance.Redo();
        }
    }

    public void Backup()
    {
        Redos.Clear();

        Debug.Log("State backup and redo stack cleared");


        Undos.Push(CreateUndoRedoInfo());
    }

    public void Initialize(ConfigurationUIManager uiManager, ExperimentContainer expContainer)
    {
        this.uiManager = uiManager;
        this.ExpContainer = expContainer;
    }

    [ContextMenu("UNDO")]
    public void Undo()
    {
        // Redos.Clear();

        if (Undos.Count == 0)
        {
            PopUpManager.ShowPopup("Nothing to Undo.", PopUp.MessageType.Negative);
            return;
        }

        //put current state into redos
        Redos.Push(CreateUndoRedoInfo());

        //load backup
        var state = Undos.Pop();

        Dictionary<string, object> fileParameters =
             (Dictionary<string, object>)MiniJSON.Json.Deserialize(state.Data);

        uiManager.LoadFile(fileParameters);
        uiManager.BlockPanel.GetComponent<BlockPanel>().Populate(state.SelectedBlock);

        // log unde and redo
        //LogUndoRedo();

        PopUpManager.ShowPopup("Undo!", PopUp.MessageType.Positive);
    }

    [ContextMenu("REDO")]
    public void Redo()
    {
        if (Redos.Count == 0)
        {
            PopUpManager.ShowPopup("Nothing to Redo.", PopUp.MessageType.Negative);
            return;
        }

        //put current state into undos
        Undos.Push(CreateUndoRedoInfo());

        var state = Redos.Pop();

        //load from redo
        Dictionary<string, object> fileParameters =
             (Dictionary<string, object>)MiniJSON.Json.Deserialize(state.Data);

        uiManager.LoadFile(fileParameters);
        uiManager.BlockPanel.GetComponent<BlockPanel>().Populate(state.SelectedBlock);

        // log undos and redos
        //LogUndoRedo();

        PopUpManager.ShowPopup("Redo!", PopUp.MessageType.Positive);
    }

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
    }

    public void Clear()
    {
        Undos.Clear();
        Redos.Clear();
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
