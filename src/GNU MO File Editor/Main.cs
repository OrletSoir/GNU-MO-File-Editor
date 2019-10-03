using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GNU_MO_File_Editor
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
		}

		private MOReader _moReader;
		private bool _dataLoaded = false;

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_dataLoaded && MessageBox.Show("You have data loaded, are you sure you want to load new set?", "Data loaded", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				return;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					toolStripStatusLabel1.Text = "Loading...";

					Refresh();

					_moReader = new MOReader(openFileDialog1.FileName);
					dataSet1.Clear();

					toolStripProgressBar1.Value = 0;
					toolStripProgressBar1.Visible = true;
					toolStripProgressBar1.Maximum = (int)_moReader.Count;

					Refresh();

					for (int i = 0; i < _moReader.Count; i++)
					{
						DataRow row = dataTable1.NewRow();
						MOLine line = _moReader[i];

						row["index"] = line.Index;
						row["id"] = line.Original;
						row["value"] = line.Translated.Replace("\n", Environment.NewLine);

						dataTable1.Rows.Add(row);

						if (i%100 == 0)
						{
							toolStripProgressBar1.Value = i;

							toolStripStatusLabel1.Text = string.Format("Loading line {0} of {1}...", i, _moReader.Count);

							//if (i % 2500 == 0)
							//	Application.DoEvents();
							//else
							statusStrip1.Refresh();
						}
					}

					toolStripProgressBar1.Value = (int)_moReader.Count - 1;
					toolStripStatusLabel1.Text = "Resizing rows...";
					statusStrip1.Refresh();
					Application.DoEvents();

					dataGridView1.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);

					toolStripProgressBar1.Visible = false;
					saveToolStripMenuItem.Enabled = true;
					toolStripStatusLabel1.Text = string.Format("Loaded {0} lines.", _moReader.Count);

					_dataLoaded = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("There was an error during loading:\r\n\r\n{0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					toolStripStatusLabel1.Text = "Saving...";
					Refresh();

					_moReader.SaveMOFile(saveFileDialog1.FileName);

					toolStripStatusLabel1.Text = "Saved!";
				}
				catch (Exception ex)
				{
					MessageBox.Show(string.Format("There was an error during loading:\r\n\r\n{0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_dataLoaded && MessageBox.Show("You have data loaded, are you sure you want to quit?", "Data loaded", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
				return;

			this.Close();
		}

		private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			TextBox tb = (TextBox)e.Control;
			tb.Multiline = true;
		}

		private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			dataGridView1.AutoResizeRow(e.RowIndex);
		}

		private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0 || e.ColumnIndex < 0)
				return;

			int i = -1;

			int.TryParse(dataTable1.Rows[e.RowIndex][0].ToString(), out i);

			if (i < 0)
				return;

			MOLine line = _moReader[i];

			line.Translated = dataTable1.Rows[e.RowIndex][e.ColumnIndex].ToString().Replace(Environment.NewLine, "\n");

			_moReader[i] = line;
		}

		private void forumthreadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(@"https://forum.worldofwarships.com/topic/154616-mo-file-editor/");
		}

		private void btnPrev_Click(object sender, EventArgs e)
		{
			int pos = 0;

			if (dataGridView1.SelectedRows.Count > 0)
				int.TryParse(dataGridView1.SelectedRows[0].Cells[0].Value.ToString(), out pos);

			if (dataGridView1.SelectedCells.Count > 0)
				int.TryParse(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[0].Value.ToString(), out pos);

			for (int i = pos-1; i >= 0; i--)
			{
				MOLine line = _moReader[i];
				if (line.Original.IndexOf(tbSearch.Text, StringComparison.InvariantCultureIgnoreCase) != -1 || line.Translated.IndexOf(tbSearch.Text, StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					dataGridView1.ClearSelection();
					dataGridView1.Rows[i].Selected = true;

					dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];

					return;
				}
			}

			MessageBox.Show(string.Format("No previous instances of \"{0}\" have been found", tbSearch.Text));
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			int pos = 0;

			if (dataGridView1.SelectedRows.Count > 0)
				int.TryParse(dataGridView1.SelectedRows[0].Cells[0].Value.ToString(), out pos);

			if (dataGridView1.SelectedCells.Count > 0)
				int.TryParse(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[0].Value.ToString(), out pos);

			for (int i = pos+1; i < _moReader.Count; i++)
			{
				MOLine line = _moReader[i];
				if (line.Original.IndexOf(tbSearch.Text, StringComparison.InvariantCultureIgnoreCase) != -1 || line.Translated.IndexOf(tbSearch.Text, StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					dataGridView1.ClearSelection();
					dataGridView1.Rows[i].Selected = true;

					dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells[0];

					return;
				}
			}

			MessageBox.Show(string.Format("No further instances of \"{0}\" have been found", tbSearch.Text));
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_dataLoaded && MessageBox.Show("You have data loaded, are you sure you want to quit?", "Data loaded", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.C && e.Control)
			{
				if (dataGridView1.SelectedCells.Count > 0)
				{
					List<string > sl = new List<string>();

					foreach (DataGridViewCell sc in dataGridView1.SelectedCells)
						sl.Add(sc.Value.ToString());

					Clipboard.SetText(string.Join(Environment.NewLine, sl.ToArray()));
				}
			}
		}
	}
}
