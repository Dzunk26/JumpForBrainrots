using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static Vector3 WorldToScreenPoint(Camera camera, Vector3 worldPoint) {
        return camera.WorldToScreenPoint(worldPoint);
    }

    public static string SciNotToUSName(BigDouble value) {
        string displayNumber = $"{(value.Mantissa * BigDouble.Pow(10, value.Exponent % 3)):G3}";

        int suffixIndex = (int)BigDouble.Floor(BigDouble.Abs(value.Exponent)).ToDouble();
        string name = string.Empty;
        int suffixOffset = 0;

        if (value.Exponent < 33) {
            suffixIndex /= 3;
            name += $"{Constant.SUFFIXES[suffixIndex]}";
        }
        else {
            suffixIndex = (suffixIndex - 3) / 3;
            int tempSuffixIndex = suffixIndex;
            List<int> indices = new List<int>();

            for (int i = 0; i < suffixIndex.ToString().Length; i++) {
                int lastNum = tempSuffixIndex % 10;
                indices.Add(lastNum);
                tempSuffixIndex /= 10;
                name += Constant.SUFFIXES[indices[i] + suffixIndex];
                suffixOffset += 10;
            }
        }

        return $"{displayNumber} {name}";
    }
}