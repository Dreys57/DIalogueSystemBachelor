using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSDialogueGroupSO : ScriptableObject
{
    [field: SerializeField] public string groupName { get; set; }

    public void Initialize(string GroupName)
    {
        groupName = GroupName;
    }
}
