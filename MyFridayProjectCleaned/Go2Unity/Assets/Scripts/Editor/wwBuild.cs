using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class wwBuild {
  // this is a hacky attempt to get some concise error logging back to the calling script.
  const  string ENV_ERROR_PATH = "UNITY_ERROR_PATH";
  static string buildOutputPath = System.Environment.GetEnvironmentVariable(ENV_ERROR_PATH);

  public static void build() {

    if (!string.IsNullOrEmpty(buildOutputPath)) {
      System.IO.File.Delete(buildOutputPath);
    }

    bool anyBuild = false;

    anyBuild = anyBuild || new wwBuilderOSX().tryBuild();

    if (!anyBuild) {
      wwBuild.logErrorAndExit("no build flavor indicated.");
    }
  }

  public static void logErrorAndExit(string msg) {
    if (!string.IsNullOrEmpty(buildOutputPath)) {
      System.IO.File.WriteAllText(buildOutputPath, msg + "\n");
    }
    Debug.LogError("wwBuildLog error: " + msg);
    EditorApplication.Exit(1);
  }

  public static void logInfo(string msg) {
    Debug.LogError("wwBuildLog info: " + msg);
  }

}

// class handling  building for one platform.
public abstract class wwBuilder {
  private   bool         buildMe      = false;
  protected BuildOptions buildOpts    = BuildOptions.None;
  protected string       platformPath = null;

  public abstract string platform();


  public static readonly string TOK_PLATFORM = "-platform";
  public static readonly string TOK_OSX   = "osx";

  public static string[] cScenes = new string[] {
    "Assets/Scenes/Turing/Start.unity",
    "Assets/Scenes/Turing/Lobby.unity",
    "Assets/Scenes/Turing/Main.unity",
    "Assets/Scenes/Turing/RemoteControl.unity",
    "Assets/Scenes/Turing/Map.unity",
    "Assets/Scenes/Turing/AdminScene.unity",
    "Assets/Scenes/Turing/Vault.unity",
    "Assets/Scenes/Turing/Profiles.unity",
    "Assets/Scenes/Turing/Options.unity",
    "Assets/Scenes/Turing/Community.unity",
    "Assets/Scenes/Turing/AnimShop.unity",
    "Assets/Scenes/Turing/InternalTesting.unity",
  };

  public static string consumeFirstArg(List<string>args, string forArg = null) {
    if (args.Count == 0) {
      if (forArg != null) {
        wwBuild.logErrorAndExit("missing parameter for: " + forArg);
      }
      else {
        wwBuild.logErrorAndExit("too few args");
      }
      return null;
    }
    else {
      string ret = args[0];
      args.RemoveAt(0);
      return ret;
    }
  }


  public bool tryBuild() {
    parseArgs();
    if (buildMe) {
      build();
      return true;
    }
    else {
      return false;
    }
  }

  public virtual void parseArgs() {
    List<string> args = new List<string>(System.Environment.GetCommandLineArgs());
    string param;

    while (args.Count > 0) {
      string arg = consumeFirstArg(args);

      if (arg == TOK_PLATFORM) {
        param = consumeFirstArg(args, arg);
        if (param == platform()) {
          buildMe = true;
        }

        platformPath = consumeFirstArg(args, arg);
      }
    }
  }

  public abstract void build();

  protected void handleBuildResults(string results) {
    if (string.IsNullOrEmpty(results)) {
      // no error
      return;
    }

    wwBuild.logErrorAndExit(results);
  }

}

public class wwBuilderOSX : wwBuilder {
  public override string platform() {
    return TOK_OSX;
  }

  public override void build() {
    wwBuild.logInfo("building \"" + platform() + "\"");

    handleBuildResults(BuildPipeline.BuildPlayer(cScenes, platformPath, BuildTarget.StandaloneOSXIntel64, buildOpts));

    wwBuild.logInfo("built \"" + platform() + "\"");
  }
}
