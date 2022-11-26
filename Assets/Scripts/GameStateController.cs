using System;

namespace Minesweeper
{
    public class GameStateController
    {
        public static GameState CurrentGameState;
    }
    public enum GameState
    {
        MENU,
        GAME,
        GAME_END
    }
}