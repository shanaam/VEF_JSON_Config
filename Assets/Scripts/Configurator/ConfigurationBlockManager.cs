using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class ConfigurationBlockManager : MonoBehaviour
{
    public List<GameObject> Blocks = new List<GameObject>();
    public GameObject BlockPrefab;

    public GameObject Content;
    public GameObject SelectedBlockText;
    public GameObject InsertInstructionButton;
    public GameObject Dummy;

    public bool Dragged;

    private ExperimentContainer expContainer;
    private ConfigurationUIManager uiManager;

    public HashSet<GameObject> SelectedBlocks = new HashSet<GameObject>();
    public HashSet<GameObject> SelectedNotches = new HashSet<GameObject>();
    public List<Dictionary<string, object>> CopiedBlocks = new List<Dictionary<string, object>>();

    private const float BLOCK_SPACING = 110f;
    private const float INITIAL_OFFSET = -390f;

    private float blockViewLeftSide, blockViewRightSide;

    public GameObject anchorNotch;

    public PopUp PopUpManager;

    public enum BlockType
    {
        Selected,
        Normal,
        Aligned,
        Rotated,
        Clamped
    }

    readonly static Dictionary<BlockType, ColorBlock> BlockColours = new Dictionary<BlockType, ColorBlock>()
    {
        {BlockType.Selected, ColourPaletteHelper.SetColourPalette(Color.yellow, 1)},
        {BlockType.Normal, ColourPaletteHelper.SetColourPalette(Color.white, 1)},
        {BlockType.Aligned, ColourPaletteHelper.SetColourPalette(Color.red, 0.75f)},
        {BlockType.Rotated, ColourPaletteHelper.SetColourPalette(Color.green, 0.75f)},
        {BlockType.Clamped, ColourPaletteHelper.SetColourPalette(Color.blue, 0.75f)},
    };

    public void Start()
    {
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        blockViewLeftSide = corners[0].x;
        blockViewRightSide = corners[2].x;

        anchorNotch.GetComponent<Button>().onClick.AddListener(
                () => { OnNotchPress(anchorNotch); });
    }

    public void InitializeBlockPrefabs(ConfigurationUIManager manager, ExperimentContainer expContainer)
    {
        anchorNotch.SetActive(true);

        this.expContainer = expContainer;
        this.uiManager = manager;

        for (int i = 0; i < Blocks.Count; i++)
        {
            Destroy(Blocks[i]);
        }

        Blocks.Clear();
        CopiedBlocks.Clear();

        List<object> per_block_type = expContainer.Data["per_block_type"] as List<object>;

        for (int i = 0; i < per_block_type.Count; i++)
        {
            GameObject g = Instantiate(BlockPrefab, Content.transform);
            g.name = "Block " + i;

            BlockComponent blckCmp = g.GetComponent<BlockComponent>();

            blckCmp.Block.GetComponentInChildren<Text>().text =
                g.name + "\n" + Convert.ToString(per_block_type[i]);

            g.transform.position = new Vector3(
                INITIAL_OFFSET + (i * BLOCK_SPACING), 0f, 0f) + transform.position;

            blckCmp.BlockController = this;
            blckCmp.BlockID = i;

            Blocks.Add(g);

            blckCmp.Block.GetComponent<Button>().onClick.AddListener(
                () => { uiManager.OnClickBlock(g); });

            blckCmp.Block.GetComponent<Button>().onClick.AddListener(
                () => { OnClickBlock(g); });

            blckCmp.Notch.GetComponent<Button>().onClick.AddListener(
                () => { OnNotchPress(blckCmp.Notch); });
        }

        UnselectAll();
        UpdateBlockButtons();
    }

    void Update()
    {
        GetComponent<ScrollRect>().enabled = !Dragged;

        if (Dragged)
        {
            // Adjust scroll view position
            GetComponent<ScrollRect>().enabled = true;
            if (Input.mousePosition.x <= blockViewLeftSide)
            {
                GetComponent<ScrollRect>().horizontalNormalizedPosition -= 0.8f * Time.deltaTime;
            }
            else if (Input.mousePosition.x >= blockViewRightSide)
            {
                GetComponent<ScrollRect>().horizontalNormalizedPosition += 0.8f * Time.deltaTime;
            }
            GetComponent<ScrollRect>().enabled = false;
        }
        else
        {
            //Select All input
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
            {
                if (SelectedBlocks.Count != Blocks.Count)
                {
                    foreach (GameObject g in Blocks)
                    {
                        SelectedBlocks.Add(g);
                    }
                    SelectedNotches.Clear();

                    UpdateNotchButtons();
                    UpdateBlockButtons();
                }
            }
            //Copy shortcut
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
            {
                CopyBlocks();
            }
            //Paste shortcut
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                PasteBlocks();
            }
            //Cut shortcut
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            {
                CopyBlocks();
                RemoveBlock();
            }
            //Delete shortcut
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                RemoveBlock();
            }
            //New block / insert shortcut
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
            {
                InsertInstructions();
            }
        }
    }

    /// <summary>
    /// Sets colour of block depending on if its selected, and per block type. 
    /// Sets SelectedBlockTest based on which blocks are selected.
    /// </summary>
    private void UpdateBlockButtons()
    {
        var newList = expContainer.Data["per_block_type"] as List<object>;

        SelectedBlockText.GetComponent<Text>().text = "Selected Blocks: ";
        foreach (GameObject g in Blocks)
        {
            GameObject block = g.GetComponent<BlockComponent>().Block;
            if (SelectedBlocks.Contains(g))
            {
                block.GetComponent<Button>().colors = BlockColours[BlockType.Selected];

                SelectedBlockText.GetComponent<Text>().text +=
                    g.GetComponent<BlockComponent>().BlockID + ", ";
            }
            else
            {
                if (newList[g.GetComponent<BlockComponent>().BlockID].Equals("aligned"))
                    block.GetComponent<Button>().colors = BlockColours[BlockType.Aligned];
                else if (newList[g.GetComponent<BlockComponent>().BlockID].Equals("rotated"))
                    block.GetComponent<Button>().colors = BlockColours[BlockType.Rotated];
                else if (newList[g.GetComponent<BlockComponent>().BlockID].Equals("clamped"))
                    block.GetComponent<Button>().colors = BlockColours[BlockType.Clamped];
                else
                    block.GetComponent<Button>().colors = BlockColours[BlockType.Normal];
            }

        }


        string s =
            SelectedBlockText.GetComponent<Text>().text.Substring(0,
                SelectedBlockText.GetComponent<Text>().text.Length - 2);

        SelectedBlockText.GetComponent<Text>().text = s;
    }

    private void UpdateNotchButtons()
    {
        if (SelectedBlocks.Count == 0)
        {
            SelectedBlockText.GetComponent<Text>().text = "Selected Blocks: None";
        }

        foreach (GameObject g in Blocks)
        {
            GameObject notch = g.GetComponent<BlockComponent>().Notch;
            if (SelectedNotches.Contains(notch))
            {
                notch.GetComponent<Button>().colors = BlockColours[BlockType.Selected];
            }
            else
            {
                notch.GetComponent<Button>().colors = BlockColours[BlockType.Normal];
            }
        }

        if (SelectedNotches.Contains(anchorNotch))
        {
            anchorNotch.GetComponent<Button>().colors = BlockColours[BlockType.Selected]; ;
        }
        else
        {
            anchorNotch.GetComponent<Button>().colors = BlockColours[BlockType.Normal];
        }
    }

    /// <summary>
    /// Executes when user clicks on a notch.
    /// If not holding shift, clear all selected notch.
    /// Adds block to list of selected notches if its not there already. Else, removes notch.
    /// Clears all selected blocks.
    /// </summary>
    public void OnNotchPress(GameObject notch)
    {
        if (Dragged) return;

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            SelectedNotches.Clear();
        }

        if (!SelectedNotches.Contains(notch))
        {
            SelectedNotches.Add(notch);
        }
        else
        {
            SelectedNotches.Remove(notch);
        }

        SelectedBlocks.Clear();

        InsertInstructionButton.SetActive(SelectedNotches.Count > 0);

        UpdateBlockButtons();
        UpdateNotchButtons();
    }

    /// <summary>
    /// Executes when user clicks on a block.
    /// If not holding shift, clear all selected blocks.
    /// Adds block to list of selected blocks if its not there already. Else, removes block.
    /// Clears all selected notches.
    /// </summary>
    public void OnClickBlock(GameObject block)
    {
        if (Dragged) return;

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            SelectedBlocks.Clear();
        }

        if (!SelectedBlocks.Contains(block))
        {
            SelectedBlocks.Add(block);
        }
        else
        {
            SelectedBlocks.Remove(block);
        }

        // If a user selects a block, remove all selected notches
        SelectedNotches.Clear();

        UpdateBlockButtons();
        UpdateNotchButtons();
    }

    /// <summary>
    /// Clears all selected Notches and selected Blocks.
    /// </summary>
    public void UnselectAll()
    {
        SelectedNotches.Clear();
        SelectedBlocks.Clear();
    }

    /// <summary>
    /// Executes ONCE on the frame the user begins dragging a block
    /// </summary>
    /// <param name="draggedObject"></param>
    public void OnBlockBeginDrag(GameObject draggedObject)
    {
        UndoRedo.instance.Backup();

        if (!SelectedBlocks.Contains(draggedObject))
        {
            // If the user drags a block they did not highlight when the
            // user has already selected a block(s), 
            if (SelectedBlocks.Count > 0)
            {
                SelectedBlocks.Clear();
            }

            SelectedBlocks.Add(draggedObject);
            UpdateBlockButtons();
        }

        // If the user selected notches, remove them from the selection
        // since we are dragging a block
        SelectedNotches.Clear();
        UpdateNotchButtons();

        // Unparent
        foreach (GameObject g in SelectedBlocks)
        {
            g.transform.SetParent(gameObject.transform);
        }

        // Enable dummy object to act as a spacer
        Dummy.SetActive(true);
    }

    /// <summary>
    /// Executes when the user clicks and drags a block
    /// </summary>
    /// <param name="draggedObject"></param>
    public void OnBlockDrag(GameObject draggedObject, Vector2 mousePosition)
    {
        // If user selected multiple blocks, also attach them to the mouse
        int j = 0;
        foreach (GameObject g in Blocks)
        {
            if (SelectedBlocks.Contains(g))
            {
                g.transform.position =
                    new Vector3((BLOCK_SPACING * j) + mousePosition.x, mousePosition.y, 0f);
                j++;
            }
        }

        // Snaps the blocks into its correct position as the user drags the block
        // around the screen

        // Position blocks left of the cursor
        int k = Blocks.Count;
        j = 0;
        for (int i = 0; i < Blocks.Count; i++)
        {
            if (!SelectedBlocks.Contains(Blocks[i]))
            {
                if (Blocks[i].transform.position.x < mousePosition.x)
                {
                    Blocks[i].transform.SetSiblingIndex(j);
                    j++;
                }
                else
                {
                    k = i;
                    j++;
                    break;
                }
            }
        }

        Dummy.transform.SetSiblingIndex(j);

        // Position blocks right of the cursor
        for (int i = k; i < Blocks.Count; i++)
        {
            if (!SelectedBlocks.Contains(Blocks[i]))
            {
                Blocks[i].transform.SetSiblingIndex(j);
                j++;
            }
        }
    }

    /// <summary>
    /// Executes when the user lets go of a block
    /// </summary>
    /// <param name="draggedObject"></param>
    public void OnEndDrag(GameObject draggedObject)
    {
        // Squish selected blocks to be next to each other
        int j = 1;
        foreach (GameObject g in Blocks)
        {
            if (g != draggedObject && SelectedBlocks.Contains(g))
            {
                g.transform.position = draggedObject.transform.position +
                                       new Vector3(j, 0f, 0f);
                j++;
            }

            // Reparent
            g.transform.SetParent(Content.transform);
        }

        // Reorganize the blocks based on their x coordinate
        Blocks.Sort((a, b) =>
            a.GetComponent<RectTransform>().position.x.CompareTo(
            b.GetComponent<RectTransform>().position.x));

        // Reorganize per_block list
        var keys = expContainer.Data.Keys.ToList();
        foreach (string key in keys)
        {
            if (key.StartsWith("per_block"))
            {
                List<object> tempList = new List<object>();
                var oldList = expContainer.Data[key] as List<object>;
                for (int i = 0; i < Blocks.Count; i++)
                {
                    tempList.Add(oldList[Blocks[i].GetComponent<BlockComponent>().BlockID]);
                }

                expContainer.Data[key] = tempList;
            }
        }

        var newList = expContainer.Data["per_block_type"] as List<object>;

        // Fix numbering for the new block orientation
        ReadjustBlocks();

        uiManager.Dirty = true;
        Dummy.SetActive(false);
    }

    /// <summary>
    /// Executes when New button is pressed.
    /// Adds a new blank block to end of list. 
    /// </summary>
    public void AddBlock()
    {
        InsertNewBlock();

        PopUpManager.ShowPopup("New block added to the end of the list.", PopUp.MessageType.Positive);
    }

    /// <summary>
    /// Executes when the remove button is pressed. The currently selected block is deleted
    /// </summary>
    public void RemoveBlock()
    {
        if (SelectedBlocks.Count == 0)
        {
            PopUpManager.ShowPopup("There are no blocks currently selected.", PopUp.MessageType.Negative);
            return;
        }

        UndoRedo.instance.Backup();

        PopUpManager.ShowPopup("Block Deleted.", PopUp.MessageType.Positive);


        foreach (GameObject block in SelectedBlocks)
        {
            int blockToRemove = block.GetComponent<BlockComponent>().BlockID;

            // Remove the specific block from each per_block_ parameter
            var keys = expContainer.Data.Keys.ToList();

            foreach (string key in keys)
            {
                if (key.StartsWith("per_block"))
                {
                    List<object> per_block_list = expContainer.Data[key] as List<object>;

                    if (per_block_list.Count == 0)
                    {
                        return;
                    }

                    per_block_list.RemoveAt(blockToRemove);
                }
            }

            // Remove the visual representation of the block
            GameObject g = Blocks[blockToRemove];
            Blocks.RemoveAt(blockToRemove);
            Destroy(g);

            // Every block to the right of the block that was removed must be readjusted as
            // their indexes are no longer correct
            //ReadjustBlocks(blockToRemove + 1, Blocks.Count);
            ReadjustBlocks();

            // Loads the currently selected block
            if (Blocks.Count > 0)
            {
                uiManager.CurrentSelectedBlock = Math.Max(
                    uiManager.CurrentSelectedBlock - 1, 0);
                uiManager.BlockPanel.GetComponent<BlockPanel>().Populate(uiManager.CurrentSelectedBlock);
            }
            else
            {
                // If there are no more blocks, then disable the ability to edit them
                foreach (Transform child in uiManager.BlockPanel.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        SelectedBlocks.Clear();

        uiManager.Dirty = true;
    }

    /// <summary>
    /// Executes when the Insert Block button is pressed.
    /// Creates a new default instruction block where the currently selected notch is. 
    /// </summary>
    public void InsertInstructions()
    {
        if (SelectedNotches.Count > 0)
        {
            foreach (GameObject notch in SelectedNotches)
            {
                InsertNewBlock(notch);
            }
            PopUpManager.ShowPopup("New block inserted.", PopUp.MessageType.Positive);
        }
        else
        {
            InsertNewBlock();
            PopUpManager.ShowPopup("New block added to the end of the list.", PopUp.MessageType.Positive);
        }
    }

    public void InsertNewBlock(GameObject notch = null, Dictionary<string, object> copiedBlock = null, int count = 0)
    {
        UndoRedo.instance.Backup();

        // Instantiate prefab that represents the block in the UI
        List<object> per_block_type = expContainer.Data["per_block_type"] as List<object>;
        GameObject g = Instantiate(BlockPrefab, Content.transform);
        g.name = "Block " + per_block_type.Count;

        BlockComponent blckCmp = g.GetComponent<BlockComponent>();

        blckCmp.BlockController = this;

        int insertIndex = 0;
        if (notch != null)
        {
            
            if (notch.name.Equals("AnchorNotch"))
            {
                insertIndex = -1;
            }
            else
            {
                insertIndex = notch.GetComponentInParent<BlockComponent>().BlockID + count;
            }
            g.transform.SetSiblingIndex(insertIndex + 2);
        }
        else
        {
            //if no specified notch, send to end of list
            insertIndex = per_block_type.Count - 1;
            g.transform.SetSiblingIndex(per_block_type.Count);
        }

        // Note: We set block ID before adding another block to the dictionary because
        // block ID is zero based and the count will be 1 off after the GameObject
        // is set up.
        foreach (KeyValuePair<string, object> kp in expContainer.Data)
        {
            if (kp.Key.StartsWith("per_block"))
            {
                List<object> per_block_list = expContainer.Data[kp.Key] as List<object>;

                object o = copiedBlock != null ?
                   copiedBlock[kp.Key] :
                   expContainer.GetDefaultValue(kp.Key); //get from copied block, or from default

                // The default value of 
                if (o is IList && o.GetType().IsGenericType &&
                    o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
                {
                    per_block_list.Insert(insertIndex + 1, (o as List<object>)[0]);
                }
                else
                {
                    per_block_list.Insert(insertIndex + 1, o);
                }
            }
        }

        uiManager.Dirty = true;

        // Add listener for onClick function
        blckCmp.Block.GetComponent<Button>().onClick.AddListener(
            () => { uiManager.OnClickBlock(g); });

        blckCmp.Block.GetComponent<Button>().onClick.AddListener(
            () => { OnClickBlock(g); });

        blckCmp.Notch.GetComponent<Button>().onClick.AddListener(
            () => { OnNotchPress(blckCmp.Notch); });

        Blocks.Insert(insertIndex + 1, g);


        // Fix numbering for the new block orientation
        ReadjustBlocks();

        //ResetBlockText();
        UpdateBlockButtons();
    }

    public void ReadjustBlocks()
    {
        ReadjustBlocks(0, Blocks.Count);
    }

    /// <summary>
    /// Fix numbering for the new block orientation. Readjust indexes of blocks.
    /// Sets block name, id, displayed text based on block index and per block type.
    /// </summary>
    public void ReadjustBlocks(int start, int end)
    {
        List<object> per_block_type = expContainer.Data["per_block_type"] as List<object>;

        for (int i = start; i < end; i++)
        {
            Blocks[i].name = "Block " + i;

            BlockComponent blockCmp = Blocks[i].GetComponent<BlockComponent>();

            blockCmp.BlockID = i;
            blockCmp.Block.GetComponentInChildren<Text>().text = Blocks[i].name + "\n" + per_block_type[i];

            Blocks[i].transform.SetSiblingIndex(i);
        }
    }

    /// <summary>
    /// Updates the text display for the currently selected block if
    /// per_block_type is changed
    /// </summary>
    public void UpdateBlockText()
    {
        List<object> per_block_type = uiManager.ExpContainer.Data["per_block_type"] as List<object>;

        Blocks[uiManager.CurrentSelectedBlock].GetComponentInChildren<Text>().text =
            Blocks[uiManager.CurrentSelectedBlock].name + "\n" +
            Convert.ToString(per_block_type[uiManager.CurrentSelectedBlock]);
    }

    /// <summary>
    /// Copies all selected blocks when user presses ctrl + c
    /// Sets list of copied blocks to list of selected blocks
    /// </summary>
    public void CopyBlocks()
    {
        CopiedBlocks.Clear();

        // Make a deep copy of the data of each block
        foreach (GameObject b in SelectedBlocks)
        {
            Dictionary<string, object> CopiedParams = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kp in expContainer.Data)
            {
                if (kp.Key.StartsWith("per_block"))
                {
                    List<object> per_block_list = expContainer.Data[kp.Key] as List<object>;

                    CopiedParams.Add(kp.Key, per_block_list[b.GetComponent<BlockComponent>().BlockID]);
                }
            }
            CopiedBlocks.Add(CopiedParams);
        }

        if (SelectedBlocks.Count > 0)
        {
            PopUpManager.ShowPopup("Copied!", PopUp.MessageType.Positive);
            uiManager.tipManager.SetTip(TipManager.TipType.Copy);
        }
        else
            PopUpManager.ShowPopup("Nothing is selected.", PopUp.MessageType.Negative);
    }

    /// <summary>
    /// Pastes all copied blocks when user presses ctrl + p
    /// Pastes blocks in order at the currently selected notch
    /// </summary>
    public void PasteBlocks()
    {
        if (SelectedNotches.Count == 1)
        {
            if (CopiedBlocks.Count > 0)
                PopUpManager.ShowPopup("Pasted!", PopUp.MessageType.Positive);
            else
            {
                PopUpManager.ShowPopup("Nothing is Copied.", PopUp.MessageType.Negative);
                return;
            }

            //IEnumerable<GameObject> query = CopiedBlocks.OrderBy(pet => pet.name);

            // Finds currently selected notch
            GameObject notch = null;
            foreach (GameObject notches in SelectedNotches)
            {
                notch = notches;
            }

            int count = 0;
            foreach (Dictionary<string, object> dic in CopiedBlocks)
            {
                InsertNewBlock(notch, dic, count);
                count++;
            }

            ReadjustBlocks();
        }
        else
        {
            PopUpManager.ShowPopup("Select one Notch to paste to.", PopUp.MessageType.Negative);
        }
    }
}
