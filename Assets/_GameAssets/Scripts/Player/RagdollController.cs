using UnityEngine;
using System.Collections;

public class RagdollController : MonoBehaviour 
{
	public Animator animatorController;
	public Rigidbody[] rigidBodies;
	private Collider[] colliders;

	private bool _isActive = false;

	void Awake () 
	{
		animatorController.enabled = true;
		colliders = new Collider[rigidBodies.Length];
		for( int i = 0 ; i < rigidBodies.Length; i++)
		{
			rigidBodies[i].isKinematic = true;
			rigidBodies[i].useGravity = false;
			colliders[i] = rigidBodies[i].GetComponent<Collider>();
			colliders[i].enabled = false;
		}
	}

	public void ActivateRagDoll(bool value)
	{
		_isActive = value;
		animatorController.enabled = !value;
		for( int i = 0 ; i < rigidBodies.Length; i++)
		{
			rigidBodies[i].isKinematic = !value;
			rigidBodies[i].useGravity = value;
			colliders[i].enabled = value;
		}
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			ActivateRagDoll(!_isActive);
		}
	}

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
	}
}
