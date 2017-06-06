using UnityEngine;
using System.Collections;
using TMPro;

public class LanguageTestController : MonoBehaviour {

  public TextMeshProUGUI label;

	// Use this for initialization
	void Start () {
    string s = PIBInterface.Actions.getCanonicalTextLanguage();
    label.text = "current canonical text language: " + s;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
