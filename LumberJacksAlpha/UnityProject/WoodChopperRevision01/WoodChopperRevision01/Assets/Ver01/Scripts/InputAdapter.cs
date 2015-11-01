using UnityEngine;
using System.Collections;

public class InputAdapter : MonoBehaviour {
    public delegate void OnTap(Vector2 pos);
    public delegate void OnDrag(Vector2 pos, Vector2 delta);

    public event OnDrag onDrag;
    public event OnTap onTap;

    Vector3 last_pos;
    void Update()
    {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if ( Input.GetMouseButton(0) )  // tap 
        {
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
