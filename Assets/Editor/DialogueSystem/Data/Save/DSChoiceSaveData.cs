using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DSChoiceSaveData
{
    [field: SerializeField] public string text { get; set; }
    [field: SerializeField] public string nodeID { get; set; }
}
