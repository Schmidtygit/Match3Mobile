using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{  
    [SerializeField]
    private Sprite[] sprites;

    private int type;
    public int Type { get => type; }

    private SpriteRenderer sr;
    private Vector3 desiredPosition;
    private static float fallSpeed = 10;
    public State state { get; private set; }

    public static bool busy;
    int direction;

    public enum State
    {
        Normal,
        Drag,
        Spawning
    }

    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private void Update()
    {
        if (Board.Freeze)
            return;

        if (state == State.Normal)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredPosition, fallSpeed * Time.deltaTime);


        }
        else if (state == State.Drag)
        {
            Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (direction == 1)
            {
                transform.position = new Vector3(desiredPosition.x, m.y);
            }
            else if (direction == 2)
            {
                transform.position = new Vector3(m.x, desiredPosition.y);
            }
            else
            {
                //transform.position = new Vector3(m.x, m.y);
            }

            Board.CheckForSwap(this);
        }
        
    }

    public void Randomize()
    {
        sr = GetComponent<SpriteRenderer>();
        type = Random.Range(0, sprites.Length);
        sr.sprite = sprites[type];
        state = State.Normal;

    }

    public int GetShape()
    {
        return type;
    }

    public void SetPosition(Vector3 pos)
    {
        desiredPosition = pos;
    }

    public Vector3 GetPosition()
    {
        return desiredPosition;
    }

    private void OnMouseDown()
    {
        if (!busy)
        {
            state = State.Drag;
            busy = true;
            direction = 0;
            Board.StartBackup();
            StartCoroutine(GetDirectionOfDrag());
        }
    }

    private void OnMouseUp()
    {
        state = State.Normal;
        busy = false;

        Board.OnPlace();
    }

    public void OnMatch()
    {
        Destroy(gameObject);
    }

    IEnumerator GetDirectionOfDrag()
    {
        while (busy)
        {
            Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int pos1 = Board.WorldToSlotPos(desiredPosition);
            Vector2Int pos2 = Board.WorldToSlotPos(m);

            if (pos1 != pos2)
            {
                Vector3 v3 = m - desiredPosition;
                if (Mathf.Abs(v3.y) > Mathf.Abs(v3.x))
                    direction = 1;
                else
                    direction = 2;
                break;
            }

            yield return new WaitForEndOfFrame();

            
        }
    }

    IEnumerator Spawn()
    {
        transform.localScale = Vector3.zero;

        for (int i = 0; i < 25; i++)
        {
            yield return new WaitForSeconds(0.01f);

            transform.localScale += Vector3.one * (1f / 20f);
        }

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.01f);

            transform.localScale -= Vector3.one * (1f / 20f);
        }

    }

    
}
