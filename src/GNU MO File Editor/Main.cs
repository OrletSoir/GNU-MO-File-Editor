using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GNU_MO_File_Editor
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
			impexpToolStripMenuItem.Enabled = false;
		}

		private MOReader _moReader;
		private bool _dataLoaded = false;

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_dataLoaded && MessageBox.Show("You have data loaded, are you sure you want to load new set?", "Data loaded", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

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
                dataTable1.Columns[0].DataType = typeof(Int32);
                for (int i = 0; i < _moReader.Count; i++)
                {
                    DataRow row = dataTable1.NewRow();
                    MOLine line = _moReader[i];
                    row["index"] = line.Index;
                    row["id"] = line.Original;
                    row["value"] = line.Translated.Replace("\n", Environment.NewLine);

                    dataTable1.Rows.Add(row);

                    if (i % 100 == 0)
                    {
                        toolStripProgressBar1.Value = i;

                        toolStripStatusLabel1.Text = $"Loading line {i} of {_moReader.Count}...";

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
                toolStripStatusLabel1.Text = $"Loaded {_moReader.Count} lines.";
                impexpToolStripMenuItem.Enabled = true;
                _dataLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error during loading:\r\n\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                toolStripStatusLabel1.Text = "Saving...";
                Refresh();

                _moReader.SaveMOFile(saveFileDialog1.FileName);

                toolStripStatusLabel1.Text = "Saved!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error during loading:\r\n\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			UpdateInternalArray(i);
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

			for (int i = pos - 1; i >= 0; i--)
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

			MessageBox.Show($"No previous instances of \"{tbSearch.Text}\" have been found");
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			int pos = 0;

			if (dataGridView1.SelectedRows.Count > 0)
				int.TryParse(dataGridView1.SelectedRows[0].Cells[0].Value.ToString(), out pos);

			if (dataGridView1.SelectedCells.Count > 0)
				int.TryParse(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[0].Value.ToString(), out pos);

			for (int i = pos + 1; i < _moReader.Count; i++)
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

			MessageBox.Show($"No further instances of \"{tbSearch.Text}\" have been found");
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
					List<string> sl = new List<string>();

					foreach (DataGridViewCell sc in dataGridView1.SelectedCells)
						sl.Add(sc.Value.ToString());

					Clipboard.SetText(string.Join(Environment.NewLine, sl.ToArray()));
				}
			}
		}

		private bool UpdateInternalArray(int index)
		{
			try
			{
                MOLine line = _moReader[index];

                line.Translated = dataTable1.Rows[index][2].ToString().Replace(Environment.NewLine, "\n");

                _moReader[index] = line;
                return true;
            }
			catch
			{
				return false;
			}
        }
		private bool IsSeparatorSet()
		{
			if (separatorCombobox.SelectedItem == null)
			{
				MessageBox.Show("Please select a separator character");
				return false;
			}

			return true;
		}

		private bool GetCharFromString(string s, out char separator)
		{
			if (s == "TAB")
			{
				separator = '\t';
				return true;
			}
			else
			{
				if (!char.TryParse(s, out separator))
				{
					MessageBox.Show("Separator error");
					return false;
				}
				return true;
			}
		}

		private void Export_Click(object sender, EventArgs e)
		{
			if (!IsSeparatorSet())
			{
				return;
			}

			char separator;
			if (!GetCharFromString(separatorCombobox.SelectedItem.ToString(), out separator))
			{
				return;
			}

			if (saveFileDialog2.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			string filename = saveFileDialog2.FileName;

			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				int i = 1;
				using (StreamWriter sw = new StreamWriter(filename, true, Encoding.UTF8))
				{

					foreach (var item in dataTable1.Rows)
					{
						DataRow row = (DataRow)item;
						string index = row[0].ToString();
						string id = row[1].ToString();
						string text = row[2].ToString();
						int textRows = row[2].ToString().Split('\n').Length;
						string output = textRows.ToString() + separator + index + separator + id + separator + text;

						sw.WriteLine(output);

						toolStripStatusLabel1.Text = $"Exporting line {i} of {dataTable1.Rows.Count}...";
						statusStrip1.Refresh();

						i++;
					}
				}

				toolStripStatusLabel1.Text = $"{i} lines exported";
			}
			catch (IOException ex)
			{
				MessageBox.Show($"Read error: {ex.Message}");
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unknown erroe: {ex.Message}");
				return;
			}
		}

		private void Import_Click(object sender, EventArgs e)
		{
			if (!IsSeparatorSet())
			{
				return;
			}

			char separator;
			if (!GetCharFromString(separatorCombobox.SelectedItem.ToString(), out separator))
			{
				return;
			}

			if (openFileDialog2.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			string filename = openFileDialog2.FileName;

			List<string> lines = new List<string>();
			using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
			{
				string line;

				while ((line = sr.ReadLine()) != null)
				{
					lines.Add(line);
				}
			}

			string value = "";
			int index = 0;
			string id = "";
			int rowcountPerItem = 0;
			int failedrows = 0;
			for (int i = 0; i < lines.Count; i++)
			{
				value = "";
				if (lines[i].ToString().Contains(separator.ToString()))
				{
					string Fullline = lines[i].TrimEnd('\r', '\n');
					string[] item = Fullline.Split(separator);//new line
					rowcountPerItem = int.Parse(item[0]);
					index = int.Parse(item[1]);
					id = item[2];
					value = item[3];

					if (rowcountPerItem > 1)
					{
						for (int j = 1; j < rowcountPerItem; j++)
						{
							Fullline = lines[i + j].TrimEnd('\r', '\n');
							value += Environment.NewLine + Fullline;
						}
						i += rowcountPerItem - 1;
					}

					DataRow[] row2 = dataTable1.Select("index=" + index);
					if (row2.Length > 0)
					{
						string lastvalue = row2[0]["value"].ToString();
                        row2[0]["value"] = value;
                        if (!UpdateInternalArray(index))
						{
							row2[0]["value"] = lastvalue;
                            failedrows++;
                        }
                    }
					else
					{
						failedrows++;
					}
				}
			}

			toolStripStatusLabel1.Text = "Resizing rows...";
			statusStrip1.Refresh();
			Application.DoEvents();
			dataGridView1.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
			toolStripStatusLabel1.Text = $"{lines.Count} rows imported, {failedrows} failed";
		}

		private void separatorCombobox_TextChanged(object sender, EventArgs e)
		{
			toolStripStatusLabel1.Text = $"Selected separator: {separatorCombobox.SelectedItem}";
		}
    }
}
