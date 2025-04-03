using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    public string filePath = "highscores.txt"; 
    public TextMeshProUGUI highScoreText; 

    private Dictionary<string, int> highScores = new Dictionary<string, int>();

    void Start()
    {
        LoadHighScores();
        DisplayHighScores();
    }

    void LoadHighScores()
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError("High score file not found: " + fullPath);
            return;
        }

        string[] lines = File.ReadAllLines(fullPath);
        highScores.Clear();

        foreach (string line in lines)
        {
            //Debug.Log("parsing");
            string[] parts = line.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[1], out int score))
            {
                highScores[parts[0].Trim()] = score;
            }
        }

        // Sort dictionary by score in descending order
        highScores = highScores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    void DisplayHighScores()
    {
        highScoreText.text = "High Scores:\n";
        int rank = 1;
        
        foreach (var entry in highScores)
        {
            //Debug.Log("displaying");
            highScoreText.text += $"{rank}. {entry.Key}: {entry.Value}\n";
            rank++;

        }
    }
    
    public void SaveHighScore(string playerName, int score)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);

        // Append the new score to the dictionary
        if (highScores.ContainsKey(playerName))
        {
            if (highScores[playerName] < score)
            {
                highScores[playerName] = score;
            }
        }
        else
        {
            highScores[playerName] = score;
        }

        
        highScores = highScores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        
        List<string> lines = new List<string>();
        foreach (var entry in highScores)
        {
            lines.Add($"{entry.Key},{entry.Value}");
        }
        File.WriteAllLines(fullPath, lines);

        DisplayHighScores();
    }
}