using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class AutoBuilder {

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
        if(arguments[3]=="alpha"){
          PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "USE_ALPHA");
        }
		    BuildVersionEditorUpdate.UpdateBuildInfo(BuildTarget.iOS, arguments[1], int.Parse(arguments[2]));
        BuildOptions option = BuildOptions.SymlinkLibraries | BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(GetScenePaths(), "Platforms/iOS", BuildTarget.iOS, option);
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
        if(arguments[3]=="alpha"){
          PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "USE_ALPHA");
        }
	    	BuildVersionEditorUpdate.UpdateBuildInfo(BuildTarget.Android, arguments[1], int.Parse(arguments[2]));
        BuildOptions option = BuildOptions.AcceptExternalModificationsToPlayer;
        BuildPipeline.BuildPlayer(GetScenePaths(), "Platforms/Go2Android", BuildTarget.Android, option);
      }
      else{
        Debug.LogError("Wrong build platform target, please switch platform and rebuild");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
      }
    }

}