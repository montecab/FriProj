using UnityEngine;
using System.Collections.Generic;

namespace Turing {

  
  public class trRobotSounds : Singleton<trRobotSounds> {
    public Dictionary<trBehaviorType, List<trRobotSound>> categoriesDash = new Dictionary<trBehaviorType, List<trRobotSound>>();
    public Dictionary<trBehaviorType, List<trRobotSound>> categoriesDot  = new Dictionary<trBehaviorType, List<trRobotSound>>();
    private Dictionary<trBehaviorType, string>             categoryNames  = new Dictionary<trBehaviorType, string>();
    
    bool initialized = false;
    
    public const trBehaviorType HIDDEN_SOUNDS_CATEGORY = trBehaviorType.SOUND_INTERNAL;
    
    #region list management
    void init() {
      if (initialized) {
        return;
      }
      
      initialized = true;
      
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=1901035233
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      setCategoryName(trBehaviorType.SOUND_USER, "@!@Custom@!@");
      setCategoryName(trBehaviorType.SOUND_INTERNAL, "@!@n/a@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Brave@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Cautious@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Curious@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Frustrated@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Happy@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_SILLY, "@!@Silly@!@");
      setCategoryName(trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Surprised@!@");
      setCategoryName(trBehaviorType.SOUND_ANIMAL, "@!@Animal@!@");
      setCategoryName(trBehaviorType.SOUND_SFX, "@!@Effects@!@");
      setCategoryName(trBehaviorType.SOUND_TRANSPORT, "@!@Vehicles@!@");
                                          
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=0
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      addSound(1000, "VOICE0", "VOICE0", trBehaviorType.SOUND_USER, "@!@Custom Sound 1@!@");
      addSound(1001, "VOICE1", "VOICE1", trBehaviorType.SOUND_USER, "@!@Custom Sound 2@!@");
      addSound(1002, "VOICE2", "VOICE2", trBehaviorType.SOUND_USER, "@!@Custom Sound 3@!@");
      addSound(1003, "VOICE3", "VOICE3", trBehaviorType.SOUND_USER, "@!@Custom Sound 4@!@");
      addSound(1004, "VOICE4", "VOICE4", trBehaviorType.SOUND_USER, "@!@Custom Sound 5@!@");
      addSound(1005, "VOICE5", "VOICE5", trBehaviorType.SOUND_USER, "@!@Custom Sound 6@!@");
      addSound(1006, "VOICE6", "VOICE6", trBehaviorType.SOUND_USER, "@!@Custom Sound 7@!@");
      addSound(1007, "VOICE7", "VOICE7", trBehaviorType.SOUND_USER, "@!@Custom Sound 8@!@");
      addSound(1008, "VOICE8", "VOICE8", trBehaviorType.SOUND_USER, "@!@Custom Sound 9@!@");
      addSound(1009, "VOICE9", "VOICE9", trBehaviorType.SOUND_USER, "@!@Custom Sound 10@!@");
      addSound(3000, "FOOBAR_DASH", "FOOBAR_DOT", trBehaviorType.SOUND_INTERNAL, "@!@@!@");
      addSound(2001, "COW_MOO11A", "COW_MOO11A", trBehaviorType.SOUND_ANIMAL, "@!@Cow@!@");
      addSound(2002, "CROCODILE", "CROCODILE", trBehaviorType.SOUND_ANIMAL, "@!@Crocodile@!@");
      addSound(2003, "DINOSAUR_3", "DINOSAUR_3", trBehaviorType.SOUND_ANIMAL, "@!@Dinosaur@!@");
      addSound(2004, "DUCK_QUACK", "DUCK_QUACK", trBehaviorType.SOUND_ANIMAL, "@!@Duck@!@");
      addSound(2005, "ELEPHANT_0", "ELEPHANT_0", trBehaviorType.SOUND_ANIMAL, "@!@Elephant@!@");
      addSound(2006, "FX_03_GOAT", "FX_03_GOAT", trBehaviorType.SOUND_ANIMAL, "@!@Goat@!@");
      addSound(2007, "FX_CAT_01", "FX_CAT_01", trBehaviorType.SOUND_ANIMAL, "@!@Cat@!@");
      addSound(2008, "FX_DOG_02", "FX_DOG_02", trBehaviorType.SOUND_ANIMAL, "@!@Dog@!@");
      addSound(2009, "FX_LION_01", "FX_LION_01", trBehaviorType.SOUND_ANIMAL, "@!@Lion@!@");
      addSound(0, "", "", trBehaviorType.SOUND_ANIMAL, "@!@Horse #1@!@");
      addSound(2011, "HORSEWHIN3", "HORSEWHIN3", trBehaviorType.SOUND_ANIMAL, "@!@Horse@!@");
      addSound(2012, "PIGSNORT_2", "PIGSNORT_2", trBehaviorType.SOUND_ANIMAL, "@!@Pig@!@");
      addSound(2013, "ROOSTER_01", "ROOSTER_01", trBehaviorType.SOUND_ANIMAL, "@!@Rooster@!@");
      addSound(2014, "ALRIGHT_03", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@All Right!@!@");
      addSound(2015, "BRAGGING1A", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Bragging@!@");
      addSound(2016, "", "CANNONBALL", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Cannonball!@!@");
      addSound(2017, "CHARGE_03", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Charge #1@!@");
      addSound(2018, "CHARGE_04", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Charge #2@!@");
      addSound(2019, "TRUMPET_01", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Charge #3@!@");
      addSound(2020, "COMING", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Coming!@!@");
      addSound(2021, "", "DOIT_DOT", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Do it!@!@");
      addSound(2022, "", "GO_DOT", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Go!@!@");
      addSound(2023, "GOTTADOIT", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Got to do it!@!@");
      addSound(2024, "HEREICOME", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Here I come!@!@");
      addSound(2025, "", "HOLD_ME", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Hold me!@!@");
      addSound(2026, "", "ON_A_ROLL", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@I'm on a roll!@!@");
      addSound(2027, "LETS_DO_IT", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Let's do it!@!@");
      addSound(2028, "LETSGO_02", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Let's go! #1@!@");
      addSound(2029, "LETSGO_03", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Let's go! #2@!@");
      addSound(2030, "LETS_GO", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Let's go! #3@!@");
      addSound(2031, "", "MEMEMEE", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Me, me, me!@!@");
      addSound(2032, "", "METOO", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Me too!@!@");
      addSound(2033, "NO_FEAR", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@No fear!@!@");
      addSound(2034, "OHH_06", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Ohhh!@!@");
      addSound(2035, "OKALRIGHT1", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Okay, alright@!@");
      addSound(2036, "OKAY_03", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Okay@!@");
      addSound(2037, "", "READYSET", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Ready, set, go!@!@");
      addSound(2038, "", "SHAKE_IT", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Shake it, Baby!@!@");
      addSound(2039, "TAH_DAH_01", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Tah dah!@!@");
      addSound(2040, "UMMHMM_02", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Umm hmm@!@");
      addSound(2041, "", "CATCHME", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Whoa, catch me!@!@");
      addSound(2042, "WUH_HO_5", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Wuh ho!@!@");
      addSound(2043, "", "YOLO", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@Yolo!@!@");
      addSound(2044, "", "ANGLEHERE", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Hey, what's the angle, here?@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Danger, Will Robinson!@!@");
      addSound(2046, "HELP_HELP", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Help!@!@");
      addSound(2047, "HEY_A", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Hey@!@");
      addSound(2048, "", "HIDEME", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Hide me!@!@");
      addSound(2049, "HMM_MAYBE", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Hmm, maybe...@!@");
      addSound(2050, "HMM_NO", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Hmm, no@!@");
      addSound(2051, "", "TOSS_UP", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@It's a toss up...@!@");
      addSound(2052, "", "WITHMAYBE", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@I'm gonna go with...maybe@!@");
      addSound(2053, "", "WITHNO", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@I'm gonna go with...no@!@");
      addSound(2054, "", "WITHYES", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@I'm gonna go with...yes@!@");
      addSound(2055, "", "LITTLEHELP", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Little help?@!@");
      addSound(2056, "NERVOUS_01", "NERVOUS_01", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Nervous #1@!@");
      addSound(2057, "NERVOUS_03", "NERVOUS_03", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Nervous #2@!@");
      addSound(2058, "NERVOUS_06", "NERVOUS_06", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Nervous #3@!@");
      addSound(2059, "NERVOUS_07", "NERVOUS_07", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Nervous #4@!@");
      addSound(2060, "NERVOUS_09", "NERVOUS_09", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Nervous #5@!@");
      addSound(2061, "NERVOUS_10", "NERVOUS_10", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Nervous #6@!@");
      addSound(2062, "", "NOTSURTHIS", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Not sure about this...@!@");
      addSound(2063, "OOPS_02", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Oops #1@!@");
      addSound(2064, "OOPS_03", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Oops #2@!@");
      addSound(2065, "", "ROLL_BY_ME", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Roll that by me again@!@");
      addSound(2066, "", "CYA", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@See ya!@!@");
      addSound(2067, "", "UHMM", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Uhmmm...@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Uhmmm, maybe...@!@");
      addSound(2069, "", "UNH_NUH", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Un, unh!@!@");
      addSound(2070, "WELL_OKAY", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Well...okay@!@");
      addSound(2071, "WHASHORT", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Wha? #1@!@");
      addSound(2072, "WHUH_OH_20", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Wuh oh!@!@");
      addSound(2073, "WOAH_NO", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Whoa, no!@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@One@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Two@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Three@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Four@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Five@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Six@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Seven@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Eight@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Nine@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Ten@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Eleven@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Twelve@!@");
      addSound(2086, "", "ANSWERIS", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@And, the answer is...@!@");
      addSound(2087, "", "BORED_A", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Bored #1@!@");
      addSound(2088, "", "BORED_B", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Bored #2@!@");
      addSound(2089, "COOL", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Cool!@!@");
      addSound(2090, "CURIOUS_04", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Huh? #1@!@");
      addSound(2091, "CURIOUS_06", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Huh? #2@!@");
      addSound(2092, "HUH_06", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Huh? #3@!@");
      addSound(2093, "", "EYE_SPY", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@I spy with my glowing eye...@!@");
      addSound(2094, "I_SEE", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@I see...@!@");
      addSound(2095, "INTRESTING", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Interesting...@!@");
      addSound(2096, "LETMESEE", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Let me see that one@!@");
      addSound(2097, "", "LITTLEPEEK", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Just a little peek?@!@");
      addSound(2098, "LOOK_THAT", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Look at that@!@");
      addSound(2099, "ME_SEE", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Let me see@!@");
      addSound(2100, "", "OVERHERE", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@I'm gonna look over there@!@");
      addSound(2101, "", "WHATSHERE", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Oh, what's over here...@!@");
      addSound(2102, "CURIOUS_05", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Unh!@!@");
      addSound(2103, "", "WHOGOTHERE", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Wait, who goes there?@!@");
      addSound(2104, "DASH_WHAA1", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Wha?@!@");
      addSound(2105, "", "WHEREAMI", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Whoa, where am I?@!@");
      addSound(2106, "", "LEFT", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Left@!@");
      addSound(2107, "", "RIGHT", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Right@!@");
      addSound(2108, "", "UP", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Up@!@");
      addSound(2109, "", "DOWN", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Down@!@");
      addSound(2110, "", "BLUE", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Blue@!@");
      addSound(2111, "", "GREEN", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Green@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Orange@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Purple@!@");
      addSound(2114, "", "RED", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Red@!@");
      addSound(2115, "", "YELLOW", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@Yellow@!@");
      addSound(2116, "", "AYEAYE", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Aye, aye, aye!@!@");
      addSound(2117, "", "BUBYE", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Buh bye!@!@");
      addSound(2118, "BO_V7_VARI", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Bye!@!@");
      addSound(2119, "", "NOTCOMPUTE", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Does not compute@!@");
      addSound(2120, "FORGET_IT", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Forget it!@!@");
      addSound(2121, "GROWL_11A", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Growl@!@");
      addSound(2122, "HEAVYOBJEC", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Grunt@!@");
      addSound(2123, "HUMPH", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Humph!@!@");
      addSound(2124, "", "HUMPH_A", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Humph! #1@!@");
      addSound(2125, "", "HUMPH_B", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Humph! #2@!@");
      addSound(2126, "", "OUTOFHERE", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@I'm out of here!@!@");
      addSound(2127, "NO_WAY", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@No way!@!@");
      addSound(2128, "NOT_GOOD", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Not good@!@");
      addSound(2129, "NOTTHATONE", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Not that one@!@");
      addSound(2130, "", "OH_FINE_HI", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Oh fine, hi.@!@");
      addSound(2131, "OH_NO_UNH", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Oh no...un nuh@!@");
      addSound(2132, "Bo_OKAY_03", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Okay@!@");
      addSound(2133, "SNORING", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Snore@!@");
      addSound(2134, "", "WAKEUP_DOT", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Wake up@!@");
      addSound(2135, "", "WAITWHAT", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Wait...wha?@!@");
      addSound(2136, "", "WHYYOU", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Why, you!@!@");
      addSound(2137, "", "MYBUTTONS", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@You're really pushing my buttons.@!@");
      addSound(2138, "", "YAYAYAH", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Yeah, yeah, yeah!@!@");
      addSound(2139, "", "YAWN_DOT", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Yawn@!@");
      addSound(2140, "TIRED_YAWN", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Yawn@!@");
      addSound(2141, "", "YOUSHUSH", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@You shush!@!@");
      addSound(2142, "BO_V7_YAWN", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Ahh@!@");
      addSound(2143, "AWESOME", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Awesome@!@");
      addSound(2144, "", "AWWW_A", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Awww #1@!@");
      addSound(2145, "", "AWWW_B", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Awww #2@!@");
      addSound(2146, "AWWW_04", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Awww@!@");
      addSound(2147, "FANTASTIC", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Fantastic@!@");
      addSound(2148, "GIGGLE_03", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Giggle #1@!@");
      addSound(2149, "V7GIGGLEI6", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Giggle #2@!@");
      addSound(2150, "GOOD_ONE", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Good one@!@");
      addSound(2151, "", "BIRTHDAY", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Happy birthday!@!@");
      addSound(2152, "HEY_B", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Hey@!@");
      addSound(2153, "DASH_HI_VO", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Hi!@!@");
      addSound(2154, "", "HI", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Hi!@!@");
      addSound(2155, "HAPPYLAUGH", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Laugh@!@");
      addSound(2156, "", "NICE_ONE", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Nice one!@!@");
      addSound(2157, "DASH_OKAY1", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Okay #3@!@");
      addSound(2158, "", "ON_BUTTON", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@On the button@!@");
      addSound(2159, "ON_THE_WAY", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@On the way@!@");
      addSound(2160, "", "TOODLES", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Toodles!@!@");
      addSound(2161, "HAVINGFUN1", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Whoooo!@!@");
      addSound(2162, "YAH_02", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Yeah!@!@");
      addSound(2163, "YIPPEE", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Yippee! #1@!@");
      addSound(2164, "YIPPEE_02", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Yippee! #2@!@");
      addSound(2165, "SIGH_DASH", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Sigh@!@");
      addSound(2166, "THATS_IT", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@That's it!@!@");
      addSound(2167, "WAYTOGO", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Way to go!@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@We're sure having a ball!@!@");
      addSound(2169, "", "WHEE_DOT", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Whee!@!@");
      addSound(2170, "WHEEYEEYEE", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Whee yee yee!@!@");
      addSound(2171, "WHISTLE_A", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Whistle #1@!@");
      addSound(2172, "WHISTLE_B", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Whistle #2@!@");
      addSound(2173, "", "YEP_DOT", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Yep!@!@");
      addSound(2174, "", "YES_DOT", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Yes@!@");
      addSound(2175, "AREAWESOME", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@You, my good friend, are awesome@!@");
      addSound(2176, "", "WELCOME", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@You're welcome@!@");
      addSound(2177, "", "NOTES_DO", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Do@!@");
      addSound(2178, "", "NOTES_RE", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Re@!@");
      addSound(2179, "", "NOTES_MI", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Mi@!@");
      addSound(2180, "", "NOTES_FA", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Fa@!@");
      addSound(2181, "", "NOTES_SO", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@So@!@");
      addSound(2182, "", "NOTES_LA", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@La@!@");
      addSound(2183, "", "NOTES_TI", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Ti@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Ayi, Ayi, Ayi, Ayi song@!@");
      addSound(2185, "", "BEEPBOOP", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Beep boop bing bong@!@");
      addSound(2186, "BING_BONG", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Bing Bong song@!@");
      addSound(2187, "US_LIPBUZZ", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Brrrraaaahhh!@!@");
      addSound(2188, "BURP", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Burp!@!@");
      addSound(2189, "", "BURP", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Burp!@!@");
      addSound(2190, "", "CRYING", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Crying@!@");
      addSound(2191, "CONFUSED_2", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Dizzy@!@");
      addSound(2192, "", "DIZZY_A", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Dizzy #1@!@");
      addSound(2193, "", "DIZZY_B", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Dizzy #2@!@");
      addSound(2194, "DOODAYDOO", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Doo Day Doo song@!@");
      addSound(2195, "", "GIGGLE", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Giggle@!@");
      addSound(2196, "HELLOSHIFT", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Hello, hello, hello, hello...@!@");
      addSound(2197, "", "HOWDY", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Howdy!@!@");
      addSound(2198, "INPUTS", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Inputs and Outputs song@!@");
      addSound(2199, "", "LOOK_EYES", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Look into my eyes!@!@");
      addSound(2200, "", "MYARPMYARP", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Myarp, myarp@!@");
      addSound(0, "", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@My favorite food is spaghetti and meatballs.@!@");
      addSound(2202, "CONFUSED_6", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Pffft!@!@");
      addSound(2203, "RASPBERRY", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Raspberry@!@");
      addSound(2204, "", "RASPBERRY", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Raspberry@!@");
      addSound(2205, "", "SHAKEROBOT", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Shake, shake, shake! Shake your robot!@!@");
      addSound(2206, "CONFUSED_3", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Uh uh@!@");
      addSound(2207, "", "UPSIDEDOWN", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Upside down, you can turn me!@!@");
      addSound(2208, "VROOM", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Vroom vroom!@!@");
      addSound(2209, "EXCITED_01", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Wheee!@!@");
      addSound(2210, "EXCITED_02", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Whoo hoo!@!@");
      addSound(2211, "EXCITED_06", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Whoo wee!@!@");
      addSound(2212, "YAHOO_02", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Yahoo!@!@");
      addSound(2213, "YAUHHUH", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Yeah uh huh@!@");
      addSound(2214, "YEEHAW", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Yeehaw!@!@");
      addSound(2215, "", "SNORE_DOT", trBehaviorType.SOUND_VOCAL_SILLY, "@!@ZZzzzz@!@");
      addSound(2216, "BWAHH", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Bwahh!@!@");
      addSound(2217, "CONFUSED_1", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Aiyiyi@!@");
      addSound(2218, "HAHAHOT", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Ha, ha, hot!@!@");
      addSound(2219, "", "HAHAHOT", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Ha, ha, hot!@!@");
      addSound(2220, "", "HAHAH", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Ha, hah!@!@");
      addSound(2221, "SCARED_ME", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Haha, you scared me!@!@");
      addSound(2222, "", "IMSCARED", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@I'm scared!@!@");
      addSound(2223, "SURPRISE02", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Ohhh!@!@");
      addSound(2224, "V7_OHNO_09", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Oh no!@!@");
      addSound(2225, "THATS_COOL", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@That's cool!@!@");
      addSound(2226, "WHAT_THE1", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@What the...?@!@");
      addSound(2227, "CONFUSED_5", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Whoa! #1@!@");
      addSound(2228, "CONFUSED_8", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Whoa! #2@!@");
      addSound(2229, "DASH_WOW_3", "", trBehaviorType.SOUND_VOCAL_SURPRISED, "@!@Wow!@!@");
      addSound(2230, "FXBOING", "", trBehaviorType.SOUND_SFX, "@!@Boing!@!@");
      addSound(2231, "DBDELIVERY", "", trBehaviorType.SOUND_SFX, "@!@Doorbell@!@");
      addSound(2232, "GOBBLE_001", "GOBBLE_001", trBehaviorType.SOUND_SFX, "@!@Gobble@!@");
      addSound(2233, "", "LS_1", trBehaviorType.SOUND_SFX, "@!@Light sword #1@!@");
      addSound(2234, "", "LS_2", trBehaviorType.SOUND_SFX, "@!@Light sword #2@!@");
      addSound(2235, "", "LSIMPACT", trBehaviorType.SOUND_SFX, "@!@Light sword hit@!@");
      addSound(2236, "ROAR", "", trBehaviorType.SOUND_SFX, "@!@Monster roar@!@");
      addSound(2237, "ROBOT_01", "ROBOT_01", trBehaviorType.SOUND_SFX, "@!@Robot #1@!@");
      addSound(2238, "ROBOT_02", "ROBOT_02", trBehaviorType.SOUND_SFX, "@!@Robot #2@!@");
      addSound(2239, "ROBOT_03", "ROBOT_03", trBehaviorType.SOUND_SFX, "@!@Robot #3@!@");
      addSound(2240, "ROBOT_04", "ROBOT_04", trBehaviorType.SOUND_SFX, "@!@Robot #4@!@");
      addSound(2241, "ROBOT_05", "ROBOT_05", trBehaviorType.SOUND_SFX, "@!@Robot #5@!@");
      addSound(2242, "ROBOT_06", "ROBOT_06", trBehaviorType.SOUND_SFX, "@!@Robot #6@!@");
      addSound(2243, "ROBOT_07", "ROBOT_07", trBehaviorType.SOUND_SFX, "@!@Robot #7@!@");
      addSound(2244, "ROBOT_08", "ROBOT_08", trBehaviorType.SOUND_SFX, "@!@Robot #8@!@");
      addSound(2245, "SHORTBOOST", "SHORTBOOST", trBehaviorType.SOUND_SFX, "@!@Short rocket boost@!@");
      addSound(2246, "SPEEDBOOST", "SPEEDBOOST", trBehaviorType.SOUND_SFX, "@!@Rocket boost@!@");
      addSound(2247, "X_SIREN_02", "X_SIREN_02", trBehaviorType.SOUND_SFX, "@!@Siren@!@");
      addSound(2248, "TRDELIVERY", "", trBehaviorType.SOUND_SFX, "@!@Truck beeps@!@");
      addSound(2249, "BOT_CUTE_0", "BOT_CUTE_0", trBehaviorType.SOUND_SFX, "@!@Weird Beeps #1@!@");
      addSound(2250, "OT_CUTE_03", "OT_CUTE_03", trBehaviorType.SOUND_SFX, "@!@Weird Beeps #2@!@");
      addSound(2251, "OT_CUTE_04", "OT_CUTE_04", trBehaviorType.SOUND_SFX, "@!@Weird Beeps #3@!@");
      addSound(2252, "AIRPORTJET", "AIRPORTJET", trBehaviorType.SOUND_TRANSPORT, "@!@Airplane@!@");
      addSound(2253, "HAPPY_HONK", "HAPPY_HONK", trBehaviorType.SOUND_TRANSPORT, "@!@Car horn@!@");
      addSound(2254, "BREAKDOWN", "", trBehaviorType.SOUND_TRANSPORT, "@!@Car trouble@!@");
      addSound(2255, "CHANGETIRE", "", trBehaviorType.SOUND_TRANSPORT, "@!@ Change tire@!@");
      addSound(2256, "ENGINE_REV", "ENGINE_REV", trBehaviorType.SOUND_TRANSPORT, "@!@Engine@!@");
      addSound(2257, "FLATTIRE2A", "FLATTIRE2A", trBehaviorType.SOUND_TRANSPORT, "@!@Flat tire@!@");
      addSound(2258, "HELICOPTER", "HELICOPTER", trBehaviorType.SOUND_TRANSPORT, "@!@Helicopter@!@");
      addSound(2259, "", "JET", trBehaviorType.SOUND_TRANSPORT, "@!@Jet@!@");
      addSound(2260, "SPINOUT_01", "SPINOUT_01", trBehaviorType.SOUND_TRANSPORT, "@!@Spin out@!@");
      addSound(2261, "TRACTOR_02", "", trBehaviorType.SOUND_TRANSPORT, "@!@Tractor@!@");
      addSound(2262, "TRAIN_WHIS", "TRAIN_WHIS", trBehaviorType.SOUND_TRANSPORT, "@!@Train@!@");
      addSound(2263, "TRUCKHORN", "TRUCKHORN", trBehaviorType.SOUND_TRANSPORT, "@!@Truck horn@!@");
      addSound(2264, "TIRESQUEAL", "TIRESQUEAL", trBehaviorType.SOUND_TRANSPORT, "@!@Tire squeal@!@");
      addSound(2265, "TUGBOAT_01", "TUGBOAT_01", trBehaviorType.SOUND_TRANSPORT, "@!@Tugboat@!@");
      addSound(2266, "BLOW_KISS", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Blow kiss@!@");
      addSound(2267, "HOWDY", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Howdy doo!@!@");
      addSound(2268, "TOODLE_OO", "", trBehaviorType.SOUND_VOCAL_SILLY, "@!@Toodle ooh!@!@");
      addSound(2269, "KISS", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Kiss@!@");
      addSound(2270, "BIRTHDAY", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Happy birthday!@!@");
      addSound(2271, "GOODBYE", "", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Goodbye@!@");
      addSound(2272, "", "BLOW_KISS", trBehaviorType.SOUND_VOCAL_HAPPY, "@!@Blow kiss@!@");
      addSound(2273, "HEY_C", "", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Hey@!@");
      addSound(2274, "HOWSGOING", "", trBehaviorType.SOUND_VOCAL_CURIOUS, "@!@How's it going?@!@");
      addSound(2275, "TOODLES", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Toodles...@!@");
      addSound(2276, "HELLOECHO", "", trBehaviorType.SOUND_VOCAL_CAUTIOUS, "@!@Helloooo!@!@");
      addSound(2277, "CYA", "", trBehaviorType.SOUND_VOCAL_BRAVE, "@!@See ya!@!@");
      addSound(2278, "", "WHOA", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Whoa!@!@");
      addSound(2279, "", "FACEPLANT", trBehaviorType.SOUND_VOCAL_FRUSTRATED, "@!@Face Plant!@!@");
    }

    public Sprite GetIcon(trRobotSound sound){
      Sprite icon = null;
      if (sound != null){
        icon = trIconFactory.GetIcon(sound.category);                    
      }
      return icon;
    }

    public int GetUserFacingIndex(trState state){
      if(!state.Behavior.isSoundBehaviour()){
        return -1;
      }
      trRobotSound sound = GetSound(state, piRobotType.DASH); 
      if(sound == null){
        sound = GetSound(state, piRobotType.DOT);
      }
      if(sound == null){ // ingredient panel doesn't know about the index
        return 1;
      }
      return(GetUserFacingIndex(sound));
    }

    public int GetUserFacingIndex(trRobotSound sound){
      init();
      if(sound == null){
        WWLog.logError("Sound is null");
        return -1;
      }
      if(categoriesDash.ContainsKey(sound.category) && categoriesDash[sound.category].Contains(sound)){
        return categoriesDash[sound.category].IndexOf(sound) + 1;
      }
      else if(categoriesDot.ContainsKey(sound.category)&& categoriesDot[sound.category].Contains(sound)){
        return categoriesDot[sound.category].IndexOf(sound) + 1;
      }

      WWLog.logError("Cannot find sound " + sound.UserFacingNameUnlocalized + " in dash or dot sounds");
      return -1;
    }
    
    private void setCategoryName(trBehaviorType category, string name) {
      categoryNames[category] = name;
    }
    
    public string getCategoryNameLocalized(trBehaviorType category) {
      if (!categoryNames.ContainsKey(category)) {
        WWLog.logError("no name for category: " + category.ToString());
        return category.ToString();
      }
      
      return wwLoca.Format(categoryNames[category]);
    }

    public trRobotSound GetSound(trState state, piRobotType type){
      if(!state.Behavior.isSoundBehaviour()){
        return null;
      }
      uint soundID =(uint)state.BehaviorParameterValue;
      return GetSound(soundID, type);
    }
    
    public trRobotSound GetSound(uint id, piRobotType robotType) {
      init();
      return getSound(id, categoryDictForRobotType(robotType));
    }

    public trRobotSound GetSound(uint id){
      trRobotSound sound = GetSound(id, piRobotType.DASH);
      if (sound == null) {
        sound = GetSound(id, piRobotType.DOT);
      }
      return sound;
    }
    
    private trRobotSound getSound(uint id, Dictionary<trBehaviorType, List<trRobotSound>> categoriesDict) {
      if (categoriesDict == null) {
        return null;
      }
      
      foreach (List<trRobotSound> sounds in categoriesDict.Values) {
        foreach (trRobotSound sound in sounds) {
          if (sound.id == id) {
            return sound;
          }
        }
      }
      
      return null;
    }
    
    private void addSound(int id, string filenameDash, string filenameDot, trBehaviorType category, string userFacingName) {      
      init();
      if (id == 0) {
        return;
      }
      
      if (string.IsNullOrEmpty(filenameDash) && string.IsNullOrEmpty(filenameDot)) {
        WWLog.logError("sound for no robots! id: " + id);
        return;
      }
      
      // check for dupes
      if ((getSound((uint)id, categoriesDash) != null) || (getSound((uint)id, categoriesDash) != null)) {
        WWLog.logError("duplicate sound id: " + id);
        return;
      }

      // trim leading & trailing spaces      
      userFacingName = userFacingName.Trim();
      
      if (!string.IsNullOrEmpty(filenameDash)) {
        addSound((uint)id, filenameDash, category, userFacingName, categoriesDash);
      }
      
      if (!string.IsNullOrEmpty(filenameDot)) {
        addSound((uint)id, filenameDot, category, userFacingName, categoriesDot);
      }
    }
    
    private void addSound(uint id, string filename, trBehaviorType category, string userFacingName, Dictionary<trBehaviorType, List<trRobotSound>> categoriesDict
      ) {
      init();
      if (!categoriesDict.ContainsKey(category)) {
        categoriesDict[category] = new List<trRobotSound>();
      }
      
      trRobotSound sound = new trRobotSound(id, filename, category, userFacingName);

      categoriesDict[category].Add(sound);
    }
    
    private Dictionary<trBehaviorType, List<trRobotSound>> categoryDictForRobotType(piRobotType robotType) {
      init ();
      Dictionary<trBehaviorType, List<trRobotSound>> ret = null;
      
      if (robotType == piRobotType.DASH) {
        ret = categoriesDash;
      }
      else if (robotType == piRobotType.DOT) {
        ret = categoriesDot;
      }
      else {
        WWLog.logError("unhandled robot type: " + robotType.ToString());
      }
      
      return ret;
    }
    
    public List<trBehaviorType> GetCategories() {
      init ();
      List<trBehaviorType> ret = new List<trBehaviorType>();
      foreach (trBehaviorType trBT in categoryNames.Keys) {
        ret.Add (trBT);
      }
      return ret;
    }
    
    public List<trBehaviorType> getUserFacingCategories(piRobotType robotType) {
      init ();
      List<trBehaviorType> ret = new List<trBehaviorType>();
      Dictionary<trBehaviorType, List<trRobotSound>> dict = categoryDictForRobotType(robotType);
      if (dict != null) {
        foreach (trBehaviorType key in dict.Keys) {
          if (key != HIDDEN_SOUNDS_CATEGORY) {
            ret.Add(key);
          }
        }
      }
      
      return ret;
    }
    
    public List<trRobotSound> GetCategory(trBehaviorType category, piRobotType robotType) {
      Dictionary<trBehaviorType, List<trRobotSound>> dict = categoryDictForRobotType(robotType);
      if (dict == null) {
        return null;
      }
      
      if (!dict.ContainsKey(category)) {
        WWLog.logError("unknown category for robot type. cat: " + category.ToString() + " robotType: " + robotType.ToString());
        return null;
      }
      
      List<trRobotSound> soundsForCategory = new List<trRobotSound>();
      foreach(trRobotSound sound in dict[category]){
        if (trRewardsManager.Instance.IsAvailableRobotSound(sound.id)){
          soundsForCategory.Add(sound);
        }
      }
      return soundsForCategory;
    }
    
    #endregion list management
  }
  
  
  public class trRobotSound {
    public  const uint           INVALID_ID      = 0;
    public  uint                 id              = INVALID_ID;
    public  string               filename        = ""; // filename on robot
    public  trBehaviorType       category        = trBehaviorType.SOUND_INTERNAL;
    private string               userFacingName  = "";

    public trRobotSound(uint id, string filename, trBehaviorType category, string userFacingName) {
      this.id             = id;
      this.filename       = filename;
      this.category       = category;
      this.userFacingName = userFacingName;
    }


    public string UserFacingNameLocalized {
      get {
        return wwLoca.Format(userFacingName);
      }
    }

    public string UserFacingNameUnlocalized {
      get {
        return userFacingName;
      }
    }
  }
}
