///////////////////////////////////////////////////////////////////////////////
// 
// PlainGraph
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
// PURPOSE.
//
// License: GNU Lesser General Public License (LGPLv3)
//
// Jing, Lu (lujing@unvell.com)
//
// Copyright (C) unvell.com, 2013. All Rights Reserved
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Unvell.UIControl.PlainGraph;
using Unvell.UIControl.PlainGraphTest;

namespace Unvell.UIControl.PlainGraphDemo
{
	public partial class DemoForm : Form
	{
		public DemoForm()
		{
			InitializeComponent();
			comboBox1.Items.AddRange(Enum.GetNames(enumType: typeof(PlainGraphType)));
		}

		private readonly DataSource _ds = new DataSource();

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			comboBox1.SelectedIndex = 2;
			_ds.Caption = "PlainGraph Demo Chart";
			_ds.XTitle = "Year";
			_ds.YTitle = "Count";

			// record 1
			var data1 = new Dictionary<int, double> {
				{2005, 300},
				{2006, 450},
				{2007, 500},
				{2008, 530},
				{2009, 680},
				{2010, 890},
				{2011, 1330}
			};

			var record = _ds.add_data("Book", data1, Color.OliveDrab);
			record.Set[6].Style.EndCap = System.Drawing.Drawing2D.LineCap.DiamondAnchor;
			var data2 = new Dictionary<int, double> {
				{2005, 110},
				{2006, 150},
				{2007, 180},
				{2008, 378},
				{2009, 750},
				{2010, 1290},
				{2011, 1630}
			};
			_ds.add_data("Software", data2, Color.Orchid);

			var data3 = new Dictionary<int, double> {
				{2005, 320},
				{2006, 410},
				{2007, 560},
				{2008, 595},
				{2009, 600},
				{2010, 670},
				{2011, 820}
			};

			_ds.add_data("DVD", data3, Color.SaddleBrown);
			graph.DataSource = _ds;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			graph.GraphType = (PlainGraphType)Enum.GetValues(typeof(PlainGraphType)).GetValue(comboBox1.SelectedIndex);
		}

		private void chkShowLegend_CheckedChanged(object sender, EventArgs e)
		{
			graph.IsShowLegend = chkShowLegend.Checked;
		}

		private void chkShowEntityName_CheckedChanged(object sender, EventArgs e)
		{
			graph.IsShowEntityName = chkShowEntityName.Checked;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new AboutBox().ShowDialog();
		}

		private void printReviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var ppd = new PrintPreviewDialog()
			{
				Document = new PrintDocument()
			};
			graph.Print(ppd.Document);

			ppd.PrintPreviewControl.Zoom = 1d;

			var screen_size = Screen.FromControl(this).WorkingArea;
			ppd.SetBounds(50, 50, screen_size.Width / 2, screen_size.Height - 100);

			ppd.ShowDialog();
		}

		private void printToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var pd = new PrintDialog {Document = new PrintDocument(), UseEXDialog = true,};
			graph.Print(pd.Document);
			pd.ShowDialog();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
