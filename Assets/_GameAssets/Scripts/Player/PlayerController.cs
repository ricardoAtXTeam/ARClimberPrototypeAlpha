using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	public Transform characterTransform;
	public Animator playerAnimator;

	public Transform bottomRaycastHelper;
	public Transform middleRaycastHelper;
	public Transform topRaycastHelper;

	public Transform[] climbRaycastHelpers;

	public float raycastCharWidth = 1f;


	public TriggerListener climbAreaListener;
	public bool relativeToCam = true;
	public Camera mainCamera;
	public float movementSpeed = 5;
	public float climbSpeed = 5;

	public Joystick joystick;

	public TriggerListener collListener;
	public Transform debugMesh;
	public float angleAdjustment = 90f;
	public Vector3 characterClimbAdjust = new Vector3( 0f,0f,-1.88f);

	public bool adjustSpeedToTerrain = false;
	private float _raycastHelperLenght = 2f;

	private Vector3 _originalCharLocalPos;
	private float _forwardRayLength = 1f;
	private float _groundRayLength = 500f;
	private Transform _transform;
	private Vector3 _currentPos = new Vector3(0,0,0);
	private Vector3 _forwardDir = new Vector3(0,0,0);
	private Vector3 _targetPos = new Vector3(0,0,0);
	private Vector3 _raycastYAdjust;
	private Vector3 _raycastZAdjust;
	private float _rampBoost = 1f;
	private float _maxHeightMult = 0.95f;
	private Vector2 _angleHelper = new Vector2(0f,0f);
	private float _currentAngle = 0f;

	private Transform _cameraTransform;
	private Vector3 _debugRayOrigin;
	private Vector3 _transformedUpVector;

	private float _heightSpeedMod = 1f;
	private bool _isClimbing = false;

	private bool _isMoving = false;

	private Vector3 _originalPos;
	private bool _lockMovement = false;
	private Vector3 _lastValidPos;

	void Awake()
	{
		_transform = transform;
		_currentPos = _transform.position;
		_lastValidPos = _currentPos;
		_originalPos = _currentPos;
		_cameraTransform = mainCamera.transform;
		_debugRayOrigin = new Vector3(0f,_transform.localScale.y,0f);
		_raycastYAdjust = new Vector3(0f,200f,0f);
		_raycastZAdjust = new Vector3(0f,0f,-200f);
		_transformedUpVector = -Vector3.up;
		_originalCharLocalPos = characterTransform.localPosition;
		climbAreaListener.OnTriggerStayEvent.AddListener += OnStayClimbArea;
		climbAreaListener.OnTriggerExitEvent.AddListener += OnExitClimbArea;
		collListener.OnTriggerEnterEvent.AddListener += OnCollisionWithDebris;
	}


	public void Reset()
	{
		_isClimbing = false;
		_isMoving = false;
		_currentAngle = 0f;
		_heightSpeedMod = 1f;
		_transform.position = _originalPos;
		_currentPos = _originalPos;
		characterTransform.localPosition = _originalCharLocalPos;
		characterTransform.rotation = Quaternion.identity;
		_forwardDir = Vector3.zero;
		_targetPos = Vector3.zero;
	}

#region Event Listeners

	void OnCollisionWithDebris( params object[] vars )
	{
		if(_isClimbing)
		{
			if( (_currentPos-_lastValidPos).magnitude > 5f)
			{
				_lastValidPos = _currentPos;
			}
			else
			{
				_currentPos =_lastValidPos;
				_transform.position = _currentPos;
			}
		}
	}

	void OnStayClimbArea( params object[] vars)
	{
		if(!_isClimbing)
		{
			_isClimbing = true;
			collListener.TriggerCount = 0;
		}
	}

	void OnExitClimbArea( params object[] vars)
	{
		_isClimbing = false;
	}
#endregion

