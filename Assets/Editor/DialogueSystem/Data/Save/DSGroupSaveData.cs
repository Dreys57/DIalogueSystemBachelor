using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DSGroupSaveData
{
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public Vector2 position { get; set; }
}
