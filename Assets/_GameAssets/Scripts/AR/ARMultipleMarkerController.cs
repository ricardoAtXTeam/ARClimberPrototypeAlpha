using UnityEngine;
using System.Collections.Generic;

public class ARMultipleMarkerController  : MonoBehaviour
{
	public Camera arCamera;
	public GameObject visibilityLabel;

	public GameObject resetButton;
	public GameObject incRotButton;
	public GameObject decRotGameObject;
	public UILabel currRotLabel;
	public float initialRotation = 0f;

	public GameObject resetHeightButton;
	public GameObject incHeightButton;
	public GameObject decHeightButton;
	public UILabel currScaleLabel;

	
	public bool hideOnNoMarker = false;
	public ARController[] markers;
	public bool automaticFecth = false;
	public GameObject[] gameObjects;
	private Vector4 _data = new Vector4(0f,0f,0f,0f);
	private Vector3 _center = new Vector3(0f,0f,0f);
	private int _markersVisibleCount = 0;
	private bool _destroyed = false;
	private float _markerRotationInc = 10f;
	private float _currentAngle = 0f;
	private float _originalScale = -1;
	private float _currentScale = -1;
	private float _scaleInc = 1f;
	private Vector3 _scaleVec = new Vector3(0f,0f,0f);
	private string _noRotString = "0";
	private EventDispatcher _onTrackersStatusModified = new EventDispatcher();
	private EventDispatcher _onMaxTrackersDetected = new EventDispatcher();
	private int _maxTrackersDetected = 0;
	private Quaternion _originalMarkersRot;
	private Vector3 _currentMarkersPosition = new Vector3(0f,110f,0f);
	private Vector3 _originalMarkersPos;

	void Awake()
	{
		_originalMarkersPos = _currentMarkersPosition;

		_destroyed = false;
		_originalMarkersRot = Quaternion.AngleAxis(initialRotation,Vector3.right);
		for( int i = 0 ; i < markers.Length ; i++ )
		{
			markers[i].OnTrackerStatusModified.AddListener += OnTrackerStatusChanged;
			markers[i].CachedTransform.rotation = _originalMarkersRot;
			markers[i].CachedTransform.localPosition = _currentMarkersPosition;

			if(_originalScale < -1 && markers[i].gameObject.activeSelf)
			{
				_originalScale = markers[i].transform.localScale.x;
				_currentScale = _originalScale;
			}
		}

		currScaleLabel.text = _originalScale.ToString();

		currRotLabel.text = initialRotation.ToString();

		UIEventListener.Get( resetButton).onClick += (go) =>
		{
			ResetMarkersRotation();
		};

		UIEventListener.Get( incRotButton ).onClick += (go) =>
		{
			IncrementMarkerRotation( _markerRotationInc );
		};

		UIEventListener.Get( decRotGameObject ).onClick += (go) =>
		{
			IncrementMarkerRotation( -_markerRotationInc );
		};

		UIEventListener.Get( resetHeightButton ).onClick += (go) =>
		{
			ResetCamScale();
		};
		
		UIEventListener.Get( incHeightButton ).onClick += (go) =>
		{
			IncrementScale( _scaleInc );
		};
		
		UIEventListener.Get( decHeightButton ).onClick += (go) =>
		{
			IncrementScale( -_scaleInc );
		};


		if(automaticFecth)
			InvokeRepeating( "AnalyseMarkers",1f,1f );
	}


	public Vector4 AnalyseMarkers()
	{
		float minX = float.MaxValue;
		float minZ = float.MaxValue;

		float maxX = float.MinValue;
		float maxZ = float.MinValue;
		_markersVisibleCount = 0;
		for( int i = 0 ; i < markers.Length ; i++)
		{
			if(markers[i].MarkerVisible)
			{
				_markersVisibleCount += 1;
				minX = Mathf.Min( minX,markers[i].CachedTransform.position.x);
				minZ = Mathf.Min( minZ,markers[i].CachedTransform.position.z);

				maxX = Mathf.Max( maxX,markers[i].CachedTransform.position.x);
				maxZ = Mathf.Max( maxZ,markers[i].CachedTransform.position.z);
			}
		}

		_data.x = minX;
		_data.y = minZ;
		_data.z = maxX;
		_data.w = maxZ;

		if(_markersVisibleCount == 1 )
		{
			_center.x = minX;
			_center.z = maxZ;
		}
		else
		{
			_center.x = minX + (maxX -minX)*0.5f;
			_center.z = minZ + (maxZ - minZ)*0.5f;
		}

	
		return _data;
	}

