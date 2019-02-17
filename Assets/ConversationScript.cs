using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationScript : MonoBehaviour
{
    public ConversationManagerScript ConversationManager;
    public Sprite MyPortrait;
    public int myCsvID;
    public string nextDialogID;

    private void Start()
    {
        ConversationManager = Object.FindObjectOfType<ConversationManagerScript>();
        if (nextDialogID == "")
        {
            nextDialogID = transform.name;
        }
    }

    public void GetTalkedTo()
    {
        ConversationManager.StartDialog(myCsvID, nextDialogID);
    }
}