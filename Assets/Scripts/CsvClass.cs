using System.Collections.Generic;
using UnityEngine;

/*  This class is used to import and manipulate CSV files.  
 *  
 *  It is not currently being used in this project though we frequently use it for 
 *  importing dialog and other text assets.
 */

public class Csv
{
    public List<List<string>> contents = new List<List<string>>();
    string rowDeliniator = "\n";

    public Csv(TextAsset text)
    {
        makeCsv(text);
    }

    void makeCsv(TextAsset text)
    {
        string csvString = text.ToString();

        List<string> rows = new List<string>();
        rows.AddRange(csvString.Split('\n')); //AddRange fills a list with an array, I think

        for (int i = 0; i < rows.Count; i++)
        {
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
            for (int iRow = 0; iRow < contents.Count; iRow++)
            {
                column.Add(contents[iRow][iColumn]);
            }
            newContents.Add(column);
        }
        contents = newContents;
    }

    public void reportCSV()
    {
        for (int iOuter = 0; iOuter < contents.Count; iOuter++)
        {
            for (int iInner = 0; iInner < contents[iOuter].Count; iInner++)
            {
                Debug.Log("[" + iOuter + "] [" + iInner + "] : " + contents[iOuter][iInner]);
            }
        }
    }
}
