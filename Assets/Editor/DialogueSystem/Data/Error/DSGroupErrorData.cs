using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DSGroupErrorData
{
    public DSErrorData errorData { get; set; }
    public List<DSGroup> groups { get; set; }

    public DSGroupErrorData()
    {
        errorData = new DSErrorData();
        groups = new List<DSGroup>();
    }
}
