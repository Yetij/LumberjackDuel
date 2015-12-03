using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIAdapter : MonoBehaviour {
    ToggleAdapter[] ui_plants;

    public TreeType selectedPlant { get; private set; }
    public Toggle currentSelected { get; private set; }

    Text ac, time, player1_points;
    Text[] player_points;

    static int plants_nb = 3;
    void Start()
    {
        ui_plants = GetComponentsInChildren<ToggleAdapter>();
        foreach (var u in ui_plants)
        {
            u.onTreeSelected += TreeSelected;
            u.onTreeDeselected += TreeDeselected;
        }
        
        for(int i =0; i < plants_nb; i ++ )
        {
            var type = (TreeType ) PlayerPrefs.GetInt("fukingtreehash_tree" + i);
            ui_plants[i].treeType = type;
            var _t = MonoRefCenter.instance.Get(type);
            if ( _t.Icon != null)  ui_plants[i].SetImage(_t.Icon);
        }
        ac = GameObject.Find("_#TextAc").GetComponent<Text>();
        time = GameObject.Find("_#TextTime").GetComponent<Text>();
        var player1_points = GameObject.Find("_#TextPlayer1Points").GetComponent<Text>();
        var player2_points = GameObject.Find("_#TextPlayer2Points").GetComponent<Text>();
        player_points = new Text[2] { player1_points, player2_points };

        if (ac == null | time == null | player1_points == null | player2_points == null)
        {
            throw new UnityException("Something is missing, check names of ui elements in Hierarchy");
        }
    }

    void TreeSelected(Toggle toggle, TreeType type )
    {
        if(currentSelected != null & currentSelected != toggle )
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

    public void Timer(int _time)
    {
        time.text = _time.ToString();
    }

    public void AC(int _ac)
    {
        ac.text = _ac.ToString();
    }

    public void Points(int player, int points)
    {
        player_points[player].text = points.ToString();
    }
}
