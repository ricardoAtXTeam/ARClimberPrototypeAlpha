using UnityEngine;
using System.Collections;

class LoadingScreen : MonoBehaviour
{
	static string nextScene = string.Empty;
	static string suspendedScene = string.Empty;
	private AsyncOperation _asyncLoad;
	public UILabel tipsLabel = null;
	public string[] tips = null;

	public static void LoadScene(string scene, bool suspended = false)
	{
		if (suspended)
		{
			suspendedScene = scene;
		}
		else
		{
			nextScene = scene;
			Application.LoadLevel("LoadingScreen");
		}
	}
	
	public static void LoadSuspendedScene()
	{
		if (suspendedScene.Length > 0)
		{
			nextScene = suspendedScene;
			suspendedScene = string.Empty;
			Application.LoadLevel("LoadingScreen");
		}
	}
	
	IEnumerator Start()
	{
		tipsLabel.text = tips[ Random.Range(0,tips.Length)];
		Time.timeScale = 1f;
		Resources.UnloadUnusedAssets ();
		System.GC.Collect();
		_asyncLoad = Application.LoadLevelAsync(nextScene);
		yield return _asyncLoad;
	}	
}
