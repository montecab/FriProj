using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _LeoPoint : MonoBehaviour {

	public InputField xField;
	public InputField yField;

	public int x {
		get { 
			return System.Int32.Parse(xField.text);
		}
	}

	public int y {
		get { 
			return System.Int32.Parse(yField.text);
		}
	}

	public Vector2 vector {
		get { 
			return new Vector2(x, y);
		}
	}

	public string ToString() {
		return "x: " + x + ", y: " + y;
	}
}
