/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/




using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine.Rendering;


namespace monoflow { 

    public class EnableOpenGLES2 : MonoBehaviour {

#if (UNITY_IOS || UNITY_ANDROID ) && UNITY_EDITOR 
      [PostProcessSceneAttribute(1)]
      public static void OnPostprocessScene() {
          
           // Debug.Log("OnPostprocessScene");
            if (Application.isPlaying){return;}

            BuildTarget BT = BuildTarget.Android;
    #if UNITY_IOS
            BT = BuildTarget.iOS;
    #elif UNITY_ANDROID
            BT = BuildTarget.Android;
    #endif

            if (PlayerSettings.GetUseDefaultGraphicsAPIs(BT))
            {
                Debug.LogWarning("<color='red'>MPMP</color>: Auto Graphics API is selected in your publish settings! Switching to GraphicsDeviceType.OpenGLES2 explicit.");
                SwitchToOpenGLES2(BT);
            }
            else if(PlayerSettings.GetGraphicsAPIs(BT)[0] != GraphicsDeviceType.OpenGLES2)
            {
                Debug.LogWarning("<color='red'>MPMP</color>: Switching to GraphicsDeviceType.OpenGLES2 explicit.");
                SwitchToOpenGLES2(BT);
            }

        }
#endif
/*
	#if UNITY_EDITOR_OSX && UNITY_STANDALONE_OSX
		[PostProcessSceneAttribute(2)]
		public static void OnPostprocessSceneOSX() {
			BuildTarget BT = EditorUserBuildSettings.activeBuildTarget;
			if(BT != BuildTarget.StandaloneOSXIntel64)
			{
				Debug.LogWarning("<color='red'>MPMP</color>:MPMP runs only as x86_64 on OSX. Switching to BuildTarget.StandaloneOSXIntel64 explicit.<color='orange'><b>Please built again!</b></color>");
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel64);
			}
		}
	#endif
*/
        public static void SwitchToOpenGLES2(BuildTarget BT) {

            PlayerSettings.SetUseDefaultGraphicsAPIs(BT, false);

            GraphicsDeviceType[] apis = { GraphicsDeviceType.OpenGLES2 };
      
            PlayerSettings.SetGraphicsAPIs(BT, apis);
        }

    }

}


