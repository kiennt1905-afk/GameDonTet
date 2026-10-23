using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OAnQuanUI : MonoBehaviour
{
    public OAnQuanManager manager;
    
    [Header("Bàn Cờ")]
    public Sprite boardSprite;
    public Transform[] squareContainers; 
    public Text[] squareTexts; 
    public Button[] squareButtons; 

    [Header("Thông tin")]
    public Text p1ScoreText;
    public Text p2ScoreText;
    public Text turnText;

    [Header("Các Màn Hình Flow")]
    public GameObject startPanel;
    public GameObject inGamePanel; 
    public GameObject directionPanel;
    public GameObject gameOverPanel;
    public Text winnerText;

    [Header("Prefabs Sỏi (Sprites)")]
    public Sprite stoneSprite;
    public Sprite mandarinSprite;

    [Header("Archer Animations")]
    public Sprite[] archerIdleSprites;
    public Sprite[] archerWalkSprites;
    public Sprite[] archerAtkSprites;
    public Sprite[] archerHitSprites;

    [Header("Hero (Quan) Animations")]
    public Sprite[] heroIdleSprites;
    public Sprite[] heroRunSprites;
    public Sprite[] heroAtkSprites;
    public Sprite[] heroDeathSprites;
    
    private List<GameObject>[] stonesOnBoard = new List<GameObject>[12];
    private List<GameObject> stonesInHand = new List<GameObject>();
    private Transform canvasTransform;
    private int selectedIndex = -1;
    public float animSpeed = 0.3f; 

    void Start()
    {
        if (manager != null)
        {
            manager.uiManager = this;
        }
        canvasTransform = FindAnyObjectByType<Canvas>().transform;
        
        for(int i=0; i<12; i++)
        {
            stonesOnBoard[i] = new List<GameObject>();
        }

        if (startPanel != null) startPanel.SetActive(true);
        if (inGamePanel != null) inGamePanel.SetActive(false);
        if (directionPanel != null) directionPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        SetInteractable(false);
    }

    public void StartGame()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (inGamePanel != null) inGamePanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        manager.InitBoard();
    }

    public void RestartGame()
    {
        ClearAllStones();
        StartGame();
    }

    public void EndGameEarly()
    {
        manager.ForceGameOver();
    }

    public void SetupInitialBoard()
    {
        for (int i = 0; i < 12; i++)
        {
            foreach(var s in stonesOnBoard[i]) Destroy(s);
            stonesOnBoard[i].Clear();

            int stoneCount = manager.board[i];
            
            if (i == 5 || i == 11)
            {
                if (stoneCount >= 10)
                {
                    SpawnStone(i, true); 
                    stoneCount -= 10;
                }
            }

            for (int j = 0; j < stoneCount; j++)
            {
                SpawnStone(i, false);
            }
            UpdateSquareText(i);
        }
        UpdateScoreAndTurn();
        SetInteractable(true);
    }

    private GameObject SpawnStone(int squareIndex, bool isMandarin)
    {
        GameObject stoneObj = new GameObject(isMandarin ? "Mandarin" : "Stone");
        stoneObj.transform.SetParent(squareContainers[squareIndex], false);
        
        Image img = stoneObj.AddComponent<Image>();
        
        bool useSingle = manager != null && manager.useSingleSprites;
        
        if (useSingle)
        {
            img.sprite = isMandarin ? manager.singleMandarinSprite : manager.singleStoneSprite;
            if (img.sprite == null) img.sprite = isMandarin ? mandarinSprite : stoneSprite; 
        }
        else
        {
            img.sprite = isMandarin ? mandarinSprite : stoneSprite;
        }

        img.color = Color.white; 

        RectTransform rect = stoneObj.GetComponent<RectTransform>();
        
        if (!useSingle)
        {
            ArcherStoneUI archerAnim = stoneObj.AddComponent<ArcherStoneUI>();
            
            if (!isMandarin)
            {
                float randX = Random.Range(-35f, 35f);
                float randY = Random.Range(-35f, 15f); 
                rect.anchoredPosition = new Vector2(randX, randY);
                rect.sizeDelta = new Vector2(60, 60);
                archerAnim.Init(archerIdleSprites, archerWalkSprites, archerAtkSprites, archerHitSprites);
            }
            else
            {
                rect.anchoredPosition = new Vector2(0, -20);
                rect.sizeDelta = new Vector2(90, 90);
                archerAnim.Init(heroIdleSprites, heroRunSprites, heroAtkSprites, heroDeathSprites);
            }
        }
        else
        {
            rect.sizeDelta = isMandarin ? new Vector2(90, 90) : new Vector2(60, 60);
            if (!isMandarin)
            {
                float randX = Random.Range(-35f, 35f);
                float randY = Random.Range(-35f, 15f); 
                rect.anchoredPosition = new Vector2(randX, randY);
            }
            else
            {
                rect.anchoredPosition = new Vector2(0, -20);
            }
        }

        stonesOnBoard[squareIndex].Add(stoneObj);
        return stoneObj;
    }

    public void ToggleTheme()
    {
        if (manager != null)
        {
            manager.useSingleSprites = !manager.useSingleSprites;
            
            GameObject boardGrid = GameObject.Find("BoardGrid");
            if (boardGrid != null)
            {
                Image boardImage = boardGrid.GetComponent<Image>();
                if (boardImage != null)
                {
                    if (manager.useSingleSprites && manager.singleBoardSprite != null)
                    {
                        boardImage.sprite = manager.singleBoardSprite;
                    }
                    else
                    {
                        boardImage.sprite = boardSprite != null ? boardSprite : (manager.mainBoardSprite != null ? manager.mainBoardSprite : null);
                    }
                }
            }

            // Reload stones to apply new theme
            ClearAllStones();
            SetupInitialBoard();
        }
    }

    public IEnumerator AnimatePickup(int squareIndex)
    {
        stonesInHand.Clear();
        List<GameObject> toPick = new List<GameObject>(stonesOnBoard[squareIndex]);
        stonesOnBoard[squareIndex].Clear();

        foreach (var stone in toPick)
        {
            stone.transform.SetParent(canvasTransform, true);
            stonesInHand.Add(stone);
            ArcherStoneUI archer = stone.GetComponent<ArcherStoneUI>();
            if (archer != null) archer.SetState(ArcherStoneUI.State.Walk);
        }
        
        UpdateSquareText(squareIndex);
        yield return new WaitForSeconds(animSpeed);
    }

    public IEnumerator AnimateDrop(int squareIndex)
    {
        if (stonesInHand.Count > 0)
        {
            GameObject stone = stonesInHand[0];
            stonesInHand.RemoveAt(0);

            stone.transform.SetParent(squareContainers[squareIndex], true);
            stonesOnBoard[squareIndex].Add(stone);
            
            ArcherStoneUI archer = stone.GetComponent<ArcherStoneUI>();
            if (archer != null) archer.SetState(ArcherStoneUI.State.Walk);

            Vector3 startPos = stone.transform.position;
            Vector3 endPos = squareContainers[squareIndex].position;
            endPos += new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);

            float time = 0;
            while(time < animSpeed)
            {
                if (stone == null) break;
                stone.transform.position = Vector3.Lerp(startPos, endPos, time / animSpeed);
                time += Time.deltaTime;
                yield return null;
            }
            if (stone != null) 
            {
                stone.transform.position = endPos;
                if (archer != null) archer.SetState(ArcherStoneUI.State.Idle);
            }
            UpdateSquareText(squareIndex);
        }
        yield return new WaitForSeconds(animSpeed * 0.5f); 
    }

    public IEnumerator AnimateCapture(int squareIndex, bool toPlayer1)
    {
        List<GameObject> toCapture = new List<GameObject>(stonesOnBoard[squareIndex]);
        stonesOnBoard[squareIndex].Clear();
        UpdateSquareText(squareIndex);

        Vector3 targetPos = toPlayer1 ? p1ScoreText.transform.position : p2ScoreText.transform.position;

        Vector3[] startPositions = new Vector3[toCapture.Count];
        for (int i = 0; i < toCapture.Count; i++)
        {
            if (toCapture[i] != null)
            {
                startPositions[i] = toCapture[i].transform.position;
                ArcherStoneUI archer = toCapture[i].GetComponent<ArcherStoneUI>();
                if (archer != null) archer.SetState(ArcherStoneUI.State.Hit);
            }
        }

        float time = 0;
        while (time < animSpeed * 2)
        {
            for (int i = 0; i < toCapture.Count; i++)
            {
                if (toCapture[i] != null)
                {
                    toCapture[i].transform.position = Vector3.Lerp(startPositions[i], targetPos, time / (animSpeed * 2));
                }
            }
            time += Time.deltaTime;
            yield return null;
        }

        foreach (var stone in toCapture)
        {
            if (stone != null) Destroy(stone);
        }
        
        UpdateScoreAndTurn();
        yield return new WaitForSeconds(animSpeed);
    }

    public IEnumerator AnimateRaiDan(int startIndex, bool isPlayer1Turn)
    {
        UpdateScoreAndTurn();
        
        for (int i = startIndex; i < startIndex + 5; i++)
        {
            GameObject newStone = SpawnStone(i, false);
            newStone.transform.position = isPlayer1Turn ? p1ScoreText.transform.position : p2ScoreText.transform.position;
            
            ArcherStoneUI archer = newStone.GetComponent<ArcherStoneUI>();
            if (archer != null) archer.SetState(ArcherStoneUI.State.Walk);

            Vector3 endPos = squareContainers[i].position;
            float time = 0;
            while(time < animSpeed)
            {
                if (newStone == null) break;
                newStone.transform.position = Vector3.Lerp(newStone.transform.position, endPos, time / animSpeed);
                time += Time.deltaTime;
                yield return null;
            }
            if (newStone != null && archer != null) archer.SetState(ArcherStoneUI.State.Idle);
            UpdateSquareText(i);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ClearAllStones()
    {
        for (int i = 0; i < 12; i++)
        {
            foreach(var s in stonesOnBoard[i]) 
            {
                if (s != null) Destroy(s);
            }
            stonesOnBoard[i].Clear();
            UpdateSquareText(i);
        }
        foreach(var s in stonesInHand)
        {
            if (s != null) Destroy(s);
        }
        stonesInHand.Clear();
    }

    private void UpdateSquareText(int index)
    {
        if (squareTexts.Length > index && squareTexts[index] != null)
        {
            int totalValue = 0;
            foreach(var s in stonesOnBoard[index])
            {
                if (s != null)
                {
                    if (s.name.Contains("Mandarin")) totalValue += 10;
                    else totalValue += 1;
                }
            }
            
            squareTexts[index].text = totalValue > 0 ? totalValue.ToString() : "";
        }
    }

    public void UpdateScoreAndTurn()
    {
        if (p1ScoreText != null) p1ScoreText.text = "Bạn: " + manager.p1Score;
        if (p2ScoreText != null) p2ScoreText.text = "Máy: " + manager.p2Score;

        if (turnText != null)
        {
            turnText.text = manager.isPlayer1Turn ? "Lượt của Bạn" : "Lượt của Máy";
        }
    }

    public void SetInteractable(bool interactable)
    {
        for (int i = 0; i < 12; i++)
        {
            if (squareButtons.Length > i && squareButtons[i] != null)
            {
                if (interactable && manager.isPlayer1Turn && i >= 0 && i <= 4 && manager.board[i] > 0)
                {
                    squareButtons[i].interactable = true;
                }
                else
                {
                    squareButtons[i].interactable = false;
                }
            }
        }
    }

    public void OnSquareClicked(int index)
    {
        if (!manager.isPlayer1Turn || manager.isGameOver) return;
        if (manager.board[index] == 0) return;

        selectedIndex = index;
        directionPanel.SetActive(true);
        directionPanel.GetComponent<RectTransform>().position = squareContainers[index].position + new Vector3(0, 80, 0);
        SetInteractable(false); 
    }

    public void OnDirectionSelected(bool isRight)
    {
        directionPanel.SetActive(false);
        int direction = isRight ? 1 : -1;
        
        manager.PlayerSelectMove(selectedIndex, direction);
        selectedIndex = -1;
    }

    public void ShowGameOver(int p1Score, int p2Score)
    {
        if (inGamePanel != null) inGamePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (winnerText != null)
        {
            if (p1Score > p2Score) winnerText.text = "BẠN THẮNG!";
            else if (p1Score < p2Score) winnerText.text = "MÁY THẮNG!";
            else winnerText.text = "HÒA NHAU!";
        }
    }

    public void BackTo3DScene()
    {
        PlayerPrefs.SetFloat("PlayerPosX", -24f);
        PlayerPrefs.SetFloat("PlayerPosY", 0.7f);
        PlayerPrefs.SetFloat("PlayerPosZ", 214f);
        PlayerPrefs.SetFloat("PlayerRotY", -360f);
        PlayerPrefs.SetInt("HasSavedPos", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Cho"); 
    }
}
