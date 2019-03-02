using System.Collections.Generic;
using UnityEngine;

/* 
 * This script should be attached to all GameObjects that you can have a conversation with.  PlayerControlScript.PerformPrimaryAction() 
 * calls the public GetTalkedTo() method in the event that we do a primary action while facing and adjacent to a GameObject that has this
 * script attached.
 * 
 * myConversation is a list of Conversation objects that contains all of the conversations that I can perform.  Conversations are 
 * a list of dialogLine objects and a list of Sprites containing all of the portraits used in the conversation.
 * 
 * Each dialogLine object has a string defining the text to display, a duration, defining how long the line is displayed, and an index 
 * to the portraits list defining which portrait to display during this line.
 * 
 * Simple conversations can be defined in the inspector while designing/prototyping levels and objects.  More complex conversations and 
 * interactions will probably benefit from an import tool of some kind.
 * 
 * The most common kind of conversation is one that consists of a single line of dialog said by this GameObject.  The "singleDialogLine" field
 * can be set in the inspector.  If the singleDialogLine field is not blank and there aren't other conversations defined, this script will 
 * generate a Conversation using that dialogLine and this GameObject's "MyPortrait" field. If the MyPortrait field is blank, the Conversation
 * will default to using the Player portrait defined in the ConversationManager -- this would be used to have the Player make an observation
 * about something she has examined, for instance.
 * 
 * This is designed to make it easy to add basic NPCs or interactable items.
 * 
 * There is a nextConversationIndex which determines which Conversation from the list to trigger.  The logic for how this is set is probably
 * outside of the scope of this function and should be handled individually with different GameObjects based on the desired game logic.
 */

public class ConversationScript : MonoBehaviour
{
    [SerializeField] //The default portrait to display in the event that other portraits aren't specified by the Conversation
    Sprite MyPortrait;

    [SerializeField]    //Use this field if you only need to store and trigger a single line of dialog.  
    string singleDialogLine;

    [SerializeField]    //The length of time (in seconds) to display a line of dialog if it isn't specified by the conversation source.
    float defaultDialogLineDuration = .7f;

    [SerializeField] //A list of conversations this GameObject can trigger.  Relatively simple conversations can be written directly in                      
    public List<Conversation> myConversations;  //the inspector.  See Conversation.cs for complete understanding of the Conversation and 
                                                //DialogLine classes.

    int nextConversationIndex;  //Starts at zero.  Should get modified by other game logic if there are multiple possible conversations to have

    ConversationManagerScript ConversationManager; //A single ConversationManagerScript should be attached to the ConversationManager GameObject
                                                   //under the ConversationManagerHUD

    private void Start()
    {
        nextConversationIndex = 0;

        //Ensure we have a ConversationManager
        ConversationManager = Object.FindObjectOfType<ConversationManagerScript>();
        if (ConversationManager == null)
        {
            throw new System.Exception("You must have a ConversationManagerScript in your scene to have conversations");
        }

        //If we haven't defined a more elaborate conversation and we have a specified single line of dialog, generate a conversation for it.
        if (singleDialogLine != "" && (myConversations == null || myConversations.Count == 0))
        {
            GenerateConversationFromSingleDialogLine(singleDialogLine);
        }
    }

    //This method is called from PlayerControlScript() and tells the ConversationManager to start our next conversation.
    public void GetTalkedTo()
    {
        ConversationManager.StartConversation(myConversations[nextConversationIndex]);
    }


    //Generates a Conversation object from a single line of dialog using MyPortrait or the Player's portrait if I don't have a portrait.
    void GenerateConversationFromSingleDialogLine(string text)
    {
        Sprite portrait = MyPortrait;
        if (portrait == null)
        {
            portrait = ConversationManager.playerPortraitImage;
        }
        myConversations = new List<Conversation>();
        Conversation myConversation = new Conversation(MyPortrait, text, defaultDialogLineDuration);
        myConversations.Add(myConversation);
    }
}

