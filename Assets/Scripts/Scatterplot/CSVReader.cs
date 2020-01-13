using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//[ExecuteInEditMode]
public class CSVReader : MonoBehaviour
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";         // Define delimiters by a regular expression
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";                           // Define line delimiters by a regular experession 
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> Read(string file) 
    {
        var list = new List<Dictionary<string, object>>(); 

        TextAsset data = Resources.Load(file) as TextAsset;

        // Split data.text into lines using LINE_SPLIT_RE characters
        var lines = Regex.Split(data.text, LINE_SPLIT_RE); 

        if (lines.Length <= 1)
            return list;

        // Split header (element 0)
        var header = Regex.Split(lines[0], SPLIT_RE); 

        for (var i = 1; i < lines.Length; i++)
        {
            var values = Regex.Split(lines[i], SPLIT_RE);                       // Split lines according to SPLIT_RE, store in var (usually string array)
            if (values.Length == 0 || values[0] == "")                          // Skip to end of loop (continue) if value is 0 length OR first value is empty
                continue;       

            var entry = new Dictionary<string, object>();

            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j]; 
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", ""); 
                object finalvalue = value;

                // Attempt to parse value into int or float
                if (int.TryParse(value, out int n))
                    finalvalue = n;
                else if (float.TryParse(value, out float f))
                    finalvalue = f;

                entry[header[j]] = finalvalue;
            }

            list.Add(entry); 
        }

        return list; 
    }
}
