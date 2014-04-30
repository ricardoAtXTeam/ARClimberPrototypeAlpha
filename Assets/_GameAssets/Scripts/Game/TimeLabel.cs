using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class TimeLabel : MonoBehaviour 
{
	private UILabel _label;
	private EventDispatcher _returnToPoolEvent = new EventDispatcher();
	private TweenParms _tweenAlphaParms;
	private TweenParms _tweenPositionParms;
	private Transform _transform;
	private Vector3 _origin;

	public void Init(Vector3 origin, Vector3 target)
	{
		_transform = transform;
		_origin = origin;
		_transform.position = origin;

		_tweenAlphaParms = new TweenParms().Prop("alpha",0f).OnComplete(ReturnToPoolEvent);
		_tweenPositionParms = new TweenParms().Prop("position",target);
		_label = GetComponent<UILabel>();
		gameObject.SetActive( false );
	}

	public void Activate( string txt , float ttl )
	{
		gameObject.SetActive( true );
		_transform.position = _origin;
		_label.alpha = 1f;
		_label.text = txt;
		gameObject.SetActive( true );

		HOTween.To(_label,ttl,_tweenAlphaParms);
		HOTween.To(_transform,ttl,_tweenPositionParms);
	}

	public void Activate( float time , float ttl )
	{
		Activate( time.ToString(),ttl);
	}

	void ReturnToPoolEvent()
	{
		gameObject.SetActive( false );
		_returnToPoolEvent.Dispatch( this );
	}
	public EventDispatcher OnReturnToPool
	{
		get
		{
			return _returnToPoolEvent;
		}
	}
}
