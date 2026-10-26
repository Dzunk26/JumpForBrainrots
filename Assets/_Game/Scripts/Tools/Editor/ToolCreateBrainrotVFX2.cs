using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Pack: prefab trong pack VFX (bo phan nam duoi dat) | Orbit: sprite bay vong quanh than | Rise: sprite bay len quanh than
// Twinkle: lap lanh trong vo bao quanh than | Ribbon: vet sang (trail) bay vong quanh than, nghieng duoc nhu quy dao nguyen tu
public enum BrainrotVFX2Kind {
    Pack,
    Orbit,
    Rise,
    Twinkle,
    Ribbon
}

// 1 lop hieu ung cua VFX2, kich thuoc / vi tri tinh theo model:
// height: x chieu cao model (0 = chan, 1 = dinh dau) | radius: x ban kinh than | band, size: x don vi model
public class BrainrotVFX2Layer {
    public string label;
    public BrainrotVFX2Kind kind;
    public string assetPath;
    public int frameMin = -1;
    public int frameMax = -1;
    public Color colorA = Color.white;
    public Color colorB = Color.white;
    public bool isRainbow;
    public int count = 8;
    public float height = 0.5f;
    public float radius = 1.2f;
    public float band = 0.12f;
    public float speed = 0.25f;
    public float size = 0.12f;
    public float spin;
    public float drift;
    public float distance = 0.9f;
    public Vector2 lifetime;
    public bool isGrow;
    public Vector3 tilt;
    public float fitSize = 1.3f;

    public BrainrotVFX2Layer(string label, BrainrotVFX2Kind kind, string assetPath, int count) {
        this.label = label;
        this.kind = kind;
        this.assetPath = assetPath;
        this.count = count;
    }

    public BrainrotVFX2Layer Frame(int frame) {
        return Frames(frame, frame);
    }

    public BrainrotVFX2Layer Frames(int min, int max) {
        frameMin = min;
        frameMax = max;
        return this;
    }

    public BrainrotVFX2Layer At(float height, float radius, float band) {
        this.height = height;
        this.radius = radius;
        this.band = band;
        return this;
    }

    public BrainrotVFX2Layer Speed(float speed) {
        this.speed = speed;
        return this;
    }

    public BrainrotVFX2Layer Size(float size) {
        this.size = size;
        return this;
    }

    public BrainrotVFX2Layer Tint(string colorA, string colorB) {
        this.colorA = BrainrotVFX2Recipes.HexColor(colorA);
        this.colorB = BrainrotVFX2Recipes.HexColor(colorB);
        return this;
    }

    public BrainrotVFX2Layer Rainbow() {
        isRainbow = true;
        return this;
    }

    public BrainrotVFX2Layer Spin(float degreesPerSecond) {
        spin = degreesPerSecond;
        return this;
    }

    public BrainrotVFX2Layer Tilt(float x, float z) {
        tilt = new Vector3(x, 0f, z);
        return this;
    }

    public BrainrotVFX2Layer Drift(float drift) {
        this.drift = drift;
        return this;
    }

    public BrainrotVFX2Layer Life(float min, float max) {
        lifetime = new Vector2(min, max);
        return this;
    }

    public BrainrotVFX2Layer Distance(float distance) {
        this.distance = distance;
        return this;
    }

    public BrainrotVFX2Layer Grow() {
        isGrow = true;
        return this;
    }

    public BrainrotVFX2Layer Fit(float fitSize) {
        this.fitSize = fitSize;
        return this;
    }
}

public class BrainrotVFX2Recipe {
    public BrainrotVFXTheme theme;
    public List<BrainrotVFX2Layer> layers = new List<BrainrotVFX2Layer>();
}

// cong thuc VFX2 rieng cho tung brainrot (key = ten prefab model)
// khong dung vong aura / magic circle / de galaxy duoi chan, chi dung hieu ung bao quanh than
// rarity cang cao cang nhieu lop: Basic 2 -> Rare 3 -> Epic 4 -> Legendary 5 -> Mythic 6 -> Secret / Celestial 7 -> Divine 8 -> Infinity 9
public static class BrainrotVFX2Recipes {
    private static string ETFX_MAT = "Assets/Epic Toon FX/Materials/";
    private static string ETFX = "Assets/Epic Toon FX/Prefabs/";
    private static string HOVL = "Assets/Hovl Studio/Magic effects pack/Prefabs/";
    private static string VOXEL = "Assets/Voxel Arsenal/Prefabs/";
    private static string SCIFI = "Assets/Archanor/Sci-Fi Arsenal/Sci-Fi Effects/Prefabs/";
    private static string CFXR = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/";

    // sprite sheet: FRUIT / SLICE 3x3 = chuoi, dau, cam, dua hau, tao, le, dua, bi ngo, nho
    public static string FRUIT = ETFX_MAT + "Misc/Unsorted/fruit";
    public static string SLICE = ETFX_MAT + "Misc/Unsorted/fruitslices";
    public static string LEAF = ETFX_MAT + "Misc/Unsorted/leaf_3x3_AB";
    public static string CONFETTI = ETFX_MAT + "Misc/Unsorted/confetti_AB";
    public static string NOTES = ETFX_MAT + "Misc/Unsorted/notes_AB";
    public static string COINS = ETFX_MAT + "Misc/Unsorted/coins_AB";
    public static string FEATHER = ETFX_MAT + "Misc/Unsorted/feather_AB";
    public static string CLOUD = ETFX_MAT + "Basics/cloud_2x2_soft_AB";
    public static string SNOW = ETFX_MAT + "Misc/Unsorted/snowflake_AB";
    public static string STAR_STICKER = ETFX_MAT + "Misc/Stars/star_outlined_colored_AB";
    public static string STAR = ETFX_MAT + "Misc/Stars/star_ADD";
    public static string HEART = ETFX_MAT + "Misc/Hearts/heart_AB";
    public static string BUBBLE = ETFX_MAT + "Misc/Bubbles/bubble_round2_ADD";
    public static string SOAP_BUBBLE = ETFX_MAT + "Misc/Bubbles/bubble_soap_ADD";
    public static string DROP = ETFX_MAT + "Misc/Liquid/drop_AB";
    public static string RING = ETFX_MAT + "Basics/ring_ADD";
    public static string GLOW = ETFX_MAT + "Basics/glow1_ADD";
    public static string SPARKLE = ETFX_MAT + "Basics/sparkle_ADD";
    public static string SPARKLE_THIN = ETFX_MAT + "Basics/sparkle2_ADD";
    public static string LIGHTNING = ETFX_MAT + "Misc/Lightning/lightning1_ADD";
    public static string EMOJI_COOL = ETFX_MAT + "Misc/Emojis/em_cool";
    public static string EMOJI_KISS = ETFX_MAT + "Misc/Emojis/em_kissyface";
    public static string EMOJI_STAR = ETFX_MAT + "Misc/Emojis/em_starstruck";
    public static string EMOJI_HEART = ETFX_MAT + "Misc/Emojis/em_heart";
    public static string EMOJI_CUTE = ETFX_MAT + "Misc/Emojis/em_cute";
    public static string EMOJI_HAPPY = ETFX_MAT + "Misc/Emojis/em_happy";
    public static string TRAIL_SOURCE = "Assets/Hovl Studio/Magic effects pack/Materials/Trail67";

    private static Dictionary<string, int> SHEET_TILES = new Dictionary<string, int> {
        { FRUIT, 3 }, { SLICE, 3 }, { LEAF, 3 }, { CONFETTI, 3 },
        { NOTES, 2 }, { COINS, 2 }, { FEATHER, 2 }, { CLOUD, 2 }
    };

    public static int GetTiles(string materialPath) {
        return SHEET_TILES.TryGetValue(materialPath, out int tiles) ? tiles : 1;
    }

    public static Color HexColor(string hex) {
        if (string.IsNullOrEmpty(hex)) return Color.white;

        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
    }

