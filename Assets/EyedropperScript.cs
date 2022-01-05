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
    private const int NumOfOptions = 2; //Currently: 3-digit hex, 6-digit hex; More can definitely be added at a later date.
    private string Code = String.Empty;

    void Awake () {
        Bomb.OnBombSolved += Hide;
        Bomb.OnBombExploded += Hide;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Slash) && !Lock) {
            Toggle = !Toggle;
        } else if (Input.GetKeyDown(KeyCode.Comma)) {
            Option = (Option + (NumOfOptions - 1)) % NumOfOptions;
        } else if (Input.GetKeyDown(KeyCode.Period)) {
            Option = (Option + 1) % NumOfOptions;
        }

        if (Toggle) {
            Selection = GetPixelColorAtMousePosition();
            string Hex = ColorUtility.ToHtmlStringRGB(Selection);

            switch (Option) {
                case 0: Code = Hex[0].ToString() + Hex[2] + Hex[4]; break;
                case 1: Code = Hex; break;
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
}
