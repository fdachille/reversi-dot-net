using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Reversi
{
  public delegate void MyDelegate();

  public interface I
  {
    event MyDelegate MyEvent;
    void UpdateLabels();
  }

	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
    private Reversi.BoardDisplay m_imageDisplay;
    private System.Windows.Forms.Label m_whiteLabel;
    private System.Windows.Forms.Label m_blackLabel;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.m_imageDisplay = new Reversi.BoardDisplay();
      this.m_whiteLabel = new System.Windows.Forms.Label();
      this.m_blackLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // m_imageDisplay
      // 
      this.m_imageDisplay.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.m_imageDisplay.Name = "m_imageDisplay";
      this.m_imageDisplay.Size = new System.Drawing.Size(320, 320);
      this.m_imageDisplay.TabIndex = 0;
      // 
      // m_whiteLabel
      // 
      this.m_whiteLabel.Location = new System.Drawing.Point(320, 8);
      this.m_whiteLabel.Name = "m_whiteLabel";
      this.m_whiteLabel.Size = new System.Drawing.Size(72, 16);
      this.m_whiteLabel.TabIndex = 1;
      this.m_whiteLabel.Text = "White";
      // 
      // m_blackLabel
      // 
      this.m_blackLabel.Location = new System.Drawing.Point(320, 32);
      this.m_blackLabel.Name = "m_blackLabel";
      this.m_blackLabel.Size = new System.Drawing.Size(72, 16);
      this.m_blackLabel.TabIndex = 2;
      this.m_blackLabel.Text = "Black";
      // 
      // MainForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(392, 320);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                  this.m_blackLabel,
                                                                  this.m_whiteLabel,
                                                                  this.m_imageDisplay});
      this.Name = "MainForm";
      this.Text = "Reversi";
      this.BackColor = Color.White;
      this.ResumeLayout(false);

      I i = this.m_imageDisplay;
      i.MyEvent += new MyDelegate(UpdateCounters);

    }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

    private void UpdateCounters()
    {
      m_whiteLabel.Text = string.Format("White: {0}", m_imageDisplay.GetWhiteCount());
      m_blackLabel.Text = string.Format("Black: {0}", m_imageDisplay.GetBlackCount());
    }
	}
}
