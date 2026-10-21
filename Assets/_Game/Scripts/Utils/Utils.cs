using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class Utils {
    public static Vector3 WorldToScreenPoint(Camera camera, Vector3 worldPoint) {
        return camera.WorldToScreenPoint(worldPoint);
    }

    public static int GetEnumCount<T>() where T : Enum {
        return Enum.GetNames(typeof(T)).Length;
    }

    public static string SciNotToUSName(BigDouble value) {
        if (value.Exponent < 3) return value.ToDouble().ToString("0.##", CultureInfo.InvariantCulture);
        if (value.Exponent >= Constant.MAX_NAMED_EXPONENT) return value.ToString("E2");

        int exponent = (int)value.Exponent;
        int groupIndex = exponent / 3;
        int remainder = exponent % 3;

        double displayValue = Math.Round(value.Mantissa * Math.Pow(10, remainder), 2 - remainder);
        if (Math.Abs(displayValue) >= 1000) {
            displayValue /= 1000;
            groupIndex++;
        }

        return displayValue.ToString("0.##", CultureInfo.InvariantCulture) + "" + GetSuffixName(groupIndex);
    }

    private static string GetSuffixName(int groupIndex) {
        if (groupIndex < Constant.SUFFIXES.Length) return Constant.SUFFIXES[groupIndex];

        int latinIndex = groupIndex - 1;
        int suffixOffset = 0;
        string name = string.Empty;

        while (latinIndex > 0) {
            name += Constant.SUFFIXPARTS[latinIndex % 10 + suffixOffset];
            latinIndex /= 10;
            suffixOffset += 10;
        }

        return name;
    }
}