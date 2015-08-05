using UnityEngine;
using System.Collections;

public class TouchInput : MonoBehaviour
{
	public float minSwipeDistance = 20f;


	Vector2 startPos;

	AbsInputListener attachedListener;
	p2Map localMap;
	p2Gui gui;

	void Start () {
		localMap = p2Map.Instance;
		gui = p2Gui.Instance;
	}
	
	public void AddListener (AbsInputListener listener ) {
		attachedListener = listener;
	}

	bool isInControlZone;
	bool isInMapZone;

	bool IsInControlZone (Vector2 t ) {
		return gui.IsInControlZone();
	}

	bool IsInMapZone (Vector2 t ) {
		return localMap.IsInMapZone(t);
	}
	
#if UNITY_STANDALONE_WIN
	bool touchStart;

	Vector2 _tmp;
	Vector2 ConvertAxes (Vector3 v ) {
		_tmp.x = v.x;
		_tmp.y = v.z;
		return _tmp;
	}

	Vector2 lastPos;

#elif UNITY_ANDROID
	Touch lastTouch;
#endif

	void Update () {
#if UNITY_STANDALONE_WIN
		if ( Input.GetMouseButtonDown(0) ) {
			if ( !touchStart) {
				startPos = ConvertAxes(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				lastPos = startPos;
				touchStart = true;
				isInControlZone = IsInControlZone( startPos );
				isInMapZone = IsInMapZone( startPos );
			}
		}


		if ( Input.GetMouseButtonUp(0)) { 
			if ( touchStart ) {
				if ( !isInControlZone ) {
					var current = ConvertAxes(Camera.main.ScreenToWorldPoint(Input.mousePosition));
					var delta = current -  startPos;
					
					if ( Mathf.Abs(delta.y) > minSwipeDistance) {
						
						float swipeValue = Mathf.Sign(delta.y);
						
						if (swipeValue > 0){
							attachedListener.OnApprove();
						}else if (swipeValue < 0) {
							attachedListener.OnCancel();
						}
						
					} else if ( isInMapZone ) {
						attachedListener.OnMapZoneTap(current);
					}
				}
				touchStart = false;
			}
		}

		if ( touchStart ) {
			var current = ConvertAxes(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
			if (isInControlZone ) {
				attachedListener.OnControlZoneTouchMove(current - lastPos);
				lastPos = current;
			}
			if ( isInMapZone ) {
				attachedListener.OnMapZoneTouchMove(current);
			}
		}
		

#elif UNITY_ANDROID 
		if ( Input.touchCount > 0 ) {
			var t = Input.GetTouch(0);
			switch (t.phase) {
			case TouchPhase.Began:
				startPos = t.position;
				isInControlZone = IsInControlZone( startPos );
				isInMapZone = IsInMapZone( startPos );
				break;
			case TouchPhase.Moved:
				if (isInControlZone ) {
					attachedListener.OnControlZoneTouchMove(t.deltaPosition);
				}
				if ( isInMapZone ) {
					attachedListener.OnMapZoneTouchMove(t.position);
				}
				break;
			case TouchPhase.Ended:
				if ( isInControlZone ) break;

				var delta = t.position - startPos;
				Debug.Log("swipe distance = " + delta.y);

				if (Mathf.Abs(delta.y) > minSwipeDistance) {
					
					float swipeValue = Mathf.Sign(delta.y);
					
					if (swipeValue > 0){
						attachedListener.OnApprove();
					}else if (swipeValue < 0) {
						attachedListener.OnCancel();
					}
							
				} else if ( isInMapZone ) {
					attachedListener.OnMapZoneTap(t.position);
				}
				break;
			}
		} 
#else 
		throw new UnityException("Platform not supported");
#endif
	}


	private static TouchInput _instance;
	public static TouchInput Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(TouchInput)) as TouchInput;
				if ( _instance == null ) throw new UnityException("Object of type TouchInput not found");
			}
			return _instance;
		}
	}
}

