using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MnMJournalReader
{
    public partial class Form1 : Form
    {
        private string journalPath = string.Empty;  // Path to the journal directory

        public Form1()
        {
            InitializeComponent();

            // Set form properties
            this.Text = "MnM Journal Reader";
            this.Size = new Size(600, 500);

            // Create UI controls
            var lblCharacter = new Label { Text = "Character:", Location = new Point(10, 10), AutoSize = true };
            var cbCharacters = new ComboBox { Location = new Point(100, 10), Width = 200 };
            var lblNPC = new Label { Text = "NPC:", Location = new Point(10, 40), AutoSize = true };
            var cbNPCs = new ComboBox { Location = new Point(100, 40), Width = 200 };
            var lblDisplay = new Label { Text = "Displaying:", Location = new Point(10, 70), AutoSize = true };
            var dgvLog = new DataGridView
            {
                Location = new Point(10, 100),
                Width = 500,
                Height = 300,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,  // Hide the blank row headers
                AllowUserToOrderColumns = true, // Allow column reordering
                MultiSelect = false, // Disable multi-select
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // Select entire rows
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, // Resize with form
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells // Auto-adjust row heights
            };

            // Dictionary to track sort direction for each column
            var columnSortDirections = new Dictionary<string, SortOrder>
            {
                { "DateTime", SortOrder.Ascending },
                { "NpcName", SortOrder.Ascending },
                { "Message", SortOrder.Ascending }
            };

            var colDateTime = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Timestamp",
                HeaderText = "Date/Time",
                Name = "DateTime",
                Width = 150,
                SortMode = DataGridViewColumnSortMode.Automatic // Enable sorting for this column
            };
            var colNpcName = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NpcName",
                HeaderText = "NPC Name",
                Name = "NpcName",
                Width = 120,
                SortMode = DataGridViewColumnSortMode.Automatic // Enable sorting for this column
            };
            var colMessage = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Message",
                HeaderText = "Message",
                Name = "Message",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                SortMode = DataGridViewColumnSortMode.Automatic, // Enable sorting for this column
                DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } // Enable word wrapping
            };
            dgvLog.Columns.Add(colDateTime);
            dgvLog.Columns.Add(colNpcName);
            dgvLog.Columns.Add(colMessage);

            // Add SortCompare event handler to ensure correct sorting
            dgvLog.SortCompare += (s, e) =>
            {
                if (e.Column.Index == 0) // DateTime column
                {
                    if (e.CellValue1 == null || e.CellValue2 == null)
                    {
                        e.SortResult = 0;
                        e.Handled = true;
                        return;
                    }
                    DateTime dt1 = (DateTime)e.CellValue1;
                    DateTime dt2 = (DateTime)e.CellValue2;
                    e.SortResult = DateTime.Compare(dt1, dt2);
                }
                else if (e.Column.Index == 1) // NPC Name column
                {
                    string name1 = e.CellValue1?.ToString() ?? "";
                    string name2 = e.CellValue2?.ToString() ?? "";
                    e.SortResult = string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase);
                }
                else if (e.Column.Index == 2) // Message column
                {
                    string msg1 = e.CellValue1?.ToString() ?? "";
                    string msg2 = e.CellValue2?.ToString() ?? "";
                    e.SortResult = string.Compare(msg1, msg2, StringComparison.OrdinalIgnoreCase);
                }
                e.Handled = true;
            };

            // Define search controls with centered positioning
            const int margin = 10; // Margin between controls
            const int bottomMargin = 30; // Fixed distance from bottom of form
            
            var txtSearch = new TextBox { 
                Width = 150,
                Height = 23,
                Anchor = AnchorStyles.Bottom // Keep it anchored to bottom
            };
            
            var btnSearch = new Button { 
                Text = "Search Current NPC",
                Width = 120,
                Height = 23, 
                Anchor = AnchorStyles.Bottom // Keep it anchored to bottom
            };
            
            var btnSearchAll = new Button { 
                Text = "Search All NPCs",
                Width = 120,
                Height = 23,
                Anchor = AnchorStyles.Bottom // Keep it anchored to bottom
            };
            
            // Calculate total width of all controls + margins
            int totalWidth = txtSearch.Width + margin + btnSearch.Width + margin + btnSearchAll.Width;
            
            // Use the Resize event which is more reliable than SizeChanged
            this.Resize += (s, e) => {
                CenterSearchControls();
            };
            
            // Method to center the search controls
            void CenterSearchControls()
            {
                // Calculate the starting X position to center all controls
                int startX = (this.ClientSize.Width - totalWidth) / 2;
                
                // Calculate Y position as fixed distance from bottom of form
                int searchY = this.ClientSize.Height - bottomMargin - txtSearch.Height;
                
                // Position the controls
                txtSearch.Location = new Point(startX, searchY);
                btnSearch.Location = new Point(startX + txtSearch.Width + margin, searchY);
                btnSearchAll.Location = new Point(startX + txtSearch.Width + margin + btnSearch.Width + margin, searchY);
            }
            
            // Initialize positions immediately
            CenterSearchControls();
            
            // Add controls to the form
            this.Controls.Add(lblCharacter);
            this.Controls.Add(cbCharacters);
            this.Controls.Add(lblNPC);
            this.Controls.Add(cbNPCs);
            this.Controls.Add(lblDisplay);
            this.Controls.Add(dgvLog);
            this.Controls.Add(txtSearch);
            this.Controls.Add(btnSearch);
            this.Controls.Add(btnSearchAll);

            // Event handler for character selection
            cbCharacters.SelectedIndexChanged += (s, e) =>
            {
                string? selectedCharacter = cbCharacters.SelectedItem as string;
                if (selectedCharacter != null)
                {
                    string characterPath = Path.Combine(journalPath, selectedCharacter);
                    if (Directory.Exists(characterPath))
                    {
                        var npcs = Directory.GetFiles(characterPath).Select(Path.GetFileName).ToArray();
                        cbNPCs.Items.Clear();
                        cbNPCs.Items.AddRange(npcs!);
                        cbNPCs.SelectedIndex = -1; // Clear selection
                        dgvLog.DataSource = null;
                        lblDisplay.Text = "Displaying:";
                    }
                }
            };

            // Event handler for NPC selection
            cbNPCs.SelectedIndexChanged += (s, e) =>
            {
                string? selectedNPC = cbNPCs.SelectedItem as string;
                if (selectedNPC != null)
                {
                    string? character = cbCharacters.SelectedItem as string;
                    if (character != null)
                    {
                        string npcFile = Path.Combine(journalPath, character, selectedNPC);
                        if (File.Exists(npcFile))
                        {
                            string[] lines = File.ReadAllLines(npcFile);
                            var logEntries = new List<LogEntry>();
                            foreach (string line in lines)
                            {
                                var entry = ParseLogEntry(line);
                                if (entry != null)
                                {
                                    logEntries.Add(entry);
                                }
                            }
                            if (logEntries.Count > 0)
                            {
                                dgvLog.DataSource = null; // Clear existing data
                                dgvLog.DataSource = logEntries; // Set new data
                                lblDisplay.Text = $"Displaying: {selectedNPC}'s log";
                            }
                            else
                            {
                                MessageBox.Show("No valid log entries found in the file.");
                                lblDisplay.Text = "Displaying: No entries";
                            }
                        }
                    }
                }
            };

            // Event handler for search within current NPC log
            btnSearch.Click += (s, e) =>
            {
                string searchTerm = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchTerm) && dgvLog.DataSource is List<LogEntry> currentEntries)
                {
                    var filtered = currentEntries
                        .Where(entry => entry.Message.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 || 
                                        entry.NpcName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                    dgvLog.DataSource = null; // Clear existing data
                    dgvLog.DataSource = filtered;
                    lblDisplay.Text = $"Search results for '{searchTerm}' in current log";
                }
            };

            // Event handler for search across all NPCs
            btnSearchAll.Click += (s, e) =>
            {
                string searchTerm = txtSearch.Text.Trim();
                string? character = cbCharacters.SelectedItem as string;
                if (!string.IsNullOrEmpty(searchTerm) && character != null)
                {
                    string characterPath = Path.Combine(journalPath, character);
                    if (Directory.Exists(characterPath))
                    {
                        List<LogEntry> allEntries = new List<LogEntry>();
                        foreach (string npcFile in Directory.GetFiles(characterPath))
                        {
                            string npcFileName = Path.GetFileName(npcFile);
                            string[] lines = File.ReadAllLines(npcFile);
                            foreach (string line in lines)
                            {
                                var entry = ParseLogEntry(line);
                                if (entry != null && (entry.Message.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 || 
                                                     entry.NpcName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                                {
                                    // If no NPC name was parsed, use the filename
                                    if (string.IsNullOrEmpty(entry.NpcName))
                                    {
                                        entry.NpcName = npcFileName;
                                    }
                                    allEntries.Add(entry);
                                }
                            }
                        }
                        dgvLog.DataSource = null; // Clear existing data
                        dgvLog.DataSource = allEntries;
                        lblDisplay.Text = $"Search results for '{searchTerm}' in all NPCs";
                    }
                }
            };

            // Event handler for column header click to sort
            dgvLog.ColumnHeaderMouseClick += (s, e) =>
            {
                DataGridViewColumn column = dgvLog.Columns[e.ColumnIndex];
                
                // Toggle the sort direction
                SortOrder newSortOrder = SortOrder.None;
                if (columnSortDirections.TryGetValue(column.Name, out SortOrder currentDirection))
                {
                    newSortOrder = currentDirection == SortOrder.Ascending ? 
                                  SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    newSortOrder = SortOrder.Ascending;
                }
                
                columnSortDirections[column.Name] = newSortOrder;
                
                // Apply the sort
                if (dgvLog.DataSource is List<LogEntry> entries)
                {
                    List<LogEntry> sortedEntries;
                    
                    if (column.Name == "DateTime")
                    {
                        sortedEntries = newSortOrder == SortOrder.Ascending 
                            ? entries.OrderBy(e => e.Timestamp).ToList() 
                            : entries.OrderByDescending(e => e.Timestamp).ToList();
                    }
                    else if (column.Name == "NpcName")
                    {
                        sortedEntries = newSortOrder == SortOrder.Ascending 
                            ? entries.OrderBy(e => e.NpcName).ToList() 
                            : entries.OrderByDescending(e => e.NpcName).ToList();
                    }
                    else // Message column
                    {
                        sortedEntries = newSortOrder == SortOrder.Ascending 
                            ? entries.OrderBy(e => e.Message).ToList() 
                            : entries.OrderByDescending(e => e.Message).ToList();
                    }
                    
                    dgvLog.DataSource = sortedEntries;
                    
                    // Update the column header to show sort direction
                    foreach (DataGridViewColumn col in dgvLog.Columns)
                    {
                        col.HeaderCell.SortGlyphDirection = col.Name == column.Name 
                            ? newSortOrder : SortOrder.None;
                    }
                }
            };

            // Initialize journal path and load characters
            string localLowPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "..\\LocalLow");
            journalPath = Path.Combine(localLowPath, "Niche Worlds Cult", "Monsters and Memories", "journal");

            if (Directory.Exists(journalPath))
            {
                var characters = Directory.GetDirectories(journalPath).Select(Path.GetFileName).ToArray();
                if (characters.Length > 0)
                {
                    cbCharacters.Items.AddRange(characters!);
                }
                else
                {
                    MessageBox.Show("No character folders found in the journal directory.");
                }
            }
            else
            {
                MessageBox.Show("Journal directory not found at: " + journalPath);
            }
        }

        // Helper method to parse log entries
        private LogEntry? ParseLogEntry(string line)
        {
            int idxAM = line.IndexOf(" AM:");
            int idxPM = line.IndexOf(" PM:");
            int idx = -1;
            string suffix = "";

            if (idxAM >= 0)
            {
                idx = idxAM;
                suffix = " AM";
            }
            else if (idxPM >= 0)
            {
                idx = idxPM;
                suffix = " PM";
            }

            if (idx >= 0)
            {
                string timestampStr = line.Substring(0, idx + suffix.Length);
                string fullMessage = line.Substring(idx + suffix.Length + 1).Trim(); // +1 for the colon
                
                // Parse the NPC name and actual message
                string npcName = string.Empty;
                string message = fullMessage;
                
                int saysIndex = fullMessage.IndexOf(" says ");
                if (saysIndex >= 0)
                {
                    npcName = fullMessage.Substring(0, saysIndex).Trim();
                    message = fullMessage.Substring(saysIndex + 6).Trim(); // 6 = " says ".Length
                }
                
                DateTime timestamp = DateTime.ParseExact(timestampStr, "M/d/yyyy h:mm tt", CultureInfo.InvariantCulture);
                return new LogEntry { Timestamp = timestamp, NpcName = npcName, Message = message };
            }
            return null;
        }
    }

    public class LogEntry : IComparable<LogEntry>
    {
        public DateTime Timestamp { get; set; }
        public string NpcName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // IComparable implementation to support sorting (used if list is sorted manually)
        public int CompareTo(LogEntry? other)
        {
            if (other == null) return 1;
            return Timestamp.CompareTo(other.Timestamp);
        }
    }
}