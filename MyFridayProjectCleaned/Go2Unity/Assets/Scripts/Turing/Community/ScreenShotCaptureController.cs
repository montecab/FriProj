using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class ScreenShotCaptureController: MonoBehaviour {
  public GameObject[] DisabledUI;
  public Camera myCamera;

  public int resWidth = 2550; 
  public int resHeight = 3300;
  protected bool takeShot = false;
  private Dictionary<GameObject, bool> objTable = new Dictionary<GameObject, bool>();

  protected TextureFormat format = TextureFormat.ARGB32;


  // Use this for initialization
  protected virtual void Start () {

  }

  public void takeScreenshot(){
    takeShot = true;
  }

  public virtual string ScreenShotName(int width, int height) {
    return string.Format("{0}/screen_{1}x{2}_{3}.png", 
      Application.dataPath, 
      width, height, 
      System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
  }

  protected virtual void doBeforeScreenShot(){
    objTable.Clear();
    foreach(GameObject obj in DisabledUI){
      objTable.Add(obj, obj.activeSelf);
      obj.SetActive(false);
    }
  }

  protected virtual void doAfterScreenShot(){
    foreach(GameObject obj in objTable.Keys){
      obj.SetActive(objTable[obj]);
    }
    objTable.Clear();
  }

  protected virtual void LateUpdate(){
    if (takeShot) 
    {
      doBeforeScreenShot();
      RenderTexture rt = new RenderTexture(resWidth, resHeight, 0);

      myCamera.targetTexture = rt;
      Texture2D screenShot = new Texture2D(resWidth, resHeight, format, false);
      myCamera.Render();
      RenderTexture.active = rt;
      screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
      myCamera.targetTexture = null;
      RenderTexture.active = null; 
      Destroy(rt);
      byte[] bytes = screenShot.EncodeToPNG();
      string filename = ScreenShotName(resWidth, resHeight);

      System.IO.File.WriteAllBytes(filename, bytes);
      takeShot = false;
      doAfterScreenShot();
    }
  }

}
