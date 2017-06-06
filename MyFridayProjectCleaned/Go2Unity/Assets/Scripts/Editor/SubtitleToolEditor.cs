#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using WW.SimpleJSON;

namespace Turing{
  public class SubtitleToolEditor : EditorWindow {

    private Dictionary<string, trSubtitleInfo> subtitles = new Dictionary<string, trSubtitleInfo>();
    private TextAsset csvTextAsset = null;
    private string pathJSON = "/Resources/TuringProto/Subtitles.json";

    [MenuItem("WW/Subtitles/Subtitle Tool", false, 0)]
    private static void Init () {
      SubtitleToolEditor window = (SubtitleToolEditor) EditorWindow.GetWindow( typeof( SubtitleToolEditor ) );
      window.Show();
    }

    private void OnGUI(){
      csvTextAsset = (TextAsset)EditorGUILayout.ObjectField(csvTextAsset, typeof(TextAsset), false);
      GUILayout.BeginHorizontal();
      GUILayout.Label("JSON file path:");
      pathJSON = GUILayout.TextField(pathJSON);
      GUILayout.EndHorizontal();
      if (GUILayout.Button("Convert CSV to JSON") && csvTextAsset!=null){
        CSVToJSON();
      }
    }

    private void CSVToJSON(){
      //Parse csv to subtitles
      string[,] grid = SplitCsvGrid(csvTextAsset.text);
      DebugOutputGrid(grid);
      for (int y = 1; y < grid.GetLength(1); y++){
        string file_name = grid[0, y];
        if (!string.IsNullOrEmpty(file_name)){
          if (!subtitles.ContainsKey(file_name)){
            subtitles.Add(file_name, new trSubtitleInfo());
            subtitles[file_name].FileName= file_name;
          }
          float startTime = float.Parse(grid[1,y]);
          float endTime = float.Parse(grid[2,y]);
          //string speaker = grid[3,y];
          string caption = grid[4,y];
          subtitles[file_name].Captions.Add(new trCaptionInfo(caption, startTime, endTime));
        }
      }
      //Export subtitles to JSON
      JSONClass jsc = new JSONClass();
      JSONArray subtitlesArray = new JSONArray();
      foreach (string key in subtitles.Keys){
         subtitlesArray.Add(subtitles[key].ToJson());
      }
      jsc[TOKENS.SUBTITLES] = subtitlesArray;
      WW.SaveLoad.wwDataSaveLoadManagerStatic.Save(jsc.ToString(), Application.dataPath+pathJSON);
    }

    public static string[,] SplitCsvGrid(string csvText){
      string[] lines = csvText.Split("\n"[0]);
      // finds the max width of row
      int width = 0; 
      for (int i = 0; i < lines.Length; i++){
        string[] row = SplitCsvLine( lines[i] ); 
        width = Mathf.Max(width, row.Length); 
      }
      // creates new 2D string grid to output to
      string[,] outputGrid = new string[width + 1, lines.Length + 1]; 
      for (int y = 0; y < lines.Length; y++){
        string[] row = SplitCsvLine( lines[y] ); 
        for (int x = 0; x < row.Length; x++) {
          outputGrid[x,y] = row[x]; 
          // This line was to replace "" with " in my output. 
          // Include or edit it as you wish.
          outputGrid[x,y] = outputGrid[x,y].Replace("\"\"", "\"");
        }
      }
      return outputGrid; 
    }

    public static string[] SplitCsvLine(string line){
      return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
      @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", 
      System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
      select m.Groups[1].Value).ToArray();
    }

    public static void DebugOutputGrid(string[,] grid){
      string textOutput = ""; 
      for (int y = 0; y < grid.GetUpperBound(1); y++) { 
        for (int x = 0; x < grid.GetUpperBound(0); x++) {

          textOutput += grid[x,y]; 
          textOutput += "|"; 
        }
        textOutput += "\n"; 
      }
      Debug.Log(textOutput);
    }

  }
}
#endif