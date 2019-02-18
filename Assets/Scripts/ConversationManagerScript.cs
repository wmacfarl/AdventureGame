using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;//allows to use file and csv

public class ConversationManagerScript : MonoBehaviour
{
    public GameObject HUDCanvas;

    GameObject speakerImageContainerGameobject;
    GameObject dialogText;
    public Sprite emptyPortraitImage;
    Conversation currentConversation;
    List<List<string>> currentDialog = new List<List<string>>();

    float dialogLineStartTime;
    float currentDialogLineDuration;

    public bool isDialogActive()
    {
        return (currentConversation != null);
    }

    // Start is called before the first frame update
    void Start()
    {
      HUDCanvas = GameObject.Find("ConversationHudCanvas");
      speakerImageContainerGameobject = HUDCanvas.transform.Find("Speaker Image").gameObject;
      dialogText = HUDCanvas.transform.Find("DialogText").gameObject;
        EndConversation();
    }


    public void StartConversation(Conversation conversation)
    {
        currentConversation = conversation;
        currentConversation.currentLineIndex = 0;
        StartCoroutine(DisplayNextDialogLine());
    }

    IEnumerator DisplayNextDialogLine()
    {
        if (currentConversation.currentLineIndex < currentConversation.DialogLines.Count)
        {
            DialogLine currentLine = currentConversation.DialogLines[currentConversation.currentLineIndex];
            SetDialogText(currentLine.text);
            SetDialogPortrait(currentConversation.Portraits[currentLine.portraitIndex]);
            yield return new WaitForSeconds(currentLine.duration);
            currentConversation.currentLineIndex++;
            StartCoroutine(DisplayNextDialogLine());
        }
        else
        {
            EndConversation();
        }
    }

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

    void SetDialogPortrait(Sprite sprite)
    {
        speakerImageContainerGameobject.GetComponent<Image>().sprite = sprite;
    }

    void SetDialogText(string text)
    {
        dialogText.GetComponent<Text>().text = text;
    }

}



public class Csv
{
  public List<List <string> > contents = new List<List <string> >();
	string rowDeliniator = "\n";

	public Csv(TextAsset text)
	{
		makeCsv(text);
	}

	void makeCsv(TextAsset text)
	{
		string csvString = text.ToString();

		List<string> rows = new List<string>();
		rows.AddRange(csvString.Split('\n') ); //AddRange fills a list with an array, I think

		for (int i=0; i< rows.Count; i++){
			List<string> row = new List<string>();
			row.AddRange(rows[i].Split(','));//AddRange fills a list with an array, I think

			row[1] = row[1].Replace("~", ",");

			contents.Add(row);
		}

	}

	void rotateCsv()
	{
		List<List<string>> newContents = new List<List<string>>();
		for (int iColumn = 0; iColumn < contents[0].Count; iColumn++)
		{
			List<string> column = new List<string>();
			for (int iRow=0; iRow<contents.Count; iRow++)
			{
				column.Add(contents[iRow][iColumn]);
			}
			newContents.Add(column);
		}
		contents = newContents;
	}

	public void reportCSV ()
	{
		for (int iOuter=0; iOuter<contents.Count; iOuter++)
		{
			for (int iInner=0; iInner<contents[iOuter].Count; iInner++)
			{
				Debug.Log("[" + iOuter + "] [" + iInner + "] : " + contents[iOuter][iInner] );
			}
		}
	}
}