    public static Dictionary<string, BrainrotVFX2Recipe> CreateAll() {
        Dictionary<string, BrainrotVFX2Recipe> recipes = new Dictionary<string, BrainrotVFX2Recipe>();

        // ===== Basic: 1 hieu ung dac trung + 1 lop lap lanh =====

        // ech + lop xe: bong xa phong bay len + dom dom xanh
        Add(recipes, "Boneca Ambalabu", BrainrotVFXTheme.Nature,
            Rise("Soap Bubbles", SOAP_BUBBLE, 7).At(0.1f, 1f, 0.08f).Speed(0.3f).Size(0.11f),
            Twinkle("Fireflies", GLOW, 10).Tint("#B6FF5C", "#FFF27A").Size(0.07f).Life(1.8f, 2.8f).Drift(0.12f));

        // nguoi cay rung: la xanh xoay quanh + lap lanh vang chanh
        Add(recipes, "Brr Brr Patapim", BrainrotVFXTheme.Nature,
            Orbit("Leaf Swirl", LEAF, 9).Tint("#4FD35B", "#C2F04A").At(0.5f, 1.25f, 0.22f).Speed(0.22f).Size(0.12f).Spin(90f),
            Twinkle("Forest Sparkle", SPARKLE_THIN, 8).Tint("#E8FF8A", "#FFFFFF").Size(0.09f));

        // ninja ca phe: phi tieu ngoi sao xoay nhanh + hoi ca phe boc len tu dau
        Add(recipes, "Cappuccino Assassino", BrainrotVFXTheme.Steam,
            Orbit("Shuriken Stars", STAR, 4).Tint("#E6ECF5", "#AEB8CC").At(0.55f, 1.3f, 0.04f).Speed(0.45f).Size(0.09f).Spin(720f),
            Rise("Coffee Steam", CLOUD, 6).Tint("#F3E2C7AA", "#C89B6D88").At(0.75f, 0.45f, 0.06f).Speed(0.2f).Distance(0.45f).Size(0.2f).Spin(20f));

        // voi xuong rong sa mac: cat vang lap lanh xoay nguoc + dom sang am bay len
        Add(recipes, "Lirili Larila", BrainrotVFXTheme.Sparkle,
            Orbit("Sand Sparkles", SPARKLE_THIN, 10).Tint("#FFD36B", "#FFB347").At(0.45f, 1.2f, 0.28f).Speed(-0.18f).Size(0.08f).Spin(60f),
            Rise("Desert Glow", GLOW, 8).Tint("#FFC86B", "#9BE36B").Speed(0.25f).Size(0.07f));

        // ca map xanh: giot nuoc bay vong + lap lanh xanh bien
        Add(recipes, "Tralalero Tralala", BrainrotVFXTheme.Water,
            Orbit("Water Drops", DROP, 8).Tint("#7FD8FF", "#C6F3FF").At(0.5f, 1.25f, 0.12f).Speed(0.3f).Size(0.08f),
            Twinkle("Ocean Sparkle", SPARKLE, 8).Tint("#8FE3FF", "#FFFFFF").Size(0.08f));

        // khuc go danh trong: not nhac cam go + lap lanh theo nhip
        Add(recipes, "Tung Sahur", BrainrotVFXTheme.Rhythm,
            Orbit("Drum Notes", NOTES, 6).Tint("#FFB347", "#FFE07A").At(0.6f, 1.25f, 0.2f).Speed(0.25f).Size(0.12f),
            Twinkle("Beat Sparkle", SPARKLE, 8).Tint("#FFD27A", "#FFFFFF").Size(0.08f));

        // ===== Rare: 3 lop =====

        // ca sau may bay: may trang troi + vet phan luc xanh la + tia lua dong co
        Add(recipes, "Bombardiro Crocodilo", BrainrotVFXTheme.Sparkle,
            Orbit("Sky Clouds", CLOUD, 7).Tint("#FFFFFFDD", "#DDF1FFDD").At(0.45f, 1.25f, 0.14f).Speed(0.12f).Size(0.26f).Spin(10f),
            Ribbon("Jet Stream", 2).Tint("#7CFF6B", "#3FD0FF").At(0.5f, 1.4f, 0f).Speed(0.5f).Tilt(15f, 0f),
            Rise("Engine Sparks", SPARKLE_THIN, 8).Tint("#FFB347", "#FF6B3D").Speed(0.45f).Size(0.06f));

        // ngong may bay: long vu trang + vet gio + lap lanh troi
        Add(recipes, "Bombombini Gusini", BrainrotVFXTheme.Sparkle,
            Orbit("Goose Feathers", FEATHER, 8).Tint("#FFFFFF", "#FFF4D6").At(0.5f, 1.3f, 0.24f).Speed(0.18f).Size(0.13f).Spin(60f),
            Ribbon("Wind Trail", 2).Tint("#BFE8FF", "#FFFFFF").At(0.5f, 1.45f, 0f).Speed(-0.45f).Tilt(-12f, 0f),
            Twinkle("Sky Sparkle", SPARKLE_THIN, 10).Tint("#FFE58A", "#FFFFFF").Size(0.08f));

        // voi dua: qua dua bay vong + voi phun nuoc + la dua
        Add(recipes, "Cocofanto Elefanto", BrainrotVFXTheme.Nature,
            Orbit("Coconuts", FRUIT, 5).Frame(6).At(0.5f, 1.3f, 0.12f).Speed(0.2f).Size(0.15f).Spin(25f),
            Rise("Trunk Splash", DROP, 9).Tint("#8FE3FF", "#D6F7FF").At(0.2f, 0.9f, 0.1f).Speed(0.45f).Size(0.06f),
            Orbit("Palm Leaves", LEAF, 6).Tint("#3CC85A", "#8BE04E").At(0.72f, 1.1f, 0.12f).Speed(-0.15f).Size(0.11f).Spin(70f));

        // heo bom: tim hong + tia lua ngoi no + ngoi sao bung
        Add(recipes, "Svinina Bombardino", BrainrotVFXTheme.Sweet,
            Orbit("Piggy Hearts", HEART, 6).Tint("#FF8FC8", "#FF5FA2").At(0.55f, 1.25f, 0.15f).Speed(0.22f).Size(0.11f).Spin(15f),
            Rise("Fuse Sparks", SPARKLE_THIN, 12).Tint("#FFD34D", "#FF7A2E").At(0.3f, 0.9f, 0.1f).Speed(0.5f).Size(0.07f).Spin(120f),
            Twinkle("Pop Stars", STAR, 6).Tint("#FFE066", "#FFFFFF").Size(0.06f));

        // tom meo duoi nuoc: bong bong + sao cam + lap lanh bien
        Add(recipes, "Trippi Troppi", BrainrotVFXTheme.Water,
            Rise("Sea Bubbles", BUBBLE, 9).Tint("#8FF0FF", "#FFFFFF").Speed(0.3f).Size(0.08f),
            Orbit("Orange Stars", STAR_STICKER, 5).Tint("#FFB070", "#FFD08A").At(0.55f, 1.25f, 0.15f).Speed(0.25f).Size(0.1f).Spin(45f),
            Twinkle("Sea Sparkle", SPARKLE, 8).Tint("#7FE7FF", "#FFC27A").Size(0.07f));

        // ===== Epic: 4 lop =====

        // capybara trong qua dua: lat dua + bong bong suoi nong + la sen + dai song nuoc
        Add(recipes, "Burbaloni Luliloli", BrainrotVFXTheme.Water,
            Orbit("Coconut Slices", SLICE, 6).Frame(6).At(0.5f, 1.3f, 0.15f).Speed(0.2f).Size(0.13f).Spin(40f),
            Rise("Hot Spring Bubbles", BUBBLE, 9).Tint("#9FF3FF", "#FFFFFF").Speed(0.3f).Size(0.08f),
            Orbit("Lily Leaves", LEAF, 6).Tint("#4ED36B", "#B4F06A").At(0.75f, 1.1f, 0.1f).Speed(-0.2f).Size(0.1f).Spin(80f),
            Ribbon("Splash Ribbon", 2).Tint("#5FD8FF", "#B8F4FF").At(0.4f, 1.45f, 0f).Speed(0.4f).Tilt(10f, 0f));

        // quy co ca phe: tim hon + emoji hon gio + hoi espresso + kim tuyen hong
        Add(recipes, "Espresso Signora", BrainrotVFXTheme.Sweet,
            Orbit("Kiss Hearts", HEART, 7).Tint("#FF5F8F", "#FF9EC4").At(0.55f, 1.25f, 0.18f).Speed(0.2f).Size(0.11f).Spin(15f),
            Orbit("Kissy Emoji", EMOJI_KISS, 2).At(0.85f, 1.15f, 0.04f).Speed(-0.15f).Size(0.14f),
            Rise("Espresso Steam", CLOUD, 6).Tint("#F5E6D3AA", "#D9B48F88").At(0.78f, 0.45f, 0.06f).Speed(0.18f).Distance(0.45f).Size(0.2f).Spin(20f),
            Twinkle("Glam Sparkle", SPARKLE_THIN, 10).Tint("#FFC2E0", "#FFFFFF").Size(0.09f));

        // lac da tu lanh: bong tuyet + dai bang + tinh the + sao bang
        Add(recipes, "Frigo Camelo", BrainrotVFXTheme.Ice,
            Orbit("Snowflakes", SNOW, 8).Tint("#BFF2FF", "#FFFFFF").At(0.5f, 1.3f, 0.24f).Speed(0.18f).Size(0.12f).Spin(60f),
            Ribbon("Frost Ribbon", 2).Tint("#7FE0FF", "#E0FBFF").At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(18f, 0f),
            Twinkle("Ice Glint", SPARKLE, 12).Tint("#CFF6FF", "#FFFFFF").Size(0.08f),
            Orbit("Frost Stars", STAR, 5).Tint("#9BE7FF", "#FFFFFF").At(0.85f, 1f, 0.05f).Speed(-0.3f).Size(0.06f).Spin(120f));

        // dan choi kinh ram: dong vang + emoji ngau + lap lanh tien + dai bling
        Add(recipes, "Gangster Footera", BrainrotVFXTheme.Sparkle,
            Orbit("Gold Coins", COINS, 8).Tint("#FFD23F", "#FFB300").At(0.5f, 1.3f, 0.2f).Speed(0.28f).Size(0.1f).Spin(200f),
            Orbit("Cool Emoji", EMOJI_COOL, 2).At(0.85f, 1.15f, 0.04f).Speed(-0.18f).Size(0.14f),
            Rise("Cash Sparkle", SPARKLE_THIN, 10).Tint("#7DFF7A", "#FFE36B").Speed(0.4f).Size(0.06f),
            Ribbon("Bling Ribbon", 2).Tint("#FFD54A", "#FFF3B0").At(0.45f, 1.45f, 0f).Speed(0.5f).Tilt(-10f, 0f));

        // ca voi sat thu: dai song nghieng + giot nuoc + bong bong lo phun + anh bien
        Add(recipes, "Orcalero Orcala", BrainrotVFXTheme.Water,
            Ribbon("Wave Ribbons", 3).Tint("#3FA9FF", "#9AE6FF").At(0.5f, 1.4f, 0f).Speed(0.35f).Tilt(20f, 0f),
            Orbit("Water Drops", DROP, 8).Tint("#8AD8FF", "#E0F7FF").At(0.5f, 1.2f, 0.24f).Speed(-0.25f).Size(0.07f),
            Rise("Blowhole Bubbles", BUBBLE, 8).Tint("#BFF4FF", "#FFFFFF").Speed(0.35f).Size(0.07f),
            Twinkle("Sea Glint", SPARKLE_THIN, 8).Tint("#E6FBFF", "#7FD3FF").Size(0.08f));

        // chuot pho mai: mieng pho mai + lap lanh vang + vun vang + dai rau meo
        Add(recipes, "Tim Cheese", BrainrotVFXTheme.Sweet,
            Orbit("Cheese Bits", SLICE, 7).Frame(7).Tint("#FFE27A", "#FFF0A0").At(0.5f, 1.25f, 0.2f).Speed(0.22f).Size(0.12f).Spin(50f),
            Twinkle("Cheesy Sparkle", SPARKLE, 10).Tint("#FFE45C", "#FFF8C2").Size(0.08f),
            Rise("Golden Crumbs", GLOW, 10).Tint("#FFD84D", "#FFB833").Speed(0.3f).Size(0.05f),
            Ribbon("Whisker Ribbon", 2).Tint("#FFE680", "#FFFFFF").At(0.6f, 1.4f, 0f).Speed(0.45f).Tilt(12f, 0f));

        // ===== Legendary: 5 lop =====

        // ballerina: 2 bo dai mua nghieng cheo + not nhac hong + tim + kim tuyen
        Add(recipes, "Ballerina Cappuccina", BrainrotVFXTheme.Sweet,
            Ribbon("Dance Ribbons", 3).Tint("#FF8FD0", "#FFC6E8").At(0.5f, 1.4f, 0f).Speed(0.4f).Tilt(25f, 0f),
            Ribbon("Dance Ribbons Cross", 2).Tint("#FFFFFF", "#FFB3DE").At(0.6f, 1.25f, 0f).Speed(-0.35f).Tilt(-25f, 10f),
            Orbit("Ballet Notes", NOTES, 6).Tint("#FF9AD5", "#FFD1EC").At(0.6f, 1.3f, 0.2f).Speed(0.2f).Size(0.11f),
            Rise("Tutu Hearts", HEART, 7).Tint("#FFB6DB", "#FF7FBF").Speed(0.3f).Size(0.07f).Spin(20f),
            Twinkle("Pink Glitter", SPARKLE_THIN, 14).Tint("#FFD6EE", "#FFFFFF").Size(0.09f));

        // chien binh chim: long do + long vang xoay nguoc + dai chien thang + than hong + tia sang vang
        Add(recipes, "Brri Brri Bicus Dicus", BrainrotVFXTheme.Fire,
            Orbit("Red Feathers", FEATHER, 6).Tint("#FF4A3D", "#FF8A3D").At(0.5f, 1.3f, 0.2f).Speed(0.22f).Size(0.13f).Spin(60f),
            Orbit("Gold Feathers", FEATHER, 5).Tint("#FFD23F", "#FFF07A").At(0.68f, 1.15f, 0.14f).Speed(-0.2f).Size(0.11f).Spin(60f),
            Ribbon("Victory Ribbon", 2).Tint("#FFD23F", "#FFF3B0").At(0.5f, 1.45f, 0f).Speed(0.5f).Tilt(15f, 0f),
            Rise("Embers", GLOW, 12).Tint("#FF9A3D", "#FFD25A").Speed(0.45f).Size(0.05f),
            Pack("Gold Sparks", HOVL + "Sparks/Sparks flashing yellow").Fit(1.3f));

        // khi chuoi: chuoi bay vong + lat chuoi xoay nguoc + la rung + dai vang + nang lap lanh
        Add(recipes, "Chimpanzini Bananini", BrainrotVFXTheme.Sweet,
            Orbit("Bananas", FRUIT, 6).Frame(0).At(0.5f, 1.3f, 0.15f).Speed(0.24f).Size(0.15f).Spin(35f),
            Orbit("Banana Slices", SLICE, 6).Frame(0).At(0.75f, 1.05f, 0.1f).Speed(-0.3f).Size(0.09f).Spin(90f),
            Orbit("Jungle Leaves", LEAF, 5).Tint("#3DCB55", "#9BE84B").At(0.32f, 1.2f, 0.1f).Speed(0.15f).Size(0.11f).Spin(70f),
            Ribbon("Banana Ribbon", 2).Tint("#FFE14D", "#FFF6B0").At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(-15f, 0f),
            Twinkle("Sunny Sparkle", SPARKLE, 12).Tint("#FFF07A", "#FFFFFF").Size(0.08f));

        // huou cao co dua hau + "celeste": lat dua hau + sao dan + dai sao + bui sao + suong sao
        Add(recipes, "Girafa Celeste", BrainrotVFXTheme.Cosmic,
            Orbit("Melon Slices", SLICE, 6).Frame(3).At(0.45f, 1.3f, 0.15f).Speed(0.2f).Size(0.13f).Spin(40f),
            Orbit("Celeste Stars", STAR_STICKER, 6).At(0.8f, 1.15f, 0.12f).Speed(-0.22f).Size(0.1f).Spin(60f),
            Ribbon("Star Ribbon", 2).Tint("#7FD7FF", "#C9B6FF").At(0.6f, 1.45f, 0f).Speed(0.4f).Tilt(20f, 0f),
            Twinkle("Star Dust", STAR, 10).Tint("#BFE6FF", "#FFF3A6").Size(0.05f),
            Pack("Star Fog", ETFX + "Environment/Stars/StarFog/StarFogGreen").Fit(1.5f));

        // ===== Mythic: 6 lop =====

        // bach tuoc viet quat: qua viet quat + 2 bo dai xuc tu + bong muc + lap lanh + khien bong bong
        Add(recipes, "Blueberrinni Octopusini", BrainrotVFXTheme.Water,
            Orbit("Blueberries", FRUIT, 8).Frame(8).Tint("#7C9BFF", "#9AB0FF").At(0.5f, 1.3f, 0.2f).Speed(0.22f).Size(0.12f).Spin(30f),
            Ribbon("Tentacle Ribbons", 4).Tint("#4A7DFF", "#9FC4FF").At(0.4f, 1.35f, 0f).Speed(0.35f).Tilt(25f, 0f),
            Ribbon("Tentacle Ribbons Cross", 3).Tint("#8A6BFF", "#C3B5FF").At(0.6f, 1.25f, 0f).Speed(-0.3f).Tilt(-25f, 0f),
            Rise("Ink Bubbles", BUBBLE, 10).Tint("#9FC8FF", "#FFFFFF").Speed(0.3f).Size(0.07f),
            Twinkle("Berry Glint", SPARKLE_THIN, 12).Tint("#B8CCFF", "#FFFFFF").Size(0.08f),
            Pack("Bubble Shield", HOVL + "Magic shields/Magic shield blue").Fit(1.35f));

        // khi dua hau: dua hau to + lat dua xoay nguoc + dai xanh do + giot nuoc ep + lap lanh + tien xanh
        Add(recipes, "Gorillo Watermelondrillo", BrainrotVFXTheme.Nature,
            Orbit("Watermelons", FRUIT, 5).Frame(3).At(0.45f, 1.35f, 0.12f).Speed(0.18f).Size(0.17f).Spin(25f),
            Orbit("Melon Slices", SLICE, 7).Frame(3).At(0.75f, 1.15f, 0.1f).Speed(-0.28f).Size(0.1f).Spin(80f),
            Ribbon("Melon Ribbons", 3).Tint("#3FDB5A", "#FF5A6E").At(0.5f, 1.45f, 0f).Speed(0.4f).Tilt(18f, 0f),
            Rise("Juice Drops", DROP, 10).Tint("#FF6B81", "#FF9AA8").Speed(0.4f).Size(0.05f),
            Twinkle("Fresh Sparkle", SPARKLE, 12).Tint("#B8FF9A", "#FFFFFF").Size(0.08f),
            Pack("Green Fairies", VOXEL + "Environment/Fairies/FairiesGreen").Fit(1.4f));

        // quy ong mu cao - ao thuat gia: sao + dai phep + kim tuyen + vong hao quang tren mu + tia phep
        Add(recipes, "Matteo", BrainrotVFXTheme.Sparkle,
            Orbit("Magic Stars", STAR_STICKER, 7).At(0.55f, 1.3f, 0.2f).Speed(0.25f).Size(0.1f).Spin(90f),
            Ribbon("Magic Ribbons", 3).Tint("#B36BFF", "#FF7AE0").At(0.5f, 1.4f, 0f).Speed(0.42f).Tilt(22f, 0f),
            Rise("Confetti", CONFETTI, 12).Speed(0.35f).Size(0.07f).Spin(300f),
            Orbit("Top Hat Halo", GLOW, 14).Tint("#FFE07A", "#FFFFFF").At(1.08f, 0.45f, 0.02f).Speed(0.3f).Size(0.07f),
            Twinkle("Magic Glint", SPARKLE_THIN, 12).Tint("#E3C2FF", "#FFFFFF").Size(0.09f),
            Pack("Magic Sparks", HOVL + "Sparks/Sparks flashing pink").Fit(1.3f));

        // qua cam - than sam set: cam + not nhac + tia set chop + dai cam + lap lanh + dien quanh than
        Add(recipes, "Odin Din Din Dun", BrainrotVFXTheme.Electric,
            Orbit("Oranges", FRUIT, 6).Frame(2).At(0.5f, 1.3f, 0.15f).Speed(0.22f).Size(0.14f).Spin(40f),
            Orbit("Thunder Notes", NOTES, 6).Tint("#FFE14D", "#FFB347").At(0.75f, 1.15f, 0.14f).Speed(-0.25f).Size(0.1f),
            Twinkle("Thunder Flicker", LIGHTNING, 6).Tint("#FFF6A0", "#A8E8FF").Size(0.24f).Life(0.15f, 0.3f),
            Ribbon("Orange Ribbons", 3).Tint("#FF9A2E", "#FFD45C").At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(-18f, 0f),
            Twinkle("Citrus Sparkle", SPARKLE, 10).Tint("#FFD27A", "#FFFFFF").Size(0.08f),
            Pack("Electrified", CFXR + "Electric/CFXR Electrified 2").Fit(1.1f));

        // rua thanh long: canh hoa hong + la xanh + dai hong-xanh + than hong + lap lanh + khien hong
        Add(recipes, "Torrtuginni Dragonfrutini", BrainrotVFXTheme.Sweet,
            Orbit("Dragon Petals", LEAF, 8).Tint("#FF5FB8", "#FF9AD8").At(0.5f, 1.3f, 0.2f).Speed(0.22f).Size(0.12f).Spin(80f),
            Orbit("Green Leaves", LEAF, 5).Tint("#6BE05A", "#B8F05A").At(0.8f, 1.1f, 0.1f).Speed(-0.25f).Size(0.1f).Spin(80f),
            Ribbon("Dragon Ribbons", 3).Tint("#FF4FB0", "#9CFF5A").At(0.5f, 1.4f, 0f).Speed(0.4f).Tilt(20f, 0f),
            Rise("Dragon Embers", GLOW, 12).Tint("#FF6BC8", "#FFB0E0").Speed(0.4f).Size(0.05f),
            Twinkle("Shell Sparkle", SPARKLE_THIN, 12).Tint("#FFC2EA", "#C8FF9A").Size(0.08f),
            Pack("Pink Shield", HOVL + "Magic shields/Magic shield pink").Fit(1.35f));

        // ===== Secret: 7 lop =====

        // cua dau bep ao thuat: trai cay nau an + 2 bo dai phep + bong bong cua + vong sao tren mu + kim tuyen + khien tim
        Add(recipes, "Chef Crabracadabra", BrainrotVFXTheme.Shadow,
            Orbit("Chef Fruits", FRUIT, 8).Frames(0, 8).At(0.5f, 1.35f, 0.2f).Speed(0.22f).Size(0.12f).Spin(50f),
            Ribbon("Spell Ribbons", 3).Tint("#B06BFF", "#FF6BD0").At(0.5f, 1.4f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Spell Ribbons Cross", 2).Tint("#FF8A4D", "#FFD45C").At(0.55f, 1.3f, 0f).Speed(-0.4f).Tilt(-20f, 0f),
            Rise("Crab Bubbles", BUBBLE, 10).Tint("#FFD0B8", "#FFFFFF").Speed(0.3f).Size(0.07f),
            Orbit("Star Halo", STAR, 12).Tint("#FFE07A", "#FFB3F0").At(1.08f, 0.5f, 0.02f).Speed(0.35f).Size(0.06f).Spin(90f),
            Twinkle("Spell Sparkle", SPARKLE_THIN, 14).Tint("#E2B8FF", "#FFFFFF").Size(0.09f),
            Pack("Sparkle Shield", SCIFI + "Combat/Shield/SparkleShield/PurpleSparkleShield").Fit(1.35f));

        // so sach "JOB": not nhac giay + dong luong + dai giay + phao giay + y tuong lap lanh + tia sang trang
        Add(recipes, "Job Job Job Sahur", BrainrotVFXTheme.Rhythm,
            Orbit("Paper Notes", NOTES, 7).Tint("#FFFFFF", "#CFE3FF").At(0.55f, 1.3f, 0.2f).Speed(0.25f).Size(0.11f),
            Orbit("Pay Day Coins", COINS, 6).Tint("#FFD23F", "#FFB300").At(0.8f, 1.15f, 0.1f).Speed(-0.3f).Size(0.09f).Spin(180f),
            Ribbon("Paper Ribbons", 3).Tint("#FFFFFF", "#9CC8FF").At(0.5f, 1.4f, 0f).Speed(0.45f).Tilt(20f, 0f),
            Rise("Paper Confetti", CONFETTI, 10).Speed(0.35f).Size(0.06f).Spin(300f),
            Orbit("Happy Emoji", EMOJI_HAPPY, 2).At(0.35f, 1.3f, 0.04f).Speed(-0.18f).Size(0.13f),
            Twinkle("Idea Sparkle", SPARKLE, 12).Tint("#FFFFFF", "#BFE0FF").Size(0.08f),
            Pack("White Sparks", HOVL + "Sparks/Sparks flashing white").Fit(1.3f));

        // bo sao tho: vanh dai sao tho nghieng quanh than + sao tren vanh + giot sua + sao choi + lap lanh + suong sao
        Add(recipes, "La Vacca Saturno Saturnita", BrainrotVFXTheme.Cosmic,
            Orbit("Saturn Ring", GLOW, 40).Tint("#FFD9A0", "#FFB3E6").At(0.45f, 1.6f, 0.02f).Speed(0.12f).Size(0.05f).Tilt(20f, 0f),
            Orbit("Saturn Ring Stars", STAR, 12).Tint("#FFFFFF", "#FFE7A3").At(0.45f, 1.75f, 0.03f).Speed(0.12f).Size(0.05f).Spin(90f).Tilt(20f, 0f),
            Orbit("Milk Drops", DROP, 5).Tint("#FFFFFF", "#F0F0FF").At(0.62f, 1.2f, 0.2f).Speed(-0.2f).Size(0.08f),
            Ribbon("Comet Ribbons", 2).Tint("#9C7BFF", "#7FD8FF").At(0.5f, 1.35f, 0f).Speed(0.4f).Tilt(-30f, 0f),
            Twinkle("Space Twinkle", SPARKLE_THIN, 14).Tint("#D6C8FF", "#FFFFFF").Size(0.09f),
            Rise("Moon Dust", GLOW, 10).Tint("#C8B6FF", "#FFE7A3").Speed(0.25f).Size(0.04f),
            Pack("Star Fog", ETFX + "Environment/Stars/StarFog/StarFogBlue").Fit(1.6f));

        // dien thoai wifi: song tin hieu tu dau + tia set chop + dai du lieu + hat du lieu + lap lanh + dien
        Add(recipes, "Pot Hotspot", BrainrotVFXTheme.Electric,
            Rise("Signal Waves", RING, 4).Tint("#5CF2FF", "#7DFF9A").At(0.95f, 0f, 0f).Speed(0.25f).Distance(0.35f).Size(0.3f).Grow(),
            Twinkle("Bolt Flicker", LIGHTNING, 5).Tint("#7DF9FF", "#C6FFDA").Size(0.22f).Life(0.15f, 0.3f),
            Ribbon("Data Ribbons", 3).Tint("#3DF5FF", "#7DFF9A").At(0.5f, 1.4f, 0f).Speed(0.55f).Tilt(15f, 0f),
            Orbit("Data Bits", GLOW, 16).Tint("#6BFFE0", "#3DB8FF").At(0.5f, 1.2f, 0.32f).Speed(0.35f).Size(0.04f),
            Twinkle("Pixel Sparkle", SPARKLE, 10).Tint("#BFFFF6", "#FFFFFF").Size(0.08f),
            Rise("Signal Glow", GLOW, 8).Tint("#5CF2FF", "#FFFFFF").Speed(0.35f).Size(0.04f),
            Pack("Electrified", CFXR + "Electric/CFXR Electrified 3").Fit(1.1f));

        // lon soda: bot ga day dac + emoji ngau + dai soda + giot soda + lap lanh + tia xanh + vung sang
        Add(recipes, "Swag Soda", BrainrotVFXTheme.Sparkle,
            Rise("Soda Fizz", BUBBLE, 18).Tint("#BFF0FF", "#FFFFFF").At(0.1f, 0.8f, 0.1f).Speed(0.5f).Size(0.045f),
            Orbit("Cool Emoji", EMOJI_COOL, 3).At(0.6f, 1.3f, 0.15f).Speed(0.2f).Size(0.14f),
            Ribbon("Soda Ribbons", 3).Tint("#3D8BFF", "#FFFFFF").At(0.5f, 1.4f, 0f).Speed(0.45f).Tilt(-20f, 0f),
            Orbit("Soda Splash", DROP, 8).Tint("#7FD0FF", "#C9F0FF").At(0.85f, 1.1f, 0.1f).Speed(-0.3f).Size(0.06f),
            Twinkle("Swag Sparkle", SPARKLE_THIN, 14).Tint("#FFFFFF", "#9FD6FF").Size(0.09f),
            Orbit("Fizz Stars", STAR, 6).Tint("#9FD6FF", "#FFFFFF").At(0.3f, 1.25f, 0.08f).Speed(0.35f).Size(0.05f).Spin(180f),
            Pack("Blue Sparks", HOVL + "Sparks/Sparks flashing blue").Fit(1.3f));

        // tralalerito xanh ngoc: not nhac nuoc + 2 bo dai song + bong xa phong + lap lanh + giot nuoc + khien mem
        Add(recipes, "Los Tralaleritos", BrainrotVFXTheme.Rhythm,
            Orbit("Splash Notes", NOTES, 7).Tint("#5FF0FF", "#B8FFF6").At(0.55f, 1.3f, 0.2f).Speed(0.25f).Size(0.11f),
            Ribbon("Wave Ribbons", 3).Tint("#2FD6FF", "#9AF7FF").At(0.45f, 1.4f, 0f).Speed(0.4f).Tilt(22f, 0f),
            Ribbon("Wave Ribbons Cross", 2).Tint("#FFFFFF", "#7FEFFF").At(0.6f, 1.3f, 0f).Speed(-0.4f).Tilt(-18f, 0f),
            Rise("Bubbles", SOAP_BUBBLE, 8).Speed(0.3f).Size(0.08f),
            Twinkle("Aqua Sparkle", SPARKLE, 12).Tint("#B5FBFF", "#FFFFFF").Size(0.08f),
            Orbit("Aqua Drops", DROP, 6).Tint("#7FEFFF", "#E0FFFF").At(0.85f, 1.1f, 0.08f).Speed(-0.3f).Size(0.06f),
            Pack("Soft Shield", SCIFI + "Combat/Shield/SoftShield/SoftShieldBlue").Fit(1.35f));

        // ===== Celestial: 7 lop, phong cach vu tru (quy dao nguyen tu, hao quang) =====

        // rong vang: 2 bo dai lua cheo + than lua + sao lua + hao quang mat troi + lap lanh vang + suong sao vang
        Add(recipes, "Dragon Cannelloni", BrainrotVFXTheme.Fire,
            Ribbon("Fire Ribbons", 3).Tint("#FF7A1A", "#FFD23F").At(0.5f, 1.4f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Fire Ribbons Cross", 3).Tint("#FFB347", "#FF4A1A").At(0.5f, 1.4f, 0f).Speed(-0.45f).Tilt(-25f, 0f),
            Rise("Dragon Embers", GLOW, 16).Tint("#FF8A1A", "#FFE14D").Speed(0.5f).Size(0.05f),
            Orbit("Fire Stars", STAR_STICKER, 7).Tint("#FFB070", "#FFD08A").At(0.55f, 1.3f, 0.2f).Speed(0.25f).Size(0.1f).Spin(90f),
            Orbit("Sun Halo", GLOW, 16).Tint("#FFD23F", "#FF8A1A").At(1.08f, 0.5f, 0.02f).Speed(0.3f).Size(0.07f),
            Twinkle("Golden Twinkle", SPARKLE_THIN, 14).Tint("#FFE58A", "#FFFFFF").Size(0.09f),
            Pack("Star Fog", ETFX + "Environment/Stars/StarFog/StarFogYellow").Fit(1.6f));

        // nha truong hoc: sao dan vo + 2 bo dai but mau cau vong + phao giay + emoji me sao + lap lanh + tien xanh
        Add(recipes, "Esok Sekolah", BrainrotVFXTheme.Cosmic,
            Orbit("Star Stickers", STAR_STICKER, 8).At(0.55f, 1.3f, 0.2f).Speed(0.22f).Size(0.11f).Spin(60f),
            Ribbon("Crayon Ribbons", 3).Rainbow().At(0.5f, 1.4f, 0f).Speed(0.4f).Tilt(25f, 0f),
            Ribbon("Crayon Ribbons Cross", 3).Rainbow().At(0.5f, 1.4f, 0f).Speed(-0.4f).Tilt(-25f, 15f),
            Rise("Confetti", CONFETTI, 14).Speed(0.35f).Size(0.065f).Spin(300f),
            Orbit("Starstruck Emoji", EMOJI_STAR, 2).At(0.95f, 1f, 0.04f).Speed(-0.18f).Size(0.13f),
            Twinkle("Cosmic Twinkle", SPARKLE_THIN, 14).Tint("#C8E6FF", "#FFFFFF").Size(0.09f),
            Pack("Blue Fairies", VOXEL + "Environment/Fairies/FairiesBlue").Fit(1.4f));

        // voi dau tay: dau tay + lat dau xoay nguoc + dai do + dai la + tim ngot bay len + vong sao + duong lap lanh
        Add(recipes, "Strawberry Elephant", BrainrotVFXTheme.Sweet,
            Orbit("Strawberries", FRUIT, 7).Frame(1).At(0.5f, 1.3f, 0.15f).Speed(0.22f).Size(0.14f).Spin(35f),
            Orbit("Strawberry Slices", SLICE, 7).Frame(1).At(0.8f, 1.1f, 0.1f).Speed(-0.28f).Size(0.09f).Spin(80f),
            Ribbon("Berry Ribbons", 3).Tint("#FF3D5A", "#FF9AB0").At(0.5f, 1.45f, 0f).Speed(0.42f).Tilt(22f, 0f),
            Ribbon("Leaf Ribbons", 2).Tint("#5BE06B", "#C8FF9A").At(0.5f, 1.35f, 0f).Speed(-0.38f).Tilt(-22f, 0f),
            Rise("Sweet Hearts", HEART, 9).Tint("#FF6B8A", "#FFB3C6").Speed(0.3f).Size(0.07f).Spin(20f),
            Orbit("Star Halo", STAR, 14).Tint("#FFE07A", "#FFC2D6").At(1.08f, 0.5f, 0.02f).Speed(0.3f).Size(0.06f).Spin(90f),
            Twinkle("Sugar Sparkle", SPARKLE_THIN, 14).Tint("#FFD6E0", "#FFFFFF").Size(0.09f));

        // ca map hong no: giot nuoc hong + 2 bo dai bien + bong xa phong + emoji tim + lap lanh hong + tien hong
        Add(recipes, "Tralalita Tralala", BrainrotVFXTheme.Sweet,
            Orbit("Pink Drops", DROP, 8).Tint("#FF9AD5", "#FFD0EA").At(0.5f, 1.3f, 0.2f).Speed(0.25f).Size(0.07f),
            Ribbon("Ocean Ribbons", 3).Tint("#FF7AC8", "#7FE0FF").At(0.5f, 1.4f, 0f).Speed(0.4f).Tilt(25f, 0f),
            Ribbon("Ocean Ribbons Cross", 3).Tint("#FFFFFF", "#FFB3E0").At(0.5f, 1.35f, 0f).Speed(-0.4f).Tilt(-20f, 0f),
            Rise("Love Bubbles", SOAP_BUBBLE, 10).Speed(0.3f).Size(0.08f),
            Orbit("Heart Emoji", EMOJI_HEART, 4).At(0.8f, 1.15f, 0.1f).Speed(-0.2f).Size(0.1f),
            Twinkle("Pink Twinkle", SPARKLE_THIN, 14).Tint("#FFC2E6", "#FFFFFF").Size(0.09f),
            Pack("Pink Fairies", VOXEL + "Environment/Fairies/FairiesPink").Fit(1.4f));

        // ===== Divine: 8 lop, vang thanh khiet + hao quang tren dau =====

        // ca sau trai cay: bao trai cay + lat trai cay + dai vang thanh + dai xanh + hao quang + anh sang bay len + kim tuyen vang + lap lanh vang
        Add(recipes, "Glorbo Fruttodrillo", BrainrotVFXTheme.Nature,
            Orbit("Fruit Storm", FRUIT, 10).Frames(0, 8).At(0.5f, 1.35f, 0.2f).Speed(0.22f).Size(0.13f).Spin(45f),
            Orbit("Fruit Slices", SLICE, 8).Frames(0, 8).At(0.8f, 1.1f, 0.1f).Speed(-0.3f).Size(0.09f).Spin(90f),
            Ribbon("Holy Ribbons", 3).Tint("#FFE14D", "#FFFFFF").At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Croc Ribbons", 3).Tint("#5BE06B", "#C8FF9A").At(0.5f, 1.4f, 0f).Speed(-0.4f).Tilt(-25f, 0f),
            Orbit("Holy Halo", GLOW, 18).Tint("#FFE58A", "#FFFFFF").At(1.1f, 0.5f, 0.02f).Speed(0.3f).Size(0.07f),
            Rise("Holy Light", SPARKLE_THIN, 14).Tint("#FFF3B0", "#FFFFFF").Speed(0.45f).Size(0.07f),
            Twinkle("Gold Glitter", SPARKLE, 14).Tint("#FFD84D", "#FFFFFF").Size(0.08f),
            Pack("Gold Sparkle", ETFX + "Interactive/Loot/ItemSparkle/ItemSparkleYellow").Fit(1.5f));

        // gau truc chuoi: chuoi + la tre + dai trang + dai vang + hao quang + emoji de thuong + anh sang + tien vang
        Add(recipes, "Pandaccini Bananini", BrainrotVFXTheme.Sweet,
            Orbit("Bananas", FRUIT, 7).Frame(0).At(0.5f, 1.35f, 0.18f).Speed(0.22f).Size(0.14f).Spin(35f),
            Orbit("Bamboo Leaves", LEAF, 8).Tint("#5BE06B", "#B8F05A").At(0.75f, 1.15f, 0.14f).Speed(-0.25f).Size(0.11f).Spin(80f),
            Ribbon("White Ribbons", 2).Tint("#FFFFFF", "#E8F4FF").At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Gold Ribbons", 2).Tint("#FFD23F", "#FFF3B0").At(0.5f, 1.4f, 0f).Speed(-0.45f).Tilt(-25f, 0f),
            Orbit("Holy Halo", GLOW, 18).Tint("#FFF3B0", "#FFFFFF").At(1.1f, 0.5f, 0.02f).Speed(0.3f).Size(0.07f),
            Orbit("Cute Emoji", EMOJI_CUTE, 2).At(0.3f, 1.3f, 0.04f).Speed(0.18f).Size(0.13f),
            Rise("Holy Light", SPARKLE_THIN, 14).Tint("#FFE58A", "#FFFFFF").Speed(0.45f).Size(0.07f),
            Pack("Yellow Fairies", VOXEL + "Environment/Fairies/FairiesYellow").Fit(1.4f));

        // ho trai cay: cam + lat cam + dai cam + dai trang vang + than + vong sao + kim tuyen + tia vang
        Add(recipes, "Tigroligre Frutonni", BrainrotVFXTheme.Fire,
            Orbit("Oranges", FRUIT, 7).Frame(2).At(0.5f, 1.35f, 0.18f).Speed(0.22f).Size(0.14f).Spin(40f),
            Orbit("Orange Slices", SLICE, 8).Frame(2).At(0.78f, 1.12f, 0.1f).Speed(-0.3f).Size(0.09f).Spin(90f),
            Ribbon("Tiger Ribbons", 3).Tint("#FF8A1A", "#FFD23F").At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Tiger Ribbons Cross", 3).Tint("#FFFFFF", "#FFE58A").At(0.5f, 1.4f, 0f).Speed(-0.45f).Tilt(-25f, 0f),
            Rise("Tiger Embers", GLOW, 14).Tint("#FF8A1A", "#FFE14D").Speed(0.45f).Size(0.05f),
            Orbit("Star Halo", STAR, 14).Tint("#FFE14D", "#FFFFFF").At(1.1f, 0.5f, 0.02f).Speed(0.32f).Size(0.06f).Spin(90f),
            Twinkle("Gold Glitter", SPARKLE, 14).Tint("#FFD84D", "#FFFFFF").Size(0.08f),
            Pack("Gold Sparks", HOVL + "Sparks/Sparks flashing yellow").Fit(1.35f));

        // ===== Infinity: 9 lop, cau vong =====

        // ngua piano: 2 tang not nhac cau vong + 3 dai cau vong + phao giay + hao quang cau vong + kim tuyen + lap lanh cau vong
        Add(recipes, "Cavallo Virtuoso", BrainrotVFXTheme.Rhythm,
            Orbit("Rainbow Notes", NOTES, 10).Rainbow().At(0.5f, 1.35f, 0.24f).Speed(0.22f).Size(0.12f),
            Orbit("Rainbow Notes Inner", NOTES, 8).Rainbow().At(0.85f, 1.15f, 0.1f).Speed(-0.3f).Size(0.09f),
            Ribbon("Rainbow Ribbons", 3).Rainbow().At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Rainbow Ribbons Cross", 3).Rainbow().At(0.5f, 1.45f, 0f).Speed(-0.45f).Tilt(-25f, 0f),
            Ribbon("Rainbow Ribbons Side", 2).Rainbow().At(0.5f, 1.3f, 0f).Speed(0.4f).Tilt(0f, 30f),
            Rise("Rainbow Confetti", CONFETTI, 16).Speed(0.35f).Size(0.065f).Spin(300f),
            Orbit("Rainbow Halo", GLOW, 22).Rainbow().At(1.1f, 0.55f, 0.02f).Speed(0.35f).Size(0.07f),
            Twinkle("Rainbow Glitter", SPARKLE_THIN, 18).Rainbow().Size(0.09f),
            Pack("Rainbow Sparkle", ETFX + "Interactive/Loot/ItemSparkle/ItemSparkleRainbow").Fit(1.5f));

        // dua oranguntan: bao trai cay nhiet doi + lat trai cay + la co + 3 dai cau vong + hao quang sao + anh sang cau vong + lap lanh mem
        Add(recipes, "Orangutini Ananassini", BrainrotVFXTheme.Nature,
            Orbit("Tropical Fruits", FRUIT, 12).Frames(0, 8).At(0.5f, 1.35f, 0.22f).Speed(0.22f).Size(0.13f).Spin(45f),
            Orbit("Fruit Slices", SLICE, 10).Frames(0, 8).At(0.82f, 1.12f, 0.1f).Speed(-0.3f).Size(0.09f).Spin(90f),
            Orbit("Palm Leaves", LEAF, 8).Tint("#3DCB55", "#B8F05A").At(0.3f, 1.25f, 0.1f).Speed(0.18f).Size(0.11f).Spin(80f),
            Ribbon("Rainbow Ribbons", 3).Rainbow().At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Rainbow Ribbons Cross", 3).Rainbow().At(0.5f, 1.45f, 0f).Speed(-0.45f).Tilt(-25f, 0f),
            Orbit("Rainbow Star Halo", STAR, 16).Rainbow().At(1.1f, 0.55f, 0.02f).Speed(0.35f).Size(0.06f).Spin(90f),
            Rise("Rainbow Glow", GLOW, 16).Rainbow().Speed(0.45f).Size(0.05f),
            Twinkle("Rainbow Glitter", SPARKLE_THIN, 18).Rainbow().Size(0.09f),
            Pack("Soft Rainbow Sparkle", ETFX + "Interactive/Loot/ItemSparkleSoft/ItemSparkleSoftRainbow").Fit(1.5f));

        // hong hac dau tay: dau tay + long hong + 3 dai cau vong + tim bay len + bong xa phong + hao quang + lap lanh + khien hong
        Add(recipes, "Strawberrelli Flamingelli", BrainrotVFXTheme.Sweet,
            Orbit("Strawberries", FRUIT, 9).Frame(1).At(0.5f, 1.35f, 0.2f).Speed(0.22f).Size(0.13f).Spin(35f),
            Orbit("Flamingo Feathers", FEATHER, 10).Tint("#FF7AC0", "#FFB3DA").At(0.8f, 1.15f, 0.12f).Speed(-0.28f).Size(0.11f).Spin(60f),
            Ribbon("Rainbow Ribbons", 3).Rainbow().At(0.5f, 1.45f, 0f).Speed(0.45f).Tilt(25f, 0f),
            Ribbon("Rainbow Ribbons Cross", 3).Rainbow().At(0.5f, 1.45f, 0f).Speed(-0.45f).Tilt(-25f, 0f),
            Ribbon("Rainbow Ribbons Side", 2).Rainbow().At(0.5f, 1.3f, 0f).Speed(0.4f).Tilt(0f, 30f),
            Rise("Love Hearts", HEART, 12).Tint("#FF6BA8", "#FFC2DE").Speed(0.32f).Size(0.07f).Spin(20f),
            Orbit("Rainbow Halo", GLOW, 22).Rainbow().At(1.1f, 0.55f, 0.02f).Speed(0.35f).Size(0.07f),
            Twinkle("Rainbow Glitter", SPARKLE_THIN, 18).Rainbow().Size(0.09f),
            Pack("Pink Shield", HOVL + "Magic shields/Magic shield pink").Fit(1.4f));

        return recipes;
    }

    private static void Add(Dictionary<string, BrainrotVFX2Recipe> recipes, string modelName, BrainrotVFXTheme theme, params BrainrotVFX2Layer[] layers) {
        BrainrotVFX2Recipe recipe = new BrainrotVFX2Recipe();
        recipe.theme = theme;
        recipe.layers.AddRange(layers);
        recipes[modelName] = recipe;
    }

    private static BrainrotVFX2Layer Orbit(string label, string material, int count) {
        return new BrainrotVFX2Layer(label, BrainrotVFX2Kind.Orbit, material, count);
    }

    private static BrainrotVFX2Layer Rise(string label, string material, int count) {
        BrainrotVFX2Layer layer = new BrainrotVFX2Layer(label, BrainrotVFX2Kind.Rise, material, count);
        layer.height = 0.12f;
        layer.radius = 0.95f;
        layer.band = 0.1f;
        layer.speed = 0.3f;
        return layer;
    }

    private static BrainrotVFX2Layer Twinkle(string label, string material, int count) {
        BrainrotVFX2Layer layer = new BrainrotVFX2Layer(label, BrainrotVFX2Kind.Twinkle, material, count);
        layer.radius = 1.15f;
        layer.lifetime = new Vector2(0.6f, 1.2f);
        return layer;
    }

    private static BrainrotVFX2Layer Ribbon(string label, int count) {
        BrainrotVFX2Layer layer = new BrainrotVFX2Layer(label, BrainrotVFX2Kind.Ribbon, GLOW, count);
        layer.size = 0.06f;
        return layer;
    }

    private static BrainrotVFX2Layer Pack(string label, string prefabPath) {
        return new BrainrotVFX2Layer(label, BrainrotVFX2Kind.Pack, prefabPath, 0);
    }
}

// dung VFX2 cho 1 brainrot: tao tung lop theo cong thuc, kich thuoc theo bounds that cua model (root VFX scale 1)
// lop tu tao dung ParticleSystem moi + material co san cua pack (trai cay, not nhac, tim, sao...), lop Pack copy prefab cua pack
public class BrainrotVFX2Builder {
    private static string TRAIL_MATERIAL_NAME = "VFX2 Trail Additive.mat";
    private static float ORBIT_LIFETIME = 4f;
    private static float RIBBON_LIFETIME = 20f;
    private static float MEASURE_TIME = 3f;
    private static float MEASURE_PERCENTILE = 0.03f;
    private static string[] GROUND_PART_WORDS = new string[] { "ground", "circle", "ring", "decal", "floor", "zone", "pedestal", "shockwave" };

    private Transform root;
    private Vector3 groundCenter;
    private float modelWidth;
    private float modelHeight;
    private float bodyRadius;
    private float unit;
    private Material trailMaterial;

    public static GameObject Build(string vfxName, BrainrotVFX2Recipe recipe, Bounds modelBounds, Material trailMaterial) {
        BrainrotVFX2Builder builder = new BrainrotVFX2Builder(modelBounds, trailMaterial);
        return builder.BuildVFX(vfxName, recipe);
    }

    // trail mac dinh cua pack la alpha blend, tao ban additive cho dai sang ruc hon (luu vao folder VFX2)
    public static Material GetOrCreateTrailMaterial(string folder) {
        string path = folder + "/" + TRAIL_MATERIAL_NAME;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;

        Material glowMaterial = LoadMaterial(BrainrotVFX2Recipes.GLOW);
        Material trailSource = LoadMaterial(BrainrotVFX2Recipes.TRAIL_SOURCE);
        if (glowMaterial == null || trailSource == null) return glowMaterial;

        material = new Material(glowMaterial);
        material.SetTexture("_BaseMap", trailSource.GetTexture("_BaseMap"));
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private BrainrotVFX2Builder(Bounds bounds, Material trailMaterial) {
        this.trailMaterial = trailMaterial;
        modelWidth = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.z), 0.1f);
        modelHeight = Mathf.Max(bounds.size.y, 0.1f);
        groundCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        // model gay cao thi van giu vong bay du rong, model det rong (may bay) thi sprite theo be ngang
        bodyRadius = Mathf.Max(modelWidth * 0.5f, modelHeight * 0.3f);
        unit = Mathf.Max(modelHeight, bodyRadius * 1.6f);
    }

    private GameObject BuildVFX(string vfxName, BrainrotVFX2Recipe recipe) {
        GameObject vfx = new GameObject(vfxName);
        root = vfx.transform;

        foreach (BrainrotVFX2Layer layer in recipe.layers) {
            switch (layer.kind) {
                case BrainrotVFX2Kind.Pack:
                    AddPackLayer(layer);
                    break;
                case BrainrotVFX2Kind.Orbit:
                    AddOrbitLayer(layer);
                    break;
                case BrainrotVFX2Kind.Rise:
                    AddRiseLayer(layer);
                    break;
                case BrainrotVFX2Kind.Twinkle:
                    AddTwinkleLayer(layer);
                    break;
                case BrainrotVFX2Kind.Ribbon:
                    AddRibbonLayer(layer);
                    break;
            }
        }

        return vfx;
    }

    // ===== lop tu tao =====

    // vong sprite quanh than (hoac hao quang tren dau khi height > 1): phat trong hinh xuyen nam ngang roi quay quanh truc Y
    private void AddOrbitLayer(BrainrotVFX2Layer layer) {
        ParticleSystem particle = CreateParticle(layer, layer.height);
        if (particle == null) return;

        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = ORBIT_LIFETIME;
        main.maxParticles = Mathf.CeilToInt(layer.count * 1.5f) + 2;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = layer.count / ORBIT_LIFETIME;

        SetDonutShape(particle, layer.radius * bodyRadius, Mathf.Max(layer.band * unit, 0.001f));
        SetVelocity(particle, 0f, layer.speed);
        SetFade(particle, 0.12f, 0.12f);
        SetPopSize(particle);
    }

    // sprite bay len quanh than, tu do cao height bay them distance x chieu cao model, xoay nhe quanh than
    private void AddRiseLayer(BrainrotVFX2Layer layer) {
        ParticleSystem particle = CreateParticle(layer, layer.height);
        if (particle == null) return;

        float lifetime = layer.distance / Mathf.Max(layer.speed, 0.01f);
        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.85f, lifetime * 1.15f);
        main.maxParticles = Mathf.CeilToInt(layer.count * 1.5f) + 2;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = layer.count / lifetime;

        if (layer.radius > 0.001f) {
            SetDonutShape(particle, layer.radius * bodyRadius, Mathf.Max(layer.band * unit, 0.001f));
        }
        else {
            ParticleSystem.ShapeModule shape = particle.shape;
            shape.enabled = false;
        }

        SetVelocity(particle, layer.speed * modelHeight, layer.radius > 0.001f ? 0.06f : 0f);
        SetFade(particle, 0.15f, 0.35f);

        if (layer.isGrow) {
            SetSizeCurve(particle, new AnimationCurve(new Keyframe(0f, 0.25f), new Keyframe(1f, 1.6f)));
        }
        else {
            SetPopSize(particle);
        }
    }

