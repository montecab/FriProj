using UnityEngine;
using System.Collections;

public class Utilities {

  
  public float minimum_distance(Vector2 _v, Vector2 _w, Vector2 _p) {

    // Return minimum distance between line segment vw and point p

    float l2 = Vector2.Distance(_v, _w);  // i.e. |w-v|^2 -  avoid a sqrt
    if (l2 == 0.0) return Vector2.Distance(_p, _v);   // v == w case
    float _t = Vector2.Dot(_p - _v, _w - _v) / l2;
    if (_t < 0.0) return Vector2.Distance(_p, _v);
    else if (_t > 1.0) return Vector2.Distance(_p, _w);
    Vector2 _projection = _v + _t * (_w - _v);
    return Vector2.Distance(_p, _projection);
  }

  //vector3 to angle z
  public static float directionToAngle(Vector3 dir){
    float _rotZ = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg + 90.0f;
    float _rot360 = (_rotZ - 180f) < 0.0f ? 360.0f + (_rotZ - 180f) : (_rotZ - 180f);
    return _rot360;
  }

  /// <summary>
  /// Convert HSV to RGB
  /// h is from 0-360
  /// s,v values are 0-1
  /// r,g,b values are 0-255
  /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
  /// </summary>
  public static Color HsvToRgb(float h, float S, float V)
  {
    // ######################################################################
    // T. Nathan Mundhenk
    // mundhenk@usc.edu
    // C/C++ Macro HSV to RGB

    float r;
    float b;
    float g;

    float H = h * 360;
    while (H < 0) { H += 360; };
    while (H >= 360) { H -= 360; };
    float R, G, B;
    if (V <= 0)
    { R = G = B = 0; }
    else if (S <= 0)
    {
      R = G = B = V;
    }
    else
    {
      float hf = H / 60;
      int i = (int)Mathf.Floor(hf);
      float f = hf - i;
      float pv = V * (1 - S);
      float qv = V * (1 - S * f);
      float tv = V * (1 - S * (1 - f));
      switch (i)
      {
        
        // Red is the dominant color
        
      case 0:
        R = V;
        G = tv;
        B = pv;
        break;
        
        // Green is the dominant color
        
      case 1:
        R = qv;
        G = V;
        B = pv;
        break;
      case 2:
        R = pv;
        G = V;
        B = tv;
        break;
        
        // Blue is the dominant color
        
      case 3:
        R = pv;
        G = qv;
        B = V;
        break;
      case 4:
        R = tv;
        G = pv;
        B = V;
        break;
        
        // Red is the dominant color
        
      case 5:
        R = V;
        G = pv;
        B = qv;
        break;
        
        // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
        
      case 6:
        R = V;
        G = tv;
        B = pv;
        break;
      case -1:
        R = V;
        G = pv;
        B = qv;
        break;
        
        // The color is not defined, we should throw an error.
        
      default:
        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
        R = G = B = V; // Just pretend its black/white
        break;
      }
    }
    r = Clamp((int)(R * 255.0));
    g = Clamp((int)(G * 255.0));
    b = Clamp((int)(B * 255.0));

    return new Color(r, g, b, 1);
  }

  /// <summary>
  /// Disance between 3D points project on XY plane.
  /// </summary>
  /// <returns>The distance between 3D points project on XY plane.</returns>
  /// <param name="v1">V1.</param>
  /// <param name="v2">V2.</param>
  public static float disBetween3DPointsProjectOnXYPlane(Vector3 v1, Vector3 v2){
    Vector2 a = new Vector2 (v1.x, v1.y);
    Vector2 b = new Vector2 (v2.x, v2.y);
    return (a - b).magnitude;
  }

  /// <summary>
  /// Clamp a value to 0-255
  /// </summary>
  public static int Clamp(int i)
  {
    if (i < 0) return 0;
    if (i > 255) return 255;
    return i;
  }
}
