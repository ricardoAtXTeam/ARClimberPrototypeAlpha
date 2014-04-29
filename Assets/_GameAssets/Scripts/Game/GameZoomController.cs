using UnityEngine;
using System.Collections;

public class GameZoomController : MonoBehaviour 
{
	public ARMultipleMarkerController trackerController;

	public Vector3 zoomDirection = new Vector3(0f,0f,1f);
	public float zoomSpeed = 1f;
	public ARMultipleMarkerController markersController;
	public Vector3 minZoom = new Vector3(0f,0f,-10f);
	public Vector3 maxZoom = new Vector3(0f,0f,110f);
	private float _lastTouchDist = -1;
	private float _touchDistThreshold = 1f;
	private float _touchDistAdjuster = 0.01f;
	private Vector3 _currentPosition = new Vector3(0f,0f,0f);

	private void Awake()
	{
		_currentPosition = markersController.CurrentMarkersPosition;
		minZoom.x = maxZoom.x = _currentPosition.x;
		minZoom.y = maxZoom.y = _currentPosition.y;
		trackerController.OriginalMarkersPos = minZoom;
		trackerController.SetMarkersLocalPosition( minZoom );
	}

	void Update () 
	{
		bool updatePos = false;

		if(Input.touchCount == 2)
		{
			float sqrDist = (Input.touches[0].position - Input.touches[1].position).sqrMagnitude;
			if(_lastTouchDist > 0 )
			{
				float touchAux = sqrDist - _lastTouchDist;

				if( Mathf.Abs(touchAux ) > _touchDistThreshold)
				{
					_currentPosition = (_currentPosition + zoomDirection*touchAux*_touchDistAdjuster*Time.deltaTime );
					updatePos = true;
				}
			}
			_lastTouchDist = sqrDist;
		}
		else
		{
			_lastTouchDist = -1;
		}

#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.P))
		{
			_currentPosition = (_currentPosition + zoomDirection*zoomSpeed*Time.deltaTime );
		}

		if(Input.GetKeyDown(KeyCode.O))
		{
			_currentPosition = (_currentPosition - zoomDirection*zoomSpeed*Time.deltaTime );
		}
#endif

		if(updatePos)
		{
			_currentPosition.x = Mathf.Clamp (_currentPosition.x,minZoom.x,maxZoom.x);
			_currentPosition.y = Mathf.Clamp (_currentPosition.y,minZoom.y,maxZoom.y);
			_currentPosition.z = Mathf.Clamp (_currentPosition.z,minZoom.z,maxZoom.z);
			trackerController.SetMarkersLocalPosition( _currentPosition );
		}

	}
}