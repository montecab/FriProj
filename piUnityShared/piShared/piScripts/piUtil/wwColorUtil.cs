using UnityEngine;
using System.Collections;

public class wwColorUtil {

  //normal color
  public static Color orange = new Color(1, 0.5f, 1, 1);
  
  public static Color newColor(int r, int g, int b, float a) {
    return new Color(r / 255f, g / 255f, b / 255f, a);
  }
  
  public static Color newColor(int r, int g, int b, int a) {
    return new Color(r, g, b, a / 255f);
  }
  
  public static Color newColor(int r, int g, int b) {
    return newColor(r, g, b, 255);
  }
  
  
  public static Color ColorWithAlpha(Color c, float a) {
    Color ret = c;
    ret.a = a;
    return ret;
  }

  // all input params are in the range [0, 1].
  public static Color HSVtoRGB(float hue, float sat, float val) {
    while (hue < 0) {
      hue += 1;
    }
    while (hue > 1) {
      hue -= 1;
    }
    float hhh = Mathf.Lerp(0, 360, hue);
    float c = sat * val;
    float x = c * (float)(1 - Mathf.Abs(((hhh / 60.0f) % 2.0f) - 1));
    float m = val - c;
    
    Color ret;
    
    if      (hhh <  60) {
      ret = new Color(c + m, x + m, 0 + m);
    }      
    else if (hhh < 120) {
      ret = new Color(x + m, c + m, 0 + m);
    }      
    else if (hhh < 180) {
      ret = new Color(0 + m, c + m, x + m);
    }      
    else if (hhh < 240) {
      ret = new Color(0 + m, x + m, c + m);
    }      
    else if (hhh < 300) {
      ret = new Color(x + m, 0 + m, c + m);
    }      
    else { // if (hhh < 360) {
      ret = new Color(c + m, 0 + m, x + m);
    }
    
    return ret;
  }
}
