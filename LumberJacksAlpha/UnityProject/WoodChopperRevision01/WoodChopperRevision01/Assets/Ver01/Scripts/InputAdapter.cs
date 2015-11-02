using UnityEngine;
using System.Collections;

public class InputAdapter : MonoBehaviour {
    public delegate void OnTap(Vector2 pos);
    public delegate void OnDrag(Vector2 pos, Vector2 delta);

    public event OnDrag onDrag;
    public event OnTap onTap;

    float interval = 0.25f;
    float last_tap = 0;

    Vector3 last_pos;
    void Update()
    {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if ( Input.GetMouseButton(0) & Time.time - last_tap > interval )  // tap 
        {
            last_tap = Time.time;
            last_pos = pos;
            if ( onTap != null )
            {
                onTap(pos);
            }
        }

        if ( Input.GetMouseButtonDown(0)) // drag
        {
            var delta = pos - last_pos;
            last_pos = pos;
            if ( onDrag != null )
            {
                onDrag(pos, delta);
            }
        }
        
    }
}
