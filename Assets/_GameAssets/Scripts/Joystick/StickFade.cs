using UnityEngine;
using System.Collections;
using Holoville.HOTween;
public class StickFade : MonoBehaviour 
{
	public float startFadeTime = 5f;
	public float fadeTime = 10f;
	
	private GUITexture _texture;
	private Color _finalColor = new Color(1f,1f,1f,0.1f);
	void Start () 
	{
		_texture = GetComponent<GUITexture>();
		Invoke ("FadeOut", startFadeTime);
	}
	
	void FadeOut()
	{
		HOTween.To( _texture,fadeTime,new TweenParms().Prop("color",_finalColor));
	}
	
}
