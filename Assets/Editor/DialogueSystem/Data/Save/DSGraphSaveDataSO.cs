using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSGraphSaveDataSO : ScriptableObject
{
    [field: SerializeField] public string fileName { get; set; }
    [field: SerializeField] public  List<DSGroupSaveData> groups { get; set; }
    [field: SerializeField] public List<DSNodeSaveData> nodes { get; set; }
    [field: SerializeField] public List<string> oldGroupNames { get; set; }
    [field: SerializeField] public List<string> oldUngroupedNodeNames { get; set; }
    [field: SerializeField] public SerializableDictionary<string, List<string>> oldGroupedNodeNames { get; set; }

    public void Initialize(string FileName)
    {
        fileName = FileName;

        groups = new List<DSGroupSaveData>();
        nodes = new List<DSNodeSaveData>();
    }
}
