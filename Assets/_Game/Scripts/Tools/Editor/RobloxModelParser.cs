using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using UnityEngine;

public struct RobloxNumberKey {
    public float time;
    public float value;
    public float envelope;
}

public struct RobloxColorKey {
    public float time;
    public Color color;
}

public struct RobloxCFrame {
    public Vector3 position;
    public float[] rotation; // row-major R00..R22

    // Roblox la he toa do tay phai (-Z la forward), Unity la tay trai -> lat truc Z
    public Vector3 GetUnityPosition(float scale) {
        return new Vector3(position.x, position.y, -position.z) * scale;
    }

    public Quaternion GetUnityRotation() {
        if (rotation == null) return Quaternion.identity;

        // M' = S * M * S voi S = diag(1, 1, -1)
        float[] m = (float[])rotation.Clone();
        m[2] = -m[2];
        m[5] = -m[5];
        m[6] = -m[6];
        m[7] = -m[7];

        Vector3 up = new Vector3(m[1], m[4], m[7]);
        Vector3 forward = new Vector3(m[2], m[5], m[8]);
        if (forward.sqrMagnitude < 0.0001f || up.sqrMagnitude < 0.0001f) return Quaternion.identity;
        return Quaternion.LookRotation(forward, up);
    }
}

public class RobloxInstance {
    public string className;
    public string referent;
    public string name;
    public RobloxInstance parent;
    public List<RobloxInstance> children = new List<RobloxInstance>();
    public Dictionary<string, XmlElement> properties = new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, object> attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public bool Has(string prop) {
        return properties.ContainsKey(prop);
    }

    public string GetString(string prop, string fallback = "") {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        return element.InnerText;
    }

    public float GetFloat(string prop, float fallback = 0f) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        return RobloxModelParser.ParseFloat(element.InnerText, fallback);
    }

    public int GetInt(string prop, int fallback = 0) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        int value;
        return int.TryParse(element.InnerText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : fallback;
    }

    public bool GetBool(string prop, bool fallback = false) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        return element.InnerText.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    public Vector2 GetVector2(string prop, Vector2 fallback = default) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        return new Vector2(ChildFloat(element, "X"), ChildFloat(element, "Y"));
    }

    // tra ve vector theo toa do Roblox, chua doi truc
    public Vector3 GetVector3(string prop, Vector3 fallback = default) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        return new Vector3(ChildFloat(element, "X"), ChildFloat(element, "Y"), ChildFloat(element, "Z"));
    }

    // NumberRange luu dang "min max"
    public Vector2 GetNumberRange(string prop, Vector2 fallback) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;
        float[] values = RobloxModelParser.ParseFloatList(element.InnerText);
        if (values.Length == 0) return fallback;
        if (values.Length == 1) return new Vector2(values[0], values[0]);
        return new Vector2(values[0], values[1]);
    }

    // NumberSequence luu dang "time value envelope time value envelope ..."
    public List<RobloxNumberKey> GetNumberSequence(string prop) {
        List<RobloxNumberKey> keys = new List<RobloxNumberKey>();
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return keys;

        float[] values = RobloxModelParser.ParseFloatList(element.InnerText);
        for (int i = 0; i + 2 < values.Length; i += 3) {
            keys.Add(new RobloxNumberKey { time = values[i], value = values[i + 1], envelope = values[i + 2] });
        }
        return keys;
    }

    // ColorSequence luu dang "time r g b envelope ..."
    public List<RobloxColorKey> GetColorSequence(string prop) {
        List<RobloxColorKey> keys = new List<RobloxColorKey>();
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return keys;

        float[] values = RobloxModelParser.ParseFloatList(element.InnerText);
        for (int i = 0; i + 4 < values.Length; i += 5) {
            keys.Add(new RobloxColorKey { time = values[i], color = new Color(values[i + 1], values[i + 2], values[i + 3], 1f) });
        }
        return keys;
    }

    public Color GetColor3(string prop, Color fallback) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return fallback;

        if (element.Name == "Color3uint8") {
            uint packed;
            if (!uint.TryParse(element.InnerText.Trim(), out packed)) return fallback;
            return new Color32((byte)((packed >> 16) & 0xFF), (byte)((packed >> 8) & 0xFF), (byte)(packed & 0xFF), 255);
        }
        return new Color(ChildFloat(element, "R"), ChildFloat(element, "G"), ChildFloat(element, "B"), 1f);
    }

    public string GetContent(string prop) {
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return "";
        XmlNode url = element.SelectSingleNode("url");
        return url != null ? url.InnerText.Trim() : element.InnerText.Trim();
    }

    public string GetRef(string prop) {
        string value = GetString(prop).Trim();
        return value == "null" ? "" : value;
    }

    public bool TryGetCFrame(string prop, out RobloxCFrame cframe) {
        cframe = new RobloxCFrame();
        XmlElement element;
        if (!properties.TryGetValue(prop, out element)) return false;

        cframe.position = new Vector3(ChildFloat(element, "X"), ChildFloat(element, "Y"), ChildFloat(element, "Z"));
        cframe.rotation = new float[9];
        for (int row = 0; row < 3; row++) {
            for (int col = 0; col < 3; col++) {
                cframe.rotation[row * 3 + col] = ChildFloat(element, "R" + row + col, row == col ? 1f : 0f);
            }
        }
        return true;
    }

    public float GetAttributeFloat(string attribute, float fallback) {
        object value;
        if (!attributes.TryGetValue(attribute, out value)) return fallback;
        if (value is double) return (float)(double)value;
        if (value is float) return (float)value;
        if (value is string) return RobloxModelParser.ParseFloat((string)value, fallback);
        return fallback;
    }

    private static float ChildFloat(XmlElement element, string childName, float fallback = 0f) {
        XmlNode child = element.SelectSingleNode(childName);
        return child != null ? RobloxModelParser.ParseFloat(child.InnerText, fallback) : fallback;
    }
}

