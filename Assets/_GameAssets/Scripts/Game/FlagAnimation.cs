using UnityEngine;
using System.Collections;

public class FlagAnimation : MonoBehaviour 
{
	private Animation _animation;
	void Awake()
	{
		_animation = animation;
		_animation.clip.wrapMode = WrapMode.Loop;
	}
	void Update()
	{
		if(!_animation.isPlaying)
		{
			_animation.Play();
		}
	}
}
