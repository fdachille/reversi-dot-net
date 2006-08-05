using System;

namespace Reversi
{
	/// <summary>
	/// Summary description for Game.
	/// </summary>
	public class Game
	{
    private Board.Pieces m_currentPlayer;
    public Board.Pieces CurrentPlayer
    {
      get
      {
        return m_currentPlayer;
      }
      set
      {
        m_currentPlayer = value;
      }
    }

		public Game()
		{
      m_currentPlayer = new Board.Pieces();
      m_currentPlayer = Board.Pieces.White;
		}

    public void NextPlayer()
    {
      if (m_currentPlayer == Board.Pieces.White)
        m_currentPlayer = Board.Pieces.Black;
      else
        m_currentPlayer = Board.Pieces.White;
    }
	}
}
