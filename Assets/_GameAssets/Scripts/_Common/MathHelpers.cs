using UnityEngine;

public class MathHelpers
{
	public static float INV_PI = 1f/Mathf.PI;

	public static float TWO_PI = 2f*Mathf.PI;
	public static float INV_TWO_PI = 1f/(2f*Mathf.PI);

	public static float HALF_PI = 0.5f*Mathf.PI;
	public static float INV_HALF_PI = 1f/( 0.5f*Mathf.PI );

	public static float RandomFromVec2( Vector2 vec)
	{
		return Random.Range( vec.x,vec.y);
	}

	public Vector3 RandomPointInsideBox( BoxCollider collider )
	{
		return collider.center + new Vector3(	Random.Range(-0.5f*collider.size.x,0.5f*collider.size.x),
					                            Random.Range(-0.5f*collider.size.y,0.5f*collider.size.y),
					                            Random.Range(-0.5f*collider.size.z,0.5f*collider.size.z) );
	}
}