using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DSGroup : Group
{
    public string ID { get; set; }
    public string oldTitle { get; set; }

    private Color _defaultBorderColor;
    private float _defaultBorderWidth;

    public DSGroup(string groupTitle, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();
        title = groupTitle;
        oldTitle = groupTitle;
        
        SetPosition(new Rect(position, Vector2.zero));
        
        _defaultBorderColor = contentContainer.style.borderBottomColor.value;
        _defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
    }

    public void SetErrorStyle(Color color)
    {
        contentContainer.style.borderBottomColor = color;
        contentContainer.style.borderBottomWidth = 2f;
    }

    public void ResetStyle()
    {
        contentContainer.style.borderBottomColor = _defaultBorderColor;
        contentContainer.style.borderBottomWidth = _defaultBorderWidth;
    }
}