// doc file .rbxmx (XML) xuat tu Roblox Studio thanh cay RobloxInstance
public static class RobloxModelParser {
    public static List<RobloxInstance> ParseFile(string filePath) {
        XmlDocument document = new XmlDocument();
        document.Load(filePath);

        List<RobloxInstance> roots = new List<RobloxInstance>();
        if (document.DocumentElement == null) return roots;

        foreach (XmlNode node in document.DocumentElement.ChildNodes) {
            XmlElement element = node as XmlElement;
            if (element != null && element.Name == "Item") {
                roots.Add(ParseItem(element, null));
            }
        }
        return roots;
    }

    public static float ParseFloat(string text, float fallback = 0f) {
        string value = text.Trim();
        if (value.Length == 0) return fallback;

        string upper = value.ToUpperInvariant();
        if (upper == "INF" || upper == "+INF") return float.PositiveInfinity;
        if (upper == "-INF") return float.NegativeInfinity;
        if (upper.Contains("NAN")) return fallback;

        float result;
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : fallback;
    }

    public static float[] ParseFloatList(string text) {
        string[] parts = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        float[] values = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++) {
            values[i] = ParseFloat(parts[i]);
        }
        return values;
    }

    private static RobloxInstance ParseItem(XmlElement element, RobloxInstance parent) {
        RobloxInstance instance = new RobloxInstance {
            className = element.GetAttribute("class"),
            referent = element.GetAttribute("referent"),
            parent = parent
        };

        foreach (XmlNode node in element.ChildNodes) {
            XmlElement child = node as XmlElement;
            if (child == null) continue;

            if (child.Name == "Properties") {
                foreach (XmlNode propertyNode in child.ChildNodes) {
                    XmlElement property = propertyNode as XmlElement;
                    if (property == null) continue;
                    string propertyName = property.GetAttribute("name");
                    if (!string.IsNullOrEmpty(propertyName)) instance.properties[propertyName] = property;
                }
            } else if (child.Name == "Item") {
                instance.children.Add(ParseItem(child, instance));
            }
        }

        instance.name = instance.GetString("Name", instance.className);
        ParseAttributes(instance);
        return instance;
    }

    // attributes (vd EmitCount, EmitDelay cua cac VFX pack) duoc luu base64 nhi phan trong AttributesSerialize
    private static void ParseAttributes(RobloxInstance instance) {
        string base64 = instance.GetString("AttributesSerialize").Trim();
        if (base64.Length == 0) return;

        byte[] data;
        try {
            data = Convert.FromBase64String(base64);
        } catch (FormatException) {
            return;
        }

        int offset = 0;
        try {
            uint count = ReadUInt(data, ref offset);
            for (uint i = 0; i < count; i++) {
                string key = ReadString(data, ref offset);
                byte type = data[offset++];
                object value;
                if (!TryReadAttributeValue(data, ref offset, type, out value)) return;
                if (value != null) instance.attributes[key] = value;
            }
        } catch (IndexOutOfRangeException) {
            // du lieu bi cat ngang thi giu lai nhung attribute da doc duoc
        }
    }

    private static bool TryReadAttributeValue(byte[] data, ref int offset, byte type, out object value) {
        value = null;
        switch (type) {
            case 0x02: value = ReadString(data, ref offset); return true;
            case 0x03: value = data[offset++] != 0; return true;
            case 0x05: value = BitConverter.ToSingle(data, offset); offset += 4; return true;
            case 0x06: value = BitConverter.ToDouble(data, offset); offset += 8; return true;
            case 0x09: offset += 8; return true;   // UDim
            case 0x0A: offset += 16; return true;  // UDim2
            case 0x0E: offset += 4; return true;   // BrickColor
            case 0x0F: offset += 12; return true;  // Color3
            case 0x10: offset += 8; return true;   // Vector2
            case 0x11: offset += 12; return true;  // Vector3
            case 0x17: offset += 4 + (int)BitConverter.ToUInt32(data, offset) * 12; return true; // NumberSequence
            case 0x19: offset += 4 + (int)BitConverter.ToUInt32(data, offset) * 20; return true; // ColorSequence
            case 0x1B: offset += 8; return true;   // NumberRange
            case 0x1C: offset += 16; return true;  // Rect
            default: return false;
        }
    }

    private static uint ReadUInt(byte[] data, ref int offset) {
        uint value = BitConverter.ToUInt32(data, offset);
        offset += 4;
        return value;
    }

    private static string ReadString(byte[] data, ref int offset) {
        int length = (int)ReadUInt(data, ref offset);
        string value = Encoding.UTF8.GetString(data, offset, length);
        offset += length;
        return value;
    }
}
