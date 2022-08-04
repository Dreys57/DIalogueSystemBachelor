using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSDialogueSO : ScriptableObject
{
    [field: SerializeField] public string dialogueName { get; set; }
    [field: SerializeField] [field: TextArea()] public string dialogueText { get; set; }
    [field: SerializeField] [field: TextArea()] public string characterNameText { get; set; }
    [field: SerializeField] public List<DSDialogueChoiceData> choices { get; set; }
    [field: SerializeField] public DSDialogueType dialogueType { get; set; }
    [field: SerializeField] public bool isStartingDialogue { get; set; }

    public void Initialize(string DialogueName, string DialogueText, string CharacterNameText, List<DSDialogueChoiceData> Choices,
        DSDialogueType type, bool IsStartingDialogue)
    {
        dialogueName = DialogueName;
        dialogueText = DialogueText;
        characterNameText = CharacterNameText;
        choices = Choices;
        dialogueType = type;
        isStartingDialogue = IsStartingDialogue;
    }
}
