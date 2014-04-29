using UnityEngine;

public class TriggerListener : MonoBehaviour
{
	private EventDispatcher _onTriggerEnterEvent = new EventDispatcher();
	private EventDispatcher _onTriggerStayEvent = new EventDispatcher();
	private EventDispatcher _onTriggerExitEvent = new EventDispatcher();
	private int _triggerCount = 0;

	void OnTriggerEnter( Collider other )
	{
		_triggerCount += 1;
		_onTriggerEnterEvent.Dispatch();
	}

	void OnTriggerStay( Collider other )
	{
		_onTriggerStayEvent.Dispatch();
	}

	void OnTriggerExit( Collider other )
	{
		_triggerCount -= 1;
		_onTriggerExitEvent.Dispatch();
	}

	public EventDispatcher OnTriggerEnterEvent
	{
		get
		{
			return _onTriggerEnterEvent;
		}
	}

	public EventDispatcher OnTriggerStayEvent
	{
		get
		{
			return _onTriggerStayEvent;
		}
	}

	public EventDispatcher OnTriggerExitEvent
	{
		get
		{
			return _onTriggerExitEvent;
		}
	}

	public int TriggerCount
	{
		get
		{
			return _triggerCount;
		}
		set
		{
			_triggerCount = 0;
		}
	}
}