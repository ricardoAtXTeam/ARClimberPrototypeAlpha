using UnityEngine;


public delegate void EventHandler(params object[] args);

public class EventDispatcher
{
	public event EventHandler AddListener;
	
	public void Dispatch (params object[] args)
	{
		if (AddListener != null)
			AddListener (args);
	}
	public void ResetListeners()
	{
		AddListener = null;
	}
}