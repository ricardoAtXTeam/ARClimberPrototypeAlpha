using UnityEngine;
using System.Collections;


class Boundary 
{
	public Vector2  min;
	public Vector2 max;
}

[RequireComponent( typeof(GUITexture) )]
public class Joystick : MonoBehaviour
{
	static private Joystick [] joysticks;					// A static collection of all joysticks
	static private bool enumeratedJoysticks = false;
								
	public Vector2 deadZone = Vector2.zero;						// Control when position is output
	public bool normalize = false; 							// Normalize output after the dead-zone?
	public Vector2 position ; 									// [-1, 1] in x,y

	public bool useCustomBoundaries = false;
	public Rect customBoundaries;

	private int lastFingerId = -1;								// Finger last used for this joystick

	private GUITexture gui;								// Joystick graphic
	private Rect defaultRect;								// Default position / extents of the joystick graphic
	private Boundary guiBoundary  = new Boundary();			// Boundary for joystick graphic
	private Vector2 guiTouchOffset;						// Offset to apply to touch input
	private Vector2 guiCenter;							// Center of joystick
	private Transform _transform;
	private GameObject _gameObject;
#if UNITY_EDITOR
	private Vector2 _simTouch = new Vector2(0f,0f);
#endif

	void Start()
	{
		_transform = transform;

		// Cache this component at startup instead of looking up every frame	
		gui = GetComponent<GUITexture>();
		
		// Store the default rect for the gui, so we can snap back to it
		defaultRect = (useCustomBoundaries)?customBoundaries:gui.pixelInset;	
	    
		defaultRect.x += _transform.position.x * Screen.width;// + gui.pixelInset.x; // -  Screen.width * 0.5;
		defaultRect.y += _transform.position.y * Screen.height;// - Screen.height * 0.5;
	    

		_gameObject = gameObject;
		Vector3 pos  = new Vector3(0f,0f,_transform.position.z);
	    _transform.position = pos;
	        
			
		// This is an offset for touch input to match with the top left
		// corner of the GUI
		guiTouchOffset.x = defaultRect.width * 0.5f;
		guiTouchOffset.y = defaultRect.height * 0.5f;
		
		// Cache the center of the GUI, since it doesn't change
		guiCenter.x = defaultRect.x + guiTouchOffset.x;
		guiCenter.y = defaultRect.y + guiTouchOffset.y;
		
		// Let's build the GUI boundary, so we can clamp joystick movement
		guiBoundary.min.x = defaultRect.x - guiTouchOffset.x;
		guiBoundary.max.x = defaultRect.x + guiTouchOffset.x;
		guiBoundary.min.y = defaultRect.y - guiTouchOffset.y;
		guiBoundary.max.y = defaultRect.y + guiTouchOffset.y;

	}
	
	void Disable()
	{
		_gameObject.SetActive(false);
		enumeratedJoysticks = false;
	}
	
	void ResetJoystick()
	{
		// Release the finger control and set the joystick back to the default position
		gui.pixelInset = defaultRect;
		lastFingerId = -1;
		position = Vector2.zero;
	}
	
	bool IsFingerDown()
	{
		return (lastFingerId != -1);
	}
		
	void LatchedFinger( int fingerId )
	{
		// If another joystick has latched this finger, then we must release it
		if ( lastFingerId == fingerId )
			ResetJoystick();
	}

	void AnalyseTouch( Vector2 touchPosition , int touchFingerId , TouchPhase touchPhase)
	{
		Vector2 guiTouchPos = touchPosition - guiTouchOffset;
		bool shouldLatchFinger = false;
		if ( gui.HitTest( touchPosition ) )
		{
			shouldLatchFinger = true;
		}		
		
		// Latch the finger if this is a new touch
		if ( shouldLatchFinger && ( lastFingerId == -1 || lastFingerId != touchFingerId ) )
		{
			lastFingerId = touchFingerId;
			// Tell other joysticks we've latched this finger
			for ( int i = 0 ; i < joysticks.Length; i++ )
			{
				if ( joysticks[i] != this )
					joysticks[i].LatchedFinger( touchFingerId );
			}						
		}				
		
		if ( lastFingerId == touchFingerId )
		{						
			// Change the location of the joystick graphic to match where the touch is
			Rect newPixelInset = gui.pixelInset;
			newPixelInset.x =  Mathf.Clamp( guiTouchPos.x, guiBoundary.min.x, guiBoundary.max.x );
			newPixelInset.y =  Mathf.Clamp( guiTouchPos.y, guiBoundary.min.y, guiBoundary.max.y );	
			gui.pixelInset = newPixelInset;
			
			
			if ( touchPhase == TouchPhase.Ended || touchPhase == TouchPhase.Canceled )
				ResetJoystick();					
		}	
	}

	void Update()
	{	
		if ( !enumeratedJoysticks )
		{
			// Collect all joysticks in the game, so we can relay finger latching messages
			joysticks = FindObjectsOfType( typeof(Joystick) ) as Joystick[];
			enumeratedJoysticks = true;
		}	
#if UNITY_EDITOR

		if( Input.GetMouseButton(0))
		{
			_simTouch.x = Input.mousePosition.x;
			_simTouch.y = Input.mousePosition.y;
			AnalyseTouch( _simTouch,0,TouchPhase.Began );
		}
		else
		{
			ResetJoystick();
		}
#else
		int count = Input.touchCount;
		if ( count == 0 )
		{
			ResetJoystick();
		}
		else
		{
			for(int i = 0;i < count; i++)
			{
				Touch touch = Input.GetTouch(i);
				AnalyseTouch( touch.position,touch.fingerId,touch.phase );		
			}
		}
#endif

		// Get a value between -1 and 1 based on the joystick graphic location
		position.x = ( gui.pixelInset.x + guiTouchOffset.x - guiCenter.x ) / guiTouchOffset.x;
		position.y = ( gui.pixelInset.y + guiTouchOffset.y - guiCenter.y ) / guiTouchOffset.y;

		// Adjust for dead zone	
		float absoluteX = Mathf.Abs( position.x );
		float absoluteY = Mathf.Abs( position.y );
		
		if ( absoluteX < deadZone.x )
		{
			// Report the joystick as being at the center if it is within the dead zone
			position.x = 0;
		}
		else if ( normalize )
		{
			// Rescale the output after taking the dead zone into account
			position.x = Mathf.Sign( position.x ) * ( absoluteX - deadZone.x ) / ( 1 - deadZone.x );
		}
			
		if ( absoluteY < deadZone.y )
		{
			// Report the joystick as being at the center if it is within the dead zone
			position.y = 0;
		}
		else if ( normalize )
		{
			// Rescale the output after taking the dead zone into account
			position.y = Mathf.Sign( position.y ) * ( absoluteY - deadZone.y ) / ( 1 - deadZone.y );
		}
	}
}