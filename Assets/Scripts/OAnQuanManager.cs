using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OAnQuanManager : MonoBehaviour
{
    public int[] board = new int[12];
    public int p1Score = 0;
    public int p2Score = 0;
    public bool isPlayer1Turn = true;
    public bool isGameOver = false;

    [Header("Theme Sprites")]
    public bool useSingleSprites = false;
    public Sprite mainBoardSprite;
    public Sprite singleBoardSprite;
    public Sprite singleStoneSprite;
    public Sprite singleMandarinSprite;

    public OAnQuanUI uiManager;

    [Header("Audio")]
    public AudioClip hitSound;

    void Start()
    {
        // Hiện lại chuột và bỏ khóa để người chơi thao tác UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Tạm thời không gọi InitBoard ở đây nữa, UI sẽ gọi khi bấm Bắt Đầu
    }

    public void InitBoard()
    {
        for (int i = 0; i < 12; i++)
        {
            if (i == 5 || i == 11) board[i] = 10;
            else board[i] = 5;
        }
        p1Score = 0;
        p2Score = 0;
        isPlayer1Turn = true;
        isGameOver = false;
        
        if (uiManager != null) uiManager.SetupInitialBoard();
    }

    public void PlayerSelectMove(int index, int direction)
    {
        if (isGameOver || !isPlayer1Turn) return;
        if (index < 0 || index > 4 || board[index] == 0) return;

        StartCoroutine(PlayTurn(index, direction));
    }

    IEnumerator PlayTurn(int startIndex, int direction)
    {
        if (uiManager != null) uiManager.SetInteractable(false);

        int currentIndex = startIndex;
        bool turnEnded = false;

        while (!turnEnded && !isGameOver)
        {
            int stones = board[currentIndex];
            board[currentIndex] = 0;
            
            if (uiManager != null) yield return StartCoroutine(uiManager.AnimatePickup(currentIndex));

            while (stones > 0 && !isGameOver)
            {
                currentIndex = (currentIndex + direction + 12) % 12;
                board[currentIndex]++;
                stones--;
                
                if (uiManager != null) yield return StartCoroutine(uiManager.AnimateDrop(currentIndex));
            }

            if (isGameOver) break;

            int nextIndex = (currentIndex + direction + 12) % 12;

            if (board[nextIndex] > 0)
            {
                if (nextIndex == 5 || nextIndex == 11)
                {
                    turnEnded = true;
                }
                else
                {
                    currentIndex = nextIndex;
                }
            }
            else
            {
                int captureIndex = (nextIndex + direction + 12) % 12;
                if (board[captureIndex] > 0)
                {
                    while (board[nextIndex] == 0 && board[captureIndex] > 0 && !isGameOver)
                    {
                        int capturedStones = board[captureIndex];
                        board[captureIndex] = 0;

                        if (isPlayer1Turn) p1Score += capturedStones;
                        else p2Score += capturedStones;

                        if (hitSound != null)
                        {
                            AudioSource.PlayClipAtPoint(hitSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
                        }

                        if (uiManager != null) yield return StartCoroutine(uiManager.AnimateCapture(captureIndex, isPlayer1Turn));

                        nextIndex = (captureIndex + direction + 12) % 12;
                        captureIndex = (nextIndex + direction + 12) % 12;
                    }
                    turnEnded = true;
                }
                else
                {
                    turnEnded = true;
                }
            }
        }

        CheckGameOver();

        if (!isGameOver)
        {
            isPlayer1Turn = !isPlayer1Turn;
            yield return StartCoroutine(CheckAndRaiDan());
            
            if (uiManager != null) uiManager.UpdateScoreAndTurn();

            if (!isPlayer1Turn)
            {
                StartCoroutine(BotPlay());
            }
            else
            {
                if (uiManager != null) uiManager.SetInteractable(true);
            }
        }
    }

    IEnumerator CheckAndRaiDan()
    {
        int start = isPlayer1Turn ? 0 : 6;
        bool isEmpty = true;
        for (int i = start; i < start + 5; i++)
        {
            if (board[i] > 0) isEmpty = false;
        }

        if (isEmpty)
        {
            if (isPlayer1Turn) p1Score -= 5;
            else p2Score -= 5;

            for (int i = start; i < start + 5; i++)
            {
                board[i] = 1;
            }
            
            if (uiManager != null)
            {
                uiManager.UpdateScoreAndTurn();
                yield return StartCoroutine(uiManager.AnimateRaiDan(start, isPlayer1Turn));
            }
        }
    }

    void CheckGameOver()
    {
        if (board[5] == 0 && board[11] == 0)
        {
            ForceGameOver();
        }
    }

    public void ForceGameOver()
    {
        if (isGameOver) return; 
        isGameOver = true;
        StopAllCoroutines(); 

        for (int i = 0; i <= 5; i++) p1Score += board[i]; 
        for (int i = 6; i <= 11; i++) p2Score += board[i]; 

        for (int i = 0; i < 12; i++) board[i] = 0;

        if (uiManager != null)
        {
            uiManager.ClearAllStones();
            uiManager.UpdateScoreAndTurn();
            uiManager.ShowGameOver(p1Score, p2Score);
        }
    }

    IEnumerator BotPlay()
    {
        yield return new WaitForSeconds(1.0f);
        if (isGameOver) yield break;

        List<int> validMoves = new List<int>();
        for (int i = 6; i < 11; i++)
        {
            if (board[i] > 0) validMoves.Add(i);
        }

        if (validMoves.Count > 0)
        {
            int randomSquare = validMoves[Random.Range(0, validMoves.Count)];
            int randomDirection = Random.Range(0, 2) == 0 ? 1 : -1;

            StartCoroutine(PlayTurn(randomSquare, randomDirection));
        }
    }
}
