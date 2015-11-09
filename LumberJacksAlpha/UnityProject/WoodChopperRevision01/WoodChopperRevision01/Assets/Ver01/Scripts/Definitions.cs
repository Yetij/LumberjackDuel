using UnityEngine;
using System.Collections;

public class Definitions {
    static public float visual_match_end = 5f;
    internal static float visual_tree_fall_time = 0.5f;
    internal static float visual_chop_player_time = 0.5f;
    internal static float visual_move_time = 0.3f;
}

public enum PLAY : int { ER1 = 0, ER2 = 1 };

public enum TreeType : int {
    None, 
    Basic,
    BonusAc,
    Ethereal,
    Monumental,
    Stone,
    Swamp
}

