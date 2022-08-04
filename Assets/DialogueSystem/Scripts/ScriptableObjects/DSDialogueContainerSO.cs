using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSDialogueContainerSO : ScriptableObject
{
    [field: SerializeField] public string fileName { get; set; }

    [field: SerializeField]
    public SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>> dialogueGroups { get; set; }

    [field: SerializeField] public List<DSDialogueSO> ungroupedDialogues { get; set; }

    public void Initialize(string FileName)
    {
        fileName = FileName;

        dialogueGroups = new SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>>();
        ungroupedDialogues = new List<DSDialogueSO>();
    }

    public List<string> GetDialogueGroupNames()
    {
        List<string> dialogueGroupNames = new List<string>();

        foreach (DSDialogueGroupSO dialogueGroup in dialogueGroups.Keys)
        {
            dialogueGroupNames.Add(dialogueGroup.groupName);
        }

        return dialogueGroupNames;
    }

    public List<string> GetGroupedDialogueNames(DSDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
    {
        List<DSDialogueSO> groupedDialogues = dialogueGroups[dialogueGroup];

        List<string> groupedDialoguesNames = new List<string>();

        foreach (DSDialogueSO groupedDialogue in groupedDialogues)
        {
            if (startingDialoguesOnly && !groupedDialogue.isStartingDialogue)
            {
                continue;
            }
            
            groupedDialoguesNames.Add(groupedDialogue.dialogueName);
        }

        return groupedDialoguesNames;
    }

    public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
    {
        List<string> ungroupedDialogueNames = new List<string>();

        foreach (DSDialogueSO ungroupedDialogue in ungroupedDialogues)
        {
            if (startingDialoguesOnly && !ungroupedDialogue.isStartingDialogue)
            {
                continue;
            }
            
            ungroupedDialogueNames.Add(ungroupedDialogue.dialogueName);
        }

        return ungroupedDialogueNames;
    }
}