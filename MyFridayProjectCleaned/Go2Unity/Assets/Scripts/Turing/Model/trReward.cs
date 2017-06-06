using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Turing;
using WW.SimpleJSON;

namespace Turing {
  public enum trRewardDurableCategory {
    ANIM_ROBOT = 1,
    SOUND_ROBOT = 2,
    BEHAVIOR = 3,
    CUE = 4
  }
  public enum trRewardType {
    IMAGE = 1,
    ANIM_VIDEO = 2    
  }

  public class trRewardDurable {
    public trRewardDurableCategory Category;
    public string Payload;

    // unfortunately since these are all separate objects, caching them is hacky now
    private trBehavior behavior;
    private trRobotSound sound;
    private trMoodyAnimation animation;
    public trTrigger trigger;

    public bool OutOfJson(JSONClass jsc){
      bool isValidDurable = false;
      piStringUtil.ParseStringToEnum<trRewardDurableCategory>(jsc[TOKENS.CATEGORY], out Category); 
      Payload = jsc[TOKENS.PAYLOAD];   
      switch(Category){
        case trRewardDurableCategory.ANIM_ROBOT:
          animation = trMoodyAnimations.Instance.getAnimation(UInt32.Parse(Payload));
          isValidDurable = (animation != null);
          break;
        case trRewardDurableCategory.SOUND_ROBOT:
          sound = trRobotSounds.Instance.GetSound(UInt32.Parse(Payload));
          isValidDurable = (sound != null);
          break;
        case trRewardDurableCategory.BEHAVIOR:
          behavior = trMapSetBehaviors.Instance.GetBehavior(Payload);
          isValidDurable = (behavior != null);
          break;
        case trRewardDurableCategory.CUE:
          trTriggerType triggerType;
          isValidDurable = piStringUtil.ParseStringToEnum<trTriggerType>(Payload, out triggerType);          
          if (isValidDurable){
            trigger = new trTrigger(triggerType);
          }
          break;
      }
      return isValidDurable;
    }

    public string DisplayTextLocalized(){
      switch(Category){
        case trRewardDurableCategory.ANIM_ROBOT:
          return (animation == null) ? "" : animation.UserFacingNameLocalized;
        case trRewardDurableCategory.SOUND_ROBOT:
          return (sound == null) ? "" : trRobotSounds.Instance.getCategoryNameLocalized(sound.category);
        case trRewardDurableCategory.BEHAVIOR:
          return (behavior == null) ? "" : behavior.UserFacingNameLocalized;
        case trRewardDurableCategory.CUE:
          return (trigger == null) ? "" : trigger.UserFacingNameLocalized;
      }
      return "";
    }

    public Sprite DisplayImage(){      
      switch (Category) {
        case trRewardDurableCategory.ANIM_ROBOT:          
          return (animation == null) ? null : trMoodyAnimations.Instance.GetIcon(animation);
        case trRewardDurableCategory.SOUND_ROBOT:
          return (sound == null) ? null : trRobotSounds.Instance.GetIcon(sound);
        case trRewardDurableCategory.BEHAVIOR:
          return (behavior == null) ? null : trMapSetBehaviors.Instance.GetIcon(behavior);
        case trRewardDurableCategory.CUE:
          return (trigger == null) ? null : trigger.ImageIcon;
      }
      return null;
    }

  }

  public class trReward : trTypedBase<trRewardType> {
    public int IQPointsRequired;
    public string Description;
    public string InternalName;
    public List<trRewardDurable> Durables = new List<trRewardDurable>();

    public string Payload;

    protected override void OutOfJson (WW.SimpleJSON.JSONClass jsc) {
      base.OutOfJson (jsc);
      IQPointsRequired = jsc[TOKENS.IQ_POINTS_REQUIRED].AsInt;
      Description = jsc[TOKENS.DESCRIPTION];
      InternalName = jsc[TOKENS.INTERNAL_NAME];
      Payload = jsc[TOKENS.PAYLOAD];

      foreach (JSONNode node in jsc[TOKENS.DURABLES].AsArray) {
        trRewardDurable durable = new trRewardDurable();
        if (durable.OutOfJson(node.AsObject)){
          Durables.Add(durable);          
        }
      }
    }

    public Sprite DisplayImage(){
      string imageName = "reward_default_image";
      if (HasStory()){
        MovieInfo movie = MovieManager.Instance.GetMovieInfo(Payload);
        if (movie != null){
          imageName = movie.ImageName;
        }
      }
      return trImageFactory.GetStoryImage(imageName);
    }

    public Sprite DisplayIcon(){
      string iconName = "reward_default_icon";
      if (HasStory()){
        MovieInfo movie = MovieManager.Instance.GetMovieInfo(Payload);
        if (movie != null){
          iconName = movie.IconName;
        }
      }
      return trIconFactory.GetIcon(iconName);
    }

    public bool HasStory(){
      return (Type == trRewardType.ANIM_VIDEO) && (Payload != null);
    }

    public MovieInfo StoryAnimation(){
      return HasStory() ? MovieManager.Instance.GetMovieInfo(Payload) : null;
    }

    /* these routines seem unused
    public bool HasDurable(trRewardDurableCategory category, string durableValue){
      bool result = false;
      foreach (trRewardDurable durable in Durables){
        if ((durable.Category == category) && (durable.Payload == durableValue)) {
          result = true;
          break;
        }
      }
      return result;
    }

    public bool HasDurable(trRewardDurableCategory category, int durableValue){
      bool result = false;
      foreach (trRewardDurable durable in Durables){
        if ((durable.Category == category) && (durable.Payload == durableValue.ToString())) {
          result = true;
          break;
        }
      }
      return result;
    }
    */
  }

}