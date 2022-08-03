using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Phantom : MonoBehaviour {
	
	public class PhantomMenu
	{
		//
		[MenuItem("Oyedoyin/Rotary Wing/Download")]
		private static void AddMotorEngine()
		{
			Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/silantro-helicopter-simulator-toolkit-142612");
		}
		//
	}
}
