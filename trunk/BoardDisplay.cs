using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Reversi
{
	/// <summary>
	/// Summary description for BoardDisplay.
	/// </summary>
	public class BoardDisplay : System.Windows.Forms.UserControl, Reversi.I
	{
    private bool imagesLoaded = false;
    private Image whitePieceImage, blackPieceImage, emptyPieceImage;
    private Board m_board;
    private int m_x, m_y;
    private Bitmap m_backBuffer;
    private Game m_game;

    public event MyDelegate MyEvent;
    public void UpdateLabels()
    {
      if (MyEvent != null)
        MyEvent();
    }

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public BoardDisplay()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call
      LoadImages();
      m_board = new Board();
      m_x = new int();
      m_y = new int();
      m_x = 0;
      m_y = 0;
      m_backBuffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
      m_game = new Game();

		}

    public void SetBoard(Board board)
    {
      m_board = board;
    }

    public void LoadImages()
    {
      emptyPieceImage = Image.FromFile("emptyPiece.png");
      whitePieceImage = Image.FromFile("whitePiece.png");
      blackPieceImage = Image.FromFile("blackPiece.png");
      imagesLoaded = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      // Call the OnPaint method of the base class.
      //base.OnPaint(e);

      // Call methods of the System.Drawing.Graphics object.
      //e.Graphics.DrawString("No image loaded...", Font, new SolidBrush(ForeColor), ClientRectangle);
      DrawAll(e.Graphics);
    }

    protected void DrawAll(Graphics g)
    {
      if (imagesLoaded)
      {
        Graphics backBufferGraphics = Graphics.FromImage(m_backBuffer);
        backBufferGraphics.Clear(System.Drawing.Color.AliceBlue);

        int counter = 0;
        int iPieceSize = 40;
        for (int y=0; y<8; y++)
        {
          for (int x=0; x<8; x++)
          {
            backBufferGraphics.DrawImage(emptyPieceImage, x*iPieceSize, y*iPieceSize);
            Board.Pieces piece = m_board.m_board[counter];
            counter++;
            if (piece == Board.Pieces.White)
              backBufferGraphics.DrawImage(whitePieceImage, x*iPieceSize, y*iPieceSize);
            else if (piece == Board.Pieces.Black)
              backBufferGraphics.DrawImage(blackPieceImage, x*iPieceSize, y*iPieceSize);
          }
        }
        if (m_game.CurrentPlayer == Board.Pieces.White)
          backBufferGraphics.DrawImage(whitePieceImage, m_x-iPieceSize/2, m_y-iPieceSize/2);
        else
          backBufferGraphics.DrawImage(blackPieceImage, m_x-iPieceSize/2, m_y-iPieceSize/2);

        g.DrawImage(m_backBuffer, 0, 0);
        backBufferGraphics.Dispose();
      }
    }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      // 
      // BoardDisplay
      // 
      this.Name = "BoardDisplay";
      this.Size = new System.Drawing.Size(320, 320);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BoardDisplay_MouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BoardDisplay_MouseUp);
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BoardDisplay_KeyUp);

    }
		#endregion

    private void BoardDisplay_MouseMove(object sender, MouseEventArgs e)
    {
      m_x = e.X;
      m_y = e.Y;
      DrawAll(CreateGraphics());
    }
  
    private void BoardDisplay_MouseUp(object sender, MouseEventArgs e)
    {
      int x = (int)(e.X / 40.0);
      int y = (int)(e.Y / 40.0);
      int iIndex = m_board.GetIndex(x,y);
      if ( e.Button == MouseButtons.Left )
        UserPlace( iIndex );
    }

    private void BoardDisplay_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyData == Keys.F)
      {
        m_board.InvertColors();
        m_game.NextPlayer();
        DrawAll(CreateGraphics());
      }
      else if (e.KeyData == Keys.C)
      {
        ComputerPlace();
      }
    }

    void UserPlace(int iPosition)
    {
      if (m_board.IsLegalMove(iPosition, m_game.CurrentPlayer))
      {
        bool bResult = m_board.PlacePiece(iPosition, m_game.CurrentPlayer);
        if (bResult)
        {
          bool bLegal = m_board.FlipNeighbors(iPosition);
          if (bLegal)
          {
            m_game.NextPlayer();
            UpdateLabels();
            DrawAll(CreateGraphics());
          }
          else
          {
            m_board.PlacePiece(iPosition, Board.Pieces.Empty);
          }
        }
        ComputerPlace();
      }
    }

    void PlaceUserPiece(int iPosition)
    {
      if (m_board.IsLegalMove(iPosition, Board.Pieces.White))
      {
        bool bResult = m_board.PlacePiece(iPosition, m_game.CurrentPlayer);
        if (bResult)
        {
          bool bLegal = m_board.FlipNeighbors(iPosition);
          if (bLegal)
          {
            m_game.NextPlayer();
            UpdateLabels();
            DrawAll(CreateGraphics());
            PlaceComputerPiece();
          }
          else
          {
            m_board.PlacePiece(iPosition, Board.Pieces.Empty);
          }
        }
      }
    }

    void PlaceComputerPiece()
    {
      if (m_board.AnyOpenLegalPositions(Board.Pieces.Black))
      {
        //int iIndex = m_board.SelectGoodPosition(m_game.m_currentPlayer, m_game.m_currentPlayer);
        //MoveQuality mq = m_board.SelectMaximumQualityMove(m_board, m_game.m_currentPlayer, 5);
        int iPosition = m_board.SelectGoodPositionAlphaBeta(m_game.CurrentPlayer, m_game.CurrentPlayer, 5);
        if (iPosition > -1)
        {
          m_board.PlacePiece(iPosition, m_game.CurrentPlayer);
          m_board.FlipNeighbors(iPosition);
          m_game.NextPlayer();
          UpdateLabels();
          DrawAll(CreateGraphics());
        }
      }
      else
      {
        m_game.NextPlayer();
      }
    }

    void ComputerPlace()
    {
      this.Cursor = Cursors.WaitCursor;
      if (m_board.AnyOpenLegalPositions(m_game.CurrentPlayer))
      {
        int iPosition = m_board.SelectGoodPositionAlphaBeta(m_game.CurrentPlayer, m_game.CurrentPlayer, 6);
        if (iPosition > -1)
        {
          m_board.PlacePiece(iPosition, m_game.CurrentPlayer);
          m_board.FlipNeighbors(iPosition);
          m_game.NextPlayer();
          UpdateLabels();
          DrawAll(CreateGraphics());
        }
      }
      else
      {
        m_game.NextPlayer();
      }
      this.Cursor = Cursors.Default;
    }

    public int GetWhiteCount()
    {
      int whiteCounter = 0, blackCounter = 0;
      m_board.CountPiecesOfEach(out whiteCounter, out blackCounter);
      return whiteCounter;
    }

    public int GetBlackCount()
    {
      int whiteCounter = 0, blackCounter = 0;
      m_board.CountPiecesOfEach(out whiteCounter, out blackCounter);
      return blackCounter;
    }
  }
}