using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebrisFactory : MonoBehaviour 
{

	public Transform[] debrisTypes;
	public Transform activeDebrisParent;
	public TerrainGenerator terrainGenerator;
	private const int POOL_SIZE = 20;

	private Stack<DebrisController>[] _objectPools;
	private BoxCollider _spawnArea;
	private List<DebrisController> _activeObjects;

	private float _rayLenght = 700f;
	private Transform _transform;
	void Awake () 
	{
		_transform = transform;

		_objectPools = new Stack<DebrisController>[debrisTypes.Length];
		_activeObjects = new List<DebrisController>();

	
		for( int j = 0 ; j < debrisTypes.Length ;j++)
		{
			_objectPools[j] = new Stack<DebrisController>();
			for( int i = 0 ; i < POOL_SIZE ; i++ )
			{
				DebrisController controller = ( Instantiate(debrisTypes[j]) as Transform).GetComponent<DebrisController>();
				controller.SetActive( false );
				controller.id = j;
				controller.CachedTransform.parent = _transform;
				_objectPools[j].Push( controller );
			}
		}

	}
	
	public void AddDebrisOnTerrain( Vector3 position, float width, float height, float yAux , int amount )
	{
		ResetActiveObjects();
		Debug.Log( "Adding debris to terrain");
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
				DebrisController currDebris = _objectPools[debrisId].Pop();
				currDebris.CachedTransform.position = currPos;
				currDebris.CachedTransform.parent = activeDebrisParent;
				currDebris.CachedTransform.rotation = Quaternion.AngleAxis(Random.Range(0f,360f),Vector3.forward);
				currDebris.SetActive( true );
				_activeObjects.Add( currDebris );
			}
			else
			{
				i -=1;
			}
		}
	}

	public void ResetActiveObjects()
	{
		if(_activeObjects.Count == 0)
			return;

		for( int i = 0 ; i < _activeObjects.Count ; i++)
		{
			_activeObjects[i].CachedTransform.parent = _transform;
			_objectPools[_activeObjects[i].id ].Push( _activeObjects[i] );
			_activeObjects[i].SetActive( false );
		}
		_activeObjects.Clear();
	}


}