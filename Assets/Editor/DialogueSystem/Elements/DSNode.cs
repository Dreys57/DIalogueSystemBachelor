using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DSNode : Node
{
    public string ID { get; set; }
    public string dialogueName { get; set; }
    public string text { get; set; }
    public string characterName { get; set; }
    public List<DSChoiceSaveData> choices { get; set; }
    public DSDialogueType dialogueType { get; set; }
    public DSGroup group { get; set; }
    
    protected DSGraphView graphView;
    
    private Color _defaultBackgroundColor;

    public virtual void Initialize(string nodeName, DSGraphView graphView, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();
        dialogueName = nodeName;
        choices = new List<DSChoiceSaveData>();
        text = "Dialogue text";
        characterName = "Character Name";

        this.graphView = graphView;
        _defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
        
        SetPosition(new Rect(position, Vector2.zero));
        
        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
        
        base.BuildContextualMenu(evt);
    }

    public virtual void Draw()
    {
        // Title container
        TextField dialogueNameTextField = DSElementUtility.CreateTextField(dialogueName, null, callback =>
        {
            TextField target = (TextField) callback.target;

            target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

            if (string.IsNullOrEmpty(target.value))
            {
                if (!string.IsNullOrEmpty(dialogueName))
                {
                    ++graphView.nameErrors;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(dialogueName))
                {
                    --graphView.nameErrors;
                }
            }
            
            if (group == null)
            {
                graphView.RemoveUngroupedNode(this);

                dialogueName = target.value;
            
                graphView.AddUngroupedNodes(this);

                return;
            }

            DSGroup currentGroup = group;
            
            graphView.RemoveGroupedNode(this, group);

            dialogueName = target.value;
            
            graphView.AddGroupedNode(this, currentGroup);
        });

        dialogueNameTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__filename-textfield",
            "ds-node__textfield__hidden"
            );
        
        titleContainer.Insert(0, dialogueNameTextField);
        
        //Input container
        Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        //Extensions Container
        VisualElement customDataContainer = new VisualElement();
        
        customDataContainer.AddToClassList(".ds-node__custom-data-container");

        Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

        TextField characterNameTextField = DSElementUtility.CreateTextArea(characterName, null, callback =>
        {
            characterName = callback.newValue;
        });

        characterNameTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__quote-textfield"
        );
        
        TextField textTextField = DSElementUtility.CreateTextArea(text, null, callabck =>
        {
            text = callabck.newValue;
        });

        textTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__quote-textfield"
            );
        

        textFoldout.Add(characterNameTextField);
        textFoldout.Add(textTextField);
        
        customDataContainer.Add(textFoldout);

        extensionContainer.Add(customDataContainer);
    }

    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    private void DisconnectInputPorts()
    {
        DisconnectPorts(inputContainer);
    }
    
    private void DisconnectOutputPorts()
    {
        DisconnectPorts(outputContainer);
    }
    
    private void DisconnectPorts(VisualElement container)
    {
        foreach (Port port in container.Children())
        {
            if (!port.connected)
            {
                continue;
            }
            
            graphView.DeleteElements(port.connections);
        }
    }

    public bool IsStartingNode()
    {
        Port inputPort = (Port) inputContainer.Children().First();

        return !inputPort.connected;
    }
    
    public void SetErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = color;
    }

    public void ResetStyle()
    {
        mainContainer.style.backgroundColor = _defaultBackgroundColor;
    }
}
