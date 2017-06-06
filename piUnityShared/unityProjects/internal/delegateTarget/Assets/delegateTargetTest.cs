using UnityEngine;
using System.Collections;

// The console output of this program is this:
//
// nullMethod1: False nullMethod2: False
// Calling in method 1
// You called me! this = New Game Object (LittleGuy)
// Calling in method 2
// You called me! this = New Game Object (LittleGuy)
// doing careful delegate callback after the GameObject the delegate belonged to has been destroyed..
// nullMethod1: False nullMethod2: True
// Calling in method 1
// You called me! this = null

public class LittleGuy : MonoBehaviour {
  public void callMe() {
    Debug.Log("You called me! this = " + this.ToString());
  }
}

public class delegateTargetTest : MonoBehaviour {

  public delegate void SimpleDelegate();

  public SimpleDelegate myDelegate;

  public void Start() {

    GameObject go = new GameObject();
    LittleGuy lg = go.AddComponent<LittleGuy>();
    StartCoroutine(beginTest(lg.callMe));

    GameObject.Destroy(go);
  }

  private IEnumerator beginTest(SimpleDelegate dtt) {
    Debug.Log("doing careful delegate callback while everything is fine..");
    tryCallDelegate(dtt);

    // give the GameObject time to be destroyed.
    yield return new WaitForSeconds(0.5f);

    Debug.Log("doing careful delegate callback after the GameObject the delegate belonged to has been destroyed..");
    tryCallDelegate(dtt);
  }

  void tryCallDelegate(SimpleDelegate dtt) {
    bool nullTestMethod1 = (dtt.Target == null);
    bool nullTestMethod2 = string.Equals(dtt.Target.ToString(), "null");
    bool nullTestMethod3 = dtt.Target.Equals(null);
    Debug.Log("nullMethod1: " + nullTestMethod1 + " nullMethod2: " + nullTestMethod2 + " nullMethod3: " + nullTestMethod3);

    if (!nullTestMethod1) {
      Debug.Log("Calling in method 1");
      dtt();
    }

    if (!nullTestMethod2) {
      Debug.Log("Calling in method 2");
      dtt();
    }

    if (!nullTestMethod3) {
      Debug.Log("Calling in method 3");
      dtt();
    }
  }
}

