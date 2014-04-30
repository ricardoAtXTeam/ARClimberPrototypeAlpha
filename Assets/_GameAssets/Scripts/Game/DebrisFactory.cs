using UnityEngine;

using System.Collections.Generic;

public class DebrisFactory : MonoBehaviour 
{
	public Transform[] debrisTypes;
	public Transform activeDebrisParent;
	public Transform[] rubbleTypes;
	public BoxCollider rubbleSpawnArea;
	public Vector2 rubbleTTL = Vector2.one;

	public Vector2 rubbleSpawnRate = Vector2.one;
	public TerrainGenerator terrainGenerator;
	private const int POOL_SIZE = 20;

	//debris
	private Stack<DebrisController>[] _debrisPools;
	private BoxCollider _debrisSpawnArea;
	private List<DebrisController> _activeDebris;

	//rubble
	private Stack<DebrisController>[] _rubblePools;

	private List<DebrisController> _activeRubble;
	private Vector3 _rubbleRayOrigin = new Vector3(0f,0f,-200f);
	private float _rubbleRayLenght = 700f;


	private float _rayLenght = 700f;
	private Transform _transform;
	private EventDispatcher _onPlayerCollisionEvent = new EventDispatcher();

	void Awake () 
	{
		_transform = transform;

		//debris
		_debrisPools = new Stack<DebrisController>[debrisTypes.Length];
		_activeDebris = new List<DebrisController>();

		for( int j = 0 ; j < debrisTypes.Length ;j++)
		{
			_debrisPools[j] = new Stack<DebrisController>();
			for( int i = 0 ; i < POOL_SIZE ; i++ )
			{
				DebrisController controller = ( Instantiate(debrisTypes[j]) as Transform).GetComponent<DebrisController>();
				controller.SetActive( false );
				controller.id = j;
				controller.CachedTransform.parent = _transform;
				_debrisPools[j].Push( controller );
			}
		}

		//rubble
		_rubblePools = new Stack<DebrisController>[rubbleTypes.Length];
		_activeRubble = new List<DebrisController>();
		
		
		for( int j = 0 ; j < rubbleTypes.Length ;j++)
		{
			_rubblePools[j] = new Stack<DebrisController>();
			for( int i = 0 ; i < POOL_SIZE ; i++ )
			{
				RubbleController controller = ( Instantiate(rubbleTypes[j]) as Transform).GetComponent<RubbleController>();
				controller.SetActive( false );
				controller.id = j;
				controller.CachedTransform.parent = _transform;
				controller.ReturnToPoolEvent.AddListener += ReturnRubbleToPool;
				controller.OnPlayerCollisionEvent.AddListener += OnPlayerCollisionEvent;
				_rubblePools[j].Push( controller );
			}
		}

		StartRandomRubbleGenerator();
	}
#region Debris
	public void AddDebrisOnTerrain( Vector3 position, float width, float height, float yAux , int amount )
	{
		ResetActiveDebris();

		Vector3 center = position ;
		float width_2 = 0.5f*width;
		float height_2 = 0.5f*height;
		float yOrigin = center.y + 0.5f*yAux;
		Vector3 currPos = new Vector3(0f, 0f ,0f);
		RaycastHit hit;

		for( int i = 0 ; i < amount ; i++ )
		{
			int debrisId = Random.Range( 0, debrisTypes.Length );

			currPos.x = Random.Range( -width_2,width_2);
			currPos.y = yOrigin;
			currPos.z = Random.Range( -height_2,0);

			if( Physics.Raycast(currPos,-Vector3.up,out hit,_rayLenght,(int)ePhysicsLayers.TERRAIN) &&
			   	!Physics.Raycast(currPos,-Vector3.up,out hit,_rayLenght,(int)ePhysicsLayers.FORBIDDEN_SPAWN_AREA ) &&
			    !Physics.Raycast(currPos,-Vector3.up,out hit,_rayLenght,(int)ePhysicsLayers.DEBRIS )
			   )
			{
				currPos.y = hit.point.y;
				DebrisController currDebris = _debrisPools[debrisId].Pop();
				currDebris.CachedTransform.position = currPos;
				currDebris.CachedTransform.parent = activeDebrisParent;
				currDebris.CachedTransform.rotation = Quaternion.AngleAxis(Random.Range(0f,360f),Vector3.forward);
				currDebris.SetActive( true );
				_activeDebris.Add( currDebris );
			}
			else
			{
				i -=1;
			}
		}
	}

	public void ResetActiveDebris()
	{
		if(_activeDebris.Count == 0)
			return;

		for( int i = 0 ; i < _activeDebris.Count ; i++)
		{
			_activeDebris[i].CachedTransform.parent = _transform;
			_debrisPools[_activeDebris[i].id ].Push( _activeDebris[i] );
			_activeDebris[i].SetActive( false );
		}
		_activeDebris.Clear();
	}
#endregion

#region Rubble
	public void StartRandomRubbleGenerator()
	{
		AddRubble();
		//Invoke( "AddRubble",Random.Range(rubbleSpawnRate.x,rubbleSpawnRate.y));
	}

	void AddRubble()
	{
		int rubbleId = Random.Range( 0,_rubblePools.Length);
		if( _rubblePools[rubbleId].Count > 0)
		{
			float width = rubbleSpawnArea.size.x*0.5f;
			float height = rubbleSpawnArea.size.y*0.5f;
			_rubbleRayOrigin.x = rubbleSpawnArea.center.x + Random.Range(-width,width);
			_rubbleRayOrigin.y = rubbleSpawnArea.center.y + Random.Range(-height,height);
			RaycastHit hit;
			if( Physics.Raycast(_rubbleRayOrigin,Vector3.forward,out hit,_rubbleRayLenght,(int)ePhysicsLayers.TERRAIN) )
			{
				DebrisController currRubble = _rubblePools[rubbleId].Pop();
				currRubble.CachedTransform.position = hit.point;
				currRubble.CachedTransform.parent = activeDebrisParent;
				currRubble.SetActive( true ,Random.Range(rubbleTTL.x,rubbleTTL.y));
				_activeDebris.Add( currRubble );
			}
		}
		Invoke( "AddRubble",Random.Range( rubbleSpawnRate.x,rubbleSpawnRate.y));
	}

	void ResetActiveRubble()
	{
		if(_activeRubble.Count == 0)
			return;
		
		for( int i = 0 ; i < _activeRubble.Count ; i++)
		{
			_activeRubble[i].CachedTransform.parent = _transform;
			_rubblePools[_activeDebris[i].id ].Push( _activeRubble[i] );
			_activeRubble[i].SetActive( false );
		}
		_activeRubble.Clear();
	}

	void ReturnRubbleToPool( params object[] vars )
	{
		DebrisController rubble = (DebrisController)vars[0];
		rubble.SetActive( false );
		_activeRubble.Remove( rubble );
		_rubblePools[ rubble.id ].Push( rubble );
	}

	void OnPlayerCollisionEvent(params object[] vars)
	{
		_onPlayerCollisionEvent.Dispatch();
	}

	public EventDispatcher PlayerCollisionEvent
	{
		get
		{
			return _onPlayerCollisionEvent;
		}
	}
#endregion

}