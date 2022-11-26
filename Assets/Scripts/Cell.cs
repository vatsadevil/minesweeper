using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Minesweeper
{
    public class Cell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public int NumValue 
        {
            get 
            {
                return _numValue;
            }
            set
            {
                _numValue = value;
                UpdateSprite();
            }
        }
        
        public bool IsMine 
        {
            get
            {
                return _isMine;
            }
            set
            {
                _isMine = value;
                // UpdateSprite();
            }
        }
        public CellType Type = CellType.NONE;
        public CellState State = CellState.CLOSE;

        private GridController _gridCon;
        private bool _isMine;
        private int _numValue;
        private Image _image = null;
        private bool _clickBegan = false;
        private System.DateTime _clickStartTime;

        private void Awake() {
            _image = GetComponent<Image>();
        }
        public void Init(int row, int col, GridController gc)
        {
            Row = row;
            Col = col;
            NumValue = -1;
            IsMine = false;
            _gridCon = gc;
        }

        private void UpdateSprite(bool manual=false)
        {
            if(_image != null)
            {
                if (State.Equals(CellState.OPEN))
                {
                    if (IsMine)
                    {
                        if(manual)
                            _image.sprite = Resources.Load<Sprite>("Sprites/mine_open");
                        else
                            _image.sprite = Resources.Load<Sprite>("Sprites/mine");
                    }
                    else
                    {
                        if(NumValue != 0)
                        {
                            _image.sprite = Resources.Load<Sprite>("Sprites/"+NumValue);
                        }
                        else
                        {
                            _image.sprite = Resources.Load<Sprite>("Sprites/empty-open");
                        }
                    }
                }
                else if (State.Equals(CellState.CLOSE))
                {
                    _image.sprite = Resources.Load<Sprite>("Sprites/empty");
                }
                else if (State.Equals(CellState.MARKED))
                {

                    _image.sprite = Resources.Load<Sprite>("Sprites/flag");
                }

            }
        }
        private void Update() {
            // if (_clickBegan)
            // {
            //     System.TimeSpan span = System.DateTime.UtcNow - _clickStartTime;
            //     Debug.Log("SpanUpdate:" + span.Milliseconds);
            // }
        }
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            if(State.Equals(CellState.OPEN)) return;

            _clickBegan = true;
            _clickStartTime = System.DateTime.UtcNow;
            Debug.Log("Click start");
        }
        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if(_clickBegan)
            {
                _clickBegan = false;
                System.TimeSpan span = System.DateTime.UtcNow - _clickStartTime;
                Debug.Log("Span"+span.Milliseconds);
                if(span.Milliseconds + (span.Seconds * 1000) >= 450)
                {
                    
                    MarkCell();
                }
                else
                {
                    ForceOpenCell(true);
                }
                
                _gridCon.StartTimer();
            }
        }

        private void MarkCell()
        {
            if(State.Equals(CellState.MARKED))
            {
                State = CellState.CLOSE;
                _gridCon.CellMarked(false);
            }
            else
            {
                State = CellState.MARKED;
                _gridCon.CellMarked(true);
            }
            UpdateSprite();
        }
        public void ForceOpenCell(bool manual=false)
        {
            if(State.Equals(CellState.MARKED) || State.Equals(CellState.OPEN)) return;

            State = CellState.OPEN;
            if(IsMine)
            {
                //KABOOM!
                if(!_gridCon.GameOver)
                {
                    _gridCon.OpenAllCells();
                }
            }
            else
            {
                //if number open just this
                //else trigger chain reaction
                if(NumValue == 0)
                {
                    OpenCell();
                }
            }
            UpdateSprite(manual);
        }

        private void OpenCell()
        {
            // if(State.Equals(CellState.MARKED)) return;
            State = CellState.OPEN;
            UpdateSprite();
            List<Cell> neighbours = _gridCon.GetNeighbours(Row, Col, false);
            foreach (var item in neighbours)
            {
                if(item.State.Equals(CellState.OPEN)) continue;
                if(!item.IsMine)
                {
                    if(item.NumValue == 0)
                    {
                        item.OpenCell();
                    }
                    else
                    {
                        item.ForceOpenCell();
                    }
                }
            }
        }

        public void Verify()
        {
            if(State.Equals(CellState.MARKED))
            {
                if(!IsMine)
                {
                     _image.sprite = Resources.Load<Sprite>("Sprites/mine_wrong");
                }
            }
        }
    }

    public enum CellType
    {
        NONE,
        MINE,
        NUMBER,
        EMPTY
    }

    public enum CellState
    {
        CLOSE,
        OPEN,
        MARKED
    }
}