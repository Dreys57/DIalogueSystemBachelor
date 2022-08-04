using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class DSIOUtility
{
    private static DSGraphView _graphView;
    
    private static string _graphFileName;
    private static string _containerFolderPath;

    private static List<DSGroup> _groups;
    private static List<DSNode> _nodes;

    private static Dictionary<string, DSDialogueGroupSO> _createdDialogueGroups;
    private static Dictionary<string, DSDialogueSO> _createdDialogues;
    
    private static Dictionary<string, DSGroup> _loadedGroups;
    private static Dictionary<string, DSNode> _loadedNodes;

    public static void Initialize(DSGraphView graphView, string graphName)
    {
        _graphView = graphView;
        
        _graphFileName = graphName;
        _containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphName}";

        _groups = new List<DSGroup>();
        _nodes = new List<DSNode>();

        _createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
        _createdDialogues = new Dictionary<string, DSDialogueSO>();

        _loadedGroups = new Dictionary<string, DSGroup>();
        _loadedNodes = new Dictionary<string, DSNode>();
    }
    
    public static void Save()
    {
        CreateStaticFolders();

        GetElementsFromGraphView();

        DSGraphSaveDataSO graphData = 
            CreateAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{_graphFileName}Graph");
        
        graphData.Initialize(_graphFileName);

        DSDialogueContainerSO dialogueContainer =
            CreateAsset<DSDialogueContainerSO>(_containerFolderPath, _graphFileName);
        
        dialogueContainer.Initialize(_graphFileName);

        SaveGroups(graphData, dialogueContainer);
        SaveNodes(graphData, dialogueContainer);
        
        SaveAsset(graphData);
        SaveAsset(dialogueContainer);
    }

    private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
    {
        SerializableDictionary<string, List<string>> groupedNodeNames =
            new SerializableDictionary<string, List<string>>();

        List<string> ungroupedNodeNames = new List<string>();

        foreach (DSNode node in _nodes)
        {
            SaveNodeToGraph(node, graphData);
            SaveNodeToScriptableObject(node, dialogueContainer);

            if (node.group != null)
            {
                groupedNodeNames.AddItem(node.group.title, node.dialogueName);

                continue;
            }
            
            ungroupedNodeNames.Add(node.dialogueName);
        }

        UpdateDialoguesChoicesConnection();

        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
    }

    private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
    {
        List<DSChoiceSaveData> choices = CloneNodeChoices(node.choices);

        DSNodeSaveData nodeData = new DSNodeSaveData()
        {
            ID = node.ID,
            name = node.dialogueName,
            choices = choices,
            dialogueText = node.text,
            characterNameText = node.characterName,
            groupID = node.group?.ID,
            dialogueType = node.dialogueType,
            position = node.GetPosition().position
        };
        
        graphData.nodes.Add(nodeData);
    }

    private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
    {
        DSDialogueSO dialogue;

        if (node.group != null)
        {
            dialogue = CreateAsset<DSDialogueSO>($"{_containerFolderPath}/Groups/{node.group.title}/Dialogues",
                node.dialogueName);
            
            dialogueContainer.dialogueGroups.AddItem(_createdDialogueGroups[node.group.ID], dialogue);
        }
        else
        {
            dialogue = CreateAsset<DSDialogueSO>($"{_containerFolderPath}/Global/Dialogues", node.dialogueName);
            
            dialogueContainer.ungroupedDialogues.Add(dialogue);
        }
        
        dialogue.Initialize(
            node.dialogueName,
            node.text,
            node.characterName,
            ConvertNodeChoicesToDialogueChoices(node.choices),
            node.dialogueType,
            node.IsStartingNode()
        );
        
        _createdDialogues.Add(node.ID, dialogue);
        
        SaveAsset(dialogue);
    }

    private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
    {
        List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();

        foreach (DSChoiceSaveData nodeChoice in nodeChoices)
        {
            DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
            {
                choiceText = nodeChoice.text
            };
            
            dialogueChoices.Add(choiceData);
        }

        return dialogueChoices;
    }
    
    private static void UpdateDialoguesChoicesConnection()
    {
        foreach (DSNode node in _nodes)
        {
            DSDialogueSO dialogue = _createdDialogues[node.ID];

            for (int choiceIndex = 0; choiceIndex < node.choices.Count; ++choiceIndex)
            {
                DSChoiceSaveData nodeChoice = node.choices[choiceIndex];

                if (string.IsNullOrEmpty(nodeChoice.nodeID))
                {
                    continue;
                }

                dialogue.choices[choiceIndex].nextDialogue = _createdDialogues[nodeChoice.nodeID];
                
                SaveAsset(dialogue);
            }
        }
    }
    
    private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DSGraphSaveDataSO graphData)
    {
        if (graphData.oldGroupedNodeNames != null && graphData.oldGroupedNodeNames.Count != 0)
        {
            foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.oldGroupedNodeNames)
            {
                List<string> nodesToRemove = new List<string>();

                if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                {
                    nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                }

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{_containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                }
            }
        }

        graphData.oldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
    }
    
    private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DSGraphSaveDataSO graphData)
    {
        if (graphData.oldUngroupedNodeNames != null && graphData.oldUngroupedNodeNames.Count != 0)
        {
            List<string> nodesToRemove = graphData.oldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

            foreach (string nodeToRemove in nodesToRemove)
            {
                RemoveAsset($"{_containerFolderPath}/Global/Dialogues", nodeToRemove);
            }
        }

        graphData.oldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
    }

    private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
    {
        List<string> groupNames = new List<string>();

        foreach (DSGroup group in _groups)
        {
            SaveGroupToGraph(group, graphData);
            SaveGroupToScriptableObject(group, dialogueContainer);
            
            groupNames.Add(group.title);
        }

        UpdateOldGroups(groupNames, graphData);
    }

    private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
    {
        DSGroupSaveData groupData = new DSGroupSaveData()
        {
            ID = group.ID,
            name = group.title,
            position = group.GetPosition().position
        };
        
        graphData.groups.Add(groupData);
    }
    
    private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
    {
        string groupName = group.title;
        
        CreateFolder($"{_containerFolderPath}/Groups", groupName);
        CreateFolder($"{_containerFolderPath}/Groups/{groupName}", "Dialogues");

        DSDialogueGroupSO dialogueGroup =
            CreateAsset<DSDialogueGroupSO>($"{_containerFolderPath}/Groups/{groupName}", groupName);
        
        dialogueGroup.Initialize(groupName);
        
        _createdDialogueGroups.Add(group.ID, dialogueGroup);
        
        dialogueContainer.dialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

        SaveAsset(dialogueGroup);
    }
    
    private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
    {
        if (graphData.oldGroupNames != null && graphData.oldGroupNames.Count != 0)
        {
            List<string> groupsToRemove = graphData.oldGroupNames.Except(currentGroupNames).ToList();

            foreach (string groupToRemove in groupsToRemove)
            {
                RemoveFolder($"{_containerFolderPath}/Groups/{groupToRemove}");
            }
        }

        graphData.oldGroupNames = new List<string>(currentGroupNames);
    }

    public static void Load()
    {
        DSGraphSaveDataSO graphData =
            LoadAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", _graphFileName);

        if (graphData == null)
        {
            EditorUtility.DisplayDialog(
                "Couldn't load the file.",
                "The file at the following path couldn't be found:\n\n" +
                $"Assets/Editor/DialogueSystem/Graphs/{_graphFileName}\n\n" +
                "Make sure you chose the right file and it is placed at the folder path mentioned above.",
                "Thanks."
            );

            return;
        }
        
        DSEditorWindow.UpdateFileName(graphData.fileName);

        LoadGroups(graphData.groups);
        LoadNodes(graphData.nodes);
        LoadNodesConnections();
    }

    private static void LoadGroups(List<DSGroupSaveData> groups)
    {
        foreach (DSGroupSaveData groupData in groups)
        {
            DSGroup group = _graphView.CreateGroup(groupData.name, groupData.position);

            group.ID = groupData.ID;
            
            _loadedGroups.Add(group.ID, group);
        }
    }
    
    private static void LoadNodes(List<DSNodeSaveData> nodes)
    {
        foreach (DSNodeSaveData nodeData in nodes)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.choices);

            DSNode node = _graphView.CreateNode(nodeData.name, nodeData.dialogueType, nodeData.position, false);

            node.ID = nodeData.ID;
            node.choices = choices;
            node.text = nodeData.dialogueText;
            node.characterName = nodeData.characterNameText;
            
            node.Draw();
            
            _graphView.AddElement(node);
            
            _loadedNodes.Add(node.ID, node);

            if (string.IsNullOrEmpty(nodeData.groupID))
            {
                continue;
            }

            DSGroup group = _loadedGroups[nodeData.groupID];

            node.group = group;
            
            group.AddElement(node);
        }
    }

    private static void LoadNodesConnections()
    {
        foreach (KeyValuePair<string, DSNode> loadedNode in _loadedNodes)
        {
            foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
            {
                DSChoiceSaveData choiceData = (DSChoiceSaveData) choicePort.userData;

                if (string.IsNullOrEmpty(choiceData.nodeID))
                {
                    continue;
                }

                DSNode nextNode = _loadedNodes[choiceData.nodeID];

                Port nextNodeInputPort = (Port) nextNode.inputContainer.Children().First();

                Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                
                _graphView.AddElement(edge);

                loadedNode.Value.RefreshPorts();
            }
        }
    }
    
    private static void CreateStaticFolders()
    {
        CreateFolder("Assets/Editor/DialogueSystem", "Graphs");
        
        CreateFolder("Assets", "DialogueSystem");
        CreateFolder("Assets/DialogueSystem", "Dialogues");
        
        CreateFolder("Assets/DialogueSystem/Dialogues", _graphFileName);
        CreateFolder(_containerFolderPath, "Global");
        CreateFolder(_containerFolderPath, "Groups");
        CreateFolder($"{_containerFolderPath}/Global", "Dialogues");
    }

    public static void CreateFolder(string path, string folderName)
    {
        if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
        {
            return;
        }

        AssetDatabase.CreateFolder(path, folderName);
    }
    
    public static void RemoveFolder(string fullPath)
    {
        FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
        FileUtil.DeleteFileOrDirectory($"{fullPath}/");
    }
    
    public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}/{assetName}.asset";

        T asset = LoadAsset<T>(path, assetName);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
        
            AssetDatabase.CreateAsset(asset, fullPath);
        }

        return asset;
    }

    public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}/{assetName}.asset";
        
        return AssetDatabase.LoadAssetAtPath<T>(fullPath);
    }
    
    public static void RemoveAsset(string path, string assetName)
    {
        AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
    }
    
    public static void SaveAsset(UnityEngine.Object asset)
    {
        EditorUtility.SetDirty(asset);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private static void GetElementsFromGraphView()
    {
        Type groupType = typeof(DSGroup);
        
        _graphView.graphElements.ForEach(graphElement =>
        {
            if (graphElement is DSNode node)
            {
                _nodes.Add(node);

                return;
            }

            if (graphElement.GetType() == groupType)
            {
                DSGroup group = (DSGroup) graphElement;
                
                _groups.Add(group);

                return;
            }
        });
    }
    
    private static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
    {
        List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();
        
        //To differentiate the choices in the graph and the saved ones, so they only coincide when we save the graph
        foreach (DSChoiceSaveData choice in nodeChoices)
        {
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                text = choice.text,
                nodeID = choice.nodeID
            };
            
            choices.Add(choiceData);
        }

        return choices;
    }
}
