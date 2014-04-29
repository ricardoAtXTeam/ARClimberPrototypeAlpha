using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	public GameObject textLabel;

	void Awake()
	{
		InvokeRepeating("ToggleLabel",0f,1f);
	}

	void ToggleLabel()
	{
		textLabel.SetActive(!textLabel.activeSelf);
	}

	void Update( )
	{
		if(Input.GetMouseButtonDown(0))
		{
			LoadingScreen.LoadScene("MultiTargetTerrain",false);
		}
	}
}