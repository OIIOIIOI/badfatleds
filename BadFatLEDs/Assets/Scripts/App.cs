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

    public Material mat;
    public Button browseButton;

    float th = Mathf.Sqrt(3);

    Texture2D tex;

    public void TestClick()
    {
        string path = EditorUtility.OpenFilePanel("Pick PNG file", "", "png");
        tex = new Texture2D(9, 16);
        tex.LoadImage(File.ReadAllBytes(path));
        //TextureScale.Bilinear(tex, gridWidth, gridHeight);

        //Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2());
        //GameObject go = new GameObject("CoolSprite");
        //go.AddComponent<SpriteRenderer>();
        //go.GetComponent<SpriteRenderer>().sprite = sprite;

        List<byte> bytesList = new List<byte>();
        //for (int y = 0; y < tex.height; y++)
        for (int y = tex.height-1; y >= 0; y--)
        {
            //Debug.Log(y);
            for (int x = 0; x < tex.width; x++)
            {
                GameObject go = GameObject.Find("Triangle_" + x + "-" + y);
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                Color color = tex.GetPixel(x, y);
                renderer.material.color = color;
                HSV hsv = colorToHSV(color);
                bytesList.Add(hsv.h);
                bytesList.Add(hsv.s);
                bytesList.Add(hsv.v);
            }
        }
        Debug.Log("new bytes: " + bytesList.Count);
        string file = "test.img";
        if (File.Exists(file))
        {
            byte[] readBytes = File.ReadAllBytes(file);
            List<byte> readBytesList = readBytes.ToList();
            Debug.Log("read bytes: " + readBytesList.Count);
            bytesList.InsertRange(0, readBytesList);
        }
        Debug.Log("total bytes: " + bytesList.Count);
        File.WriteAllBytes(file, bytesList.ToArray());
    }

    public void TestWrite ()
    {
        int x = 1;
        int y = 1;
        GameObject go = GameObject.Find("Triangle_" + x + "-" + y);
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        Color color = renderer.material.color;
        HSV hsv = colorToHSV(color);
        WriteHex(hsv);
    }

    void WriteHex (HSV hsv)
    {
        List<byte> bytesList = new List<byte>();
        //TODO bytesList.Add();
        bytesList.Add(hsv.h);
        bytesList.Add(hsv.s);
        bytesList.Add(hsv.v);
        File.WriteAllBytes("test.img", bytesList.ToArray());
    }

    void Start()
    {
        GameObject grid = new GameObject("Grid");

        GameObject go;
        bool flipped;
        int flipCheck = (startFlipped) ? 0 : 1;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                flipped = (x + y) % 2 == flipCheck;
                go = GetTriangle(flipped, (x == 0 && y == 0));
                go.transform.parent = grid.transform;
                PlaceTriangle(go, x, y);
            }
        }
        // Use the placeTriangle function to center the grid on screen
        PlaceTriangle(grid, -Mathf.FloorToInt(gridWidth / 2), -Mathf.FloorToInt(gridHeight / 2), false);

        //Debug.Log(System.Convert.ToString(7, 2));
    }

    void PlaceTriangle (GameObject go, int cx, int cy, bool rename = true)
    {
        if (rename)   go.name = "Triangle_" + cx + "-" + cy;
        float x = (verticalGrid) ? cx * th : cx;
        float y = (verticalGrid) ? cy : cy * th;
        go.transform.position = new Vector3(x, y, 0);
    }

    GameObject GetTriangle (bool flipped = false, bool first = false)
    {
        GameObject go = new GameObject();

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.motionVectors = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        Material newMat = Instantiate(mat);
        if (!first)
            newMat.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        renderer.material = newMat;

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
    public byte h;
    public byte s;
    public byte v;

    public int hInt;
    public int sInt;
    public int vInt;

    public HSV (int hh, int ss, int vv)
    {
        hInt = hh;
        sInt = ss;
        vInt = vv;

        h = System.Convert.ToByte(hh);
        s = System.Convert.ToByte(ss);
        v = System.Convert.ToByte(vv);
    }

    override public string ToString ()
    {
        return hInt + ", " + sInt + ", " + vInt;
    }
}