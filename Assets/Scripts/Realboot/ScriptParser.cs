using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class ScriptParser
{
    public List<string> lines = new List<string>(); // All lines of the script
    public int currentLineIndex = 0; // Current line being processed
    public Dictionary<string, int> labels = new Dictionary<string, int>(); // Label name to line index mapping

    public string CurrentScriptName { get; private set; }


    // Loads a script file from Resources and pre-processes labels
    public void LoadScript(string filename)
    {
        CurrentScriptName = filename;
        TextAsset asset = Resources.Load<TextAsset>(filename);
        if (asset == null)
        {
            Debug.LogError("[PARSER] Could not find script: " + filename + " in Resources.");
            return;
        }

        // Split by new line
        string[] rawLines = asset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        lines.Clear();
        labels.Clear();
        int index = 0;

        foreach (var line in rawLines)
        {
            string cleanLine = line.Trim();
            if (string.IsNullOrEmpty(cleanLine) || cleanLine.StartsWith("//")) continue;

            lines.Add(cleanLine);

            // Label detection
            if (cleanLine.StartsWith("#label"))
            {
                string[] parts = cleanLine.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    string labelName = parts[1].Trim();
                    if (!labels.ContainsKey(labelName))
                        labels.Add(labelName, index);
                }
            }
            index++;
        }
        
        currentLineIndex = 0;
        Debug.Log("[PARSER] Loaded " + filename + " with " + lines.Count + " lines and " + labels.Count + " labels.");
    }

    public bool HasMoreLines()
    {
        return currentLineIndex < lines.Count;
    }

    public string GetCurrentLine()
    {
        return lines[currentLineIndex];
    }

    public void NextLine()
    {
        currentLineIndex++;
    }

    public void GoToLabel(string labelName)
    {
        if (labels.ContainsKey(labelName))
        {
            currentLineIndex = labels[labelName];
            Debug.Log("[PARSER] Jumped to label: " + labelName + " at line" + currentLineIndex);
        }
        else
        {
            Debug.LogWarning("[PARSER] Label not found: " + labelName);
        }
    }

    // Parses a raw line into Command + Arguments
    public List<string> ParseCommand(string line)
    {
        List<string> result = new List<string>();

        // Split Command and Args by first space
        string[] firstSplit = line.Split(new char[] { ' ' }, 2);

        if (firstSplit.Length == 0) return null;

        // Add Command
        result.Add(firstSplit[0]);

        if (firstSplit.Length > 1)
        {
            // Add Args split by comma
            string argsBlock = firstSplit[1];
            // Simple split for now. If texts have commas, we need the "strD" logic from Java
            string[] args = argsBlock.Split(',');
            
            foreach(var arg in args)
            {
                result.Add(arg.Trim());
            }
        }

        return result;
    }
}