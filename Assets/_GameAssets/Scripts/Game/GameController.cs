using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour 
{
	public bool pauseOnNoMarkers = true;
	public DebrisFactory debrisFactory;
	public Transform flag;
	public Transform terrainTF;
	public BoxCollider climbAreaCollider;
	public BoxCollider rightWorldCollider;
	public BoxCollider frontWorldCollider;
	public BoxCollider backWorldCollider;

	public GameObject buildTerrainButton;
	public GameObject resetCamButton;
	public GameObject homeButton;
	public GameObject resetButton;

	public GameObject goalParent;
	public UILabel goalLabel;

	public UILabel timeElapsedLabel;

	public Transform mainCamera;
	public PlayerController playerController;
	public ARMultipleMarkerController multipleMarkerController;
	public Transform arCameraTF;

	private IntroSequence _introSequence;
	private TimeLabelFactory _timeLabelFactory;
//	private TerrainGenerator _terrainGenerator;

	private Vector3 _arCamOriginalPos;
	private Quaternion _arCamOriginalRot;

	private float _startTime;
	private bool _reverseTime = true;
	private const float MAX_TIME = 30f;
	private float _currentTime = 0f;
	private float _timePenalty = 0f;
	private float _timePenaltyTimer = 0.2f;
	private bool _timePenaltyLocked = false;


	private string _endScreenMethod = "OnDisplayEndScreenFinished";
	private string _updateElapsedTime = "UpdateElapsedTime";

//	private float _minTerrainSizeX = 200;
//	private float _minTerrainSizeY = 64f;
	private string _noTimeString = "00:00";

	void Awake()
	{

		_timeLabelFactory = GetComponent<TimeLabelFactory>();

		_introSequence = GetComponent<IntroSequence>();
		_introSequence.IntroSequenceFinishedEvent.AddListener += OnIntroFinished;

		//_terrainGenerator = terrainTF.GetComponent<TerrainGenerator>();
		flag.GetComponent<TriggerListener>().OnTriggerEnterEvent.AddListener += OnEnterFlagEvent;
		//_terrainGenerator.OnTerrainSetupComplete.AddListener += OnTerrainSetupComplete;
		_arCamOriginalPos = arCameraTF.position;
		_arCamOriginalRot = arCameraTF.rotation;

		//multipleMarkerController.OnMaxTrackersDetected.AddListener += OnMaxTrackersDetected;

		UIEventListener.Get(homeButton).onClick += (go) =>
		{
			Time.timeScale = 1f;
			LoadingScreen.LoadScene("MainMenu");
		};

		UIEventListener.Get(resetButton).onClick += (go) =>
		{
			multipleMarkerController.ResetMarkersTransform();
			ResetARCamera();
			terrainTF.position = Vector3.zero;
			terrainTF.rotation = Quaternion.identity;
			Reset();
		
		};

		multipleMarkerController.OnTrackersStatusModified.AddListener += OnTrackersStatusModified;
		InvokeRepeating( _updateElapsedTime,0f,1f);
		_startTime = Time.timeSinceLevelLoad;
		_currentTime = MAX_TIME;

		debrisFactory.PlayerCollisionEvent.AddListener += OnPlayerCollisionEvent;
	}

	void ResetARCamera()
	{
		arCameraTF.position = _arCamOriginalPos; 
		arCameraTF.rotation = _arCamOriginalRot;
	}

	void TestGameOverConditions()
	{
		if(_currentTime <= 0 )
			playerController.Kill();
	}

	void UpdateElapsedTime( )
	{
		float totalTime;
		if(_reverseTime)
		{
			if( _currentTime > 0 )
			{
				_currentTime -= 1;
				TestGameOverConditions();
			}

			totalTime = _currentTime;
		}
		else
		{
			totalTime = _timePenalty + Time.timeSinceLevelLoad - _startTime ;
		}

		timeElapsedLabel.text = TimeToString(totalTime);
	}

	void AddTimePenalty( float timePenalty )
	{
		if(_timePenaltyLocked)
			return;

		_timePenaltyLocked = true;
		Invoke( "UnlockTimePenalty",_timePenaltyTimer);

		float totalTime;
		if( _reverseTime )
		{
			if( _currentTime > 0)
			{
				_currentTime -= timePenalty;
				TestGameOverConditions();
			}
			totalTime = _currentTime;
		}
		else
		{
			_timePenalty += timePenalty;
			totalTime = _timePenalty + Time.timeSinceLevelLoad - _startTime ;
		}
		timeElapsedLabel.text = TimeToString(totalTime);

		_timeLabelFactory.ActivateLabel( ((int)timePenalty).ToString());
	}

	string TimeToString( float totalTime)
	{

		if(totalTime < 0.01f)
		{
			return _noTimeString;
		}

		if(totalTime >= 60f)
		{
			int totalMinutes = (int)(totalTime/60f);
			int totalSeconds = (int)(totalTime - totalMinutes*60f);

			string minutesStr = (totalMinutes > 9)?(totalMinutes.ToString()):("0"+totalMinutes.ToString());
			string secondsStr = ( totalSeconds > 9)?(totalSeconds.ToString()):("0"+ totalSeconds.ToString());
			return minutesStr+":"+secondsStr;
		}
		int intTotalTime = (int)totalTime;
		return ( intTotalTime > 9 ) ? ("00:"+((int)totalTime).ToString()):("00:0"+((int)totalTime).ToString());
	}

//	void UpdateColliders()
//	{
//		// CLIMB AREA
//		Vector3 climbAreacollSize = climbAreaCollider.size;
//		Vector3 climbAreacollCenter = climbAreaCollider.center;
//		float newSizeX = _terrainGenerator.TerrainCurrentSize.x;
//		
//		climbAreacollSize.x = newSizeX;
//		climbAreacollCenter.x = 0.5f*newSizeX;
//		
//		climbAreaCollider.size = climbAreacollSize;
//		climbAreaCollider.center = climbAreacollCenter;
//		
//		
//		// RIGHT COLLIDER
//		Vector3 rightWorldColliderCenter = rightWorldCollider.center;
//		rightWorldColliderCenter.x = newSizeX;
//		rightWorldCollider.center =rightWorldColliderCenter;
//		
//		//FRONT COLLIDER
//		Vector3 frontWorldColliderSize = frontWorldCollider.size ;
//		Vector3 frontWorldColliderCenter = frontWorldCollider.center;
//		frontWorldColliderSize.x = newSizeX;
//		frontWorldColliderCenter.x = 0.5f*newSizeX;
//		frontWorldCollider.size = frontWorldColliderSize;
//		frontWorldCollider.center = frontWorldColliderCenter;
//		
//		//BACK COLLIDER
//		Vector3 backWorldColliderSize = backWorldCollider.size ;
//		Vector3 backWorldColliderCenter = backWorldCollider.center;
//		backWorldColliderSize.x = newSizeX;
//		backWorldColliderCenter.x = 0.5f*newSizeX;
//		backWorldCollider.size = backWorldColliderSize;
//		backWorldCollider.center = backWorldColliderCenter;
//	}

	void Reset()
	{
		_startTime = Time.timeSinceLevelLoad;
		_currentTime = MAX_TIME;
		CancelInvoke(_updateElapsedTime);
		InvokeRepeating( _updateElapsedTime,0f,1f);
		playerController.Reset();
		_timePenaltyLocked = false;
		_timePenalty = 0f;
	}

	void ActivateIntro()
	{
		CancelInvoke(_updateElapsedTime);
		playerController.LockMovement = true;
		Time.timeScale = 1f;
		_introSequence.ActivateIntro();
	}
	
#region Event Listeners

	void UnlockTimePenalty()
	{
		_timePenaltyLocked = false;
	}

	void OnPlayerCollisionEvent(params object[] vars)
	{
		AddTimePenalty(1f);
	}

	void OnIntroFinished(params object[] vars)
	{
		Time.timeScale = (multipleMarkerController.RefreshMarkersVisibleCount() > 0)?1f:0f;
		playerController.LockMovement = false;
		Reset();
	}

//	void OnMaxTrackersDetected( params object[] vars )
//	{
//		if(_introSequence.Running)
//			return;
//
//		if(_terrainGenerator.disableProceduralTerrain)
//			return;
//
//		Vector4 data = multipleMarkerController.AnalyseMarkers();
//
//		bool updateTerrain = (bool)vars[0];
//		if(!updateTerrain)
//		{
//			float terrainNewWidth = (data.z - data.x);
//			Debug.Log(_terrainGenerator.TerrainCurrentSize.x+" "+terrainNewWidth);
//			if(_terrainGenerator.TerrainCurrentSize.x >= terrainNewWidth)
//			{
//				Debug.Log("Not building terrain");
//				return;
//			}
//			Debug.Log("Building terrain");
//		}
//
//		if(_terrainGenerator.UpdateTerrainSize(_minTerrainSizeX + multipleMarkerController.MarkersWidth,_minTerrainSizeY,false))
//		{
//			UpdateColliders();
//			
//			flag.localPosition = _terrainGenerator.LocalHighestPoint;
//			UpdateDebrisOnTerrain();
//		}
//	}

	void OnTrackersStatusModified( params object[] vars)
	{
		if(_introSequence.Running)
			return;

		if(!_introSequence.Fired && multipleMarkerController.RefreshMarkersVisibleCount() > 0 )
		{
			ActivateIntro();
			return;
		}
		if(pauseOnNoMarkers)
			Time.timeScale = ((bool)vars[0])?1f:0f;

		//ResetARCamera();
		multipleMarkerController.ResetMarkersTransform();
		//multipleMarkerController.AnalyseMarkers();
		//terrainTF.position = multipleMarkerController.MarkersCenter;
	}

	void OnEnterFlagEvent( params object[] vars )
	{
		CancelInvoke(_updateElapsedTime);
		goalParent.SetActive( true );

		goalLabel.text = TimeToString(_reverseTime?_currentTime:(Time.timeSinceLevelLoad -_startTime));
		CancelInvoke(_endScreenMethod);
		Invoke(_endScreenMethod,2f);
	}

	void OnDisplayEndScreenFinished()
	{
		goalParent.SetActive( false );
		Reset();
	}

//	void OnTerrainSetupComplete( params object[] vars )
//	{
//		flag.localPosition = _terrainGenerator.LocalHighestPoint;
//		Vector3 camPos = mainCamera.position;
//		camPos.y = flag.localPosition.y + 10;
//		UpdateDebrisOnTerrain();
//		UpdateColliders();
//
//		Vector3 terrainSize = _terrainGenerator.preMadeTerrain.terrainData.size;
//		Debug.Log("TERRAIN SETUP COMPLETE "+terrainSize);
//		playerController.CachedTransform.position = new Vector3( terrainSize.x*0.5f,terrainSize.y*0.5f,-terrainSize.z);
//	}
//
//	void UpdateDebrisOnTerrain()
//	{
//		bool activeTerrain = terrainTF.gameObject.activeSelf;
//		terrainTF.gameObject.SetActive( true );
//		int nbTrackers = Mathf.Max(1,multipleMarkerController.MaxTrackersDetected );
//		debrisFactory.AddDebrisOnTerrain(terrainTF.position,0.9f*_terrainGenerator.TerrainCurrentSize.x,0.6f*_terrainGenerator.TerrainCurrentSize.z,400,nbTrackers*15);
//		terrainTF.gameObject.SetActive( activeTerrain );
//	}

#endregion
}