    // lap lanh trong vo elip bao quanh than, hien ra roi tat (drift > 0 thi troi lo lung nhu dom dom)
    private void AddTwinkleLayer(BrainrotVFX2Layer layer) {
        ParticleSystem particle = CreateParticle(layer, 0.5f);
        if (particle == null) return;

        float averageLifetime = (layer.lifetime.x + layer.lifetime.y) * 0.5f;
        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(layer.lifetime.x, layer.lifetime.y);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = Mathf.CeilToInt(layer.count * 1.5f) + 2;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = layer.count / Mathf.Max(averageLifetime, 0.05f);

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        shape.radiusThickness = 0.35f;
        shape.scale = new Vector3(layer.radius * bodyRadius * 2f, modelHeight * 1.05f, layer.radius * bodyRadius * 2f);

        if (layer.drift > 0f) {
            ParticleSystem.NoiseModule noise = particle.noise;
            noise.enabled = true;
            noise.strength = layer.drift * unit;
            noise.frequency = 0.6f;
            noise.scrollSpeed = 0.3f;
            noise.quality = ParticleSystemNoiseQuality.Medium;
            SetFade(particle, 0.25f, 0.3f);
            SetPopSize(particle);
        }
        else {
            SetFade(particle, 0.1f, 0.3f);
            SetSizeCurve(particle, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0f)));
        }
    }

    // vet sang quay quanh than: dau sang nho + trail, cac dau chia deu tren vong (BurstSpread), nghieng theo tilt
    private void AddRibbonLayer(BrainrotVFX2Layer layer) {
        ParticleSystem particle = CreateParticle(layer, layer.height);
        if (particle == null) return;

        ParticleSystem.MainModule main = particle.main;
        main.duration = RIBBON_LIFETIME;
        main.prewarm = false;
        main.startLifetime = RIBBON_LIFETIME;
        main.maxParticles = layer.count;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)layer.count) });

        SetDonutShape(particle, layer.radius * bodyRadius, 0.001f);
        ParticleSystem.ShapeModule shape = particle.shape;
        shape.radiusThickness = 0f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.BurstSpread;

        SetVelocity(particle, 0f, layer.speed);
        SetFade(particle, 0.02f, 0.02f);

        // trail dai ~1/3 vong quay
        float period = 1f / Mathf.Max(Mathf.Abs(layer.speed), 0.05f);
        ParticleSystem.TrailModule trails = particle.trails;
        trails.enabled = true;
        trails.mode = ParticleSystemTrailMode.PerParticle;
        trails.ratio = 1f;
        trails.lifetime = Mathf.Clamp01(period * 0.33f / RIBBON_LIFETIME);
        trails.minVertexDistance = unit * 0.02f;
        trails.worldSpace = false;
        trails.dieWithParticles = true;
        trails.inheritParticleColor = true;
        trails.sizeAffectsWidth = true;
        trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(CreateFadeGradient(new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f)));

        ParticleSystemRenderer particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
        particleRenderer.trailMaterial = trailMaterial;
    }

    // ParticleSystem moi dat o truc than model, cao height x chieu cao, dung material + o sprite cua lop
    private ParticleSystem CreateParticle(BrainrotVFX2Layer layer, float height) {
        Material material = LoadMaterial(layer.assetPath);
        if (material == null) {
            Debug.LogWarning("VFX2 layer '" + layer.label + "' has no material, skipped (" + root.name + ")");
            return null;
        }

        GameObject particleObject = new GameObject(layer.label);
        particleObject.transform.SetParent(root, false);
        particleObject.transform.localPosition = groundCenter + Vector3.up * modelHeight * height;
        particleObject.transform.localRotation = Quaternion.Euler(layer.tilt);

        ParticleSystem particle = particleObject.AddComponent<ParticleSystem>();
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particle.main;
        main.duration = 5f;
        main.loop = true;
        main.prewarm = true;
        main.playOnAwake = true;
        main.startSpeed = 0f;
        main.gravityModifier = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(layer.size * unit * 0.8f, layer.size * unit * 1.2f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.startColor = GetStartColor(layer);

        // sprite xoay nhe cho song dong, spin lon thi xoay tron (phi tieu, dong xu, la)
        float spinRadians = layer.spin * Mathf.Deg2Rad;
        main.startRotation = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        if (layer.spin > 0f) {
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            ParticleSystem.RotationOverLifetimeModule rotation = particle.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(spinRadians * 0.6f, spinRadians);
        }

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.enabled = false;

        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sharedMaterial = material;
        particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;
        particleRenderer.maxParticleSize = 2f;

        SetSheetFrames(particle, layer);
        return particle;
    }

    // trai cay / la / not nhac... nam chung 1 sprite sheet, chon o theo frameMin..frameMax (-1 = ngau nhien ca sheet)
    // gia tri startFrame luu chuan hoa 0..1 nhu prefab FruitShower cua pack, gan qua SerializedObject cho chac dung don vi
    private void SetSheetFrames(ParticleSystem particle, BrainrotVFX2Layer layer) {
        int tiles = BrainrotVFX2Recipes.GetTiles(layer.assetPath);
        if (tiles <= 1) return;

        int frameCount = tiles * tiles;
        int frameMin = layer.frameMin < 0 ? 0 : layer.frameMin;
        int frameMax = layer.frameMax < 0 ? frameCount - 1 : layer.frameMax;

        ParticleSystem.TextureSheetAnimationModule sheet = particle.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = tiles;
        sheet.numTilesY = tiles;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.cycleCount = 1;

        SerializedObject serializedParticle = new SerializedObject(particle);
        serializedParticle.FindProperty("UVModule.startFrame.minMaxState").intValue = (int)ParticleSystemCurveMode.TwoConstants;
        serializedParticle.FindProperty("UVModule.startFrame.minScalar").floatValue = (frameMin + 0.02f) / frameCount;
        serializedParticle.FindProperty("UVModule.startFrame.scalar").floatValue = (frameMax + 0.48f) / frameCount;
        serializedParticle.ApplyModifiedPropertiesWithoutUndo();
    }

    // hinh xuyen nam ngang (shape Donut mac dinh nam trong mat XY nen xoay 90 do quanh X)
    private void SetDonutShape(ParticleSystem particle, float radius, float donutRadius) {
        ParticleSystem.ShapeModule shape = particle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = radius;
        shape.donutRadius = donutRadius;
        shape.radiusThickness = 1f;
        shape.arc = 360f;
        shape.rotation = new Vector3(90f, 0f, 0f);
    }

    // speed dung (don vi / giay) + quay quanh truc Y cua lop (vong / giay), cac truc phai cung che do curve
    private void SetVelocity(ParticleSystem particle, float riseSpeed, float revolutionsPerSecond) {
        ParticleSystem.VelocityOverLifetimeModule velocity = particle.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(riseSpeed * 0.8f, riseSpeed * 1.2f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f);
        velocity.orbitalY = new ParticleSystem.MinMaxCurve(revolutionsPerSecond * Mathf.PI * 2f);
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        velocity.radial = new ParticleSystem.MinMaxCurve(0f);
    }

    private void SetFade(ParticleSystem particle, float fadeIn, float fadeOut) {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particle.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(CreateFadeGradient(
            new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeIn), new GradientAlphaKey(1f, 1f - fadeOut), new GradientAlphaKey(0f, 1f)));
    }

    // bat ra hoi nay len roi co lai luc bien mat, nhin vui mat hon
    private void SetPopSize(ParticleSystem particle) {
        SetSizeCurve(particle, new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.1f, 1.15f), new Keyframe(0.18f, 1f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f)));
    }

    private void SetSizeCurve(ParticleSystem particle, AnimationCurve curve) {
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particle.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    private ParticleSystem.MinMaxGradient GetStartColor(BrainrotVFX2Layer layer) {
        if (!layer.isRainbow) return new ParticleSystem.MinMaxGradient(layer.colorA, layer.colorB);

        Gradient rainbow = new Gradient();
        rainbow.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.3f, 0.35f), 0f),
                new GradientColorKey(new Color(1f, 0.65f, 0.2f), 0.17f),
                new GradientColorKey(new Color(1f, 0.95f, 0.3f), 0.33f),
                new GradientColorKey(new Color(0.35f, 1f, 0.45f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.85f, 1f), 0.67f),
                new GradientColorKey(new Color(0.55f, 0.45f, 1f), 0.83f),
                new GradientColorKey(new Color(1f, 0.4f, 0.9f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

        ParticleSystem.MinMaxGradient startColor = new ParticleSystem.MinMaxGradient(rainbow);
        startColor.mode = ParticleSystemGradientMode.RandomColor;
        return startColor;
    }

    private Gradient CreateFadeGradient(params GradientAlphaKey[] alphaKeys) {
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, alphaKeys);
        return gradient;
    }

    // ===== lop prefab cua pack =====

    // copy prefab (unpack) -> bo script / light / am thanh -> tat phan nam duoi dat -> URP -> do kich thuoc -> vua than, dat giua than
    private void AddPackLayer(BrainrotVFX2Layer layer) {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(layer.assetPath + ".prefab");
        if (prefab == null) {
            Debug.LogWarning("VFX2 pack prefab not found: " + layer.assetPath);
            return;
        }

        GameObject effect = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        PrefabUtility.UnpackPrefabInstance(effect, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        effect.name = layer.label + " (" + prefab.name + ")";
        effect.transform.SetParent(root, false);
        effect.transform.localPosition = Vector3.zero;

        CleanEffect(effect);
        DisableGroundParts(effect);
        BrainrotVFXMaterialUpgrader.UpgradeEffect(effect);
        PrepareParticles(effect);

        Bounds effectBounds = MeasureEffect(effect);
        float effectWidth = Mathf.Max(effectBounds.size.x, effectBounds.size.z);
        float widthScale = effectWidth > 0.001f ? bodyRadius * 2f * layer.fitSize / effectWidth : 1f;
        float heightScale = effectBounds.size.y > 0.001f ? modelHeight * layer.fitSize / effectBounds.size.y : widthScale;
        float scale = Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.02f, 50f);
        effect.transform.localScale *= scale;

        Vector3 bodyCenter = groundCenter + Vector3.up * modelHeight * 0.5f;
        effect.transform.localPosition = bodyCenter - effectBounds.center * scale;
    }

    // giu script xoay cua pack, bo script tu huy / light / camera shake / am thanh
    private void CleanEffect(GameObject effect) {
        foreach (MonoBehaviour behaviour in effect.GetComponentsInChildren<MonoBehaviour>(true)) {
            if (behaviour == null || behaviour.GetType().Name.Contains("Rotat")) continue;

            Object.DestroyImmediate(behaviour);
        }

        foreach (Transform child in effect.GetComponentsInChildren<Transform>(true)) {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
        }

        foreach (Light light in effect.GetComponentsInChildren<Light>(true)) {
            Object.DestroyImmediate(light);
        }

        foreach (AudioSource audioSource in effect.GetComponentsInChildren<AudioSource>(true)) {
            Object.DestroyImmediate(audioSource);
        }

        foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true)) {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    // vong / de / vet nam ngang duoi chan (billboard nam ngang hoac ten Circle, Ring, Ground...) -> tat phat va tat ve
    private void DisableGroundParts(GameObject effect) {
        foreach (ParticleSystem particle in effect.GetComponentsInChildren<ParticleSystem>(true)) {
            ParticleSystemRenderer particleRenderer = particle.GetComponent<ParticleSystemRenderer>();
            bool isFlat = particleRenderer != null && particleRenderer.renderMode == ParticleSystemRenderMode.HorizontalBillboard;
            if (!isFlat && !IsGroundName(particle.name)) continue;

            ParticleSystem.EmissionModule emission = particle.emission;
            emission.enabled = false;
            if (particleRenderer != null) {
                particleRenderer.enabled = false;
            }
        }
    }

    private bool IsGroundName(string objectName) {
        string lowerName = objectName.ToLowerInvariant();
        foreach (string word in GROUND_PART_WORDS) {
            if (lowerName.Contains(word)) return true;
        }

        return false;
    }

    // Hierarchy de scale theo transform (root model scale 100, VFX bu 1/100), prewarm de vua hien ra da day du
    private void PrepareParticles(GameObject effect) {
        foreach (ParticleSystem particle in effect.GetComponentsInChildren<ParticleSystem>(true)) {
            ParticleSystem.MainModule main = particle.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            if (main.loop) {
                main.prewarm = true;
            }

            ParticleSystem.LightsModule lights = particle.lights;
            lights.enabled = false;

            ParticleSystem.CollisionModule collision = particle.collision;
            if (collision.enabled && collision.type == ParticleSystemCollisionType.World) {
                collision.enabled = false;
            }
        }
    }

    // chay thu roi lay vi tri + kich thuoc hat, bo 3% hat xa nhat moi phia
    private Bounds MeasureEffect(GameObject effect) {
        List<float> xs = new List<float>();
        List<float> ys = new List<float>();
        List<float> zs = new List<float>();

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems) {
            particleSystem.Simulate(MEASURE_TIME, false, true, true);
            CollectParticlePoints(particleSystem, xs, ys, zs);
        }

        foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true)) {
            if (renderer is ParticleSystemRenderer || !renderer.enabled) continue;

            Bounds rendererBounds = renderer.bounds;
            AddPoint(xs, ys, zs, rendererBounds.min, 0f);
            AddPoint(xs, ys, zs, rendererBounds.max, 0f);
        }

        foreach (ParticleSystem particleSystem in particleSystems) {
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (xs.Count == 0) {
            Debug.LogWarning("Could not measure VFX2 '" + effect.name + "', using default size");
            return new Bounds(Vector3.up * 0.5f, Vector3.one);
        }

        Vector3 min = new Vector3(GetPercentile(xs, MEASURE_PERCENTILE), GetPercentile(ys, MEASURE_PERCENTILE), GetPercentile(zs, MEASURE_PERCENTILE));
        Vector3 max = new Vector3(GetPercentile(xs, 1f - MEASURE_PERCENTILE), GetPercentile(ys, 1f - MEASURE_PERCENTILE), GetPercentile(zs, 1f - MEASURE_PERCENTILE));

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    private void CollectParticlePoints(ParticleSystem particleSystem, List<float> xs, List<float> ys, List<float> zs) {
        ParticleSystem.MainModule main = particleSystem.main;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Mathf.Max(1, particleSystem.particleCount)];
        int count = particleSystem.GetParticles(particles);

        Transform simulationTransform = particleSystem.transform;
        if (main.simulationSpace == ParticleSystemSimulationSpace.Custom && main.customSimulationSpace != null) {
            simulationTransform = main.customSimulationSpace;
        }

        Vector3 lossyScale = particleSystem.transform.lossyScale;
        float sizeScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));

        for (int i = 0; i < count; i++) {
            Vector3 position = particles[i].position;
            if (main.simulationSpace != ParticleSystemSimulationSpace.World) {
                position = simulationTransform.TransformPoint(position);
            }

            float radius = particles[i].GetCurrentSize(particleSystem) * sizeScale * 0.5f;
            AddPoint(xs, ys, zs, position, radius);
        }
    }

    private void AddPoint(List<float> xs, List<float> ys, List<float> zs, Vector3 point, float radius) {
        xs.Add(point.x - radius);
        xs.Add(point.x + radius);
        ys.Add(point.y - radius);
        ys.Add(point.y + radius);
        zs.Add(point.z - radius);
        zs.Add(point.z + radius);
    }

    private float GetPercentile(List<float> values, float percentile) {
        values.Sort();
        int index = Mathf.Clamp(Mathf.RoundToInt((values.Count - 1) * percentile), 0, values.Count - 1);
        return values[index];
    }

    private static Material LoadMaterial(string pathWithoutExtension) {
        return AssetDatabase.LoadAssetAtPath<Material>(pathWithoutExtension + ".mat");
    }
}

