using System;


namespace Reversi
{
  public class Board
  {
    #region Data Members
    public enum Pieces {Empty=0, White, Black};
    public Pieces[] m_board;
    private int[,] m_eightDirections = {{-1,0}, {-1,1}, {0,1}, {1,1}, {1,0}, {1,-1}, {0,-1}, {-1,-1}};
    #endregion

    #region Construction
    public Board()
    {
      m_board = new Pieces[64];
      m_board[GetIndex(3,3)] = Pieces.Black;
      m_board[GetIndex(4,3)] = Pieces.White;
      m_board[GetIndex(3,4)] = Pieces.White;
      m_board[GetIndex(4,4)] = Pieces.Black;
    }

    public Board(ref Board other)
    {
      m_board = new Pieces[64];
      for (int i=0; i<64; i++)
      {
        m_board[i] = other.m_board[i];
      }
    }

    public void Clone(Board other)
    {
      for (int i=0; i<64; i++)
      {
        m_board[i] = other.m_board[i];
      }
    }

    #endregion

    #region Gets, Sets
    public int GetIndex(int x, int y)
    {
      return y*8+x;
    }

    public bool PlacePiece(int iIndex, Pieces piece)
    {
      if (iIndex >= 0 && iIndex <64)
      {
        m_board[iIndex] = piece;
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool PlacePiece(int x, int y, Pieces piece)
    {
      return PlacePiece(GetIndex(x,y), piece);
    }

    public bool IsEmpty(int x, int y)
    {
      int index = GetIndex(x,y);
      if (index >= 0 && index <64)
      {
        return m_board[index] == Pieces.Empty;
      }
      else
      {
        return false;
      }
    }

    #endregion

    #region Utilities
    public Pieces GetOtherPiece(Pieces p)
    {
      if (p == Pieces.White)
        return Pieces.Black;
      else if (p == Pieces.Black)
        return Pieces.White;
      else
        return Pieces.Empty;
    }

    public Pieces GetNeighbor(int x, int y)
    {
      if (x>=0 && x<8 && y>=0 && y<8)
        return m_board[GetIndex(x,y)];
      else
        return Pieces.Empty;
    }
    public bool FlipNeighbors(int iPosition)
    {
      bool bAnyFlipped = false;
      int xPos = iPosition % 8;
      int yPos = iPosition / 8;
      Pieces myPiece = m_board[iPosition];
      Pieces opponentPiece = GetOtherPiece(myPiece);

      // loop over all directions
      for (int i=0; i<8; i++)
      {
        int dx = m_eightDirections[i,0];
        int dy = m_eightDirections[i,1];
        int x = xPos + dx;
        int y = yPos + dy;

        // see if neighber is valid
        if (GetNeighbor(x, y) == opponentPiece)
        {
          // traverse all opponent pieces
          do
          {
            x += dx;
            y += dy;
          }
          while (GetNeighbor(x, y) == opponentPiece);

          // see if our piece is at the end
          if (GetNeighbor(x, y) == myPiece)
          {
            // reached our own piece so flip all in middle
            x = xPos + dx;
            y = yPos + dy;
            while (GetNeighbor(x, y) == opponentPiece)
            {
              PlacePiece(x, y, myPiece);
              x += dx;
              y += dy;
            }
            bAnyFlipped = true;
          }
        }
      }
      return bAnyFlipped;
    }

    #endregion

    #region Move Selection
    public void GetOpenLegalPositions(ref System.Collections.Queue openPositions, Pieces myPiece)
    {
      Pieces opponentPiece = GetOtherPiece(myPiece);
      openPositions.Clear();
      for (int y=0; y<8; y++)
      {
        for (int x=0; x<8; x++)
        {
          int iIndex = GetIndex(x,y);
          if (m_board[iIndex] == Pieces.Empty)
          {
            // see if it's legal
            if (
              GetNeighbor(x-1, y-1) == opponentPiece ||
              GetNeighbor(x  , y-1) == opponentPiece ||
              GetNeighbor(x+1, y-1) == opponentPiece ||
              GetNeighbor(x-1, y  ) == opponentPiece ||
              GetNeighbor(x+1, y  ) == opponentPiece ||
              GetNeighbor(x-1, y+1) == opponentPiece ||
              GetNeighbor(x  , y+1) == opponentPiece ||
              GetNeighbor(x+1, y+1) == opponentPiece)
            {
              if (IsLegalMove(iIndex, myPiece))
              {
                openPositions.Enqueue(iIndex);
              }
            }
          }
        }
      }
    }

    public int SelectRandomPosition(Pieces myPiece)
    {
      System.Collections.Queue openPositions = new System.Collections.Queue();
      GetOpenLegalPositions(ref openPositions, myPiece);
      if (openPositions.Count == 0)
        return 0;
      else
      {
        int iRandomIndex = Reversi.FrankMath.GetRandomInteger(0, openPositions.Count-1);
        for (int i=0; i<iRandomIndex-1; i++)
        {
          openPositions.Dequeue();
        }
        return (int) openPositions.Dequeue();
      }
    }

    public int SelectGoodPosition(Pieces movePiece, Pieces myPiece)
    {
      System.Collections.Queue openPositions = new System.Collections.Queue();
      GetOpenLegalPositions(ref openPositions, myPiece);
      if (openPositions.Count == 0)
        return 0;
      else
      {
        System.Collections.SortedList positionQualityList = new System.Collections.SortedList();
        Random rand = new Random();
        foreach (int iPosition in openPositions)
        {
          bool bLegal = true;
          double fQuality = IsGoodMove(this, iPosition, movePiece, myPiece, 0, out bLegal) + rand.NextDouble() * 0.4;
          if (bLegal)
          {
            positionQualityList.Add(fQuality, iPosition);
          }
        }
        if (positionQualityList.Count == 0)
          return -1; // no place to go!
        int iBestPosition = (int) positionQualityList.GetByIndex(0);
        return iBestPosition;
      }
    }
    public int SelectGoodPositionAlphaBeta(Pieces movePiece, Pieces myPiece, int iLevels)
    {
      System.Collections.Queue openPositions = new System.Collections.Queue();
      GetOpenLegalPositions(ref openPositions, movePiece);
      if (openPositions.Count == 0)
        return 0;
      else
      {
        System.Collections.SortedList positionQualityList = new System.Collections.SortedList();
        Random rand = new Random();
        foreach (int iPosition in openPositions)
        {
          MoveQuality mq = AlphaBetaMax(this, -10000, 10000, iLevels-1, myPiece, movePiece);
          mq.fQuality += (float)(rand.NextDouble()) * 0.4F;
          positionQualityList.Add(mq.fQuality, iPosition);
        }
        if (positionQualityList.Count == 0)
          return -1; // no place to go!
        int iBestPosition = (int) positionQualityList.GetByIndex(0);
        return iBestPosition;
      }
    }

    public bool AnyOpenPositions()
    {
      foreach (Pieces p in m_board)
      {
        if (p == Pieces.Empty)
        {
          return true;
        }
      }
      return false;
    }

    public bool IsLegalMove(int iPosition, Pieces myPiece)
    {
      int xPos = iPosition % 8;
      int yPos = iPosition / 8;

      if ( m_board [ iPosition ] != Pieces.Empty )
        return false;

      Pieces opponentPiece = GetOtherPiece(myPiece);

      // loop over all directions
      for (int i=0; i<8; i++)
      {
        int dx = m_eightDirections[i,0];
        int dy = m_eightDirections[i,1];
        int x = xPos + dx;
        int y = yPos + dy;

        // see if neighber is valid
        if (GetNeighbor(x, y) == opponentPiece)
        {
          // traverse all opponent pieces
          do
          {
            x += dx;
            y += dy;
          }
          while (GetNeighbor(x, y) == opponentPiece);

          // see if our piece is at the end
          if (GetNeighbor(x, y) == myPiece)
          {
            return true;
          }
        }
      }
      return false;
    }

    public bool AnyOpenLegalPositions(Pieces movePiece)
    {
      System.Collections.Queue openPositions = new System.Collections.Queue();
      GetOpenLegalPositions(ref openPositions, movePiece);
      return openPositions.Count > 0;
    }

    public MoveQuality AlphaBetaMax(Board board, float fAlpha, float fBeta, int iLevels, Pieces myPiece, Pieces movePiece)
    {
      MoveQuality mq = new MoveQuality();
      if (iLevels == 0)
      {
        mq.fQuality = board.EvaluateBoard(myPiece);
        return mq;
      }

      System.Collections.Queue openPositions = new System.Collections.Queue();
      board.GetOpenLegalPositions(ref openPositions, movePiece);
      if (openPositions.Count == 0)
      {
        mq.fQuality = board.EvaluateBoard(myPiece);
        return mq;
      }

      foreach (int iPosition in openPositions)
      {
        Board newBoard = new Board();
        newBoard.Clone(this);
        newBoard.PlacePiece(iPosition, movePiece);
        newBoard.FlipNeighbors(iPosition);

        MoveQuality testMq = AlphaBetaMin(newBoard, fAlpha, fBeta, iLevels-1, myPiece, GetOtherPiece(movePiece));
        if (testMq.fQuality > fAlpha)
        {
          fAlpha = testMq.fQuality;
          mq.iPosition = iPosition;
        }
        if (fAlpha > fBeta)
        {
          mq.fQuality = fBeta;
          mq.iPosition = iPosition;
          return mq;
        }
      }
      mq.fQuality = fAlpha;
      return mq;
    }

    public MoveQuality AlphaBetaMin(Board board, float fAlpha, float fBeta, int iLevels, Pieces myPiece, Pieces movePiece)
    {
      MoveQuality mq = new MoveQuality();
      if (iLevels == 0)
      {
        mq.fQuality = board.EvaluateBoard(myPiece);
        return mq;
      }

      System.Collections.Queue openPositions = new System.Collections.Queue();
      board.GetOpenLegalPositions(ref openPositions, movePiece);
      if (openPositions.Count == 0)
      {
        mq.fQuality = board.EvaluateBoard(myPiece);
        return mq;
      }

      foreach (int iPosition in openPositions)
      {
        Board newBoard = new Board();
        newBoard.Clone(this);
        newBoard.PlacePiece(iPosition, movePiece);
        newBoard.FlipNeighbors(iPosition);

        MoveQuality testMq = AlphaBetaMax(newBoard, fAlpha, fBeta, iLevels-1, myPiece, GetOtherPiece(movePiece));
        if (testMq.fQuality < fBeta)
        {
          fBeta = testMq.fQuality;
          mq.iPosition = iPosition;
        }
        if (fBeta < fAlpha)
        {
          mq.fQuality = fAlpha;
          mq.iPosition = iPosition;
          return mq;
        }
      }
      mq.fQuality = fBeta;
      return mq;
    }

    #endregion

    #region Move Evaluation
    public void CountPiecesOfEach(out int iWhiteCount, out int iBlackCount)
    {
      iWhiteCount = 0;
      iBlackCount = 0;
      foreach (Pieces p in m_board)
      {
        if (p == Pieces.White)
          iWhiteCount++;
        if (p == Pieces.Black)
          iBlackCount++;
      }
    }

    public int IsGoodMove(Board board, int position, Pieces movePiece, Pieces myPiece, int iLevel, out bool bLegal)
    {
      Board newBoard = new Board(ref board);
      newBoard.PlacePiece(position, myPiece);
      bLegal = newBoard.FlipNeighbors(position);

      if (newBoard.AnyWinner())
      {
        Pieces winner = newBoard.GetWinner();
        if (winner == myPiece)
          return 1000;
        else if (winner == GetOtherPiece(myPiece))
          return -1000;
      }
      int whiteCount = 0, blackCount = 0;
      CountPiecesOfEach(out whiteCount, out blackCount);
      if (myPiece == Pieces.White)
        return whiteCount - blackCount;
      else
        return blackCount - whiteCount;
    }

    public MoveQuality SelectMaximumQualityMove(Board board, Pieces movePiece, int iLevel)
    {
      MoveQuality moveQuality = new MoveQuality();
      System.Collections.Queue openPositions = new System.Collections.Queue();
      GetOpenLegalPositions(ref openPositions, movePiece);

      if (iLevel == 0 || openPositions.Count == 0)
      {
        // we are a leaf node - just evaluate board and return
        int whiteCount=0, blackCount=0;
        CountPiecesOfEach(out whiteCount, out blackCount);
        if (whiteCount != blackCount)
        {
          if (movePiece == Pieces.White)
          {
            moveQuality.fQuality = whiteCount - blackCount;
          }
          else
          {
            moveQuality.fQuality = blackCount - whiteCount;
          }
        }
      }
      else
      {
        // need to evaluate possible moves
        System.Collections.SortedList positionQualityList = new System.Collections.SortedList();
        Random rand = new Random();
        foreach (int iPosition in openPositions)
        {
          Board newBoard = new Board();
          newBoard.Clone(this);
          newBoard.PlacePiece(iPosition, movePiece);
          bool bLegal = true;
          MoveQuality mq = SelectMinimumQualityMove(newBoard, GetOtherPiece(movePiece), iLevel-1);
          if (bLegal)
          {
            positionQualityList.Add(mq.fQuality + rand.NextDouble() * 0.4, iPosition);
          }
        }
        if (positionQualityList.Count > 0)
        {
          moveQuality.iPosition = (int) positionQualityList.GetByIndex(0);
        }
      }
      return moveQuality;
    }

    public MoveQuality SelectMinimumQualityMove(Board board, Pieces movePiece, int iLevel)
    {
      MoveQuality moveQuality = new MoveQuality();
      System.Collections.Queue openPositions = new System.Collections.Queue();
      GetOpenLegalPositions(ref openPositions, movePiece);

      if (iLevel == 0 || openPositions.Count == 0)
      {
        // we are a leaf node - just evaluate board and return
        int whiteCount=0, blackCount=0;
        CountPiecesOfEach(out whiteCount, out blackCount);
        if (whiteCount != blackCount)
        {
          if (movePiece == Pieces.White)
          {
            moveQuality.fQuality = whiteCount - blackCount;
          }
          else
          {
            moveQuality.fQuality = blackCount - whiteCount;
          }
        }
      }
      else
      {
        // need to evaluate possible moves
        System.Collections.SortedList positionQualityList = new System.Collections.SortedList();
        Random rand = new Random();
        foreach (int iPosition in openPositions)
        {
          Board newBoard = new Board();
          newBoard.Clone(this);
          newBoard.PlacePiece(iPosition, movePiece);
          bool bLegal = true;
          MoveQuality mq = SelectMaximumQualityMove(newBoard, GetOtherPiece(movePiece), iLevel-1);
          if (bLegal)
          {
            positionQualityList.Add(mq.fQuality + rand.NextDouble() * 0.4, iPosition);
          }
        }
        if (positionQualityList.Count > 0)
        {
          moveQuality.iPosition = (int) positionQualityList.GetByIndex(positionQualityList.Count - 1);
        }
      }
      return moveQuality;
    }

    #endregion

    #region Board Evaluation
    public bool AnyWinner()
    {
      if (AnyOpenPositions())
      {
        return false;
      }
      else
      {
        int whiteCount = 0;
        int blackCount = 0;
        CountPiecesOfEach(out whiteCount, out blackCount);
        return (whiteCount != blackCount);
        // Else it is a tie
      }
    }

    public Pieces GetWinner()
    {
      if (AnyWinner())
      {
        int whiteCount=0, blackCount=0;
        CountPiecesOfEach(out whiteCount, out blackCount);
        if (whiteCount > blackCount)
          return Pieces.White;
        else if (whiteCount < blackCount)
          return Pieces.Black;
        else // they are equal
          return Pieces.Empty;
      }
      else
      {
        // There is no winner
        return Pieces.Empty;
      }
    }

    public float EvaluateBoard(Pieces myPiece)
    {
      if (AnyWinner())
      {
        Pieces winner = GetWinner();
        if (winner == myPiece)
          return 1000;
        else if (winner == GetOtherPiece(myPiece))
          return -1000;
      }
      int whiteCount = 0, blackCount = 0;
      CountPiecesOfEach(out whiteCount, out blackCount);
      if (myPiece == Pieces.White)
        return whiteCount - blackCount;
      else
        return blackCount - whiteCount;
    }

    public void InvertColors()
    {
      for (int i=0; i<64; i++)
      {
        m_board[i] = GetOtherPiece(m_board[i]);
      }
    }
    #endregion
  }
}