#region Player Animator
	void UpdateAnimation()
	{
		if (playerAnimator)
		{
			playerAnimator.SetFloat("Speed",_isMoving?1f:0f);
			playerAnimator.SetFloat("Direction",_forwardDir.y);
			playerAnimator.SetBool("Climb",_isClimbing);
			playerAnimator.speed = _isClimbing ? (_isMoving ? 1f:0f):1f;
		} 
	}	
#endregion

	void FetchControllerData()
	{
		_forwardDir = Vector3.zero;
#if UNITY_EDITOR
		if(Input.GetMouseButton(0))
		{
			_forwardDir.x = joystick.position.x;
			_forwardDir.z = joystick.position.y;
		}
		else
		{
			_forwardDir.x = (Input.GetKey(KeyCode.LeftArrow) ? - 1 : (Input.GetKey(KeyCode.RightArrow) ? 1 :0));
			_forwardDir.z = (Input.GetKey(KeyCode.DownArrow) ? - 1 : (Input.GetKey(KeyCode.UpArrow) ? 1 :0));
		}
#else
		_forwardDir.x = joystick.position.x;
		_forwardDir.z = joystick.position.y;
#endif
		if(relativeToCam)
		{
			_forwardDir = _cameraTransform.TransformDirection( _forwardDir );
			_forwardDir.z = -_forwardDir.y;
			_forwardDir.y = 0;
		}
		
		_forwardDir.Normalize();
	}

	bool UpdateClimbController()
	{
		RaycastHit hitOrigin;
		_forwardDir.y = _forwardDir.z;
		_forwardDir.z = 0;
		float minZ = float.MaxValue;
		bool foundMinZ = false;
		
		for( int i = 0 ; i < climbRaycastHelpers.Length ; i++)
		{
			if( Physics.Raycast(_raycastZAdjust + climbRaycastHelpers[i].position,Vector3.forward,out hitOrigin,_groundRayLength,(int)ePhysicsLayers.TERRAIN))
			{
				minZ = Mathf.Min( hitOrigin.point.z, minZ);
				foundMinZ = true;
			}
		}
		if(foundMinZ)
		{
			//Debug.Log(Mathf.Abs(_currentPos.z - minZ));
			if(Mathf.Abs(_currentPos.z - minZ) < 10)
			{
				_currentPos.z = minZ;
			}
		}
			

		return true;

	}
	void FixedUpdate () 
	{

		if(_lockMovement)
		{
			_forwardDir = Vector3.zero;
		}
		else
		{
			FetchControllerData();
		}

		RaycastHit hitOrigin;
		RaycastHit hitTarget;
	
		if(_isClimbing)
		{
			UpdateClimbController();
		}
		else
		{
			if( Physics.Raycast(_raycastYAdjust + _currentPos,-Vector3.up,out hitOrigin,_groundRayLength,(int)ePhysicsLayers.TERRAIN))
			{
				if( Mathf.Abs(_currentPos.y - hitOrigin.point.y) > 30f )
				{
					// Assume climbing
					UpdateClimbController();
				}
				else
				{
					_currentPos.y = hitOrigin.point.y;
				}
			}
		}

		_isMoving = _forwardDir.sqrMagnitude > 0;
		if( _isMoving )
		{
			if(TestCollisions())
				return;

			_targetPos = _currentPos + _forwardDir;
			
			_heightSpeedMod = 1f;
			Debug.DrawRay( _raycastYAdjust + _targetPos,_transformedUpVector*_groundRayLength,Color.red);
			if( Physics.Raycast(_raycastYAdjust + _targetPos,_transformedUpVector,out hitTarget,_groundRayLength,(int)ePhysicsLayers.TERRAIN))
			{
				_angleHelper.x = 1f;
				_angleHelper.y = (hitTarget.point.y - hitOrigin.point.y);
				_angleHelper.Normalize();
				
				_currentAngle = Mathf.Acos( Vector2.Dot( Vector2.right,_angleHelper) ) * ( _angleHelper.y < 0 ? -1 : 1 );
			}
			else
			{
				_currentAngle = _maxHeightMult*Mathf.PI;
			}


			float speed = (_isClimbing)?climbSpeed:movementSpeed;
			if(adjustSpeedToTerrain)
			{
				_heightSpeedMod = Mathf.Clamp( (_rampBoost*1f - _currentAngle*MathHelpers.INV_HALF_PI),0.01f,0.7f);
				speed = speed*_heightSpeedMod;
			}

			_currentPos += _forwardDir*speed*Time.deltaTime;

			if(_isClimbing)
			{
				//float finalAngle = Mathf.Clamp( Mathf.Abs(angleAdjustment - _currentAngle*Mathf.Rad2Deg),0f,9f);
				Quaternion finalAngleRotation = Quaternion.identity;//Quaternion.AngleAxis(finalAngle,Vector3.right);

				debugMesh.rotation = finalAngleRotation;
				characterTransform.rotation = finalAngleRotation;

				characterTransform.localPosition = characterClimbAdjust;
			}
			else
			{ 
				//no rotations on direction containing climb values ( moving on Y )
				_forwardDir.y = 0;
				if(_forwardDir.sqrMagnitude > 0)
				{
					debugMesh.rotation =  Quaternion.LookRotation(_forwardDir);
					characterTransform.rotation =  Quaternion.LookRotation(_forwardDir);
				}

				characterTransform.localPosition = _originalCharLocalPos;
			}
		}
		_transform.position = _currentPos;
		if(!_isClimbing || collListener.TriggerCount == 0)
		{
			_lastValidPos = _currentPos; 
		}

		UpdateAnimation();
	}

	bool TestCollisions()
	{
		RaycastHit hitTarget;

		Debug.DrawRay(topRaycastHelper.position,_forwardDir*_raycastHelperLenght);
		Debug.DrawRay(middleRaycastHelper.position,_forwardDir*_raycastHelperLenght);
		Debug.DrawRay(bottomRaycastHelper.position,_forwardDir*_raycastHelperLenght);

		if(	Physics.Raycast(_currentPos+_debugRayOrigin,_forwardDir,out hitTarget,_forwardRayLength,(int)ePhysicsLayers.WALL) ||
		   Physics.Raycast(topRaycastHelper.position,_forwardDir,out hitTarget,_raycastHelperLenght,(int)ePhysicsLayers.DEBRIS) ||
		   Physics.Raycast(middleRaycastHelper.position,_forwardDir,out hitTarget,_raycastHelperLenght,(int)ePhysicsLayers.DEBRIS) ||
		   Physics.Raycast(bottomRaycastHelper.position,_forwardDir,out hitTarget,_raycastHelperLenght,(int)ePhysicsLayers.DEBRIS)  
		   )
		{
			return true;
		}

		if(_isClimbing && Mathf.Abs(_forwardDir.z) > 0.01f )
		{
			Vector3 origin;
			Vector3 dir;
			if(_forwardDir.z >0)
			{
				origin = topRaycastHelper.position;
				dir = Vector3.up;
			}
			else
			{
				origin = bottomRaycastHelper.position;
				dir = Vector3.down;
			}

			Debug.DrawRay(origin,dir*_raycastHelperLenght);
			Debug.DrawRay(origin + raycastCharWidth*Vector3.left,dir*_raycastHelperLenght);
			Debug.DrawRay(origin + raycastCharWidth*Vector3.right,dir*_raycastHelperLenght);

			if(Physics.Raycast(origin,dir,out hitTarget,_raycastHelperLenght,(int)ePhysicsLayers.DEBRIS)||
			   Physics.Raycast(origin+raycastCharWidth*Vector3.left,dir,out hitTarget,_raycastHelperLenght,(int)ePhysicsLayers.DEBRIS)||
			   Physics.Raycast(origin+raycastCharWidth*Vector3.right,dir,out hitTarget,_raycastHelperLenght,(int)ePhysicsLayers.DEBRIS))
			{
				return true;
			}
		}


		return false;
	}
#region Properties

	public bool LockMovement
	{
		set
		{
			_lockMovement = value;
		}
	}
	public Transform CachedTransform
	{
		get
		{
			return _transform;
		}
	}
#endregion
}