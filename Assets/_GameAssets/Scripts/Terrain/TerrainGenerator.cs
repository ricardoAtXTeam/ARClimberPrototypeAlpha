using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour
{
	public bool disableProceduralTerrain = true;
	public Texture2D terrainTexture;
	public Texture2D snowTexture;
	public float snowHeight = 50f;
	public Terrain preMadeTerrain;

	public Vector2 aWMinMax = new Vector2( 0.5f,0.6f);
	public Vector2 bWMinMax = new Vector2( 0f,1f);
	public Vector2 cWMinMax = new Vector2( 80f,90f);
	public Vector2 dWMinMax = new Vector2(0f,1f);

	public Vector2 bHMinMax = new Vector2(0f,1f);
	public Vector2 cHMinMax = new Vector2(150f,160f);

	public Vector2 perlinNoiseAmpMinMax = new Vector2(0.05f,0.051f);

	private TerrainData _terrainData;
	private GameObject _terrainGO;
	private Transform _terrainTransform;
	private Vector3 _highestPoint = new Vector3(0f,0f,0f);
	private float[,] _heightMap;
	private EventDispatcher _onTerrainSetupComplete = new EventDispatcher();
	private Vector3 _terrainSize;

	void Awake()
	{
		if(disableProceduralTerrain)
			return;

		_terrainGO = preMadeTerrain.gameObject;
		_terrainSize = preMadeTerrain.terrainData.size;

		_terrainTransform = _terrainGO.transform;
		_terrainGO.transform.parent = _terrainTransform;

		_terrainGO.layer = (int)eIntPhysicsLayers.TERRAIN;

		Build();
	}

	public void Build()
	{
		if(disableProceduralTerrain)
			return;

		GenerateTerrain( preMadeTerrain.terrainData,10);
		UpdateTerrainTexture(  preMadeTerrain.terrainData );
		UpdateHighestPoint( preMadeTerrain.terrainData );
		_onTerrainSetupComplete.Dispatch();
	}

	public void GenerateTerrain(TerrainData terrainData, float tileSize)
	{
		if(disableProceduralTerrain)
			return;

		_heightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];

		float width_2 = terrainData.heightmapWidth*0.5f;
		float height_2 = terrainData.heightmapHeight*0.5f;

		float aW = MathHelpers.RandomFromVec2(aWMinMax);
		float bW = MathHelpers.RandomFromVec2(bWMinMax);
		float cW = MathHelpers.RandomFromVec2(cWMinMax);
		float dW = MathHelpers.RandomFromVec2(dWMinMax);
		
		float bH = MathHelpers.RandomFromVec2(bHMinMax);
		float cH = MathHelpers.RandomFromVec2(cHMinMax);
	

		for (int i = 0; i < terrainData.heightmapWidth; i++)
		{
			for (int k = 0; k < terrainData.heightmapHeight; k++)
			{
				float xW = (float)(i - width_2);
				float xH = (float)(k - height_2);
				float heightAdjustW = aW*Mathf.Exp(- (Mathf.Pow((xW -bW),2)/(2*Mathf.Pow(cW,2)) + Mathf.Pow((xH-bH),2)/(2*Mathf.Pow(cH,2)))) + dW;

				_heightMap[i, k] = heightAdjustW +  MathHelpers.RandomFromVec2( perlinNoiseAmpMinMax )*Mathf.PerlinNoise(((float)i / (float)terrainData.heightmapWidth) * tileSize, ((float)k / (float)terrainData.heightmapHeight) * tileSize);
			}
		}

		terrainData.SetHeights(0, 0, _heightMap);

		SplatPrototype groundProto = new SplatPrototype();
		groundProto.texture = terrainTexture;
		groundProto.tileOffset = new Vector2(0f,0f);
		groundProto.tileSize = new Vector2(10f,10f);

		SplatPrototype snowProto = new SplatPrototype();
		snowProto.texture = snowTexture;
		snowProto.tileOffset = new Vector2(0f,0f);
		snowProto.tileSize = new Vector2(10f,10f);

		SplatPrototype[] prototypes = new SplatPrototype[2];
		prototypes[0] = groundProto;
		prototypes[1] = snowProto;
		terrainData.splatPrototypes = prototypes;

		_terrainTransform.localPosition = new Vector3( -0.5f*terrainData.size.x,0f,-0.5f*terrainData.size.z );

	}

	public bool UpdateTerrainSize(float newX, float newZ , bool updateLocalPosition)
	{
		if(disableProceduralTerrain)
			return false;

		if(	Mathf.Abs(_terrainSize.x-newX) < 0.01f &&
		   	Mathf.Abs(_terrainSize.z-newZ) < 0.01f )
		{
			return false;
		}

		_terrainSize.x = newX;
		_terrainSize.z = newZ;
		preMadeTerrain.terrainData.size = _terrainSize;
		_terrainTransform.localPosition = new Vector3( -0.5f*newX,0f,-0.5f*newZ );
		UpdateHighestPoint( preMadeTerrain.terrainData );
		return true;
	}

	void UpdateHighestPoint(TerrainData terrainData)
	{
		if(disableProceduralTerrain)
			return;

		float maxHeight = float.NegativeInfinity;
		float heightToTerrainCoordsW = terrainData.size.x/terrainData.heightmapWidth;
		float heightToTerrainCoordsH = terrainData.size.z/terrainData.heightmapHeight;


		for (int i = 0; i < terrainData.heightmapWidth; i++)
		{
			for (int j = 0; j < terrainData.heightmapHeight; j++)
			{
				float currHeight = terrainData.GetHeight(i,j);
				if(terrainData.GetHeight(i,j) > maxHeight)
				{
					maxHeight = currHeight;
					_highestPoint.x = i * heightToTerrainCoordsW;
					_highestPoint.z = j * heightToTerrainCoordsH;
					_highestPoint.y = maxHeight;
				}
			}
		}
	}


	void UpdateTerrainTexture( TerrainData terrainData )
	{
		if(disableProceduralTerrain)
			return;

		float[, ,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
		float alphaToHeightMapCoordsW = terrainData.heightmapWidth/terrainData.alphamapWidth;
		float alphaToHeightMapCoordsH = terrainData.heightmapHeight/terrainData.alphamapHeight;


		for (int i = 0; i < terrainData.alphamapWidth; i++)
		{
			for (int j = 0; j < terrainData.alphamapHeight; j++)
			{
				int hi = (int)(i*alphaToHeightMapCoordsW);
				int hj = (int)(j*alphaToHeightMapCoordsH);

				float terrainInterp =  Mathf.Clamp((_heightMap[hi,hj] / snowHeight),0f,1f);
				alphas[i, j, 0] = 1f - terrainInterp;
				alphas[i, j, 1] = terrainInterp;
			}
		}
		terrainData.SetAlphamaps(0, 0, alphas);
	}


	public Vector3 LocalHighestPoint
	{
		get
		{
			return ( _highestPoint );
		}
	}

	public Vector3 TerrainCurrentSize
	{
		get
		{
			return _terrainSize;
		}
	}

	public EventDispatcher OnTerrainSetupComplete
	{
		get
		{
			return _onTerrainSetupComplete;
		}
	}
}
