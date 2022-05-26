using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class UndoRedo : MonoBehaviour
{
    public Stack<string> Undos = new Stack<string>();
    public Stack<string> Redos = new Stack<string>();

    public ExperimentContainer ExpContainer;
    public ConfigurationUIManager uiManager;
    public PopUp PopUpManager;

    int i = 0;


    public static UndoRedo instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Backup()
    {
        Redos.Clear();

        Debug.Log("BACKUP");

        string json = MiniJSON.Json.Serialize(ExpContainer.Data);
        Undos.Push(json);
    }

    public void Initialize(ConfigurationUIManager uiManager, ExperimentContainer expContainer)
    {
        this.uiManager = uiManager;
        this.ExpContainer = expContainer;

        Undos.Clear();
        Redos.Clear();
    }

    public void Undo()
    {
        if (Undos.Count == 0)
        {
            PopUpManager.ShowPopup("Nothing to Undo.", PopUp.MessageType.Negative);
            return;
        }

        Debug.Log("UNDO " + Undos.Count);

        //put current state into redos
        string json = MiniJSON.Json.Serialize(ExpContainer.Data);
        Redos.Push(json);

        var state = Undos.Pop();

        //load backup
        Dictionary<string, object> fileParameters =
             (Dictionary<string, object>)MiniJSON.Json.Deserialize(state);


        uiManager.LoadFile(fileParameters);

        PopUpManager.ShowPopup("Undo!", PopUp.MessageType.Positive);
    }

    public void Redo()
    {
        if (Redos.Count == 0)
        {
            PopUpManager.ShowPopup("Nothing to Redo.", PopUp.MessageType.Negative);
            return;
        }

        Debug.Log("REDO " + Redos.Count);

        //put current state into undos
        string json = MiniJSON.Json.Serialize(ExpContainer.Data);
        Undos.Push(json);

        var state = Redos.Pop();

        //load from redo
        Dictionary<string, object> fileParameters =
             (Dictionary<string, object>)MiniJSON.Json.Deserialize(state);


        uiManager.LoadFile(fileParameters);

        PopUpManager.ShowPopup("Redo!", PopUp.MessageType.Positive);
    }
}
