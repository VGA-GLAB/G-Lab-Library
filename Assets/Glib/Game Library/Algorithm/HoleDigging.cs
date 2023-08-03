using System;
using System.Collections.Generic;

// 日本語対応
namespace Glib
{
    namespace AutoGenerate
    {
        public class HoleDigging
        {
            /// <summary>行番号と列番号を入力すると指定された迷路の要素を取得することができる</summary>
            /// <param name="row">行番号</param>
            /// <param name="column">列番号</param>
            /// <returns>指定された迷路(2次元配列)の要素</returns>
            public int this[int row, int column]
            {
                get { return _maze[row, column]; }
                set { _maze[row, column] = value; }
            }
            private int[,] _maze = null;
            /// <summary>通路拡張開始地点候補</summary>
            private List<(int, int)> _startList = new List<(int, int)>();
            private Random _random = new();

            /// <summary>穴掘り法を用いて迷路を作成する<para>壁 -> 0 | 床 -> 1</para></summary>
            /// <param name="width">迷路の横幅</param>
            /// <param name="height">迷路の縦幅</param>
            /// <exception cref="ArgumentOutOfRangeException">横幅か縦幅の大きさが５未満</exception>
            public HoleDigging(int width, int height)
            {
                // 迷路の大きさが5未満だったら、エラーを出力する。
                if (width < 5 || height < 5) throw new ArgumentOutOfRangeException();
                // 縦(横)の長さが偶数だったら、奇数に変換する。
                width = width % 2 == 0 ? ++width : width;
                height = height % 2 == 0 ? ++height : height;

                // 迷路の情報を格納する。
                _maze = new int[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // 迷路の外周を床で埋めておく。
                        if (x * y == 0 || x == width - 1 || y == height - 1)
                        {
                            _maze[x, y] = _PATH;
                        }
                        // それ以外を壁で埋める。
                        else
                        {
                            _maze[x, y] = _WALL;
                        }
                    }
                }
                DiggingPath(_maze, (1, 1));

                // 拡張が終了したら、外周を壁で囲む。
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (x * y == 0 || x == width - 1 || y == height - 1)
                        {
                            _maze[x, y] = _WALL;
                        }
                    }
                }
            }

            private void DiggingPath(int[,] maze, (int, int) coodinate)
            {
                if (_startList.Count > 0) _startList.Remove(coodinate);

                int x = coodinate.Item1;
                int y = coodinate.Item2;

                while (true)
                {
                    // 拡張できる方向を格納するリスト
                    List<Direction> dirs = new(4);

                    if (maze[x, y - 1] == _WALL && maze[x, y - 2] == _WALL) dirs.Add(Direction.UP);
                    if (maze[x, y + 1] == _WALL && maze[x, y + 2] == _WALL) dirs.Add(Direction.DOWN);
                    if (maze[x - 1, y] == _WALL && maze[x - 2, y] == _WALL) dirs.Add(Direction.LEFT);
                    if (maze[x + 1, y] == _WALL && maze[x + 2, y] == _WALL) dirs.Add(Direction.RIGHT);

                    // 拡張できる方向がなくなったら、ループを抜ける。
                    if (dirs.Count == 0) break;
                    // 通路を設置する
                    SetPath(maze, x, y);
                    int dirsIndex = _random.Next(0, dirs.Count);

                    try
                    {
                        switch (dirs[dirsIndex])
                        {
                            case Direction.UP:
                                SetPath(maze, x, --y);
                                SetPath(maze, x, --y);
                                break;
                            case Direction.DOWN:
                                SetPath(maze, x, ++y);
                                SetPath(maze, x, ++y);
                                break;
                            case Direction.LEFT:
                                SetPath(maze, --x, y);
                                SetPath(maze, --x, y);
                                break;
                            case Direction.RIGHT:
                                SetPath(maze, ++x, y);
                                SetPath(maze, ++x, y);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                if (_startList.Count > 0)
                {
                    int random = _random.Next(0, _startList.Count);
                    DiggingPath(maze, _startList[random]);
                }
            }

            private void SetPath(int[,] maze, int x, int y)
            {
                maze[x, y] = _PATH;
                // x, yが共に奇数だったら、拡張開始座標候補のリストに追加する。
                if (x % 2 != 0 && y % 2 != 0)
                {
                    _startList.Add((x, y));
                }
            }

            public int GetWidth() => _maze.GetLength(0);
            public int GetHeight() => _maze.GetLength(1);

            private const int _WALL = 0;
            private const int _PATH = 1;

            private enum Direction : byte
            {
                UP = 0,
                DOWN = 1,
                LEFT = 2,
                RIGHT = 4,
            }
        }
    }
}