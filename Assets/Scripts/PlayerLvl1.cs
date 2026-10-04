using UnityEngine;

public class PlayerLvl1 : MonoBehaviour
{
    public float speed = 5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Pobieramy komponenty z tego samego obiektu
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Pobiera wejście z klawiatury (A/D, strzałki -> -1, 0 lub 1)
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Tworzy wektor ruchu
        Vector3 move = new Vector3(moveInput, 0f, 0f);

        // Przesuwa obiekt
        transform.position += move * speed * Time.deltaTime;

        // --- ANIMACJA (Idle / Walk) ---
        // Jeśli moveInput jest różny od 0, to postać idzie (true), w przeciwnym razie stoi (false)
        bool isMoving = moveInput != 0;
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }

        // --- OBRACANIE SPRITE'A (Opcjonalnie) ---
        if (spriteRenderer != null)
        {
            if (moveInput > 0)
            {
                spriteRenderer.flipX = false; // Prawo
            }
            else if (moveInput < 0)
            {
                spriteRenderer.flipX = true;  // Lewo
            }
        }
    }
}