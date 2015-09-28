using UnityEngine;
using System.Collections;

public class p3TouchInput
{
	public float minSwipeDistance = 20f;
	Vector2 startPos;

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
	
	public void _Update ( IInputListener listener) {
		if ( listener == null ) return;
		#if UNITY_STANDALONE_WIN
		if ( Input.GetMouseButtonDown(0) ) {
			if ( !touchStart) {
				startPos = ConvertAxes(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				touchStart = true;
				lastPos = startPos;
			}
		}

		if ( touchStart ) {
			var current = ConvertAxes(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 

			var delta = current - lastPos;
			if( delta.magnitude > 0.5f ) {
				listener.OnDrag(current,delta);
			}
			lastPos = current;
		}
		
		if ( Input.GetMouseButtonUp(0)) { 
			if ( touchStart ) {
				var current = ConvertAxes(Camera.main.ScreenToWorldPoint(Input.mousePosition));
				listener.OnTouch(current);
				touchStart = false;
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
					if( listener != null ) listener.OnControlZoneTouchMove(t.deltaPosition);
				}
				if ( isInMapZone ) {
					if( listener != null ) listener.OnMapZoneTouchMove(t.position);
				}
				break;
			case TouchPhase.Ended:
				if ( isInControlZone ) break;
				
				var delta = t.position - startPos;
				
				if (Mathf.Abs(delta.y) > minSwipeDistance) {
					
					float swipeValue = Mathf.Sign(delta.y);
					
					if (swipeValue > 0){
						if( listener != null ) listener.OnSwipeUp();
					}else if (swipeValue < 0) {
						if( listener != null ) listener.OnSwipeDown();
					}
					
				} else if ( isInMapZone ) {
					if( listener != null ) listener.OnMapZoneTap(t.position);
				}
				break;
			}
		} 
		#else 
		throw new UnityException("Platform not supported");
		#endif
	}
}

