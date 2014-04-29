using UnityEngine;
using System.Collections;
using Holoville.HOTween;

[System.Serializable]
public class IntroKeyframe
{
	public Transform keyframe;
	public float duration;
	public float waitTime;

	public IntroKeyframe( Transform initKeyFrame , float initDuration , float initWaitTime )
	{
		keyframe = initKeyFrame;
		duration = initDuration;
		waitTime = initWaitTime;
	}
}

public class IntroSequence : MonoBehaviour 
{
	public UISprite foreground;
	public Camera arCamera;
	public Camera introCamera;
	public float lastKeyFrameTime = 2f;

	public IntroKeyframe[] keyframes;

	private Transform _introCamTF;
	private Transform _arCamTF;

	private EventDispatcher _introSequenceFinished = new EventDispatcher();

	private bool _fired;
	private bool _running;

	void Awake()
	{
		_introCamTF = introCamera.transform;

		_arCamTF = arCamera.transform;

		_fired = false;
		_running = false;
	}

	public void ActivateIntro()
	{
		if(_fired)
			return;

		_fired = true;
		_running = true;
		Invoke("DelayedActivate",0.5f);

	}

	void DelayedActivate()
	{
		DoFade();
		Invoke( "DelayedStart",0.5f);
	}

	void DoFade()
	{
		foreground.alpha = 0f;
		HOTween.To( foreground,0.5f,new TweenParms().Prop("alpha",1f).Loops(2,LoopType.Yoyo).UpdateType(UpdateType.TimeScaleIndependentUpdate));
	}

	void DelayedStart()
	{
		CopyCamParams();
		introCamera.gameObject.SetActive( true );

		float time = 0;

		Sequence seq = new Sequence( new SequenceParms().OnComplete( ActivateFinalAnim ).UpdateType(UpdateType.TimeScaleIndependentUpdate));

		for( int i = 0 ; i < keyframes.Length ; i++ )
		{
			seq.Insert( time , HOTween.To(_introCamTF,keyframes[i].duration,new TweenParms().Prop("rotation",keyframes[i].keyframe.rotation).UpdateType(UpdateType.TimeScaleIndependentUpdate)));
			seq.Insert( time , HOTween.To(_introCamTF,keyframes[i].duration,new TweenParms().Prop("position",keyframes[i].keyframe.position).UpdateType(UpdateType.TimeScaleIndependentUpdate)));
			time += ( keyframes[i].duration + keyframes[i].waitTime );
		}

		seq.Play();
	}

	void ActivateFinalAnim()
	{
		Debug.Log("ActivateFinalAnim");
		DoFade();
		Invoke( "IntroSequenceFinished",0.5f);
	}

	void IntroSequenceFinished()
	{
		Debug.Log("IntroSequenceFinished");
		_running = false;
		_introSequenceFinished.Dispatch();
		introCamera.gameObject.SetActive( false );
	}

	void CopyCamParams()
	{
		introCamera.nearClipPlane = arCamera.nearClipPlane;
		introCamera.farClipPlane = arCamera.farClipPlane;
		introCamera.fieldOfView = arCamera.fieldOfView;
		_introCamTF.position = _arCamTF.position;
		_introCamTF.rotation = _arCamTF.rotation;
	}

	public bool Fired
	{
		get
		{
			return _fired;
		}
	}

	public bool Running
	{
		get
		{
			return _running;
		}
	}
	public EventDispatcher IntroSequenceFinishedEvent
	{
		get
		{
			return _introSequenceFinished;
		}
	}
}