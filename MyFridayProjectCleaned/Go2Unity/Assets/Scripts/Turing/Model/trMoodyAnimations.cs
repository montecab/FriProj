using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;
using System;


namespace Turing {

  public enum trMoodyAnimationType {
    ANIMATION_CATEGORY_1          = 0,
    ANIMATION_CATEGORY_2          = 1,
    ANIMATION_CATEGORY_3          = 2,
    ANIMATION_CATEGORY_4          = 3
  }

  public class trMoodyAnimations : Singleton<trMoodyAnimations> {
    public Dictionary<trMoodyAnimationType, List<trMoodyAnimation>> categories = new Dictionary<trMoodyAnimationType, List<trMoodyAnimation>>();
    public Dictionary<trMoodyAnimationType, string>             categoryNames  = new Dictionary<trMoodyAnimationType, string>();
    
    bool initialized = false;
    //TODO: We should move most of this code into a class called trAnimation and have trMoodyAnimation derive of that. 
    private const string animFilePrefix = "RobotResources/Animations/";
    private const string kMoodIndicator = "<MOOD>";
    private const string kPngFilePrefix = "Sprites/AnimationIcons/";
    private const string kIcon = "icon";
    
    
    public string getAnimNameAccordingToMood(string name, trMoodType runningMood){
      
      if (name.Contains(kMoodIndicator)) { //If this animation is different for different moods
        string moodName = trMood.MoodString(runningMood);
        name = name.Replace(kMoodIndicator, moodName);
        return name;
      }
      else {
        return name;
      }
    }

    public string GetIconPath(string name) {
      if (name.Contains(kMoodIndicator)) { //If this animation is different for different mood
        name = name.Replace(kMoodIndicator, kIcon);
      }
      string path = kPngFilePrefix + name;
      return path;
    }

    public Sprite GetIcon(trMoodyAnimation anim){
      Sprite icon = null;
      if (anim != null){
        icon = Resources.Load<Sprite>(GetIconPath(anim.filename));
        if (icon == null) {
          WWLog.logError("Could not find animation icon: " + anim.UserFacingNameLocalized + " - " + anim.filename);
        }
      }
      else {
        WWLog.logError("asked for animation for null anim.");
      }
      return icon;
    }

    public string getJsonForAnim(string animName, trMoodType runningMood = trMood.DefaultMood, bool useDefault = true) {
      string originalName = animName;
      animName = getAnimNameAccordingToMood(animName, runningMood);

      if (animName == null || animName.Length == 0) {
        WWLog.logError("Animation file name cannot be empty");
        return null;
      }
      string fileName = animFilePrefix + animName;
      TextAsset animationAsset = Resources.Load(fileName) as TextAsset;
      if (useDefault == false && animationAsset == null) {
        return null;
      }
      if (animationAsset == null) {
        WWLog.logWarn("Could not find animation matching " + originalName + " for mood " + runningMood);
        // first look for default animation
        // then enumerate all other "moods" if that fails
        string foundJson = null;
        if (runningMood != trMood.DefaultMood) {
          foundJson = getJsonForAnim(originalName, trMood.DefaultMood, false);
        }
        if (foundJson != null) {
          return foundJson;
        }
        foreach (trMoodType searchMood in Enum.GetValues(typeof(trMoodType))) {
          if ((searchMood != runningMood) && (searchMood != trMood.DefaultMood)) {
            foundJson = getJsonForAnim(originalName, searchMood, false);
            if (foundJson != null) {
              return foundJson;
            }
          }
        }

        WWLog.logError("Could not find any animation matching " + originalName + " for any mood.");
        return getJsonForAnim("dash_happy_spark_behavior_greeting", Turing.trMoodType.HAPPY);
      }
      else {
        return animationAsset.text;
      }
    }
    
