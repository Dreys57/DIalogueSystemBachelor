using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DSDialogue))]
public class DSInspector : Editor
{
    //Dialogue SO
    private SerializedProperty _dialogueContainerProperty;
    private SerializedProperty _dialogueGroupProperty;
    private SerializedProperty _dialogueProperty;

    //Filters
    private SerializedProperty _groupedDialoguesProperty;
    private SerializedProperty _startingDialoguesOnlyProperty;

    //Indexes
    private SerializedProperty _selectedDialogueGroupIndexProperty;
    private SerializedProperty _selectedDialogueIndexProperty;

    private void OnEnable()
    {
        _dialogueContainerProperty = serializedObject.FindProperty("_dialogueContainer");
        _dialogueGroupProperty = serializedObject.FindProperty("_dialogueGroup");
        _dialogueProperty = serializedObject.FindProperty("_dialogue");

        _groupedDialoguesProperty = serializedObject.FindProperty("_groupedDialogues");
        _startingDialoguesOnlyProperty = serializedObject.FindProperty("_startingDialoguesOnly");

        _selectedDialogueGroupIndexProperty = serializedObject.FindProperty("_selectedDialogueGroupIndex");
        _selectedDialogueIndexProperty = serializedObject.FindProperty("_selectedDialogueIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDialogueContainerArea();

        DSDialogueContainerSO dialogueContainer =
            (DSDialogueContainerSO)_dialogueContainerProperty.objectReferenceValue;

        if (dialogueContainer == null)
        {
            StopDrawing("Select a dialogue container to get the rest of the inspector.");

            return;
        }

        DrawFiltersArea();

        bool currentStartingDialogueFilter = _startingDialoguesOnlyProperty.boolValue;

        List<string> dialogueNames;

        string dialogueFolderPath = $"Assets/DialogueSystem/Dialogues/{dialogueContainer.fileName}";

        string dialogueInfoMessage;

        if (_groupedDialoguesProperty.boolValue)
        {
            List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();

            if (dialogueGroupNames.Count == 0)
            {
                StopDrawing("No Dialogue Groups in this Container");

                return;
            }

            DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);

            DSDialogueGroupSO dialogueGroup = (DSDialogueGroupSO) _dialogueGroupProperty.objectReferenceValue;

            dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialogueFilter);

            dialogueFolderPath += $"/Groups/{dialogueGroup.groupName}/Dialogues";

            dialogueInfoMessage = "There are no" + (currentStartingDialogueFilter ? "Starting" : "") + " Dialogues in this Group.";
        }
        else
        {
            dialogueNames = dialogueContainer.GetUngroupedDialogueNames(currentStartingDialogueFilter);

            dialogueFolderPath += "/Global/Dialogues";

            dialogueInfoMessage = "There are no" + (currentStartingDialogueFilter ? "Starting" : "") + " ungrouped Dialogues in this Container";
        }

        if (dialogueNames.Count == 0)
        {
            StopDrawing(dialogueInfoMessage);

            return;
        }

        DrawDialogueArea(dialogueNames, dialogueFolderPath);

        serializedObject.ApplyModifiedProperties();
    }

    private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
    {
        DSInspectorUtility.DrawHelpBox(reason, messageType);
        
        DSInspectorUtility.DrawSpace(6);

        DSInspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work at runtime.",
            MessageType.Warning);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDialogueContainerArea()
    {
        DSInspectorUtility.DrawHeader("Dialogue Container");

        _dialogueContainerProperty.DrawPropertyField();

        DSInspectorUtility.DrawSpace(6);
    }

    private void DrawFiltersArea()
    {
        DSInspectorUtility.DrawHeader("Filters");

        _groupedDialoguesProperty.DrawPropertyField();
        _startingDialoguesOnlyProperty.DrawPropertyField();

        DSInspectorUtility.DrawSpace(6);
        EditorGUILayout.Space(6);
    }

    private void DrawDialogueGroupArea(DSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
    {
        DSInspectorUtility.DrawHeader("Dialogue Group");

        int oldSelectedDialogueGroupIndex = _selectedDialogueGroupIndexProperty.intValue;

        DSDialogueGroupSO oldDialogueGroup = (DSDialogueGroupSO)_dialogueGroupProperty.objectReferenceValue;

        bool isOldDialogueGroupNull = oldDialogueGroup == null;

        string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.groupName;

        UpdateIndexOnNamesList(dialogueGroupNames, _selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex,
            oldDialogueGroupName, isOldDialogueGroupNull);

        _selectedDialogueGroupIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue Group",
            _selectedDialogueGroupIndexProperty.intValue, dialogueGroupNames.ToArray());

        string selectedDialogueGroupName = dialogueGroupNames[_selectedDialogueGroupIndexProperty.intValue];

        DSDialogueGroupSO selectedDialogueGroup = DSIOUtility.LoadAsset<DSDialogueGroupSO>(
            $"Assets/DialogueSystem/Dialogues/{dialogueContainer.fileName}/Groups/{selectedDialogueGroupName}",
            selectedDialogueGroupName);

        _dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;

        DSInspectorUtility.DrawDisabledFields(() => _dialogueGroupProperty.DrawPropertyField());

        DSInspectorUtility.DrawSpace(6);
    }

    private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
    {
        DSInspectorUtility.DrawHeader("Dialogue");

        int oldSelectedDialogueIndex = _selectedDialogueIndexProperty.intValue;

        DSDialogueSO oldDialogue = (DSDialogueSO)_dialogueProperty.objectReferenceValue;
        
        bool isOldDialogueNull = oldDialogue == null;

        string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.dialogueName;

        UpdateIndexOnNamesList(dialogueNames, _selectedDialogueIndexProperty, oldSelectedDialogueIndex, oldDialogueName,
            isOldDialogueNull);

        _selectedDialogueIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue Group",
            _selectedDialogueIndexProperty.intValue, dialogueNames.ToArray());

        string selectedDialogueName = dialogueNames[_selectedDialogueIndexProperty.intValue];

        DSDialogueSO selectedDialogue = DSIOUtility.LoadAsset<DSDialogueSO>(dialogueFolderPath, selectedDialogueName);

        _dialogueProperty.objectReferenceValue = selectedDialogue;
        
        DSInspectorUtility.DrawDisabledFields(() => _dialogueProperty.DrawPropertyField());
    }

    private void UpdateIndexOnNamesList(List<string> optionsNames, SerializedProperty indexProperty, 
        int oldSelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
    {
        if (isOldPropertyNull)
        {
            indexProperty.intValue = 0;

            return;
        }

        bool oldIndexIsOutOfBounds = oldSelectedPropertyIndex > (optionsNames.Count - 1);
        bool oldNameIsDifferent = oldIndexIsOutOfBounds || (oldPropertyName != optionsNames[oldSelectedPropertyIndex]);
        
        if (oldNameIsDifferent)
        {
            if (optionsNames.Contains(oldPropertyName))
            {
                indexProperty.intValue =
                    optionsNames.IndexOf(oldPropertyName);
            }
            else
            {
                indexProperty.intValue = 0;
            }
        }

    }
}