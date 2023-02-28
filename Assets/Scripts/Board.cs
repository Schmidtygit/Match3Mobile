using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    [SerializeField]
    private Piece piecePrefab;
    private static Piece[,] slots;

    public const int Offset = -3;

    private static Piece[,] backup;

    private static List<Piece> swapList = new List<Piece>();

    // Start is called before the first frame update
    void Start()
    {
        slots = new Piece[7, 6];
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                slots[i, j] = Instantiate(piecePrefab);
                slots[i, j].transform.position = new Vector3(j + Offset, i + Offset, 0);
                slots[i, j].Randomize();
                slots[i, j].SetPosition(new Vector3(j + Offset, i + Offset, 0));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void CheckForSwap(Piece p1)
    {
        if (IsDifferentPieceInPosition(p1, p1.transform.position))
        {
            Piece p2 = null;
            if (IsPieceInPosition(p1.transform.position, out p2))
            {
                Swap(p1, p2);
            }
            
        }
    }

    public static void Swap(Piece p1, Piece p2)
    {
        Vector2Int pos1 = SlotPosition(p1);
        Vector2Int pos2 = SlotPosition(p2);

        Vector3 v3 = p1.GetPosition();
        p1.SetPosition(p2.GetPosition());
        p2.SetPosition(v3);

        slots[pos1.x, pos1.y] = p2;
        slots[pos2.x, pos2.y] = p1;

        if (!swapList.Contains(p1)) swapList.Add(p1);
        if (!swapList.Contains(p2)) swapList.Add(p2);
    }

    public static Vector2Int SlotPosition(Piece p)
    {
        for (int i = 0; i < slots.GetLength(0); i++)
        {
            for (int j = 0; j < slots.GetLength(1); j++)
            {
                if (slots[i, j] == p)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public static bool IsDifferentPieceInPosition(Piece p1, Vector3 v3)
    {
        Piece p2 = null;
        if (IsPieceInPosition(v3, out p2))
        {
            if (p1 != p2)
            {
                return true;
            }
            else return false;
        }

        return false;
    }

    public static Vector2Int WorldToSlotPos(Vector3 v3)
    {
        Vector2Int v2 = new Vector2Int(Mathf.RoundToInt(v3.y), Mathf.RoundToInt(v3.x));
        return v2;
    }

    public static bool IsPieceInPosition(Vector3 v3, out Piece p2)
    {
        Vector2Int vint = new Vector2Int(Mathf.RoundToInt(v3.y) - Offset, Mathf.RoundToInt(v3.x) - Offset);

        if (vint.x < slots.GetLength(0) && vint.x >= 0 &&
            vint.y < slots.GetLength(1) && vint.y >= 0)
        {
            p2 = slots[vint.x, vint.y];
            return true;
        }
        else
        {
            p2 = null;
            return false;
        }
    }

    public static void StartBackup()
    {
        backup = new Piece[slots.GetLength(0), slots.GetLength(1)];
        for (int i = 0; i < slots.GetLength(0); i++)
        {
            for (int j = 0; j < slots.GetLength(1); j++)
            {

                backup[i, j] = slots[i, j];
                
            }
        }
    }

    public static void RestoreFromBackup()
    {
        slots = new Piece[slots.GetLength(0), slots.GetLength(1)];
        for (int i = 0; i < slots.GetLength(0); i++)
        {
            for (int j = 0; j < slots.GetLength(1); j++)
            {

                slots[i, j] = backup[i, j];
                
            }
        }
    }

    public static List<Piece> CheckForMatches(Piece p)
    {
        List<Piece> pieces = new List<Piece>();
        Vector2Int v2 = SlotPosition(p);

        int horCount = 0;

        for (int i = v2.x + 1; i < slots.GetLength(0); i++)
        {
            if (slots[i, v2.y].GetShape() == p.GetShape())
            {
                pieces.Add(slots[i, v2.y]);

                horCount++;
            }
            else break;
        }

        for (int i = v2.x - 1; i < slots.GetLength(0); i--)
        {
            if (slots[i, v2.y].GetShape() == p.GetShape())
            {
                pieces.Add(slots[i, v2.y]);

                horCount++;
            }
            else break;
        }

        if (horCount < 2)
        {
            for (int i = 0; i < horCount; i++)
            {
                pieces.RemoveAt(pieces.Count - 1);
            }
        }

        int verCount = 0;

        for (int i = v2.y + 1; i < slots.GetLength(1); i++)
        {
            if (slots[v2.x, i].GetShape() == p.GetShape())
            {
                pieces.Add(slots[v2.x, i]);

                horCount++;
            }
            else break;
        }

        for (int i = v2.y - 1; i < slots.GetLength(1); i--)
        {
            if (slots[v2.x, i].GetShape() == p.GetShape())
            {
                pieces.Add(slots[v2.x, i]);

                horCount++;
            }
            else break;
        }

        if (verCount < 2)
        {
            for (int i = 0; i < verCount; i++)
            {
                pieces.RemoveAt(pieces.Count - 1);
            }
        }

        return pieces;
    }

    public static bool OnPlace()
    {
        bool result = false;

        List<Piece> pieces = new List<Piece>();

        foreach (Piece swap in swapList)
        {
            List<Piece> temp = CheckForMatches(swap);

            if (temp.Count >= 3)
            {
                foreach (Piece t in temp)
                {
                    if (!pieces.Contains(t))
                    {
                        pieces.Add(t);
                    }
                }
            }
        }

        result = pieces.Count >= 3;

        if (result)
        {
            foreach(Piece piece in pieces)
            {
                piece.OnMatch();
            }
        }

        return result;
    }
}
