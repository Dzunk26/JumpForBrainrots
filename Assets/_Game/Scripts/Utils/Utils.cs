using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static Vector3 WorldToScreenPoint(Camera camera, Vector3 worldPoint) {
        return camera.WorldToScreenPoint(worldPoint);
    }
}