using UnityEngine;
using System.Collections.Generic;
using WW;
using PI;

public class piTest : MonoBehaviour {

	void Start () {
    doTests();
	}

  public static void doTests() {
    #if UNITY_EDITOR
    wwPO                    .test();
    piStringUtil            .test();
    piBotBoFake             .test();
    wwCollectionUtils       .test();
    wwLEDColors             .test();
    HTTPManagerTest.Instance.test();
    piMathUtilTests         .test();
    #else
    WWLog.logError("tests attempted in non-editor build");
    #endif
  }

  public static bool assertTrue(bool val, string messageIfFalse) {
    if (!val) {
      WWLog.logError("assertTrue failed: " + messageIfFalse);
    }
    return val;
  }
}
