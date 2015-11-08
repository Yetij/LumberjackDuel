using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleAdapter : MonoBehaviour {
    public delegate void _OnValueChanged (Toggle toggle, TreeType treeType);
    public event _OnValueChanged onTreeSelected;
    public event _OnValueChanged onTreeDeselected;

    public TreeType treeType;
    private Toggle toggle;

    void Awake ()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ValueChanged);
    }

    void ValueChanged ( bool current )
    {
        if ( onTreeSelected != null & current )
        {
            onTreeSelected(toggle,treeType);
        }
        if (onTreeDeselected != null & !current)
        {
            onTreeSelected(toggle, treeType);
        }
    }

    public void Flush ()
    {
        toggle.isOn = false;
    }
}
