using System;
using System.IO;
using System.Collections.Generic;
using WW.SimpleJSON;

public static class wwFileUtil {
  public delegate void pathRecurseDelegate(string path, bool isDirectory, int depth);

  // recurses over the folder represented by path.
  // does not issue callback for path itself.
  // if path is not a directory, nothing happens.
  // this INCLUDES .META FILES
  public static void recursePath(string path, pathRecurseDelegate prd) {
    _recursePath(path, prd, 0);
  }

  private static void _recursePath(string path, pathRecurseDelegate prd, int depth) {
    if (!Directory.Exists(path)) {
      return;
    }

    List<string> subDirs  = new List<string>(Directory.GetDirectories(path));
    List<string> subFiles = new List<string>(Directory.GetFiles      (path));

    subDirs .Sort();
    subFiles.Sort();

    // first do callback on all files, then all subdirs.
    // this is just what was convenient for my first use of this,
    // other uses might want something different, in which case we should include an option param.

    foreach (string subItem in subFiles) {
      prd(subItem, false, depth);
    }

    foreach (string subItem in subDirs) {
      prd(subItem, true, depth);
      _recursePath(subItem, prd, depth + 1);
    }
  }

  // returns a recursive list of the files & folders in path.
  // each entry has a "name" field,
  // and if the entry is a directory it also has a "children" sub-array.
  // eg, this directory structure:
  // Foo/
  //   file1
  //   file2
  //   Dir1/
  //     fileA
  //     fileB
  //     DirC/
  //   Dir2/
  //
  // would come back as this, if called on "Foo":
  // [
  //   {
  //     "name" : "file1"
  //   },
  //   {
  //     "name" : "file2"
  //   },
  //   {
  //     "name" : "Dir1",
  //     "children" : [
  //       {
  //         "name" : "fileA"
  //       },
  //       {
  //         "name" : "fileB"
  //       },
  //       {
  //         "name" : "DirC",
  //         "children" : []
  //       }
  //     ]
  //   },
  //   {
  //     "name" : "Dir2",
  //     "children" : []
  //   }
  // ]
  // 
  public static JSONArray getFileList(string path) {
    JSONArray jsa = new JSONArray();

    pathRecurseDelegate prd = (string p, bool isDir, int depth) => {
      AddManifestEntry(jsa, p, isDir, depth);
    };

    wwFileUtil.recursePath(path, prd);

    return jsa;
  }

  public static bool hasFileEntries(JSONArray jsa, HashSet<string>withExtension = null) {
    foreach (JSONClass jsc in jsa) {
      bool isDir = jsc.ContainsKey("children");
      if (!isDir) {
        string ext = System.IO.Path.GetExtension(jsc["name"]);
        if (withExtension.Contains(ext)) {
          return true;
        }
      }
    }

    return false;
  }

  public static string stripExtension(string path) {
    string ext = System.IO.Path.GetExtension(path);
    if (!string.IsNullOrEmpty(ext)) {
      path = path.Substring(0, path.Length - ext.Length);
    }
    return path;
  }

  private static void AddManifestEntry(JSONArray jsa, string path, bool isDir, int depth) {
    // get just the tail
    string[] pathParts = path.Split(System.IO.Path.DirectorySeparatorChar);
    string[] lastParts = new string[depth + 1];
    Array.Copy(pathParts, pathParts.Length - 1 - depth, lastParts, 0, depth + 1);

    AddManifestEntryRecursive(lastParts, jsa, isDir);
  }

  // boy this is complicated. there must be a simpler way.
  private static void AddManifestEntryRecursive(string[] parts, JSONArray jsa, bool isDir) {
    if (parts.Length == 0) {
      return;
    }

    JSONClass jscFound = null;
    foreach (JSONClass jsc in jsa) {
      if (string.Compare(jsc["name"], parts[0]) == 0) { // note ordinary string compare doesn't work here.
        jscFound = jsc;
        break;
      }
    }

    if (jscFound == null) {
      jscFound = new JSONClass();
      jscFound["name"] = parts[0];
      if (parts.Length == 1 && isDir) {
        jscFound["children"] = new JSONArray();
      }
      jsa.Add(jscFound);
    }

    if (parts.Length > 1) {
      string[] nextParts = new string[parts.Length - 1];
      Array.Copy(parts, 1, nextParts, 0, parts.Length - 1);
      AddManifestEntryRecursive(nextParts, jscFound["children"].AsArray, isDir);
    }
  }
}

