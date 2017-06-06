using System;
using UnityEngine;
using PI;

public class piUnityUtils {

	public static Color unityColor(piColorRGB value) {
		Color ret = new Color();
		ret.r = unityLed(value.r);
		ret.g = unityLed(value.g);
		ret.b = unityLed(value.b);
		ret.a = 1;
		return ret;
	}

	public static float unityLed(byte value) {
		return (float)value / 255.0f;
	}
}

