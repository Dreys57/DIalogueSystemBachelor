using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSNodeErrorData
{
    public DSErrorData errorData { get; set; }
    public List<DSNode> nodes { get; set; }

    public DSNodeErrorData()
    {
        errorData = new DSErrorData();
        nodes = new List<DSNode>();
    }
}
