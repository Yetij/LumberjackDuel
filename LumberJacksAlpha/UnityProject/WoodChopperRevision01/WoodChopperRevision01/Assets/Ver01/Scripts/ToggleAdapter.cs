using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleAdapter : MonoBehaviour {
    public delegate void _OnValueChanged (Toggle toggle, TreeType treeType);
    public event _OnValueChanged onTreeSelected;
    public event _OnValueChanged onTreeDeselected;

    public TreeType treeType;
    private Toggle toggle;
    private Image treeDisplay;

    public bool isOn
    {
        get
        {
            return toggle.isOn;
        }
    }

    void Awake ()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ValueChanged);
        treeDisplay = transform.FindChild("Label").GetComponent<Image>();
    }

    void ValueChanged ( bool current )
    {
        if ( onTreeSelected != null & current )
        {
            onTreeSelected(toggle,treeType);
        }
        if (onTreeDeselected != null & !current)
        {
            onTreeDeselected(toggle, treeType);
        }
    }

    public void SetImage ( Sprite s)
    {
        treeDisplay.sprite = s;
    }
    public void Flush ()
    {
        toggle.isOn = false;
    }
}