	public void SetMarkersLocalPosition( Vector3 position )
	{
		Debug.Log(position);
		_currentMarkersPosition = position;
		for( int i = 0 ; i < markers.Length ; i++)
		{
			markers[i].CachedTransform.localPosition = position;
		}
	}

	public void ResetMarkersTransform()
	{
		for( int i = 0 ; i < markers.Length ; i++)
		{
			markers[i].CachedTransform.localPosition = _originalMarkersPos;
			markers[i].CachedTransform.rotation = _originalMarkersRot;
		}
		_currentAngle = 0f;
		currRotLabel.text = _noRotString;
	}
	
	public void ResetMarkersRotation()
	{
		for( int i = 0 ; i < markers.Length ; i++)
		{
			markers[i].CachedTransform.rotation = Quaternion.AngleAxis(initialRotation,Vector3.right);
		}
		_currentAngle = initialRotation;
		currRotLabel.text = initialRotation.ToString();
	}
	
	private void IncrementMarkerRotation( float value )
	{
		_currentAngle += value;
		currRotLabel.text = _currentAngle.ToString();
		for( int i = 0 ; i < markers.Length ; i++)
		{
			markers[i].CachedTransform.rotation = Quaternion.AngleAxis(_currentAngle,Vector3.right);
		}
	}

	public void ResetCamScale()
	{
		_currentScale = _originalScale;
		_scaleVec.x = _scaleVec.y = _scaleVec.z = _currentScale;

		for( int i = 0 ; i < markers.Length ; i++)
		{
			markers[i].CachedTransform.localScale = _scaleVec;
		}
		currScaleLabel.text = _originalScale.ToString();
	}

	private void IncrementScale( float incScale )
	{
		_currentScale += incScale;
		_scaleVec.x = _scaleVec.y = _scaleVec.z = _currentScale;
		for( int i = 0 ; i < markers.Length ; i++)
		{
			markers[i].CachedTransform.localScale = _scaleVec;
		}
		currScaleLabel.text = _currentScale.ToString();
	}

#region Event Listeners
	private void OnTrackerStatusChanged( params object[] vars )
	{
		if(_destroyed)
			return;

		int visibleCount = 0;
		int i;
		for( i = 0 ; i < markers.Length ; i++ )
		{
			if( markers[i].MarkerVisible)
				visibleCount += 1;
		}

		bool objectsVisible = (visibleCount > 0)?true:false;
		visibilityLabel.SetActive(!objectsVisible);
		if(hideOnNoMarker)
		{
			for(  i = 0 ; i < gameObjects.Length ; i++)
			{
				gameObjects[i].SetActive( objectsVisible);
			}
		}

		if(visibleCount > _maxTrackersDetected)
		{
			_maxTrackersDetected = visibleCount;
			_onMaxTrackersDetected.Dispatch(true);
		}
		else
		{
			_onMaxTrackersDetected.Dispatch(false);
		}

		_onTrackersStatusModified.Dispatch( objectsVisible );
	}

	private void OnDestroy()
	{
		_destroyed = true;
	}
	public int RefreshMarkersVisibleCount()
	{
		_markersVisibleCount =0;
		for( int i = 0 ; i < markers.Length ; i++)
		{
			if(markers[i].MarkerVisible)
			{
				_markersVisibleCount += 1;
			}
		}
		return _markersVisibleCount;
	}
#endregion


#region Properties
	public Vector3 MarkersCenter
	{
		get
		{
			return _center;
		}
	}

	public float MarkersWidth
	{
		get
		{
			return ((_markersVisibleCount > 0 ) ? Mathf.Abs(_data.x - _data.z) : 0);
		}
	}

	public float MarkersHeight
	{
		get
		{
			return ((_markersVisibleCount > 0 ) ? Mathf.Abs(_data.y - _data.w) : 0);
		}
	}

	public int MarkersVisibleCount
	{
		get
		{
			return _markersVisibleCount;
		}
	}

	public EventDispatcher OnTrackersStatusModified
	{
		get
		{
			return _onTrackersStatusModified;
		}
	}

	public EventDispatcher OnMaxTrackersDetected
	{
		get
		{
			return _onMaxTrackersDetected;
		}
	}
	public int MaxTrackersDetected
	{
		get
		{
			return _maxTrackersDetected;
		}
	}

	public Vector3 CurrentMarkersPosition
	{
		get
		{
			return _currentMarkersPosition;
		}
	}

	public Vector3 OriginalMarkersPos
	{
		set
		{
			_originalMarkersPos = value;
		}
	}

#endregion
}