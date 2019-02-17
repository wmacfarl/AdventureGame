using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;//allows to use file and csv

public class ConversationManagerScript : MonoBehaviour
{
    public GameObject HUDCanvas;

    public List<TextAsset> ConversationCSVs;
    public List<Csv> DialogObjects;

    GameObject speakerImage;
    Sprite currentImage;

    GameObject dialogText;

    public Sprite playerPortrait;
    public Sprite emptyPortrait;

    string currentDialogID; //Used to make sure we don't interrupt ourselves
    int currentDialogLineNumber;
    List<List<string>> currentDialog = new List<List<string>>();

    float dialogLineStartTime;
    float currentDialogLineDuration;

        public bool isDialogActive()
    {
        return (currentDialogID != "");
    }

    // Start is called before the first frame update
    void Start()
    {
      HUDCanvas = GameObject.Find("ConversationHudCanvas");
      float dialogLineStartTime = 0;
      float currentDialogLineDuration = 0;
      speakerImage = HUDCanvas.transform.Find("Speaker Image").gameObject;
      dialogText = HUDCanvas.transform.Find("DialogText").gameObject;
      SetupDialog();
    }

    void SetupDialog()
    {
        foreach (TextAsset text in ConversationCSVs)
        {
            DialogObjects.Add(new Csv(text));
        }
    }


    void Update()
    {
      updateDialog();
    }

    public void StartDialog(int CsvIndex, string dialogID)
    {
        Csv dialogObject = DialogObjects[CsvIndex];
      if (currentDialogID == "")
      {
        currentDialogID = dialogID;
        currentDialogLineNumber = 0;
        currentDialog = new List<List<string>>();
        for (int i=0;i<dialogObject.contents.Count;i++)
        {
          if (dialogObject.contents[i][0] == dialogID)
          {
            currentDialog.Add(dialogObject.contents[i]);
          }
        }
        if (currentDialog.Count > 0)
        {
          startLine();
        } else {
          endDialog();
        }
      }
    }

    void SetPicture()
    {

    }

    void startLine()
    {
      SetPicture();
      dialogText.GetComponent<Text>().text = currentDialog[currentDialogLineNumber][2];

        //set time
      currentDialogLineDuration = float.Parse(currentDialog[currentDialogLineNumber][3]);
      dialogLineStartTime = Time.time;

    }

    void updateDialog()
    {
      if (currentDialogID != "")
      {
        if (Time.time >= dialogLineStartTime + currentDialogLineDuration)
        {
          currentDialogLineNumber++;
          if (currentDialogLineNumber > currentDialog.Count - 1)
          {
            endDialog();
          } else {
            startLine();
          }
        }
      }
    }

    void endDialog()
    {
      currentDialogID = "";
      dialogLineStartTime = 0;
      speakerImage.GetComponent<Image>().sprite = emptyPortrait;
      currentDialogLineDuration = 0;
      currentDialogLineNumber = 0;
      dialogText.GetComponent<Text>().text = "";
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
