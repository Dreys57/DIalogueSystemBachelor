using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DSMultipleChoiceNode : DSNode
{
    public override void Initialize(string nodeName, DSGraphView graphView, Vector2 position)
    {
        base.Initialize(nodeName, graphView, position);

        dialogueType = DSDialogueType.MultipleChoice;
        
        DSChoiceSaveData choiceData = new DSChoiceSaveData()
        {
            text = "New Choice"
        };
        
        choices.Add(choiceData);
    }

    public override void Draw()
    {
        base.Draw();
        
        //Main container
        Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
        {
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                text = "New Choice"
            };
        
            choices.Add(choiceData);
            
            Port choicePort = CreateChoicePort(choiceData);

            outputContainer.Add(choicePort);
        });
        
        addChoiceButton.AddToClassList("ds-node__button");
        
        mainContainer.Insert(1, addChoiceButton);
        
        //Output Container
        foreach (DSChoiceSaveData choice in choices)
        {
            Port choicePort = CreateChoicePort(choice);
            
            outputContainer.Add(choicePort);
        }
        
        RefreshExpandedState();
    }

    private Port CreateChoicePort(object userData)
    {
        Port choicePort = this.CreatePort();

        choicePort.userData = userData;

        DSChoiceSaveData choiceData = (DSChoiceSaveData) userData;

        Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
        {
            if (choices.Count == 1)
            {
                return;
            }

            if (choicePort.connected)
            {
                graphView.DeleteElements(choicePort.connections);
            }

            choices.Remove(choiceData);
            
            graphView.RemoveElement(choicePort);
        });
            
        deleteChoiceButton.AddToClassList("ds-node__button");

        TextField choiceTextField = DSElementUtility.CreateTextField(choiceData.text, null, callback =>
        {
            choiceData.text = callback.newValue;
        });

        choiceTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__choice-textfield",
            "ds-node__textfield__hidden"
            );

        choicePort.Add(choiceTextField);
        choicePort.Add(deleteChoiceButton);

        return choicePort;
    }
}
