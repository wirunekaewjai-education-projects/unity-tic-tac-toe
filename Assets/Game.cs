using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour, ISlotClick
{
    // Range : 2 to 46,340 (Square root of 2,147,483,647)
    public const int LENGTH = 5;
    public const int MAX_TURNS = LENGTH * LENGTH;

    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    private Font font;

    [SerializeField]
    private GridLayoutGroup gridPanel;
    private RectTransform gridTransform;

    [SerializeField]
    private GameObject resultPanel;

    private Slot[,] slots;
    private int turnCount;

    #region SETUP
    private void Start()
    {
        SetupRule();
        SetupGrid();
        SetupSlot();
    }

    private void SetupRule()
    {
        turnCount = 0;
    }

    private void SetupGrid()
    {
        gridTransform = gridPanel.transform as RectTransform;

        gridPanel.childAlignment = TextAnchor.MiddleCenter;
        gridPanel.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridPanel.constraintCount = LENGTH;
    }

    private void SetupSlot()
    {
        slots = new Slot[LENGTH, LENGTH];
        for (int y = 0; y < LENGTH; y++)
        {
            for (int x = 0; x < LENGTH; x++)
            {
                slots[x, y] = new Slot(sprite, font, x, y);
                slots[x, y].SetParent(gridPanel.transform);
                slots[x, y].SetOnSlotClick(this);
            }
        }
    }
    #endregion

    #region UPDATE
    private void LateUpdate()
    {
        UpdateGrid();
    }
    
    private void UpdateGrid()
    {
        Vector2 rectSize = gridTransform.rect.size;

        float minSize = Mathf.Min(rectSize.x, rectSize.y);
        float eachSize = minSize / LENGTH;

        gridPanel.cellSize = new Vector2(eachSize, eachSize);
    }
    #endregion

    #region GET_RESULT
    private string GetResult()
    {
        return  GetHorizontalResult() ?? GetVerticalResult() ??
                GetDiagonalResult() ?? string.Empty;
    }

    private string GetHorizontalResult()
    {
        for (int y = 0; y < LENGTH; y++)
        {
            string firstWord = slots[0, y].GetText();

            if (string.IsNullOrEmpty(firstWord))
                break;

            bool finished = true;
            for (int x = 1; x < LENGTH; x++)
            {
                string otherWord = slots[x, y].GetText();
                if (!string.Equals(firstWord, otherWord))
                {
                    finished = false;
                    break;
                }
            }

            if (finished)
            {
                return firstWord;
            }
        }

        return null;
    }

    private string GetVerticalResult()
    {
        for (int x = 0; x < LENGTH; x++)
        {
            string firstWord = slots[x, 0].GetText();

            if (string.IsNullOrEmpty(firstWord))
                break;

            bool finished = true;
            for (int y = 0; y < LENGTH; y++)
            {
                string otherWord = slots[x, y].GetText();
                if (!string.Equals(firstWord, otherWord))
                {
                    finished = false;
                    break;
                }
            }

            if (finished)
            {
                return firstWord;
            }
        }

        return null;
    }

    private string GetDiagonalResult()
    {
        // Diagonal Check #1 : 00, 11, 22, 33, 44, ...
        string firstWord = slots[0, 0].GetText();

        if (!string.IsNullOrEmpty(firstWord))
        {
            bool finished = true;
            for (int xy = 1; xy < LENGTH; xy++)
            {
                string otherWord = slots[xy, xy].GetText();
                if (!string.Equals(firstWord, otherWord))
                {
                    finished = false;
                    break;
                }
            }

            if (finished)
            {
                return firstWord;
            }
        }

        // Diagonal Check #2 : 40, 31, 22, 13, 04
        int lastIndex = LENGTH - 1;
        firstWord = slots[lastIndex, 0].GetText();

        if (!string.IsNullOrEmpty(firstWord))
        {
            bool finished = true;
            for (int y = 1; y < LENGTH; y++)
            {
                int x = lastIndex - y;

                string otherWord = slots[x, y].GetText();
                if (!string.Equals(firstWord, otherWord))
                {
                    finished = false;
                    break;
                }
            }

            if (finished)
            {
                return firstWord;
            }
        }

        return null;
    }
    #endregion

    #region SET_RESULT
    private void SetResult(string text)
    {
        Transform child = resultPanel.transform.GetChild(0);
        Text resultText = child.GetComponent<Text>();
        resultText.text = text;

        for (int y = 0; y < LENGTH; y++)
        {
            for (int x = 0; x < LENGTH; x++)
            {
                slots[x, y].SetClickable(false);
            }
        }

        Invoke("OnShowResult", 1.5f);
    }

    private void OnShowResult()
    {
        gridPanel.gameObject.SetActive(false);
        resultPanel.SetActive(true);

        Invoke("OnRestart", 3);
    }
    #endregion

    #region RESTART
    private void OnRestart()
    {
        int level = Application.loadedLevel;
        Application.LoadLevel(level);
    }
    #endregion

    #region INTERACTION
    public void OnClick(int x, int y)
    {
        if (turnCount >= MAX_TURNS)
            return;

        string text = (turnCount % 2 == 0) ? "X" : "O";
        slots[x, y].SetText(text);

        turnCount += 1;

        string result = GetResult();
        if(!string.IsNullOrEmpty(result))
        {
            SetResult(result + " WIN !!!");
        }
        else if(turnCount >= MAX_TURNS)
        {
            SetResult("DRAWWW !!!");
        }
    }
    #endregion
}

class Slot
{
    private Button button;
    private Text text;
    
    private Sprite sprite;
    private Font font;
    private int x, y;

    private ISlotClick iSlotClick;

    public Slot(Sprite sprite, Font font, int x, int y)
    {
        this.sprite = sprite;
        this.font = font;
        this.x = x;
        this.y = y;

        SetupButton();
        SetupText();
    }

    private void SetupButton()
    {
        GameObject obj = new GameObject("Button [" + x + ", " + y + "]");
        button = obj.AddComponent<Button>();
        button.onClick.AddListener(OnClick);

        Image image = obj.AddComponent<Image>();
        image.type = Image.Type.Sliced;
        image.sprite = sprite;

        button.targetGraphic = image;

        obj.layer = LayerMask.NameToLayer("UI");
    }

    private void SetupText()
    {
        GameObject obj = new GameObject("Text");
        text = obj.AddComponent<Text>();
        text.transform.SetParent(button.transform);
        text.font = font;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.black;
        text.resizeTextForBestFit = true;
        text.resizeTextMaxSize = 120;
        text.text = string.Empty;

        ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform rt = text.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
    }

    private void OnClick()
    {
        if (null != iSlotClick)
        {
            button.interactable = false;
            iSlotClick.OnClick(x, y);
        }
    }

    public void SetOnSlotClick(ISlotClick iSlotClick)
    {
        this.iSlotClick = iSlotClick;
    }

    public void SetParent(Transform parent)
    {
        button.transform.SetParent(parent);
        button.transform.localScale = Vector3.one;
    }

    public string GetText()
    {
        return this.text.text;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetClickable(bool clickable)
    {
        this.button.interactable = clickable;
    }
}

interface ISlotClick
{
    void OnClick(int x, int y);
}
