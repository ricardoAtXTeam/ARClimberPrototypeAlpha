using UnityEngine;

public class DebrisController :MonoBehaviour
{
	public int id;
	private Transform _transform;

	public void SetActive( bool value )
	{
		gameObject.SetActive( value );
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
}