using UnityEngine;


public class ARController : MonoBehaviour,ITrackableEventHandler
{
	private int imageId;
	public GameObject[] gameObjects;	
	private TrackableBehaviour _trackableBehaviour;
	private Transform _transform;
	private EventDispatcher _onTrackerStatusModified = new EventDispatcher();
	private bool _markerVisible = false;

	void Awake()
	{
		_transform = transform;
		_trackableBehaviour = GetComponent<TrackableBehaviour>();
		if (_trackableBehaviour)
		{
			_trackableBehaviour.RegisterTrackableEventHandler(this);
		}
	}

	public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus,TrackableBehaviour.Status newStatus)
	{
		if (newStatus == TrackableBehaviour.Status.DETECTED ||
		    newStatus == TrackableBehaviour.Status.TRACKED ||
		    newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED
		    )
		{
			OnTrackingFound();
		}
		else if( newStatus == TrackableBehaviour.Status.NOT_FOUND ||
		        newStatus == TrackableBehaviour.Status.UNDEFINED ||
		        newStatus == TrackableBehaviour.Status.UNKNOWN)
		{
			OnTrackingLost();
		}
	}

	private void OnTrackingFound()
	{
		if( gameObjects != null )
		{
			for( int i = 0 ; i < gameObjects.Length ;i++)
			{
				gameObjects[i].SetActive( true );
			}
		}

		_markerVisible = true;
		_onTrackerStatusModified.Dispatch( true , this );
		Debug.Log("Trackable " + _trackableBehaviour.TrackableName + " found");
	}
	
	
	private void OnTrackingLost()
	{
		if( gameObjects != null )
		{
			for( int i = 0 ; i < gameObjects.Length ;i++)
			{
				gameObjects[i].SetActive( false );
			}
		}
		_markerVisible = false;
		_onTrackerStatusModified.Dispatch( false , this );
		Debug.Log("Trackable " + _trackableBehaviour.TrackableName + " lost");
	}

	public EventDispatcher OnTrackerStatusModified
	{
		get
		{
			return _onTrackerStatusModified;
		}
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

	public bool MarkerVisible
	{
		get
		{
			return _markerVisible;
		}
	}
}