// tao bo VFX2 (Assets/_Game/Prefabs/BrainrotVFX2) cho tat ca brainrot theo cong thuc rieng trong BrainrotVFX2Recipes
// prefab VFX2 dung kich thuoc that nhu VFX cu: gan lam con cua model thi scale = 1 / scale root model (menu Attach lam san)
public static class ToolCreateBrainrotVFX2 {
    private static string SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";
    private static string PREFABS_FOLDER = "Assets/_Game/Prefabs";
    private static string VFX2_FOLDER_NAME = "BrainrotVFX2";
    private static string OLD_VFX_FOLDER = "Assets/_Game/Prefabs/BrainrotVFX";
    private static string PREVIEW_ROOT_NAME = "Brainrot VFX2 Preview";

    [MenuItem("Tools/VFX/Brainrot VFX2/Create All VFX2")]
    private static void CreateAll() {
        string vfxFolder = GetOrCreateFolder(PREFABS_FOLDER, VFX2_FOLDER_NAME);
        string materialFolder = GetOrCreateFolder(vfxFolder, "Materials");
        Material trailMaterial = BrainrotVFX2Builder.GetOrCreateTrailMaterial(materialFolder);
        Dictionary<string, BrainrotVFX2Recipe> recipes = BrainrotVFX2Recipes.CreateAll();
        List<BrainrotSO> brainrotSOs = LoadBrainrotSOs();
        int createdCount = 0;

        try {
            for (int i = 0; i < brainrotSOs.Count; i++) {
                BrainrotModel model = brainrotSOs[i].GetBrainrotModel();
                if (model == null) continue;

                EditorUtility.DisplayProgressBar("Creating Brainrot VFX2", model.name, (float)i / brainrotSOs.Count);
                if (!recipes.TryGetValue(model.name, out BrainrotVFX2Recipe recipe)) {
                    Debug.LogWarning("No VFX2 recipe for " + model.name);
                    continue;
                }

                CreateVFX(brainrotSOs[i], model, recipe, trailMaterial, vfxFolder);
                createdCount++;
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created VFX2 for " + createdCount + " brainrots in " + vfxFolder);
    }

    [MenuItem("Tools/VFX/Brainrot VFX2/Spawn Preview In Scene")]
    private static void SpawnPreview() {
        GameObject oldPreview = GameObject.Find(PREVIEW_ROOT_NAME);
        if (oldPreview != null) {
            Undo.DestroyObjectImmediate(oldPreview);
        }

        GameObject previewRoot = new GameObject(PREVIEW_ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(previewRoot, "Spawn Brainrot VFX2 Preview");

        float spacing = 4f;
        Dictionary<BrainrotRarity, int> columnCounts = new Dictionary<BrainrotRarity, int>();
        foreach (BrainrotSO brainrotSO in LoadBrainrotSOs()) {
            BrainrotModel model = brainrotSO.GetBrainrotModel();
            if (model == null) continue;

            GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetVFX2Path(brainrotSO));
            if (vfxPrefab == null) continue;

            BrainrotRarity rarity = brainrotSO.GetRarity();
            columnCounts.TryGetValue(rarity, out int column);
            columnCounts[rarity] = column + 1;

            // bo VFX cu dang gan tren model de chi xem VFX2
            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model.gameObject, previewRoot.transform);
            PrefabUtility.UnpackPrefabInstance(modelInstance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            RemoveVFX(modelInstance);
            modelInstance.transform.localPosition = new Vector3(column * spacing, 0f, (int)rarity * spacing);

            GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(vfxPrefab, modelInstance.transform);
            vfxInstance.transform.localScale = GetInverseScale(modelInstance.transform.localScale);
        }

        Selection.activeGameObject = previewRoot;
        SceneView.FrameLastActiveSceneView();
    }

    [MenuItem("Tools/VFX/Brainrot VFX2/Attach VFX2 To Models (Replace Old VFX)")]
    private static void AttachVFX2() {
        if (!EditorUtility.DisplayDialog("Attach VFX2", "Thay VFX dang gan tren tat ca model brainrot bang VFX2? (prefab VFX cu van giu, co the Restore)", "Attach", "Cancel")) return;

        int attachedCount = 0;
        foreach (BrainrotSO brainrotSO in LoadBrainrotSOs()) {
            BrainrotModel model = brainrotSO.GetBrainrotModel();
            if (model == null) continue;

            if (AttachToModel(model, GetVFX2Path(brainrotSO))) attachedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Attached VFX2 to " + attachedCount + " brainrot models");
    }

    [MenuItem("Tools/VFX/Brainrot VFX2/Restore Old VFX On Models")]
    private static void RestoreOldVFX() {
        if (!EditorUtility.DisplayDialog("Restore Old VFX", "Gan lai VFX cu (folder BrainrotVFX) cho tat ca model brainrot?", "Restore", "Cancel")) return;

        int restoredCount = 0;
        foreach (BrainrotSO brainrotSO in LoadBrainrotSOs()) {
            BrainrotModel model = brainrotSO.GetBrainrotModel();
            if (model == null) continue;

            if (AttachToModel(model, GetOldVFXPath(brainrotSO))) restoredCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Restored old VFX on " + restoredCount + " brainrot models");
    }

    private static void CreateVFX(BrainrotSO brainrotSO, BrainrotModel model, BrainrotVFX2Recipe recipe, Material trailMaterial, string vfxFolder) {
        BrainrotVFXAnalysis analysis;
        GameObject contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(model));
        try {
            analysis = BrainrotVFXAnalyzer.Analyze(contents);
        }
        finally {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        string vfxName = model.name + " VFX2";
        GameObject vfxRoot = BrainrotVFX2Builder.Build(vfxName, recipe, analysis.bounds, trailMaterial);
        vfxRoot.AddComponent<BrainrotVFX>().Setup(recipe.theme, brainrotSO.GetRarity(), analysis.primaryColor, analysis.secondaryColor);

        string rarityFolder = GetOrCreateFolder(vfxFolder, brainrotSO.GetRarity().ToString());
        PrefabUtility.SaveAsPrefabAsset(vfxRoot, rarityFolder + "/" + vfxName + ".prefab");
        Object.DestroyImmediate(vfxRoot);
    }

    // VFX dung theo kich thuoc that (root model scale 100) nen con VFX phai scale 1/100 de bu lai
    private static bool AttachToModel(BrainrotModel model, string vfxPath) {
        GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(vfxPath);
        if (vfxPrefab == null) {
            Debug.LogWarning("VFX prefab not found: " + vfxPath);
            return false;
        }

        string modelPath = AssetDatabase.GetAssetPath(model);
        GameObject contents = PrefabUtility.LoadPrefabContents(modelPath);
        try {
            RemoveVFX(contents);

            GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(vfxPrefab, contents.transform);
            vfxInstance.transform.localPosition = Vector3.zero;
            vfxInstance.transform.localRotation = Quaternion.identity;
            vfxInstance.transform.localScale = GetInverseScale(contents.transform.localScale);

            PrefabUtility.SaveAsPrefabAsset(contents, modelPath);
        }
        finally {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        return true;
    }

    private static void RemoveVFX(GameObject modelRoot) {
        foreach (BrainrotVFX vfx in modelRoot.GetComponentsInChildren<BrainrotVFX>(true)) {
            Object.DestroyImmediate(vfx.gameObject);
        }
    }

    private static List<BrainrotSO> LoadBrainrotSOs() {
        List<BrainrotSO> brainrotSOs = new List<BrainrotSO>();
        foreach (string guid in AssetDatabase.FindAssets("t:BrainrotSO", new string[] { SO_FOLDER })) {
            BrainrotSO brainrotSO = AssetDatabase.LoadAssetAtPath<BrainrotSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (brainrotSO == null) continue;

            brainrotSOs.Add(brainrotSO);
        }

        brainrotSOs.Sort((a, b) => {
            int rarityCompare = ((int)a.GetRarity()).CompareTo((int)b.GetRarity());
            return rarityCompare != 0 ? rarityCompare : string.CompareOrdinal(a.name, b.name);
        });

        return brainrotSOs;
    }

    // prefab VFX / VFX2 chia theo folder rarity: BrainrotVFX2/<Rarity>/<model> VFX2.prefab
    private static string GetVFX2Path(BrainrotSO brainrotSO) {
        return PREFABS_FOLDER + "/" + VFX2_FOLDER_NAME + "/" + brainrotSO.GetRarity() + "/" + brainrotSO.GetBrainrotModel().name + " VFX2.prefab";
    }

    private static string GetOldVFXPath(BrainrotSO brainrotSO) {
        return OLD_VFX_FOLDER + "/" + brainrotSO.GetRarity() + "/" + brainrotSO.GetBrainrotModel().name + " VFX.prefab";
    }

    private static string GetOrCreateFolder(string parentFolder, string folderName) {
        string folderPath = parentFolder + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath)) {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        return folderPath;
    }

    private static Vector3 GetInverseScale(Vector3 scale) {
        return new Vector3(GetInverse(scale.x), GetInverse(scale.y), GetInverse(scale.z));
    }

    private static float GetInverse(float value) {
        if (Mathf.Abs(value) < 0.0001f) return 1f;

        return 1f / value;
    }
}
