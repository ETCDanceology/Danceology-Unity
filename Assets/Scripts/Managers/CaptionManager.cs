/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing data for each entry of caption data
/// </summary>
public class CaptionEntry
{
    public float Time { get; set; }
    public string Text { get; set; }
}

/// <summary>
/// Overall manager for caption data
/// </summary>
public class CaptionManager : CSingletonMono<CaptionManager>
{
    private static string path = "Captions/Caption_position(";      // General path to captions files within the Resources folder
    private List<CaptionEntry> captions;                            // List of captions that are loaded
    private int curIndex = 1;                                       // Current index for the caption file

    /// <summary>
    /// Generates and returns captions based on the current index. Should be called once a new audio file is loaded
    /// </summary>
    public List<CaptionEntry> LoadNewCaption()
    {
        TextAsset caption = Resources.Load<TextAsset>(path + curIndex++ + ")");
        captions = ParseCaptionFile(caption.text);
        return captions;
    }

    /// <summary>
    /// Returns the current caption index
    /// </summary>
    public int GetCurIndex()
    {
        return curIndex;
    }

    /// <summary>
    /// Given the string of text contained within a caption file, parses it into <code>CaptionEntry</code> objects
    /// </summary>
    private List<CaptionEntry> ParseCaptionFile(string fileText)
    {
        List<CaptionEntry> output = new List<CaptionEntry>();
        string[] lines = fileText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None); 

        foreach (string line in lines)
        {
            string[] parts = line.Split('|');

            // By splitting, first half will be timestamp and second half will be line. Only proceed if there are two halfs
            if (parts.Length == 2)
            {
                TimeSpan time = TimeSpan.Parse(parts[0]);
                string text = parts[1];
                output.Add(new CaptionEntry { Time = (float)time.TotalSeconds, Text = text });
            }
        }

        return output;
    }
}
