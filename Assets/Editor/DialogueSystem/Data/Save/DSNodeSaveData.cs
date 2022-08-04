using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DSNodeSaveData
{
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public string dialogueText { get; set; }
    [field: SerializeField] public string characterNameText { get; set; }
    [field: SerializeField] public string groupID { get; set; }
    [field: SerializeField] public List<DSChoiceSaveData> choices { get; set; }
    [field: SerializeField] public DSDialogueType dialogueType { get; set; }
    [field: SerializeField] public Vector2 position { get; set; }
}
