using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Minesweeper
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject gameWinScreen;
        [SerializeField] Transform gridParent;
        [SerializeField] Text timerText;
        [SerializeField] Text counterText;

        private int _totalRows = 9;
        private int _totalCols = 9;
        private int _totalMines = 10;
        private List<Cell> _cellObjects = new List<Cell>();
        private List<Cell> _activeMines = new List<Cell>();

        private bool _timerRunning = false;
        private System.DateTime _gameStartTime;
        private int _markedCount = 0;

        public bool GameOver { get; set; }

        private int _replayCount = 0;

        private void Start() 
        {
            // Reset();
        }

        public void ReloadGame()
        {
            _replayCount++;
            if(_replayCount>=3)
            {
                _replayCount = 0;
                AdController.Instance.ShowInterstitialAd();
            }
            Reset(_totalRows, _totalCols, _totalMines);
        }
        public void Reset(int r, int c, int m)
        {
            _totalRows = r;
            _totalCols = c;
            _totalMines = m;
            gameWinScreen.SetActive(false);
            GameOver = false;
            _timerRunning = false;
            timerText.text = "00:00";
            counterText.text = _totalMines.ToString();
            _markedCount = 0;
            CreateGrid();
            AssignMines();
            AssignNumbers();

            AdController.Instance.LoadInterstitialAd();
        }

        private void CreateGrid()
        {
            ClearGrid();
            RectTransform parentRT = gridParent.GetComponent<RectTransform>();
            for (int i = 0; i < _totalRows; i++)
            {
                for (int j = 0; j < _totalCols; j++)
                {
                    GameObject cellObj = Instantiate(Resources.Load<GameObject>("Cell"), gridParent);
                    Cell cell = cellObj.GetComponent<Cell>();
                    cell.Init(i, j, this);
                    _cellObjects.Add(cell);

                    RectTransform childRT = cellObj.GetComponent<RectTransform>();
                    float sideLength = parentRT.rect.width / _totalCols;
                    childRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sideLength);
                    childRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sideLength);

                    childRT.anchoredPosition = new Vector2(j * sideLength, i * sideLength * -1);            

                }
            }
        }

        private void AssignMines()
        {
            //first assign random bombs
            Cell randomCell = _cellObjects[Random.Range(0, _cellObjects.Count)];
            int minesAssigned = 0;
            
            while( minesAssigned < _totalMines)
            {
                if(!_activeMines.Contains(randomCell))
                {
                    _activeMines.Add(randomCell);
                    randomCell.IsMine = true;
                    minesAssigned++;
                }
                randomCell = _cellObjects[Random.Range(0, _cellObjects.Count)];
            }
        }

        private void AssignNumbers()
        { 
            for (int i = 0; i < _cellObjects.Count; i++)
            {
                Cell cell = _cellObjects[i];
                List<Cell> neighbours = GetNeighbours(cell.Row, cell.Col);
                int mineCount = neighbours.Where(c => c.IsMine).ToList().Count;
                cell.NumValue = mineCount;
            }
        }

        private void ClearGrid()
        {
            _activeMines.Clear();
            foreach (var item in _cellObjects)
            {
                Destroy(item.gameObject);
            }
            _cellObjects.Clear();
        }


        private void OnCellOpen()
        { 
            //If all cells are opened 
            //Check if all marked cells and mine cells match
            //
        }

        public void OpenAllCells()
        {
            GameOver = true;
            _timerRunning = false;
            foreach (var item in _cellObjects)
            {
                item.ForceOpenCell();
            }

            //Get all marked cell and get get wrong mark count
            List<Cell> markedCells = _cellObjects.Where(c => c.State.Equals(CellState.MARKED)).ToList();
            List<Cell> wrongCells = markedCells.Except(_activeMines).ToList();

            if(markedCells.Count == _totalMines && wrongCells.Count == 0)
            {
                GameStateController.CurrentGameState = GameState.GAME_END;
                gameWinScreen.SetActive(true);
            }
            else
            {
                foreach (var item in wrongCells)
                {
                    item.Verify();
                }
            }
        }

        public void CellMarked(bool marked)
        { 
            if(marked)
                _markedCount++;
            else
                _markedCount--;
        }

        public void StartTimer()
        {
            if (!_timerRunning)
            {
                _timerRunning = true;
                _gameStartTime = System.DateTime.Now;
            }
        }
        private void Update() {
            
            //check for game over
            if(!GameOver && DidGameEnd())
            {
                OpenAllCells();
            }

            if(_timerRunning && !GameOver)
            {
                System.TimeSpan span = System.DateTime.Now - _gameStartTime;
                string h = span.Hours <= 0 ? "" : span.Hours <= 9 ? "0" + span.Hours + ":" : span.Hours + ":";
                string m = span.Minutes <= 9 ? "0" + span.Minutes + ":" : span.Minutes + ":";
                string s = span.Seconds <= 9 ? "0" + span.Seconds : span.Seconds.ToString();
                timerText.text = string.Format("{0}{1}{2}", h, m, s);

                counterText.text = (_totalMines - _markedCount).ToString();
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(GameStateController.CurrentGameState.Equals(GameState.GAME))
                {
                    GoToMenu();
                }
            }
        }

        private bool DidGameEnd()
        {
            List<Cell> closedCells = _cellObjects.Where(c => c.State.Equals(CellState.CLOSE)).ToList();
            return closedCells.Count == 0;
        }
        public void GoToMenu()
        {
            _replayCount = 0;
            AdController.Instance.ShowInterstitialAd();
            gameWinScreen.SetActive(false);
            OpenAllCells();
            mainMenu.SetActive(true);
        }

        #region  HELPERS
        private Cell GetCellById(int row, int col)
        {
            Cell cell = _cellObjects.FirstOrDefault(c => c.Row == row && c.Col == col);
            if(cell == null)
            {
                Debug.LogFormat("Cell with id {0}-{1} not found!",row, col);
            }
            return cell;

        }
        public List<Cell> GetNeighbours(int row, int col, bool excludeDiagnols = false)
        {
            List<Cell> neighbours = new List<Cell>();
            Cell c = null;
            //e
            if (col < _totalCols - 1)
            {
                c = GetCellById(row, col + 1);
                neighbours.Add(c);
            }
            //se
            if (col < _totalCols - 1 && row < _totalRows - 1 && !excludeDiagnols)
            {
                c = GetCellById(row + 1, col + 1);
                neighbours.Add(c);
            }
            //s
            if (row < _totalRows - 1)
            {
                c = GetCellById(row + 1, col);
                neighbours.Add(c);
            }
            //sw
            if (col > 0 && row < _totalRows - 1 && !excludeDiagnols)
            {
                c = GetCellById(row + 1, col - 1);
                neighbours.Add(c);
            }
            //w
            if (col > 0)
            {
                c = GetCellById(row, col - 1);
                neighbours.Add(c);
            }
            //nw
            if (col > 0 && row > 0 && !excludeDiagnols)
            {
                c = GetCellById(row - 1, col - 1);
                neighbours.Add(c);
            }
            //n
            if (row > 0)
            {
                c = GetCellById(row - 1, col);
                neighbours.Add(c);
            }
            //ne
            if (col < _totalCols - 1 && row > 0 && !excludeDiagnols)
            {
                c = GetCellById(row - 1, col + 1);
                neighbours.Add(c);
            }

            return neighbours;
        }



        #endregion

    }

}


/*
TODO
sounds
ads

*/