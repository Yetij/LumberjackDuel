using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIAdapter : MonoBehaviour {
    ToggleAdapter[] ui_plants;

    public TreeType selectedPlant { get; private set; }
    public Toggle currentSelected { get; private set; }

    void Awake ()
    {
        ui_plants = GetComponentsInChildren<ToggleAdapter>();
        foreach ( var u in ui_plants )
        {
            u.onTreeSelected += TreeSelected;
            u.onTreeDeselected += TreeDeselected;
        }
    }

    

    void TreeSelected(Toggle toggle, TreeType type )
    {
        if( currentSelected != toggle )
        {
            currentSelected.isOn = false;
        }
        currentSelected = toggle;
        selectedPlant = type;
    }

    void TreeDeselected(Toggle toggle, TreeType type)
    {
        if (currentSelected == toggle)
        {
            currentSelected = null;
        }
        selectedPlant = TreeType.None;
    }

    internal void Timer(int time)
    {
        throw new NotImplementedException();
    }

    internal void AC(int ac)
    {
        throw new NotImplementedException();
    }

    internal void Points(int points)
    {
        throw new NotImplementedException();
    }
}
