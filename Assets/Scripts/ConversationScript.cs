using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationScript : MonoBehaviour
{
    public ConversationManagerScript ConversationManager;
    public Sprite MyPortrait;
    public List<Conversation> myConversations;
    public int nextConversationIndex;

    private void Start()
    {
        ConversationManager = Object.FindObjectOfType<ConversationManagerScript>();
    }

    public void GetTalkedTo()
    {
        ConversationManager.StartConversation(myConversations[nextConversationIndex]);
    }
}

[System.Serializable]
public class Conversation
{
    public List<Sprite> Portraits;
    public List<DialogLine> DialogLines;
    public int currentLineIndex;

}

[System.Serializable]
public class DialogLine
{
    public int portraitIndex;
    public string text;
    public float duration;
}
