using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DSDialogue : MonoBehaviour
{
    //Dialogue SO
    [SerializeField] private DSDialogueContainerSO _dialogueContainer;
    [SerializeField] private DSDialogueGroupSO _dialogueGroup;
    [SerializeField] private DSDialogueSO _dialogue;

    //Filters
    [SerializeField] private bool _groupedDialogues;
    [SerializeField] private bool _startingDialoguesOnly;
    
    //Indexes
    [SerializeField] private int _selectedDialogueGroupIndex;
    [SerializeField] private int _selectedDialogueIndex;
}
