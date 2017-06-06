using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;


namespace Turing {
  //TODO:UI: This is a deprecated class
  public class trTutorialSlidesController : MonoBehaviour {
    
    public trTutorialSlide[] Slides;
    public GameObject        NextButton;
    public string            UniqueName = uninitializedUniqueName;
    public trMultivariate.trAppOption AlwaysshowIfYes;
    
    private const string uninitializedUniqueName = "change me to anything. one per slide controller";
    private int currentSlideIndex = -1;
    private const string TUTORIAL_DONE = "tutorial_done";

    private const int kMinDaysBetweenShowingSlides = 7; //one week
    
    // Use this for initialization
    void Start () {
      if (UniqueName == uninitializedUniqueName) {
        WWLog.logError("you need to change the uniqueName field of this trTutorialSlidesController.");
      }
        
      if (ShouldDisplay) {
        nextSlide();
      }
    }
    
    void Update() {
      bool stillShowingStuff = false;
      foreach (trTutorialSlide trTS in Slides) {
        stillShowingStuff = stillShowingStuff | trTS.gameObject.activeSelf;
      }
      
      if (!stillShowingStuff) {
        turnOff();
      }
    }



    private string getPlayerPrefKey () {
      return UniqueName + TUTORIAL_DONE;
    }

    public virtual bool ShouldDisplay {
      get {
        // This tutorial is shown to the user once for every installation of the app.
        //TODO: Need to have a way to call PlayerPrefs.DeleteAll() using a shortcut. This is a nighmare to reset otherwise for testing purposes
        bool shouldDisplay = false;
        if(AlwaysshowIfYes != trMultivariate.trAppOption.NULL_OPTION){
          if(trMultivariate.isYESorSHOW(AlwaysshowIfYes)){
            PlayerPrefs.DeleteAll();
            trMultivariate.Instance.setOptionValue(AlwaysshowIfYes, trMultivariate.trAppOptionValue.NO);
          }
        }
        string lastTimeTutorialSeen = PlayerPrefs.GetString(getPlayerPrefKey(), "");
        if(lastTimeTutorialSeen == "") {
          shouldDisplay = true;
        }
        else {
          DateTime lastTimeDate = DateTime.Parse(lastTimeTutorialSeen);
          DateTime currentTime = DateTime.Now;
          if (currentTime.Subtract(lastTimeDate).Days >= kMinDaysBetweenShowingSlides) {
            shouldDisplay = true;
          }
        }
        return shouldDisplay;
      }
    }

//    bool ShouldDisplay {
//      get {
//        return wwDoOncePerTypeVal<string>.doIt(UniqueName);
//      }
//    }
    
    public void onClickNextSlide() {
      nextSlide ();
    }
    
    void nextSlide() {
      if (Slides.Length == 0) {
        turnOff();
        return;
      }
      else {
        turnOn();
      }
      
      if (currentSlideIndex >= Slides.Length) {
        return;
      }
      
      if (currentSlideIndex >= 0) {
        Slides[currentSlideIndex].fadeOut();
      }
      if (currentSlideIndex < (Slides.Length - 1)) {
        Slides[currentSlideIndex + 1].fadeIn();
      }
      
      incrementSlideIndex();

      if (currentSlideIndex == Slides.Length) {
        turnOff();
        if(NextButton != null){
          NextButton.transform.DOScale(Vector3.zero, 0.3f);
        }
       
        DateTime currentTime = DateTime.Now;
        PlayerPrefs.SetString(getPlayerPrefKey(), currentTime.ToString());
        PlayerPrefs.Save();
      }
    }
    
    void incrementSlideIndex() {
      currentSlideIndex += 1;
      
      while (muteCurrentSlide()) {
        currentSlideIndex += 1;
      }
    }
    
    // return true if and only if the current slide index is valid,
    // and the associated slide's "skipIfNO" setting indicates skip.
    bool muteCurrentSlide() {
      if ((currentSlideIndex < 0) || (currentSlideIndex >= Slides.Length)) {
        return false;
      }
      
      trMultivariate.trAppOption trAO = Slides[currentSlideIndex].skipIfNO;
      if (trAO == trMultivariate.trAppOption.NULL_OPTION) {
        return false;
      }
      
      return (trMultivariate.Instance.getOptionValue(trAO) == trMultivariate.trAppOptionValue.NO);
    }
    
    void turnOn() {
      gameObject.SetActive(true);
      piConnectionManager.Instance.hideChromeButton();
    }
    
    void turnOff() {
      gameObject.SetActive(false);
      piConnectionManager.Instance.showChromeButton();
    }
  }
}
