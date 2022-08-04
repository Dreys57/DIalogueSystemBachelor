using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DSEditorWindow : EditorWindow
{
    private DSGraphView _graphView;
    
    private readonly string _defaultFileName = "DialogueFileName";
    
    private static TextField _fileNameTextField;
    
    private Button _saveButton;
    private Button _miniMapButton;

    [MenuItem("Window/Dialogue System/Dialogue Graph")]
    public static void Open()
    {
        GetWindow<DSEditorWindow>("Dialogue Graph");
    }

    private void OnEnable()
    {
        AddGraphView();
        AddToolbar();
        AddStyles();
    }

    private void AddGraphView()
    {
        _graphView = new DSGraphView(this);

        _graphView.StretchToParentSize();

        rootVisualElement.Add(_graphView);
    }

    private void AddToolbar()
    {
        Toolbar toolbar = new Toolbar();

        _fileNameTextField = DSElementUtility.CreateTextField(_defaultFileName, "File Name:", callback =>
        {
            _fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
        });

        _saveButton = DSElementUtility.CreateButton("Save", () => {Save();});

        Button loadButton = DSElementUtility.CreateButton("Load", () => Load());
        Button clearButton = DSElementUtility.CreateButton("Clear", () => Clear());
        Button resetButton = DSElementUtility.CreateButton("Reset", () => ResetGraph());
        _miniMapButton = DSElementUtility.CreateButton("MiniMap", () => ToggleMiniMap());

        toolbar.Add(_fileNameTextField);
        toolbar.Add(_saveButton);
        toolbar.Add(loadButton);
        toolbar.Add(clearButton);
        toolbar.Add(resetButton);
        toolbar.Add(_miniMapButton);

        toolbar.AddStyleSheets("DialogueSystem/DSToolbarStyles.uss");

        rootVisualElement.Add(toolbar);
    }

    private void AddStyles()
    {
        rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
    }
    
    private void Save()
    {
        if (string.IsNullOrEmpty(_fileNameTextField.value))
        {
            EditorUtility.DisplayDialog(
                "Invalid file name",
                "Please make sure the file name you entered is valid",
                "OK!"
            );

            return;
        }
        
        DSIOUtility.Initialize(_graphView, _fileNameTextField.value);
        
        DSIOUtility.Save();
    }
    
    private void Load()
    {
        string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        
        Clear();

        DSIOUtility.Initialize(_graphView, Path.GetFileNameWithoutExtension(filePath));
        DSIOUtility.Load();
    }
    
    private void Clear()
    {
        _graphView.ClearGraph();
    }

    private void ResetGraph()
    {
        Clear();
        
        UpdateFileName(_defaultFileName);
    }

    private void ToggleMiniMap()
    {
        _graphView.ToggleMiniMap();
        
        _miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
    }
    
    public static void UpdateFileName(string newFileName)
    {
        _fileNameTextField.value = newFileName;
    }
    
    public void EnableSaving()
    {
        _saveButton.SetEnabled(true);
    }

    public void DisableSaving()
    {
        _saveButton.SetEnabled(false);
    }
}