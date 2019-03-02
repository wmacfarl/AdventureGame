using System.Collections.Generic;
using UnityEngine;

/* This file contains the classes we use to define and dialog:
 * 
 * Conversation
 * DialogLine 
 */


 /* The Conversation class is Serializable so that you can create simple conversations directly in the editor as you prototype your project.
  *  
  * Conversations are stored and referenced in a given GameObject's ConversationScript and passed to ConversationManagerScript to be displayed
  * when dialog is triggered.  
  */

[System.Serializable]
public class Conversation
{
    public List<Sprite> Portraits;      //A list containing all of the portraits that should be displayed in the HUD during the conversation
    public List<DialogLine> DialogLines;//A list of DialogLine objects defining all of the dialog in the conversation
    public int currentLineIndex;        //The line of dialog we are currently displaying.

    //A base constructor that initializes empty lists.
    public Conversation()
    {
        Portraits = new List<Sprite>();
        DialogLines = new List<DialogLine>();
        currentLineIndex = 0;
    }

    //This constructor is used by  ConversationScript when we automatically generate a conversation for a single line of dialog.
    public Conversation(Sprite singlePortrait, string singleDialogLine, float duration) : this()
    {
        this.Portraits.Add(singlePortrait);
        this.DialogLines.Add( new DialogLine(0, singleDialogLine, duration));
    }
}

/*  The DialogLine class is Serializable so that you can create simple conversations directly in the editor as you prototype your project.
 *  
 *  Each Conversation contains a list of DialogLines to display.  After a Conversation is triggered, the text of a DialogLine is displayed 
 *  in the HUD for "duration" seconds.  After "duration" seconds the next line in the list is displayed.  A sprite from the Conversation's 
 *  list of Portraits is displayed next to the text, as determined by the DialogLine's portraitIndex.
 *  
 *  Audio samples could be stored here as well and triggered by the ConversationManager for voice acting and/or sound effects to accompany
 *  dialog.   
 */

[System.Serializable]
public class DialogLine
{
    public int portraitIndex; //The index to the Conversation.Portraits list.  This determines which portrait is displayed alongside the text.
    public string text;       //The text to display in the HUD.
    public float duration;    //How long to display this line (in seconds)

    public DialogLine(int portraitIndex, string text, float duration)
    {
        this.duration = duration;
        this.portraitIndex = portraitIndex;
        this.text = text;
    }
}
