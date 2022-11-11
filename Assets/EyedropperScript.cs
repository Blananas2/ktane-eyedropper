using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using KModkit;

public class EyedropperScript : MonoBehaviour {

    public KMBombInfo Bomb;

    private Color Selection;
    public GameObject[] TextBoxes;

    private bool Toggle = false; //Keyboard shortcut: [?]
    private bool Lock = false;
    private int Option = 0; //Keyboard shortcuts: [<] and [>]
    private const int NumOfOptions = 3; //Currently: description, 6-digit hex, 3-digit hex; More can definitely be added at a later date.
    private string Code = String.Empty;

    void Awake () {
        Bomb.OnBombExploded += Hide;

        GetComponent<KMGameInfo>().OnStateChange += state => {
            if (state != KMGameInfo.State.PostGame) {
                Hide();
            }
        };
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Slash) && !Lock) {
            Toggle = !Toggle;
            Debug.LogFormat("[Eyedropper] Eyedropper's Toggle set to {0}", Toggle);
        } else if (Input.GetKeyDown(KeyCode.Comma) && Toggle) {
            Option = (Option + (NumOfOptions - 1)) % NumOfOptions;
            Debug.LogFormat("[Eyedropper] Eyedropper's Option set to {0}", Option);
        } else if (Input.GetKeyDown(KeyCode.Period) && Toggle) {
            Option = (Option + 1) % NumOfOptions;
            Debug.LogFormat("[Eyedropper] Eyedropper's Option set to {0}", Option);
        }

        if (Toggle) {
            Selection = GetPixelColorAtMousePosition();
            string Hex = ColorUtility.ToHtmlStringRGB(Selection);

            switch (Option) {
                case 0: Code = Description(Selection); break;
                case 1: Code = Hex; break;
                case 2: Code = Hex[0].ToString() + Hex[2] + Hex[4]; break;
            }

            TextBoxes[0].GetComponent<Text>().text = "Eyedropper:";
            TextBoxes[0].GetComponent<Text>().color = Selection;
            TextBoxes[1].GetComponent<Text>().text = Code;
        } else {
            TextBoxes[0].GetComponent<Text>().text = String.Empty;
            TextBoxes[1].GetComponent<Text>().text = String.Empty;
        }
    }

    void Hide () {
        Toggle = false;
        Lock = true;
    }

    Color GetPixelColorAtMousePosition()
    {
        //Initialize variables and textures
        var width = Screen.width;
        var height = Screen.height;
        var camera = Camera.main;
        var mousePosition = Input.mousePosition;
        var renderTexture = new RenderTexture(width, height, 24);
        var outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        //Capture Texture2D
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;
        outputTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        //Reset values
        camera.targetTexture = null;
        RenderTexture.active = null;

        //Get color of pixel
        var pixelColor = outputTexture.GetPixel((int)mousePosition.x, (int)mousePosition.y);

        //Destroy objects
        Destroy(renderTexture);
        Destroy(outputTexture);

        return pixelColor; //ty Qk
    }

    /* Note: The code below was heavily inspired by Microsoft PowerToys' Color Picker.
       I probably didn't need to include it's MIT Open Source license but better safe than sorry.
       See PT_LICENSE for that license. */

    string Description (Color col) {
        float p = 2.2204460492503131e-16f;
        float min = Math.Min(Math.Min(col.r, col.g), col.b);
        float max = Math.Max(Math.Max(col.r, col.g), col.b);
        float dif = max - min;
        float hue; float sat;
        float lum = (max + min) / 2;
        int lim; int dex = 0;
        int[][] lev = new int[][] {
            new int[] { 8,  0,  0, 44,  0,  0,  0, 63,  0,   0, 122,   0, 134,   0,   0,   0,   0, 166, 176, 241,   0, 256,   0 },
            new int[] { 0, 10,  0, 32, 46,  0,  0,  0, 61,   0, 106,   0, 136, 144,   0,   0,   0, 158, 166, 241,   0,   0, 256 },
            new int[] { 0,  8,  0,  0, 39, 46,  0,  0,  0,  71, 120,   0, 131, 144,   0,   0, 163,   0, 177, 211, 249,   0, 256 },
            new int[] { 0, 11, 26,  0,  0, 38, 45,  0,  0,  56, 100, 121, 129,   0, 140,   0, 180,   0,   0, 224, 241,   0, 256 },
            new int[] { 0, 13, 27,  0,  0, 36, 45,  0,  0,  59, 118,   0, 127, 136, 142,   0, 185,   0,   0, 216, 239,   0, 256 }
        };
        int[][] ind = new int[][] {
            new int[] { 130, 100, 115, 100, 100, 100, 110,  75, 100,  90, 100, 100, 100, 100,  80, 100, 100, 100, 100, 100, 100, 100, 100 },
            new int[] { 170, 170, 170, 155, 170, 170, 170, 170, 170, 115, 170, 170, 170, 170, 170, 170, 170, 170, 150, 150, 170, 140, 165 },
        };
        string[][] nam = new string[][] {
            new string[] { "Coral", "Rose", "Light Orange", "Tan", "Tan", "Light Yellow", "Light Yellow", "Tan", "Light Green", "Lime", "Light Green", "Light Green", "Aqua", "Sky Blue", "Light Turquoise", "Pale Blue", "Light Blue", "Ice Blue", "Periwinkle", "Lavender", "Pink", "Tan", "Rose" },
            new string[] { "Coral", "Red", "Orange", "Brown", "Tan", "Gold", "Yellow", "Olive Green", "Olive Green", "Green", "Green", "Bright Green", "Teal", "Aqua", "Turquoise", "Pale Blue", "Blue", "Blue Gray", "Indigo", "Purple", "Pink", "Brown", "Red" },
            new string[] { "Brown", "Dark Red", "Brown", "Brown", "Brown", "Dark Yellow", "Brown", "Dark Green", "Dark Green", "Dark Green", "Dark Green", "Dark Teal", "Dark Teal", "Dark Teal", "Dark Blue", "Dark Blue", "Blue Gray", "Indigo", "Dark Purple", "Plum", "Brown", "Dark Red" }
        };

        if (Math.Abs(dif) < p) {
            hue = 0f; sat = 0f;
        } else {
            sat = dif / ((lum < 0.5f) ? (max + min) : (2f - max - min));
            float frd = ((max - col.r) / 6.0f + dif / 2f) / dif;
            float fgn = ((max - col.g) / 6.0f + dif / 2f) / dif;
            float fbu = ((max - col.b) / 6.0f + dif / 2f) / dif;

            if (Math.Abs(col.r - max) < p) {
                hue = fbu - fgn;
            } else if (Math.Abs(col.g - max) < p) {
                hue = (float)1/3 + frd - fbu;
            } else if (Math.Abs(col.b - max) < p) {
                hue = (float)2/3 + fgn - frd;
            } else {
                hue = 0f;
            }

            if (hue < 0f | hue > 1f) {
                hue += (hue < 0f ? (1f) : (-1f));
            }
        }

        hue = hue * 255;
        sat = sat * 255;
        lum = lum * 255;

        if (lum > 240) {
            return "White";
        } else if (lum < 20) {
            return "Black";
        }

        if (sat <= 20) {
            if (lum > 170) {
                return "Light Gray";
            } else if (lum > 100) {
                return "Gray";
            } else {
                return "Dark Gray";
            }
        }

        if (sat <= 75) {
            lim = 0;
        } else if (sat <= 115) {
            lim = 1;
        } else if (sat <= 150) {
            lim = 2;
        } else if (sat <= 240) {
            lim = 3;
        } else {
            lim = 4;
        }

        for (int k = 0; k < 23; k++) {
            if (hue < lev[lim][k]) {
                dex = k;
                break;
            }
        }

        if (lum > ind[1][dex]) {
            return nam[0][dex];
        } else if (lum < ind[0][dex]) {
            return nam[2][dex];
        } else {
            return nam[1][dex];
        }

        return null;
    }
}
