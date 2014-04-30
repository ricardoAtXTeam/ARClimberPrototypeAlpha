using UnityEngine;
using System.Collections.Generic;

public class TimeLabelFactory : MonoBehaviour 
{
	public Transform timeLabelPrefab;
	public Transform timePenaltyParent;
	private const int POOLS_SIZE = 30;
	private Stack<TimeLabel> _pool;
	private float _labelTTL = 1f;

	void Awake()
	{
		_pool = new Stack<TimeLabel>();
		for( int i = 0 ; i < POOLS_SIZE ; i++ )
		{
			Transform timeLabelTF = Instantiate(timeLabelPrefab,timeLabelPrefab.position,Quaternion.identity) as Transform;
			timeLabelTF.parent = timePenaltyParent;
			TimeLabel timeLabel = timeLabelTF.GetComponent<TimeLabel>();
			timeLabel.OnReturnToPool.AddListener += OnReturnToPoolEvent;
			timeLabel.Init(timeLabelPrefab.position,timeLabelPrefab.position - 0.2f*Vector3.up );
			_pool.Push( timeLabel );
		}
	}

	public void ActivateLabel( string txt )
	{
		if(_pool.Count == 0)
			return;

		TimeLabel timeLabel = _pool.Pop();
		timeLabel.Activate(txt,_labelTTL );
	}

	void OnReturnToPoolEvent( params object[] vars )
	{
		_pool.Push( (TimeLabel)vars[0]);
	}
}