    #region list management
    void init() {
      if (initialized) {
        return;
      }
      
      initialized = true;
      
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=1901035233
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      setCategoryName(trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Movement@!@");
      setCategoryName(trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Emotion@!@");
      setCategoryName(trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Interactive@!@");
      setCategoryName(trMoodyAnimationType.ANIMATION_CATEGORY_4, "@!@Passive@!@");
      
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=0
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      addAnimation(10000, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Move Forward@!@", "dash_<MOOD>_spark_behavior_forwardCycle", piRobotType.DASH, true);
      addAnimation(10001, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Move Backward@!@", "dash_<MOOD>_spark_behavior_backwardCycle", piRobotType.DASH, true);
      addAnimation(10002, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Turn Left@!@", "dash_<MOOD>_spark_behavior_turnLeft", piRobotType.DASH, true);
      addAnimation(10003, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Turn Right@!@", "dash_<MOOD>_spark_behavior_turnRight", piRobotType.DASH, true);
      addAnimation(10004, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Turn Around@!@", "dash_<MOOD>_spark_behavior_turnAround", piRobotType.DASH, true);
      addAnimation(10005, trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Say Hi@!@", "dash_<MOOD>_spark_behavior_greeting", piRobotType.DASH, true);
      addAnimation(10006, trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Laugh@!@", "dash_<MOOD>_spark_behavior_laughing", piRobotType.DASH, true);
      addAnimation(10007, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Avoid Obstacle@!@", "dash_<MOOD>_spark_behavior_avoidObstacle", piRobotType.DASH, true);
      addAnimation(10008, trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Say Bye@!@", "dash_<MOOD>_spark_behavior_bye", piRobotType.DASH, true);
      addAnimation(10009, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Look Around@!@", "dash_<MOOD>_spark_behavior_lookAround", piRobotType.DASH, true);
      addAnimation(10010, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Dance Left@!@", "dash_<MOOD>_spark_behavior_dance_left", piRobotType.DASH, true);
      addAnimation(10011, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Dance Right@!@", "dash_<MOOD>_spark_behavior_dance_right", piRobotType.DASH, true);
      addAnimation(10012, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Search @!@", "dash_<MOOD>_spark_behavior_search", piRobotType.DASH, true);
      addAnimation(10013, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Push@!@", "dash_<MOOD>_spark_behavior_pushing", piRobotType.DASH, true);
      addAnimation(10014, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Act Dizzy@!@", "dash_<MOOD>_spark_behavior_dizzy", piRobotType.DASH, true);
      addAnimation(10015, trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Celebrations@!@", "dash_<MOOD>_spark_behavior_celebration", piRobotType.DASH, true);
      addAnimation(10016, trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Take Off@!@", "dash_<MOOD>_spark_behavior_takeOff", piRobotType.DASH, true);
      addAnimation(10017, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Stop@!@", "dash_<MOOD>_spark_behavior_stop", piRobotType.DASH, true);
      addAnimation(10018, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Fall Asleep@!@", "dash_<MOOD>_spark_behavior_fallAsleep", piRobotType.DASH, true);
      addAnimation(10019, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Wake Up@!@", "dash_<MOOD>_spark_behavior_wakeUp", piRobotType.DASH, true);
      addAnimation(10020, trMoodyAnimationType.ANIMATION_CATEGORY_4, "@!@Sleep@!@", "dash_<MOOD>_spark_behavior_sleeping", piRobotType.DASH, true);
      addAnimation(10021, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Burp@!@", "dash_<MOOD>_spark_behavior_burping", piRobotType.DASH, true);
      addAnimation(10022, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Cry@!@", "dash_<MOOD>_spark_behavior_crying", piRobotType.DASH, false);
      addAnimation(10023, trMoodyAnimationType.ANIMATION_CATEGORY_3, "@!@Kiss@!@", "dash_<MOOD>_spark_behavior_kissing", piRobotType.DASH, true);
      addAnimation(10024, trMoodyAnimationType.ANIMATION_CATEGORY_4, "@!@Look Left@!@", "dot_<MOOD>_spark_behavior_lookLeft", piRobotType.DOT, true);
      addAnimation(10025, trMoodyAnimationType.ANIMATION_CATEGORY_4, "@!@Look Right@!@", "dot_<MOOD>_spark_behavior_lookRight", piRobotType.DOT, true);
      addAnimation(10026, trMoodyAnimationType.ANIMATION_CATEGORY_4, "@!@Dance@!@", "dot_<MOOD>_spark_behavior_dancing", piRobotType.DOT, true);
      addAnimation(10027, trMoodyAnimationType.ANIMATION_CATEGORY_4, "@!@Play With Me!@!@", "dot_<MOOD>_spark_behavior_playWithMe", piRobotType.DOT, true);
      addAnimation(10028, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Laugh@!@", "dot_<MOOD>_spark_behavior_laughing", piRobotType.DOT, true);
      addAnimation(10029, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Wait@!@", "dot_<MOOD>_spark_behavior_waiting", piRobotType.DOT, true);
      addAnimation(10030, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Say Hi@!@", "dot_<MOOD>_spark_behavior_greeting", piRobotType.DOT, true);
      addAnimation(10031, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Act Dizzy@!@", "dot_<MOOD>_spark_behavior_getDizzy", piRobotType.DOT, true);
      addAnimation(10032, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Cry@!@", "dot_<MOOD>_spark_behavior_crying", piRobotType.DOT, true);
      addAnimation(10033, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Say Bye@!@", "dot_<MOOD>_spark_behavior_bye", piRobotType.DOT, true);
      addAnimation(10034, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Kiss@!@", "dot_<MOOD>_spark_behavior_kissing", piRobotType.DOT, true);
      addAnimation(10035, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Celebrations@!@", "dot_<MOOD>_spark_behavior_celebration", piRobotType.DOT, true);
      addAnimation(10036, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@8 ball@!@", "dot_<MOOD>_spark_behavior_eightBall", piRobotType.DOT, true);
      addAnimation(10037, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Light Behavior@!@", "dot_<MOOD>_spark_behavior_lightBehavior", piRobotType.DOT, true);
      addAnimation(10038, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Light Sword@!@", "dot_<MOOD>_spark_behavior_lightSword", piRobotType.DOT, true);
      addAnimation(10039, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@PO turned over reaction@!@", "dash_<MOOD>_powerOn_turnedOverReaction_help", piRobotType.DASH, false);
      addAnimation(10040, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@PO turned over reaction No head@!@", "dash_<MOOD>_powerOn_turnedOverReaction_whatThe_noHead", piRobotType.DASH, false);
      addAnimation(10041, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@PO turned over reaction with head@!@", "dash_<MOOD>_powerOn_turnedOverReaction_whatThe_withHeadAnimation", piRobotType.DASH, false);
      addAnimation(10042, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@PO distance sensor interesting@!@", "dash_<MOOD>_powerOn_distanceSensorReaction_interesting", piRobotType.DASH, false);
      addAnimation(10043, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@PO distance sensor ohh@!@", "dash_<MOOD>_powerOn_distanceSensorReaction_ohh", piRobotType.DASH, false);
      addAnimation(10044, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@PO robots are awesome@!@", "dash_<MOOD>_powerOn_seeRobot_areAwesome", piRobotType.DASH, false);
      addAnimation(10045, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@PO turned upright well okay@!@", "dash_<MOOD>_powerOn_turnedUprightReaction_wellOkay", piRobotType.DASH, false);
      addAnimation(10046, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@FW power on@!@", "dash_cautious_firmware_powerOn", piRobotType.DASH, false);
      addAnimation(10047, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@FW fall asleep@!@", "dash_tired_firmware_fallAsleep", piRobotType.DASH, false);
      addAnimation(10048, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@FW power on@!@", "dot_cautious_firmware_powerOn", piRobotType.DOT, false);
      addAnimation(10049, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@FW fall asleep@!@", "dot_tired_firmware_fallAsleep", piRobotType.DOT, false);
      addAnimation(10050, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Transfer@!@", "dash_happy_spark_smTranfer", piRobotType.DASH, false);
      addAnimation(10051, trMoodyAnimationType.ANIMATION_CATEGORY_1, "@!@Transfer@!@", "dot_happy_spark_smTranfer", piRobotType.DOT, false);
      addAnimation(10052, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Wake Up@!@", "dot_<MOOD>_firmware_powerOn", piRobotType.DOT, true);
      addAnimation(10053, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Fall Asleep@!@", "dot_<MOOD>_spark_behavior_fallAsleep", piRobotType.DOT, true);
      // not added to index: dot_wonder_finishChallenge_01
      // not added to index: dot_wonder_finishChallenge_02
      // not added to index: dot_wonder_finishChallenge_03
      // not added to index: dot_wonder_finishChallenge_04
      // not added to index: dot_wonder_tryAgain_01
      // not added to index: dot_wonder_tryAgain_02
      // not added to index: dot_wonder_tryAgain_03
      addAnimation(10061, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Light Behavior@!@", "dash_<MOOD>_spark_behavior_lightBehavior", piRobotType.DASH, false);
      addAnimation(10062, trMoodyAnimationType.ANIMATION_CATEGORY_2, "@!@Scanning@!@", "dash_<MOOD>_spark_behavior_scanning", piRobotType.DASH, false);
    }
    
    public trMoodyAnimation GetAnimation(trState state){
      uint animationId = (uint)state.BehaviorParameterValue;
      return getAnimation(animationId);
    }

    public string getAnimationIconPath(trState state) {
      trMoodyAnimation animation = GetAnimation(state);
      if (animation == null) {
        return null;
      }
      return GetIconPath(animation.filename);
    }
    public int GetUserFacingIndex(trState state){

      trMoodyAnimation animation = GetAnimation(state); // because no mater dash or dot they should have the same index
      return(GetUserFacingIndex(animation));
    }
    
    public int GetUserFacingIndex(trMoodyAnimation animation){
      init();
      if (categories[animation.category].Contains(animation)) {
        return categories[animation.category].IndexOf(animation) + 1;
      }


      WWLog.logError("Cannot find animation " + animation.UserFacingNameLocalized);
      return -1;
    }
    
    private void setCategoryName(trMoodyAnimationType category, string name) {
      categoryNames[category] = name;
    }
    
    public string getCategoryName(trMoodyAnimationType category) {
      if (!categoryNames.ContainsKey(category)) {
        WWLog.logError("no name for category: " + category.ToString());
        return category.ToString();
      }
      
      return categoryNames[category];
    }
    
   
    private void addAnimation(int id,trMoodyAnimationType category,  string userFacingName, string filename, piRobotType robotType, bool isUserFacing) {
      trFeatureVisibility vis = isUserFacing ? trFeatureVisibility.PUBLIC : trFeatureVisibility.OFF;
      addAnimation(id, category, userFacingName, filename, robotType, vis);
    }
   
    private void addAnimation(int id,trMoodyAnimationType category,  string userFacingName, string filename, piRobotType robotType, trFeatureVisibility visibility) {
      init();
      
      if (string.IsNullOrEmpty(filename)) {
        WWLog.logError("anim file name missing: " + id);
        return;
      }

      if (getAnimation((uint)id) != null) {
        WWLog.logError("duplicate animation id: " + id);
        return;
      }
      
      if (!string.IsNullOrEmpty(filename)) {
        addAnimation((uint)id, filename, category, userFacingName, categories, robotType, visibility);
      }
    }
    
    private void addAnimation(uint id, string filename, trMoodyAnimationType category, string userFacingName,
                              Dictionary<trMoodyAnimationType, List<trMoodyAnimation>> categoriesDict , piRobotType robotType, trFeatureVisibility visibility) {
      init();
      if (!categoriesDict.ContainsKey(category)) {
        categoriesDict[category] = new List<trMoodyAnimation>();
      }
      
      trMoodyAnimation animation = new trMoodyAnimation();
      animation.id                = id;
      animation.filename          = filename;
      animation.category          = category;
      animation.userFacingNameRaw = userFacingName;
      animation.RobotType         = robotType;
      animation.visibility        = visibility;

      categoriesDict[category].Add(animation);
    }

    public trMoodyAnimation getAnimation(uint id) {
      init ();
      if (categories == null) {
        return null;
      }
      
      foreach (List<trMoodyAnimation> animations in categories.Values) {
        foreach (trMoodyAnimation animation in animations) {
          if (animation.id == id) {
            return animation;
          }
        }
      }
      
      return null;
    }
    
    public List<trMoodyAnimation> GetCategory(trMoodyAnimationType type) {
      init ();
      return categories[type];
    }
    
    public List<trMoodyAnimationType> GetCategories() {
      init ();
      List<trMoodyAnimationType> ret = new List<trMoodyAnimationType>();
      foreach (trMoodyAnimationType trBT in categoryNames.Keys) {
        ret.Add (trBT);
      }
      return ret;
    }


    public List<trMoodyAnimation> GetAllAnimations(piRobotType robotType) {
      List<trMoodyAnimation> ret = new List<trMoodyAnimation>();
      
      List<trMoodyAnimationType> cats = GetCategories();
      foreach(trMoodyAnimationType trBT in cats) {
        List<trMoodyAnimation> animationsByCategory = GetCategory(trBT);
        foreach(trMoodyAnimation animation in animationsByCategory) {
          if(animation.RobotType == robotType) {
            if (animation.Show) {
              ret.Add(animation);
            }
          }
        }
      }
      
      return ret;
    }
    public List<trMoodyAnimation> GetAllAnimations() {
      List<trMoodyAnimation> ret = new List<trMoodyAnimation>();
      
      List<trMoodyAnimationType> cats = GetCategories();
      foreach(trMoodyAnimationType trBT in cats) {
        List<trMoodyAnimation> animationsByCategory = GetCategory(trBT);
        foreach(trMoodyAnimation animation in animationsByCategory){
           ret.Add(animation);
        }
      }
      
      return ret;
    }
    
    #endregion list management
  }
  
  
  public class trMoodyAnimation {
    public const uint             INVALID_ID        = 0;
    public uint                   id                = INVALID_ID;
    public string                 filename          = ""; // filename on robot
    public trMoodyAnimationType   category          = trMoodyAnimationType.ANIMATION_CATEGORY_1;
    public string                 userFacingNameRaw = "";
    public piRobotType            RobotType         = piRobotType.DASH;
    public trFeatureVisibility    visibility        = trFeatureVisibility.OFF;

    public trMoodyAnimation() {}

    private List<trMoodType> availableMoods = new List<trMoodType>();

    public string UserFacingNameLocalized {
      get {
        return wwLoca.Format(userFacingNameRaw);
      }
    }

    public bool Show {
      get {
        if (visibility == trFeatureVisibility.PUBLIC) {
          return true;
        }
        else if (visibility == trFeatureVisibility.INTERNAL) {
          return trMultivariate.isYESorSHOW(trMultivariate.trAppOption.UNLOCK_INTERNAL_ANIMATIONS);
        }
        else {
          return false;
        }
      }
    }

    public List<trMoodType> AvailableMoods {
      get {
        if (this.availableMoods.Count > 0) {
          return this.availableMoods;
        }
        Array allMoods = Enum.GetValues(typeof(trMoodType));
        foreach(trMoodType mood in allMoods){
          if (mood  == trMoodType.NO_CHANGE) {
            continue;
          }
          //TODO: Revisit this. Resources does not have a way to check if a particular resouce exists. We are loading five different json files here
          // each time we open the animation panel. We can definitely do better here with some caching.
          string animationJson = trMoodyAnimations.Instance.getJsonForAnim(this.filename, mood, false);
          if (animationJson != null) {
            this.availableMoods.Add(mood);
          }
        }
        return this.availableMoods;
      }
    }

    #region serialization
    
    public JSONClass ToJson() {
      JSONClass jsc = new JSONClass();
      jsc[TOKENS.ID] = id.ToString();
      return jsc;
    }
    
    public static trMoodyAnimation FromJson(JSONClass jsc) {
      trMoodyAnimation ret = new trMoodyAnimation();
      
      if (jsc.ContainsKey(TOKENS.ID)) {
        uint animationId = uint.Parse(jsc[TOKENS.ID]);
        ret = trMoodyAnimations.Instance.getAnimation(animationId);
      }

      return ret;
    }
    
    #endregion serialization
  }
}
