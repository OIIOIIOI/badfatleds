using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Linq;

public class App : MonoBehaviour
{

    public bool verticalGrid;
    public bool startFlipped;
    public int gridWidth;
    public int gridHeight;
    public Material mat;// Unlit material

    float th = Mathf.Sqrt(3);// Triangle height
    Sprite displaySprite;

    void Start()
    {
        // Create empty grid object to hold all triangles
        GameObject grid = new GameObject("Grid");

        GameObject go;
        bool flipped;
        int flipCheck = (startFlipped) ? 0 : 1;
        // Create a triangle for each cell in the grid
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Find out if the new triangle should be flipped or not
                flipped = (x + y) % 2 == flipCheck;
                // Get a new triangle
                go = GetTriangle(flipped);
                // Add triangle to grid container
                go.transform.parent = grid.transform;
                // Place the triangle depending on it's coordinates
                PlaceTriangle(go, x, y);
            }
        }
        // Use the placeTriangle function to center the grid on screen
        PlaceTriangle(grid, -Mathf.FloorToInt(gridWidth / 2), -Mathf.FloorToInt(gridHeight / 2), false);
    }

    GameObject GetTriangle(bool flipped = false)
    {
        GameObject go = new GameObject();

        // Create mesh renderer
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.motionVectors = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        // Clone the original material
        Material newMat = Instantiate(mat);
        // Apply a random color
        newMat.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        renderer.material = newMat;

        // Create mesh, define vertices and triangles
        Mesh mesh = go.AddComponent<MeshFilter>().mesh;
        if (verticalGrid)
        {
            if (flipped)
            {
                mesh.vertices = new Vector3[] { new Vector3(th / 2, -1, 0), new Vector3(th / 2, 1, 0), new Vector3(-th / 2, 0, 0) };
                mesh.triangles = new int[] { 2, 1, 0 };
            }
            else
            {
                mesh.vertices = new Vector3[] { new Vector3(-th / 2, -1, 0), new Vector3(-th / 2, 1, 0), new Vector3(th / 2, 0, 0) };
                mesh.triangles = new int[] { 0, 1, 2 };
            }
        }
        else
        {
            if (flipped)
            {
                mesh.vertices = new Vector3[] { new Vector3(-1, -th / 2, 0), new Vector3(1, -th / 2, 0), new Vector3(0, th / 2, 0) };
                mesh.triangles = new int[] { 2, 1, 0 };
            }
            else
            {
                mesh.vertices = new Vector3[] { new Vector3(-1, th / 2, 0), new Vector3(1, th / 2, 0), new Vector3(0, -th / 2, 0) };
                mesh.triangles = new int[] { 0, 1, 2 };
            }
        }
        return go;
    }

    void PlaceTriangle(GameObject go, int cx, int cy, bool rename = true)
    {
        // Set the name for easy finding
        if (rename) go.name = "Triangle_" + cx + "-" + cy;
        float x = (verticalGrid) ? cx * th : cx;
        float y = (verticalGrid) ? cy : cy * th;
        go.transform.position = new Vector3(x, y, 0);
    }
    
    public void BrowseAndWriteToFile ()
    {
        // Ask user to pick a file
        string path = EditorUtility.OpenFilePanel("Pick PNG file", "", "png");
        // Load image into properly sized texture
        Texture2D tex = new Texture2D(1, 1);
        Texture2D tex2 = new Texture2D(1, 1);
        tex.LoadImage(File.ReadAllBytes(path));
        tex2.LoadImage(File.ReadAllBytes(path));
        TextureScale.Bilinear(tex, gridWidth, gridHeight);

        // Create display sprite
        //displaySprite = Sprite.Create(tex2, new Rect(0, 0, tex2.width, tex2.height), new Vector2());
        //GameObject displayGO = new GameObject("TextureDisplay");
        //displayGO.AddComponent<SpriteRenderer>();
        //displayGO.GetComponent<SpriteRenderer>().sprite = displaySprite;

        // Create empty bytes list
        List<byte> bytesList = new List<byte>();
        // Read every pixel in the texture
        for (int y = tex.height-1; y >= 0; y--)
        {
            for (int x = 0; x < tex.width; x++)
            {
                // Find the correct triangle
                GameObject go = GameObject.Find("Triangle_" + x + "-" + y);
                // Set the triangle's material color to the pixel color
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                Color color = tex.GetPixel(x, y);
                renderer.material.color = color;
                // Convert color to HSV
                HSV hsv = colorToHSV(color);
                // Add bytes to list
                bytesList.Add(hsv.h);
                bytesList.Add(hsv.s);
                bytesList.Add(hsv.v);
            }
        }

        string file = "test.img";

        // If file already exists, add existing bytes at the start of the list
        if (File.Exists(file))
        {
            byte[] readBytes = File.ReadAllBytes(file);
            List<byte> readBytesList = readBytes.ToList();
            bytesList.InsertRange(0, readBytesList);
        }
        // Write all bytes to file
        File.WriteAllBytes(file, bytesList.ToArray());
    }

    // Converts a Color object to an HSV object with values from 0 to 255
    HSV colorToHSV (Color color)
    {
        float rd = color.r;
        float gd = color.g;
        float bd = color.b;
        float max = Mathf.Max(rd, gd, bd);
        float min = Mathf.Min(rd, gd, bd);
        float h, s, v = max;

        float d = max - min;
        s = max == 0 ? 0 : d / max;

        if (max == min)
            h = 0;
        else
        {
            if (max == rd)
                h = (gd - bd) / d + (gd < bd ? 6 : 0);
            else if (max == gd)
                h = (bd - rd) / d + 2;
            else
                h = (rd - gd) / d + 4;
            h /= 6;
        }
        return new HSV(Mathf.RoundToInt(h * 255), Mathf.RoundToInt(s * 255), Mathf.RoundToInt(v * 255));
    }

}

public struct HSV
{
    // Byte values
    public byte h;
    public byte s;
    public byte v;
    // Int values
    public int hInt;
    public int sInt;
    public int vInt;

    public HSV (int hh, int ss, int vv)
    {
        // Store ints
        hInt = hh;
        sInt = ss;
        vInt = vv;
        // Store bytes
        h = System.Convert.ToByte(hh);
        s = System.Convert.ToByte(ss);
        v = System.Convert.ToByte(vv);
    }

    override public string ToString ()
    {
        return hInt + ", " + sInt + ", " + vInt;
    }
}