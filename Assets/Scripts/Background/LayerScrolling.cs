using UnityEngine;

public class LayerScrolling : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform pieceA;
    [SerializeField] private Transform pieceB;
    [SerializeField] private Transform pieceC;

    [Header("Values")]
    [SerializeField] private float scrollSpeed = 0.5f;

    private Transform[] pieces;
    private float pieceWidth;

    private void Start()
    {
        pieces = new[] { pieceA, pieceB, pieceC };

        SpriteRenderer sr = pieceA.GetComponent<SpriteRenderer>();
        pieceWidth = sr.bounds.size.x;

        pieceB.position = pieceA.position + Vector3.right * pieceWidth;
        pieceC.position = pieceB.position + Vector3.right * pieceWidth;
    }

    private void Update()
    {
        if (scrollSpeed > 0)
        {
            Vector3 movement = scrollSpeed * Time.deltaTime * Vector3.left;

            foreach (Transform piece in pieces)
            {
                piece.position += movement;
            }
        }

        float cameraX = Camera.main.transform.position.x;
        float leftLimit = cameraX - pieceWidth * 1.5f;
        float rightLimit = cameraX + pieceWidth * 1.5f;

        foreach (Transform piece in pieces)
        {
            if (piece.position.x < leftLimit)
            {
                Transform rightmost = GetRightmostPiece();
                piece.position = rightmost.position + Vector3.right * pieceWidth;
            }
            else if (piece.position.x > rightLimit)
            {
                Transform leftmost = GetLeftmostPiece();
                piece.position = leftmost.position + Vector3.left * pieceWidth;
            }
        }
    }

    private Transform GetRightmostPiece()
    {
        Transform rightmost = pieces[0];

        foreach (Transform piece in pieces)
        {
            if (piece.position.x > rightmost.position.x)
                rightmost = piece;
        }

        return rightmost;
    }

    private Transform GetLeftmostPiece()
    {
        Transform leftmost = pieces[0];

        foreach (Transform piece in pieces)
        {
            if (piece.position.x < leftmost.position.x)
                leftmost = piece;
        }

        return leftmost;
    }
}
