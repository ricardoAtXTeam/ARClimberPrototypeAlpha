using UnityEngine;

public class DebrisController : MonoBehaviour
{
	public int id;
	protected Transform _transform;
	protected EventDispatcher _returnToPoolEvent = new EventDispatcher();
	protected float _activeTime = -1;


	public virtual void SetActive( bool value , float activeTime = -1)
	{
		gameObject.SetActive( value );
		_activeTime = activeTime;
	}

	void Update()
	{
		if( _activeTime > 0)
		{
			_activeTime -= Time.deltaTime;
			if(_activeTime < 0 )
			{
				ReturnToPool();
			}
		}
	}

	virtual protected void ReturnToPool()
	{
		_returnToPoolEvent.Dispatch(this);
	}
	public Transform CachedTransform
	{
		get
		{
			if(_transform == null)
				_transform = transform;

			return _transform;
		}
	}

	public EventDispatcher ReturnToPoolEvent
	{
		get
		{
			return _returnToPoolEvent;
		}
	}
}