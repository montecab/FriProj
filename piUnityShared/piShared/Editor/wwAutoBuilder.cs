using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A generic auto builder class for Unity projects
/// </summary>
public static class wwAutoBuilder {

    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];

        for(int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        return scenes;
    }

    static void PerformiOSBuild ()
    {
      string[] arguments = System.Environment.GetCommandLineArgs();
      if(EditorUserBuildSettings.activeBuildTarget==BuildTarget.iOS){
        BuildOptions option = BuildOptions.SymlinkLibraries | BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(GetScenePaths(), arguments[1], BuildTarget.iOS, option);
      }
      else{
        Debug.LogError("Wrong build platform target, please switch platform and rebuild");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
      }
    }

    static void PerformAndroidBuild ()
    {
      string[] arguments = System.Environment.GetCommandLineArgs();
      if(EditorUserBuildSettings.activeBuildTarget==BuildTarget.Android){
        BuildOptions option = BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(GetScenePaths(), arguments[1], BuildTarget.Android, option);
      }
      else{
        Debug.LogError("Wrong build platform target, please switch platform and rebuild");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
      }
    }

}