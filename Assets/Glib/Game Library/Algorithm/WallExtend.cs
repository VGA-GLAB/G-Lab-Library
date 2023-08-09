using System;
using System.Collections.Generic;

namespace Glib
{
    namespace AutoGenerate
    {
        public class WallExtend
        {
            /// <summary>行番号と列番号を入力すると指定された迷路の要素を取得することができる</summary>
            /// <param name="row">行番号</param>
            /// <param name="column">列番号</param>
            /// <returns>指定された迷路(2次元配列)の要素</returns>
            public int this[int row, int column]
            {
                get => _maze[row, column];
                set => _maze[row, column] = value;
            }

            private int[,] _maze = null;
            /// <summary>壁生成開始地点</summary>
            private List<(int, int)> _startPoint = new List<(int, int)>();
            /// <summary>拡張中の壁の情報を格納する</summary>
            private Stack<(int, int)> _currentWall = new Stack<(int, int)>();
            private Random _random = new Random();

            /// <summary>壁伸ばし法を用いて迷路の2次元配列を作成する<para>壁 -> 0 | 床 -> 1</para></summary>
            /// <param name="width">迷路の横幅</param>
            /// <param name="height">迷路の縦幅</param>
            /// <exception cref="ArgumentOutOfRangeException">横幅か縦幅の大きさが５未満</exception>
            public WallExtend(int width, int height)
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
                        // 外周を壁で囲む。
                        if (x * y == 0 || x == width - 1 || y == height - 1)
                        {
                            _maze[x, y] = _WALL;
                        }
                        // 外周以外は床で埋める。
                        else
                        {
                            _maze[x, y] = _PATH;
                            // x, y共に偶数の座標をリストに追加する。
                            if (x % 2 == 0 && y % 2 == 0)
                            {
                                _startPoint.Add((x, y));
                            }
                        }
                    }
                }
                ExtendWall(_maze, _startPoint);
            }

            /// <summary>壁を拡張する</summary>
            private void ExtendWall(int[,] maze, List<(int, int)> startPoint)
            {
                int index = _random.Next(0, startPoint.Count);
                int x = startPoint[index].Item1;
                int y = startPoint[index].Item2;
                startPoint.RemoveAt(index);
                bool isFloor = true;

                while (isFloor)
                {
                    // 拡張できる方向を格納するリスト
                    List<Direction> dirs = new List<Direction>();

                    if (maze[x, y - 1] == _PATH && !IsCurrentWall(x, y - 2)) dirs.Add(Direction.UP);
                    if (maze[x, y + 1] == _PATH && !IsCurrentWall(x, y + 2)) dirs.Add(Direction.DOWN);
                    if (maze[x - 1, y] == _PATH && !IsCurrentWall(x - 2, y)) dirs.Add(Direction.LEFT);
                    if (maze[x + 1, y] == _PATH && !IsCurrentWall(x + 2, y)) dirs.Add(Direction.RIGHT);
                    // 拡張する方向が見つからなかったら、ループを抜ける
                    if (dirs.Count == 0) break;
                    // 壁を設置する
                    SetWall(maze, x, y);
                    int dirsIndex = _random.Next(0, dirs.Count);
                    switch (dirs[dirsIndex])
                    {
                        case Direction.UP:
                            isFloor = maze[x, y - 2] == _PATH;
                            SetWall(maze, x, --y);
                            SetWall(maze, x, --y);
                            break;
                        case Direction.DOWN:
                            isFloor = maze[x, y + 2] == _PATH;
                            SetWall(maze, x, ++y);
                            SetWall(maze, x, ++y);
                            break;
                        case Direction.LEFT:
                            isFloor = maze[x - 2, y] == _PATH;
                            SetWall(maze, --x, y);
                            SetWall(maze, --x, y);
                            break;
                        case Direction.RIGHT:
                            isFloor = maze[x + 2, y] == _PATH;
                            SetWall(maze, ++x, y);
                            SetWall(maze, ++x, y);
                            break;
                    }
                }
                // 拡張できるポイントがまだあったら拡張を続ける。
                if (startPoint.Count > 0)
                {
                    _currentWall.Clear();
                    ExtendWall(maze, startPoint);
                }
            }

            /// <summary>壁を設置する</summary>
            private void SetWall(int[,] maze, int x, int y)
            {
                maze[x, y] = _WALL;
                // x, yが共に偶数だったら、リストから削除し、スタックに格納する。
                if (x % 2 == 0 && y % 2 == 0)
                {
                    _startPoint.Remove((x, y));
                    _currentWall.Push((x, y));
                }
            }

            /// <summary>受け取った座標が拡張中の壁かどうか判定する</summary>
            /// <returns>true -> 拡張中 | false -> 拡張済</returns>
            private bool IsCurrentWall(int x, int y)
            {
                return _currentWall.Contains((x, y));
            }

            public int GetWidth() => _maze.GetLength(0);

            public int GetHeight() => _maze.GetLength(1);

            private const int _WALL = 0;
            private const int _PATH = 1;

            private enum Direction
            {
                UP = 0,
                DOWN = 1,
                LEFT = 2,
                RIGHT = 4,
            }
        }
    }
}
