using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DSSingleChoiceNode : DSNode
{
    public override void Initialize(string nodeName, DSGraphView graphView, Vector2 position)
    {
        base.Initialize(nodeName, graphView, position);

        dialogueType = DSDialogueType.SingleChoice;

        DSChoiceSaveData choiceData = new DSChoiceSaveData()
        {
            text = "Next Dialogue"
        };
        
        choices.Add(choiceData);
    }

    public override void Draw()
    {
        base.Draw();
        
        //Output Container
        foreach (DSChoiceSaveData choice in choices)
        {
            Port choicePort = this.CreatePort(choice.text);

            choicePort.userData = choice;

            outputContainer.Add(choicePort);
        }
        
        RefreshExpandedState();
    }
}
