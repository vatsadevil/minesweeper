using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Minesweeper
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] Transform bgGrid;
        [SerializeField] GridController gridCon;

        private Image[] _cellImgs;
        private List<Image> _currentFlagged = new List<Image>();
        private const int ACTIVE_FLAG_COUNT = 10;
        private Coroutine _animation;
        
        private void OnEnable() 
        {
            GameStateController.CurrentGameState = GameState.MENU;
            
            ClearCurrentFlags();
            _cellImgs = bgGrid.GetComponentsInChildren<Image>();
            int assignedCount = 0;
            while(assignedCount < ACTIVE_FLAG_COUNT)
            {
                Image img = _cellImgs[Random.Range(0, _cellImgs.Length)];
                if(!_currentFlagged.Contains(img))
                {
                    _currentFlagged.Add(img);
                    img.sprite = Resources.Load<Sprite>("Sprites/flag");
                    assignedCount++;
                }
            }
            _animation = StartCoroutine(AnimateBGGrid());

        }

        private void OnDisable() {
            if(_animation != null)
            {
                StopCoroutine(_animation);
            }
            _animation = null;
        }

        private IEnumerator AnimateBGGrid()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.50f);
                for (int i = 0; i < 2; i++)
                {
                    Image img = _currentFlagged[Random.Range(0, _currentFlagged.Count)];
                    img.sprite = Resources.Load<Sprite>("Sprites/empty");
                    _currentFlagged.Remove(img);

                    Image[] empties = _cellImgs.Except(_currentFlagged).ToArray();
                    Image img1 = empties[Random.Range(0, empties.Length)];
                    _currentFlagged.Add(img1);
                    img1.sprite = Resources.Load<Sprite>("Sprites/flag");
                }
            }
        }

        private void ClearCurrentFlags()
        {
            for (int i = 0; i < _currentFlagged.Count; i++)
            {
                Image img = _currentFlagged[i];
                img.sprite = Resources.Load<Sprite>("Sprites/empty");
            }
            _currentFlagged.Clear();
        }

        public void OnGameModeSelect(int mode)
        {
            GameStateController.CurrentGameState = GameState.GAME;
            switch(mode)
            {
                case 0:
                    gameObject.SetActive(false);
                    gridCon.Reset(9,9,10);
                    break;
                case 1:
                    gameObject.SetActive(false);
                    gridCon.Reset(20,13,40);
                    break;
                case 2:
                    gameObject.SetActive(false);
                    gridCon.Reset(27,17,80);
                    break;
            }
        }
    }
}