using UnityEngine;
using System.Collections;

public class RubbleController : DebrisController
{
	public ParticleSystem dustPS;
	public ParticleSystem rubblePS;

	private float _dustEmissionRate;
	private float _rubbleEmissionRate;
	private float _maxTTL;
	private EventDispatcher _onPlayerCollisionEvent = new EventDispatcher();
	void Awake()
	{
		_dustEmissionRate = dustPS.emissionRate;
		_rubbleEmissionRate = rubblePS.emissionRate;
		_maxTTL = Mathf.Max( dustPS.startLifetime,rubblePS.startLifetime );
	}

	override public void SetActive( bool value , float activeTime = -1)
	{
		base.SetActive( value ,activeTime );
		if( value )
		{
			dustPS.emissionRate = _dustEmissionRate;
			rubblePS.emissionRate = _rubbleEmissionRate;
		}
		CancelInvoke( "WaitOnParticleEmitters" );
	}

	override protected void ReturnToPool()
	{
		dustPS.emissionRate = 0;
		rubblePS.emissionRate = 0;
		Invoke( "WaitOnParticleEmitters" , _maxTTL );
	}

	void WaitOnParticleEmitters()
	{
		_returnToPoolEvent.Dispatch(this);
	}

	void OnTriggerStay( Collider other )
	{
		_onPlayerCollisionEvent.Dispatch();
	}

	public EventDispatcher OnPlayerCollisionEvent
	{
		get
		{
			return _onPlayerCollisionEvent;
		}
	}
}
