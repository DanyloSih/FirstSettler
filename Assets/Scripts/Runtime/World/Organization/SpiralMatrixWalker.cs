using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace World.Organization
{
    public class SpiralMatrixWalker : IMatrixWalker
    {
        public IEnumerable<Vector3Int> WalkMatrix(Vector3Int size)
        {
            foreach (var pos2D in WalkMatrix2D(new Vector2Int(size.x, size.z)))
            {
                for (int y = 0; y < size.y; y++)
                {
                    yield return new Vector3Int(pos2D.x, y, pos2D.y);
                }
            }    
        }

        public IEnumerable<Vector2Int> WalkMatrix2D(Vector2Int size)
        {
            List<Vector2Int> cash = new List<Vector2Int>();
            int centerX = size.x / 2;
            int centerY = size.y / 2;

            int distance = 1;
            int x = centerX;
            int y = centerY;

            yield return new Vector2Int(x, y);
            
            while (true)
            {
                int breaksCount = 0;
                for (int i = 0; i < distance; i++)
                {
                    x--;
                    if (IsValidIndex(x, y, size, cash))
                    {
                        cash.Add(new Vector2Int(x, y));
                        yield return new Vector2Int(x, y);
                    }
                    else
                    {
                        breaksCount++;
                        break;
                    }
                }

                for (int i = 0; i < distance; i++)
                {
                    y--;
                    if (IsValidIndex(x, y, size, cash))
                    {
                        cash.Add(new Vector2Int(x, y));
                        yield return new Vector2Int(x, y);
                    }
                    else
                    {
                        breaksCount++;
                        break;
                    }
                }

                for (int i = 0; i < distance + 1; i++)
                {
                    x++;
                    if (IsValidIndex(x, y, size, cash))
                    {
                        cash.Add(new Vector2Int(x, y));
                        yield return new Vector2Int(x, y);
                    }
                    else
                    {
                        breaksCount++;
                        break;
                    }
                }

                for (int i = 0; i < distance + 1; i++)
                {
                    y++;
                    if (IsValidIndex(x, y, size, cash))
                    {
                        cash.Add(new Vector2Int(x, y));
                        yield return new Vector2Int(x, y);
                    }
                    else
                    {
                        breaksCount++;
                        break;
                    }
                }

                distance += 2;
                if (breaksCount == 4)
                {
                    break;
                }
            }
        }

        private bool IsValidIndex(int x, int y, Vector2Int size, List<Vector2Int> cash)
        {
            return x >= 0 && x < size.x && y >= 0 && y < size.y && !cash.Any(a => a.Equals(new Vector2Int(x, y)));
        }
    }
}
