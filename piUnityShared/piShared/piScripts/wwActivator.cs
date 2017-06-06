using UnityEngine;
using System.Collections.Generic;
using PI;

// this crazy Debug-only script will ensure that the object you attach it to is fully activated __at the moment you attach this script__.
// ie, it walks up the parent chain and activates any parents necessary.
// this is convenient sometimes when debugging the scene if you see an element and would like to activate it
// without having to manually activate all the parents.
// additionally, if you remove the script via 'remove component' it will de-activate everything it activated.
//
// caveats:
// * some objects are deactivated every frame. this can't help you there.
// * on removal, this might deactivate some items which shouldn't be,
//   if they were legitimately activated after this script activated them.
// * the removal approach does not work if you hit Undo in the unity IDE.


public class wwActivator : MonoBehaviour {

  private HashSet<GameObject> activatedGameObjects = new HashSet<GameObject>();

  public wwActivator() {
    if (!gameObject.activeInHierarchy) {
      activateSelfAndParents(gameObject);
    }
  }

  private void activateSelfAndParents(GameObject go) {
    if (!go.activeSelf) {
      activatedGameObjects.Add(go);
      go.SetActive(true);
    }

    if (go.transform.parent != null) {
      activateSelfAndParents(go.transform.parent.gameObject);
    }
    else {
      WWLog.logInfo("Activated " + activatedGameObjects.Count + " GameObjects");
    }
  }

  public void OnDestroy() {
    foreach (GameObject go in activatedGameObjects) {
      go.SetActive(false);
    }
    WWLog.logInfo("Deactivated " + activatedGameObjects.Count + " GameObjects");
  }

}
