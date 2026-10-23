using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constant {
    public static string[] SUFFIXES = new string[] {
        "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No"
    };

    public static List<string> SUFFIXPARTS = new List<string>
    {
        "", "U", "D", "T", "QU", "QI", "SX", "SP", "O", "N",
        "", "Dc", "Vg", "Tg", "Qug", "Qig", "Sxg", "Spg", "Og", "Ng",
        "", "CE", "DC", "TC", "QUC", "QIC", "SXC", "SPC", "OC", "NC",
        "", "-Mi", "-Du", "-Tr"
    };
    public static int MAX_NAMED_EXPONENT = 3003;

    public static string UPGRADE_STEP_PREFIX = "+";
    public static string MAX_UPGRADE_TEXT = "MAX";

    public static string MUSIC_VOLUME_PARAM = "MusicVolume";
    public static string FX_VOLUME_PARAM = "SFXVolume";

    public static string UNKNOW_BRAINROT_NAME = "???";

    public static float SAVE_DELAY = 1f;
    public static float AUTO_SAVE_INTERVAL = 30f;

    public static long AFK_MAX_SECONDS = 24 * 60 * 60;

    public static int HOTBAR_SIZE = 5;

    public static string DEFAULT_PLAYER_NAME = "Player";

    public static string DEFAULT_BASE_NAME = "Nobody";
    public static string DEFAULT_BASE_NAME_SUFFIX = "'s Base";

    public static string FLOOR_AREA = "Floor";

    public static string PLACE_TEXT = "Place!";
    public static string SWAP_TEXT = "Swap!";
    public static string PICK_UP_TEXT = "Take it!";

    public static string MONEY_SYMBOL = "$";
    public static string INCOME_SUFIXES = "/s";

    public static string FLOOR_TITLE_SUFFIX = "nd Floor";

    public static string NOW_TITTLE_TEXT = "Now";
    public static string AFFTER_REBIRTH_TITTLE_TEXT = "After Rebirth";
    public static string REBIRTH_TEXT = "REBIRTH";
    public static string REBIRTH_READY_TEXT = "REBIRTH!";
    public static string MAX_REBIRTH_TEXT = "MAX";
    public static string REBIRTH_LEVEL_PREFIX = "Lv ";
    public static string AFTER_REBIRTH_TITLE = "After Rebirth";
    public static string MAX_REBIRTH_TITLE = "Max Rebirth";
    public static string REBIRTH_GOAL_PREFIX = "Reach <color=#7FE9FF>";
    public static string REBIRTH_GOAL_SUFFIX = " Jump Power</color> to rebirth for free";
    public static string REBIRTH_GOAL_READY = "Ready! Rebirth for <color=#8DFF5A>free</color>";
    public static string MAX_REBIRTH_GOAL = "You reached max rebirth!";
    public static string MULTIPLIER_PREFIX = "x";
    public static string BONUS_PREFIX = "+";
    public static float INCOME_INTERVAL = 1f;

    public static string PLAYER_TAG = "Player";

    public static string CHARACTER_TAG = "Character";

    public static string PLAYER_DATA = "PlayerData";

    public static float GROUNDED_GRAVITY = -2.5f;

    public static string ANIM_CLIMB_SPEED = "ClimbSpeed";

    public static float JOYSTICK_X_RATIO = 0.35f;
    public static float JOYSTICK_Y_RATIO = 0.65f;
}