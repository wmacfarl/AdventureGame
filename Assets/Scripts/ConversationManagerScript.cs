using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/* ConversationManagerScript should be attached to the ConversationManager gameObject which is part of the HUD canvas.  This script depends on 
 * a paricular set of GameObjects and UI elements to function and should generally only be used as part of the ConversationHUDCanvas prefab.
 * 
 * This Script has a publically accessible StartConversation() method that takes a Conversation as an argument and will display the contents 
 * of this conversation in the HUD.  This method is called from the ConversationScript attached to any GameObject that can trigger dialog through
 * the GetTalkedTo() function.
 */ 

public class ConversationManagerScript : MonoBehaviour
{
    //These GameObjects are part of the ConversationHUDCanvas prefab and need to be in the scene for the ConversationManagerScript to work.
    GameObject HUDCanvas;
    GameObject speakerImageContainerGameobject;
    GameObject dialogText;

    [SerializeField]    //The image to display in the HUD when there isn't any dialog happening
    Sprite emptyPortraitImage;

    [SerializeField]    //The player's portrait.  This is used as a default portrait to display when generating conversations when another
    public Sprite playerPortraitImage;  //Portrait isn't specified

    Conversation currentConversation;   //The Conversation we are currently displaying

    public bool isDialogActive()    //Used by outside scripts to determine whether to start new conversations, allow player control, etc.
    {
        return (currentConversation != null);
    }

    void Start()
    { 
      //Ensure that we have only one ConversationManagerScript in the scene
      if (Object.FindObjectsOfType<ConversationManagerScript>().Length > 1)
      {
        throw new System.Exception("Only 1 ConversationManagerScript is allowed per scene.");
      }

      //Get references to required GameObjects
      HUDCanvas = GameObject.Find("ConversationHudCanvas");
      speakerImageContainerGameobject = HUDCanvas.transform.Find("Speaker Image").gameObject;
      dialogText = HUDCanvas.transform.Find("DialogText").gameObject;

      //Clear the HUD to begin
      EndConversation();
    }

    //Called by ConversationScript to trigger displaying a conversation on the HUD.
    public void StartConversation(Conversation conversation)
    {
        currentConversation = conversation;
        currentConversation.currentLineIndex = 0;
        StartCoroutine(DisplayNextDialogLine());
    }

    //This is a coroutine called intially from StartConversation and then recursively as long as there is another line of dialog.  
    //It sets the dialog text and portrait in the HUD for the next line of dialog and then waits duration seconds before calling itself again
    IEnumerator DisplayNextDialogLine()
    {
        //Check if the conversation is over
        if (currentConversation.currentLineIndex < currentConversation.DialogLines.Count)
        {
            //If it isn't, set the dialog text and portrait from the DialogLine in the Conversation
            DialogLine currentLine = currentConversation.DialogLines[currentConversation.currentLineIndex];
            SetDialogText(currentLine.text);
            SetDialogPortrait(currentConversation.Portraits[currentLine.portraitIndex]);
            currentConversation.currentLineIndex++;

            //Wait duration seconds and then call the function again
            yield return new WaitForSeconds(currentLine.duration);
            StartCoroutine(DisplayNextDialogLine());
        }
        else
        {
            EndConversation();
        }
    }

    //Clears the HUD and sets our currentConversation to null
    void EndConversation()
    {
        SetDialogText("");
        SetDialogPortrait(emptyPortraitImage);
        if (currentConversation != null)
        {
            currentConversation.currentLineIndex = 0;
        }
        currentConversation = null;
    }

    //Sets the portrait in the HUD
    void SetDialogPortrait(Sprite sprite)
    {
        speakerImageContainerGameobject.GetComponent<Image>().sprite = sprite;
    }

    //Sets the text in the HUD
    void SetDialogText(string text)
    {
        dialogText.GetComponent<Text>().text = text;
    }

}



