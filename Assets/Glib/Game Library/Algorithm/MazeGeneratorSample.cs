using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Glib.AutoGenerate;

// 日本語対応
public class MazeGeneratorSample : MonoBehaviour
{
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;
    [SerializeField] private GameObject _wall = null;
    [SerializeField] private GameObject _path = null;

    private void Start()
    {
        var maze = new HoleDigging(_width, _height);

        for (int w = 0; w < maze.GetWidth(); w++)
            for (int h = 0; h < maze.GetHeight(); h++)
            {
                GameObject go = maze[w, h] switch
                {
                    0 => _wall,
                    1 => _path,
                    _ => throw new System.Exception(),
                };
                Instantiate(go, new Vector3
                    (w - (_width / 2f) + 0.5f, 0, h - (_height / 2f) + 0.5f), Quaternion.identity, transform);
            }
    }
}
