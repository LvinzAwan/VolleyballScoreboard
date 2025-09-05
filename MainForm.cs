using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VolleyScoreboard
{
    public class MainForm : Form
    {
        // UI controls
        TextBox txtTournament;
        TextBox txtTeamA, txtTeamB;
        PictureBox picA, picB;
        Label lblScoreA, lblScoreB;
        Label lblCurrentSet, lblMatchStatus;
        Button btnPlusA, btnMinusA, btnPlusB, btnMinusB;
        Button btnLogoA, btnLogoB;
        Button btnStartReset, btnSetWinA, btnSetWinB;
        Button btnManualSetWins, btnSwapTeams;
        ComboBox cboFormat;
        Panel panelA, panelB, panelCenter;
        
        // Enhanced UI elements
        Panel panelHeader, panelScoreSection, panelControls;
        Label lblServeIndicatorA, lblServeIndicatorB;
        NumericUpDown nudTopWinsA, nudTopWinsB, nudTargetWins;
        NumericUpDown[] setCellsA, setCellsB;
        NumericUpDown nudWinsGridA, nudWinsGridB;
        Label lblGridTeamA, lblGridTeamB;
        ProgressBar progressA, progressB;
        Label lblTeamScoreA, lblTeamScoreB;

        // Match state
        int pointsA = 0, pointsB = 0;
        int setWinsA = 0, setWinsB = 0;
        int currentSet = 1;
        int targetWins = 2;
        int totalSets => targetWins * 2 - 1;
        bool matchStarted = false;
        bool matchOver = false;
        char serving = 'A';
        (int a, int b)[] setScores = new (int a, int b)[5];
        bool suppressValueChanged = false;

        // UI Constants
        private readonly Color PrimaryColor = Color.FromArgb(20, 30, 48);
        private readonly Color SecondaryColor = Color.FromArgb(30, 42, 66);
        private readonly Color AccentColor = Color.FromArgb(52, 152, 219);
        private readonly Color TeamAColor = Color.FromArgb(46, 204, 113);
        private readonly Color TeamBColor = Color.FromArgb(231, 76, 60);
        private readonly Color GoldColor = Color.FromArgb(241, 196, 15);
        private readonly Color TextColor = Color.FromArgb(236, 240, 241);
        private readonly Color DarkTextColor = Color.FromArgb(149, 165, 166);

        public MainForm()
        {
            InitializeControls();
            InitializeForm();
            CreateMainLayout();
            SetupEventHandlers();
            InitializeDefaults();
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitializeControls()
        {
            // Initialize score buttons
            btnPlusA = CreateStyledButton("+", 16, TeamAColor);
            btnMinusA = CreateStyledButton("-", 16, TeamAColor);
            btnPlusB = CreateStyledButton("+", 16, TeamBColor);
            btnMinusB = CreateStyledButton("-", 16, TeamBColor);
            
            // Initialize set win buttons
            btnSetWinA = CreateStyledButton("Set Win", 10, TeamAColor);
            btnSetWinB = CreateStyledButton("Set Win", 10, TeamBColor);
            
            // Initialize other control buttons
            btnStartReset = CreateStyledButton("MULAI", 12, AccentColor);
            btnManualSetWins = CreateStyledButton("Set Manual", 10, Color.FromArgb(120, 80, 20));
            btnSwapTeams = CreateStyledButton("Tukar Tim", 10, Color.FromArgb(80, 80, 120));

            // Set minimum button sizes for responsiveness
            SetButtonMinSize(btnPlusA, 60, 40);
            SetButtonMinSize(btnMinusA, 60, 40);
            SetButtonMinSize(btnPlusB, 60, 40);
            SetButtonMinSize(btnMinusB, 60, 40);
            SetButtonMinSize(btnSetWinA, 90, 40);
            SetButtonMinSize(btnSetWinB, 90, 40);
        }

        private void InitializeForm()
        {
            Text = "Volleyball Scoreboard Pro";
            Size = new Size(1200, 800);
            MinimumSize = new Size(1000, 700);
            BackColor = PrimaryColor;
            ForeColor = TextColor;
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            StartPosition = FormStartPosition.CenterScreen;
            
            // Enable automatic scaling
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
        }

        private void CreateMainLayout()
        {
            var mainContainer = new TableLayoutPanel
            {
                Name = "mainContainer",
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Padding = new Padding(10),
                AutoSize = false
            };
            
            // Define row heights with percentages for better responsiveness
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Header - 12%
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // Main score - 50%
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 20));   // Set grid - 20%
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 18));   // Controls - 18%

            Controls.Add(mainContainer);

            // Create sections
            CreateHeaderSection(mainContainer);
            CreateMainScoreSection(mainContainer);
            CreateSetGridSection(mainContainer);
            CreateControlsSection(mainContainer);
        }

        private void CreateHeaderSection(TableLayoutPanel parent)
        {
            panelHeader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SecondaryColor,
                Padding = new Padding(15),
                Margin = new Padding(5)
            };
            
            // Add subtle gradient effect with border
            panelHeader.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    panelHeader.ClientRectangle,
                    SecondaryColor,
                    Color.FromArgb(40, 52, 76),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, panelHeader.ClientRectangle);
                }
                
                using (var pen = new Pen(Color.FromArgb(70, 82, 106), 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panelHeader.Width - 1, panelHeader.Height - 1);
                }
            };

            txtTournament = new TextBox
            {
                Text = "CARAKA MUDA CUP KE-2",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = GoldColor,
                BackColor = SecondaryColor,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center,
                Margin = new Padding(10),
                AutoSize = false
            };
            
            panelHeader.Controls.Add(txtTournament);
            parent.Controls.Add(panelHeader, 0, 0);
        }

        private void CreateMainScoreSection(TableLayoutPanel parent)
        {
            panelScoreSection = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(5),
                Margin = new Padding(5)
            };

            var scoreLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            
            // Responsive column styles
            scoreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.5f));
            scoreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            scoreLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37.5f));

            // Team A Panel
            panelA = CreateTeamPanel("Tim A", TeamAColor, out txtTeamA, out picA, out btnLogoA, out progressA);
            
            // Center Score Panel
            panelCenter = CreateCenterPanel();
            
            // Team B Panel
            panelB = CreateTeamPanel("Tim B", TeamBColor, out txtTeamB, out picB, out btnLogoB, out progressB);

            scoreLayout.Controls.Add(panelA, 0, 0);
            scoreLayout.Controls.Add(panelCenter, 1, 0);
            scoreLayout.Controls.Add(panelB, 2, 0);

            panelScoreSection.Controls.Add(scoreLayout);
            parent.Controls.Add(panelScoreSection, 0, 1);
        }

        private Panel CreateTeamPanel(string teamName, Color teamColor, out TextBox txtName, out PictureBox pic, out Button btnLogo, out ProgressBar progress)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                BackColor = SecondaryColor,
                MinimumSize = new Size(250, 200)
            };

            // Custom paint for rounded corners and team color accent
            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, SecondaryColor, Color.FromArgb(25, 37, 56), System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                
                // Team color accent bar at top
                using (var accentBrush = new SolidBrush(teamColor))
                {
                    e.Graphics.FillRectangle(accentBrush, 0, 0, rect.Width, 4);
                }
                
                using (var borderPen = new Pen(Color.FromArgb(60, 72, 96), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(15)
            };
            
            // Define columns: 60% for score, 40% for logo
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            
            // Define rows with better proportions
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 18));    // Team name
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));    // Main content
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 17));    // Progress + button

            txtName = new TextBox
            {
                Text = teamName,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 52, 76),
                ForeColor = TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center,
                Margin = new Padding(3),
                MinimumSize = new Size(0, 30)
            };

            // Create responsive score label with better sizing
            var scoreLabel = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 60f, FontStyle.Bold),
                ForeColor = teamColor,
                BackColor = Color.FromArgb(35, 47, 71),
                AutoSize = false,
                Margin = new Padding(3),
                BorderStyle = BorderStyle.FixedSingle,
                MinimumSize = new Size(120, 80)
            };

            pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(35, 47, 71),
                Margin = new Padding(3),
                MinimumSize = new Size(80, 80)
            };

            progress = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                ForeColor = teamColor,
                BackColor = Color.FromArgb(40, 52, 76),
                Maximum = 100,
                Value = 0,
                Margin = new Padding(3),
                MinimumSize = new Size(0, 20)
            };

            btnLogo = CreateStyledButton("Logo", 9, Color.FromArgb(60, 80, 110));
            btnLogo.Dock = DockStyle.Fill;
            btnLogo.Margin = new Padding(3);
            btnLogo.MinimumSize = new Size(60, 25);

            // Add team name spanning both columns
            layout.Controls.Add(txtName, 0, 0);
            layout.SetColumnSpan(txtName, 2);

            // Add score and logo in row 1
            layout.Controls.Add(scoreLabel, 0, 1);
            layout.Controls.Add(pic, 1, 1);

            // Add progress bar and button in row 2
            layout.Controls.Add(progress, 0, 2);
            layout.Controls.Add(btnLogo, 1, 2);

            panel.Controls.Add(layout);
            
            // Store reference to score label for later updates
            if (teamName == "Tim A")
                lblTeamScoreA = scoreLabel;
            else if (teamName == "Tim B")
                lblTeamScoreB = scoreLabel;
                
            return panel;
        }

        private Panel CreateCenterPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                BackColor = Color.FromArgb(28, 40, 62),
                MinimumSize = new Size(250, 200)
            };

            // Add gradient background
            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, Color.FromArgb(28, 40, 62), Color.FromArgb(35, 47, 71), 
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                using (var borderPen = new Pen(Color.FromArgb(70, 82, 106), 2))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 6,
                Padding = new Padding(10)
            };

            // Define rows with better proportions for responsiveness
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Header SET/TARGET
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));   // Set wins display
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));   // Main scores
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 5));    // Serve indicators
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));    // Current set
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));    // Match status

            // Define columns
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            // Create header labels
            var lblSetA = CreateHeaderLabel("SET", TeamAColor);
            var lblTarget = CreateHeaderLabel("TARGET", GoldColor);
            var lblSetB = CreateHeaderLabel("SET", TeamBColor);

            layout.Controls.Add(lblSetA, 0, 0);
            layout.Controls.Add(lblTarget, 1, 0);
            layout.Controls.Add(lblSetB, 2, 0);

            // Create panels for set wins
            var panelWinsA = CreateSetWinsPanel(TeamAColor);
            var panelTarget = CreateSetWinsPanel(GoldColor);
            var panelWinsB = CreateSetWinsPanel(TeamBColor);

            // Create responsive NumericUpDown controls
            nudTopWinsA = CreateBigNumericUpDown(0, 5, TeamAColor);
            nudTargetWins = CreateBigNumericUpDown(2, 5, GoldColor);
            nudTopWinsB = CreateBigNumericUpDown(0, 5, TeamBColor);

            // Add NumericUpDown to panels
            panelWinsA.Controls.Add(nudTopWinsA);
            panelTarget.Controls.Add(nudTargetWins);
            panelWinsB.Controls.Add(nudTopWinsB);

            // Add panels to layout
            layout.Controls.Add(panelWinsA, 0, 1);
            layout.Controls.Add(panelTarget, 1, 1);
            layout.Controls.Add(panelWinsB, 2, 1);

            // Main score section with smaller font sizing
            lblScoreA = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 60f, FontStyle.Bold), // Dikurangi dari 80f ke 60f
                ForeColor = TeamAColor,
                BackColor = Color.FromArgb(35, 47, 71),
                AutoSize = false,
                Margin = new Padding(3),
                MinimumSize = new Size(70, 50) // Dikurangi tinggi dari 60 ke 50
            };

            var scoreSeparator = new Label
            {
                Text = ":",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 30f, FontStyle.Bold), // Dikurangi dari 40f ke 30f
                ForeColor = GoldColor,
                MinimumSize = new Size(30, 50) // Dikurangi tinggi dari 60 ke 50
            };

            lblScoreB = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 60f, FontStyle.Bold), // Dikurangi dari 80f ke 60f
                ForeColor = TeamBColor,
                BackColor = Color.FromArgb(35, 47, 71),
                AutoSize = false,
                Margin = new Padding(3),
                MinimumSize = new Size(70, 50) // Dikurangi tinggi dari 60 ke 50
            };

            layout.Controls.Add(lblScoreA, 0, 2);
            layout.Controls.Add(scoreSeparator, 1, 2);
            layout.Controls.Add(lblScoreB, 2, 2);

            // Serve indicators
            lblServeIndicatorA = new Label
            {
                Text = "● SERVE",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = GoldColor,
                Visible = false,
                AutoSize = false
            };

            lblServeIndicatorB = new Label
            {
                Text = "SERVE ●",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = GoldColor,
                Visible = false,
                AutoSize = false
            };

            layout.Controls.Add(lblServeIndicatorA, 0, 3);
            layout.Controls.Add(new Panel { Dock = DockStyle.Fill }, 1, 3);
            layout.Controls.Add(lblServeIndicatorB, 2, 3);

            // Current set info
            lblCurrentSet = new Label
            {
                Text = "SET 1 - Target: 25 poin",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AccentColor,
                AutoSize = false,
                MinimumSize = new Size(0, 25)
            };
            layout.SetColumnSpan(lblCurrentSet, 3);
            layout.Controls.Add(lblCurrentSet, 0, 4);

            // Match status
            lblMatchStatus = new Label
            {
                Text = "Siap untuk memulai",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 6f, FontStyle.Italic),
                ForeColor = DarkTextColor,
                AutoSize = false,
                MinimumSize = new Size(0, 10)
            };
            layout.SetColumnSpan(lblMatchStatus, 3);
            layout.Controls.Add(lblMatchStatus, 0, 5);

            panel.Controls.Add(layout);
            return panel;
        }
        
        private Label CreateHeaderLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = color,
                AutoSize = false,
                MinimumSize = new Size(60, 25)
            };
        }

        private Panel CreateSetWinsPanel(Color borderColor)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Margin = new Padding(3),
                BackColor = Color.FromArgb(35, 47, 71),
                MinimumSize = new Size(60, 35)
            };

            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                using (var pen = new Pen(borderColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 1, 1, rect.Width - 2, rect.Height - 2);
                }
            };

            return panel;
        }

        private NumericUpDown CreateNumericUpDown(int value, int maximum, Color accentColor)
        {
            var nud = new NumericUpDown
            {
                Minimum = 0,
                Maximum = maximum,
                Value = value,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 52, 76),
                ForeColor = accentColor,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                ReadOnly = true,
                MinimumSize = new Size(50, 30)
            };

            nud.Controls[0].Hide();
            return nud;
        }

        private NumericUpDown CreateBigNumericUpDown(int value, int maximum, Color accentColor)
        {
            var nud = new NumericUpDown
            {
                Minimum = 0,
                Maximum = maximum,
                Value = value,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 28f, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 52, 76),
                ForeColor = accentColor,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                ReadOnly = true,
                MinimumSize = new Size(70, 45)
            };

            nud.Controls[0].Hide();
            return nud;
        }

        private void CreateSetGridSection(TableLayoutPanel parent)
        {
            var gridPanel = CreateSetGrid();
            parent.Controls.Add(gridPanel, 0, 2);
        }

        private Panel CreateSetGrid()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(25, 35, 53),
                Margin = new Padding(5),
                AutoScroll = true,
                MinimumSize = new Size(800, 120)
            };

            panel.Paint += (s, e) =>
            {
                using (var borderPen = new Pen(Color.FromArgb(60, 72, 96), 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 3,
                Padding = new Padding(8),
                AutoSize = false
            };

            // Setup responsive columns
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));   // Team names
            for (int i = 0; i < 5; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16)); // Set columns
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));   // Wins column

            // Setup rows
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33));

            // Headers
            var lblSet = new Label
            {
                Text = "SET",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = GoldColor,
                AutoSize = false,
                MinimumSize = new Size(80, 25)
            };
            grid.Controls.Add(lblSet, 0, 0);

            for (int i = 0; i < 5; i++)
            {
                var lblSetNum = new Label
                {
                    Text = (i + 1).ToString(),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = TextColor,
                    AutoSize = false,
                    MinimumSize = new Size(50, 25)
                };
                grid.Controls.Add(lblSetNum, i + 1, 0);
            }

            var lblWins = new Label
            {
                Text = "WINS",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = GoldColor,
                AutoSize = false,
                MinimumSize = new Size(60, 25)
            };
            grid.Controls.Add(lblWins, 6, 0);

            // Team A row
            lblGridTeamA = new Label
            {
                Text = "Tim A",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TeamAColor,
                AutoSize = false,
                MinimumSize = new Size(80, 25)
            };
            grid.Controls.Add(lblGridTeamA, 0, 1);

            setCellsA = new NumericUpDown[5];
            for (int i = 0; i < 5; i++)
            {
                int setIndex = i;
                setCellsA[i] = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 999,
                    Value = 0,
                    Dock = DockStyle.Fill,
                    TextAlign = HorizontalAlignment.Center,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    BackColor = Color.FromArgb(40, 52, 76),
                    ForeColor = TextColor,
                    BorderStyle = BorderStyle.FixedSingle,
                    Enabled = false,
                    Margin = new Padding(2),
                    MinimumSize = new Size(50, 25)
                };
                
                setCellsA[i].ValueChanged += (s, e) => UpdateSetScore(setIndex);
                grid.Controls.Add(setCellsA[i], i + 1, 1);
            }

            nudWinsGridA = CreateGridNumericUpDown(0, 5, TeamAColor);
            grid.Controls.Add(nudWinsGridA, 6, 1);

            // Team B row
            lblGridTeamB = new Label
            {
                Text = "Tim B",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TeamBColor,
                AutoSize = false,
                MinimumSize = new Size(80, 25)
            };
            grid.Controls.Add(lblGridTeamB, 0, 2);

            setCellsB = new NumericUpDown[5];
            for (int i = 0; i < 5; i++)
            {
                int setIndex = i;
                setCellsB[i] = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 999,
                    Value = 0,
                    Dock = DockStyle.Fill,
                    TextAlign = HorizontalAlignment.Center,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    BackColor = Color.FromArgb(40, 52, 76),
                    ForeColor = TextColor,
                    BorderStyle = BorderStyle.FixedSingle,
                    Enabled = false,
                    Margin = new Padding(2),
                    MinimumSize = new Size(50, 25)
                };
                
                setCellsB[i].ValueChanged += (s, e) => UpdateSetScore(setIndex);
                grid.Controls.Add(setCellsB[i], i + 1, 2);
            }

            nudWinsGridB = CreateGridNumericUpDown(0, 5, TeamBColor);
            grid.Controls.Add(nudWinsGridB, 6, 2);

            // Add tooltips
            var tooltip = new ToolTip();
            for (int i = 0; i < 5; i++)
            {
                tooltip.SetToolTip(setCellsA[i], "Edit untuk mengubah skor set");
                tooltip.SetToolTip(setCellsB[i], "Edit untuk mengubah skor set");
            }

            panel.Controls.Add(grid);
            return panel;
        }

        private NumericUpDown CreateGridNumericUpDown(int value, int maximum, Color accentColor)
        {
            var nud = new NumericUpDown
            {
                Minimum = 0,
                Maximum = maximum,
                Value = value,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 52, 76),
                ForeColor = accentColor,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                ReadOnly = true,
                MinimumSize = new Size(50, 25)
            };

            nud.Controls[0].Hide();
            return nud;
        }
                
        private void UpdateSetScore(int setIndex)
        {
            if (!matchStarted || suppressValueChanged) return;

            int scoreA = (int)setCellsA[setIndex].Value;
            int scoreB = (int)setCellsB[setIndex].Value;

            // Store scores
            setScores[setIndex] = (scoreA, scoreB);

            // If updating current set, update current scores
            if (setIndex == currentSet - 1)
            {
                pointsA = scoreA;
                pointsB = scoreB;
            }

            // Recalculate set wins
            RecalculateSetWins();
            UpdateUI();

            // Show status message
            UpdateMatchStatus($"Skor Set {setIndex + 1} diperbarui: {scoreA}-{scoreB}");
        }

        private void RecalculateSetWins()
        {
            setWinsA = setWinsB = 0;
            
            for (int i = 0; i < setScores.Length; i++)
            {
                var (a, b) = setScores[i];
                if (a == 0 && b == 0) continue;

                int target = i + 1 == totalSets ? 15 : 25;
                int minLead = 2;
                
                if (a > b && a >= target && (a - b) >= minLead)
                    setWinsA++;
                else if (b > a && b >= target && (b - a) >= minLead)
                    setWinsB++;
            }
            
            // Update displays
            suppressValueChanged = true;
            nudTopWinsA.Value = setWinsA;
            nudTopWinsB.Value = setWinsB;
            nudWinsGridA.Value = setWinsA;
            nudWinsGridB.Value = setWinsB;
            suppressValueChanged = false;
            
            UpdateProgressBars();
            CheckMatchEndByWins();
        }

        private void CreateControlsSection(TableLayoutPanel parent)
        {
            panelControls = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(22, 32, 50),
                Padding = new Padding(10),
                Margin = new Padding(5),
                MinimumSize = new Size(800, 140) // Diperbesar dari 100 ke 140
            };

            var controlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = false
            };

            // Responsive column proportions
            controlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            controlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            controlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            // Team A controls
            var teamAControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(8),
                WrapContents = true,
                AutoScroll = true
            };
            teamAControls.Controls.AddRange(new Control[] { btnMinusA, btnPlusA, btnSetWinA });
            controlLayout.Controls.Add(teamAControls, 0, 0);

            // Center controls
            var centerControls = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(8)
            };

            centerControls.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            centerControls.RowStyles.Add(new RowStyle(SizeType.Percent, 65));

            // Format combo di atas
            var lblFormat = new Label
            {
                Text = "Format:",
                AutoSize = true,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                MinimumSize = new Size(60, 25)
            };

            cboFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 52, 76),
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 9f),
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                MinimumSize = new Size(100, 25)
            };
            cboFormat.Items.AddRange(new[] { "Best of 3", "Best of 5" });

            // Row 0: Format label dan combo
            centerControls.Controls.Add(lblFormat, 0, 0);
            centerControls.Controls.Add(cboFormat, 1, 0);
            centerControls.SetColumnSpan(cboFormat, 2);

            // Row 1: Buttons
            btnStartReset = CreateStyledButton("RESET", 8, TeamBColor);
            btnManualSetWins = CreateStyledButton("MANUAL", 8, Color.FromArgb(120, 80, 20));
            btnSwapTeams = CreateStyledButton("TUKAR", 8, Color.FromArgb(80, 80, 120));
            var btnApplySets = CreateStyledButton("TERAPKAN", 8, Color.FromArgb(20, 110, 80));
            btnApplySets.Name = "btnApplySets";

            // Set responsive button sizes
            SetButtonMinSize(btnStartReset, 80, 35);
            SetButtonMinSize(btnManualSetWins, 80, 35);
            SetButtonMinSize(btnSwapTeams, 80, 35);
            SetButtonMinSize(btnApplySets, 80, 35);

            btnStartReset.Margin = new Padding(2);
            btnManualSetWins.Margin = new Padding(2);
            btnSwapTeams.Margin = new Padding(2);
            btnApplySets.Margin = new Padding(2);

            btnStartReset.Dock = DockStyle.Fill;
            btnManualSetWins.Dock = DockStyle.Fill;
            btnSwapTeams.Dock = DockStyle.Fill;
            btnApplySets.Dock = DockStyle.Fill;

            // Add buttons to row 1
            centerControls.Controls.Add(btnStartReset, 0, 1);
            centerControls.Controls.Add(btnManualSetWins, 1, 1);
            centerControls.Controls.Add(btnSwapTeams, 2, 1);
            centerControls.Controls.Add(btnApplySets, 3, 1);

            controlLayout.Controls.Add(centerControls, 1, 0);

            // Team B controls
            var teamBControls = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8),
                WrapContents = true,
                AutoScroll = true
            };
            teamBControls.Controls.AddRange(new Control[] { btnPlusB, btnMinusB, btnSetWinB });
            controlLayout.Controls.Add(teamBControls, 2, 0);

            panelControls.Controls.Add(controlLayout);
            parent.Controls.Add(panelControls, 0, 3);
        }

        private Button CreateStyledButton(string text, int fontSize, Color bgColor)
        {
            var button = new Button
            {
                Text = text,
                BackColor = bgColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", fontSize, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(3),
                UseVisualStyleBackColor = false
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ChangeColorBrightness(bgColor, 0.2f);
            button.FlatAppearance.MouseDownBackColor = ChangeColorBrightness(bgColor, -0.1f);
            
            return button;
        }

        private void SetButtonMinSize(Button button, int width, int height)
        {
            button.MinimumSize = new Size(width, height);
            button.AutoSize = false;
        }

        private Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.R;
            float green = color.G;
            float blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(Math.Max(0, Math.Min(255, (int)red)),
                                  Math.Max(0, Math.Min(255, (int)green)),
                                  Math.Max(0, Math.Min(255, (int)blue)));
        }

        private void SetupEventHandlers()
        {
            // Logo selection
            btnLogoA.Click += (s, e) => LoadLogo(picA);
            btnLogoB.Click += (s, e) => LoadLogo(picB);

            // Score changes with serve update fix
           btnPlusA.Click += (s, e) => { 
                if (!matchOver && matchStarted) { 
                    pointsA++; 
                    SetServe('A');
                    UpdateUI();
                    CheckSetWin();
                } 
            };

            btnPlusB.Click += (s, e) => { 
                if (!matchOver && matchStarted) { 
                    pointsB++; 
                    SetServe('B');
                    UpdateUI();
                    CheckSetWin();
                } 
            };

            btnMinusA.Click += (s, e) => { if (!matchOver && pointsA > 0) { pointsA--; UpdateUI(); } };
            btnMinusB.Click += (s, e) => { if (!matchOver && pointsB > 0) { pointsB--; UpdateUI(); } };

            // Set wins
            btnSetWinA.Click += (s, e) => ConfirmSetWin('A');
            btnSetWinB.Click += (s, e) => ConfirmSetWin('B');

            // Match controls
            btnStartReset.Click += (s, e) => ToggleMatchState();
            btnManualSetWins.Click += (s, e) => ShowManualSetWinsDialog();
            btnSwapTeams.Click += (s, e) => SwapTeams();

            // Format changes
            cboFormat.SelectedIndexChanged += (s, e) => UpdateFormat();

            // Numeric controls with proper event handling
            nudTargetWins.ValueChanged += (s, e) => { if (!suppressValueChanged) UpdateTargetWins(); };
            nudTopWinsA.ValueChanged += (s, e) => { if (!suppressValueChanged) UpdateSetWins('A'); };
            nudTopWinsB.ValueChanged += (s, e) => { if (!suppressValueChanged) UpdateSetWins('B'); };
            nudWinsGridA.ValueChanged += (s, e) => { if (!suppressValueChanged) UpdateSetWins('A'); };
            nudWinsGridB.ValueChanged += (s, e) => { if (!suppressValueChanged) UpdateSetWins('B'); };

            // Team name changes
            txtTeamA.TextChanged += (s, e) => { 
                if (lblGridTeamA != null) lblGridTeamA.Text = txtTeamA.Text; 
                UpdateProgressBars(); 
            };
            txtTeamB.TextChanged += (s, e) => { 
                if (lblGridTeamB != null) lblGridTeamB.Text = txtTeamB.Text; 
                UpdateProgressBars(); 
            };

            // Apply sets button
            var applySetsBtn = Controls.Find("btnApplySets", true).FirstOrDefault() as Button;
            if (applySetsBtn != null)
                applySetsBtn.Click += (s, e) => ApplySetsFromGrid();

            // Window resize event for responsive scaling
            this.Resize += (s, e) => {
                UpdateUI();
                ScaleAllFonts();
            };
            
            // Load event for initial scaling
            this.Load += (s, e) => {
                ScaleAllFonts();
                UpdateUI();
            };
        }

        private void ScaleAllFonts()
        {
            try
            {
                // Get form dimensions for scaling calculations
                int formWidth = Math.Max(this.Width, 1000);
                int formHeight = Math.Max(this.Height, 700);
                
                // Scale tournament name based on panel width
                if (txtTournament != null && panelHeader != null && panelHeader.Width > 0)
                {
                    float headerFontSize = Math.Max(14f, Math.Min(28f, panelHeader.Width / 25f));
                    txtTournament.Font = new Font("Segoe UI", headerFontSize, FontStyle.Bold);
                }

                // Scale main scores with improved algorithm
                if (lblScoreA != null)
                    ScaleFontToFit(lblScoreA, Math.Max(40f, Math.Min(120f, formWidth / 15f)));
                if (lblScoreB != null)
                    ScaleFontToFit(lblScoreB, Math.Max(40f, Math.Min(120f, formWidth / 15f)));
                    
                // Scale team scores
                if (lblTeamScoreA != null)
                    ScaleFontToFit(lblTeamScoreA, Math.Max(30f, Math.Min(80f, formWidth / 20f)));
                if (lblTeamScoreB != null)
                    ScaleFontToFit(lblTeamScoreB, Math.Max(30f, Math.Min(80f, formWidth / 20f)));

                // Scale numeric controls
                if (nudTopWinsA != null && panelCenter != null && panelCenter.Width > 0)
                {
                    float nudFontSize = Math.Max(18f, Math.Min(36f, panelCenter.Width / 12f));
                    nudTopWinsA.Font = new Font("Segoe UI", nudFontSize, FontStyle.Bold);
                    nudTopWinsB.Font = new Font("Segoe UI", nudFontSize, FontStyle.Bold);
                    nudTargetWins.Font = new Font("Segoe UI", nudFontSize, FontStyle.Bold);
                }

                // Scale serve indicators
                if (lblServeIndicatorA != null && panelCenter != null && panelCenter.Width > 0)
                {
                    float serveFontSize = Math.Max(9f, Math.Min(14f, panelCenter.Width / 30f));
                    lblServeIndicatorA.Font = new Font("Segoe UI", serveFontSize, FontStyle.Bold);
                    lblServeIndicatorB.Font = new Font("Segoe UI", serveFontSize, FontStyle.Bold);
                }

                // Scale current set and status labels
                if (lblCurrentSet != null && panelCenter != null && panelCenter.Width > 0)
                {
                    float statusFontSize = Math.Max(10f, Math.Min(16f, panelCenter.Width / 25f));
                    lblCurrentSet.Font = new Font("Segoe UI", statusFontSize, FontStyle.Bold);
                    
                    float matchStatusFontSize = Math.Max(9f, Math.Min(14f, panelCenter.Width / 30f));
                    lblMatchStatus.Font = new Font("Segoe UI", matchStatusFontSize, FontStyle.Italic);
                }
                
                // Scale team name textboxes
                if (txtTeamA != null && panelA != null && panelA.Width > 0)
                {
                    float teamNameSize = Math.Max(10f, Math.Min(18f, panelA.Width / 20f));
                    txtTeamA.Font = new Font("Segoe UI", teamNameSize, FontStyle.Bold);
                    txtTeamB.Font = new Font("Segoe UI", teamNameSize, FontStyle.Bold);
                }

                // Scale grid elements
                if (setCellsA != null && setCellsA.Length > 0)
                {
                    float gridFontSize = Math.Max(9f, Math.Min(14f, formWidth / 100f));
                    foreach (var cell in setCellsA)
                    {
                        if (cell != null)
                            cell.Font = new Font("Segoe UI", gridFontSize, FontStyle.Bold);
                    }
                    foreach (var cell in setCellsB)
                    {
                        if (cell != null)
                            cell.Font = new Font("Segoe UI", gridFontSize, FontStyle.Bold);
                    }
                    
                    if (nudWinsGridA != null)
                        nudWinsGridA.Font = new Font("Segoe UI", gridFontSize, FontStyle.Bold);
                    if (nudWinsGridB != null)
                        nudWinsGridB.Font = new Font("Segoe UI", gridFontSize, FontStyle.Bold);
                }
                
            }
            catch (Exception ex)
            {
                // Ignore font scaling errors
                System.Diagnostics.Debug.WriteLine($"Font scaling error: {ex.Message}");
            }
        }

        private void InitializeDefaults()
        {
            txtTeamA.Text = "Tim A";
            txtTeamB.Text = "Tim B";
            cboFormat.SelectedIndex = 0;
            
            suppressValueChanged = true;
            nudTargetWins.Value = targetWins;
            suppressValueChanged = false;

            SetServe('A');
            UpdateFormatVisibility();
            UpdateUI();
            UpdateCurrentSetDisplay();
            UpdateMatchStatus("Siap untuk memulai");
        }

        private void LoadLogo(PictureBox pic)
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Title = "Pilih Logo Tim"
            };
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var tmp = Image.FromFile(ofd.FileName);
                    pic.Image = new Bitmap(tmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal memuat gambar: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SetServe(char team)
        {
            serving = team;
            UpdateServeIndicators();
        }

        private void UpdateServeIndicators()
        {
            if (lblServeIndicatorA != null)
                lblServeIndicatorA.Visible = (serving == 'A') && matchStarted && !matchOver;
            if (lblServeIndicatorB != null)
                lblServeIndicatorB.Visible = (serving == 'B') && matchStarted && !matchOver;
        }

        private void UpdateFormat()
        {
            if (matchStarted) return;
            
            targetWins = cboFormat.SelectedIndex == 0 ? 2 : 3;
            suppressValueChanged = true;
            nudTargetWins.Value = targetWins;
            suppressValueChanged = false;
            
            UpdateFormatVisibility();
            UpdateCurrentSetDisplay();
        }

        private void UpdateTargetWins()
        {
            targetWins = (int)nudTargetWins.Value;
            suppressValueChanged = true;
            cboFormat.SelectedIndex = (targetWins == 2) ? 0 : (targetWins == 3 ? 1 : -1);
            suppressValueChanged = false;
            
            UpdateFormatVisibility();
            UpdateCurrentSetDisplay();
        }

        private void UpdateSetWins(char team)
        {
            if (team == 'A')
            {
                setWinsA = (int)nudTopWinsA.Value;
                suppressValueChanged = true;
                if (nudWinsGridA != null) nudWinsGridA.Value = setWinsA;
                suppressValueChanged = false;
            }
            else
            {
                setWinsB = (int)nudTopWinsB.Value;
                suppressValueChanged = true;
                if (nudWinsGridB != null) nudWinsGridB.Value = setWinsB;
                suppressValueChanged = false;
            }
            
            UpdateProgressBars();
            CheckMatchEndByWins();
        }

        private void UpdateFormatVisibility()
        {
            bool showAllSets = targetWins >= 3;
            
            for (int i = 3; i < 5; i++)
            {
                if (setCellsA?[i] != null)
                {
                    setCellsA[i].Enabled = showAllSets && matchStarted;
                    setCellsA[i].Visible = showAllSets;
                }
                if (setCellsB?[i] != null)
                {
                    setCellsB[i].Enabled = showAllSets && matchStarted;
                    setCellsB[i].Visible = showAllSets;
                }
            }
        }

        private void UpdateCurrentSetDisplay()
        {
            if (lblCurrentSet == null) return;
            
            if (matchOver)
            {
                lblCurrentSet.Text = "PERTANDINGAN SELESAI";
                lblCurrentSet.ForeColor = GoldColor;
                return;
            }

            int target = GetCurrentTargetPoints();
            string setType = currentSet == totalSets ? "SET PENENTU" : $"SET {currentSet}";
            lblCurrentSet.Text = $"{setType} - Target: {target} poin";
            lblCurrentSet.ForeColor = currentSet == totalSets ? Color.FromArgb(231, 76, 60) : AccentColor;
        }

        private void UpdateMatchStatus(string status)
        {
            if (lblMatchStatus != null)
                lblMatchStatus.Text = status;
        }

        private void UpdateProgressBars()
        {
            if (targetWins == 0) return;
            
            int progressValueA = (int)((float)setWinsA / targetWins * 100);
            int progressValueB = (int)((float)setWinsB / targetWins * 100);
            
            if (progressA != null) progressA.Value = Math.Min(100, progressValueA);
            if (progressB != null) progressB.Value = Math.Min(100, progressValueB);
        }

        private int GetCurrentTargetPoints()
        {
            return currentSet == totalSets ? 15 : 25;
        }

        private void CheckSetWin()
        {
            int target = GetCurrentTargetPoints();
            int minLead = 2;
            
            if ((pointsA >= target && pointsA - pointsB >= minLead) ||
                (pointsB >= target && pointsB - pointsA >= minLead))
            {
                string potentialWinner = pointsA > pointsB ? txtTeamA.Text : txtTeamB.Text;
                UpdateMatchStatus($"{potentialWinner} mencapai poin kemenangan");
            }
        }

        private void ToggleMatchState()
        {
            if (!matchStarted)
            {
                StartMatch();
            }
            else
            {
                var result = MessageBox.Show("Reset pertandingan?", "Konfirmasi", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    ResetMatch();
            }
        }

        private void StartMatch()
        {
            matchStarted = true;
            matchOver = false;
            
            btnStartReset.Text = "RESET";
            btnStartReset.BackColor = TeamBColor;
            
            txtTournament.ReadOnly = true;
            txtTeamA.ReadOnly = true;
            txtTeamB.ReadOnly = true;
            
            // Enable set score editing
            for (int i = 0; i < 5; i++)
            {
                if (setCellsA?[i] != null) setCellsA[i].Enabled = true;
                if (setCellsB?[i] != null) setCellsB[i].Enabled = true;
            }
            
            UpdateFormatVisibility();
            UpdateMatchStatus("Pertandingan dimulai!");
            UpdateServeIndicators();
        }

        private void ResetMatch()
        {
            matchStarted = false;
            matchOver = false;
            
            btnStartReset.Text = "MULAI";
            btnStartReset.BackColor = AccentColor;
            
            txtTournament.ReadOnly = false;
            txtTeamA.ReadOnly = false;
            txtTeamB.ReadOnly = false;
            
            // Reset scores
            pointsA = pointsB = 0;
            setWinsA = setWinsB = 0;
            currentSet = 1;
            
            // Reset and disable set cells
            suppressValueChanged = true;
            for (int i = 0; i < 5; i++)
            {
                setScores[i] = (0, 0);
                if (setCellsA?[i] != null)
                {
                    setCellsA[i].Value = 0;
                    setCellsA[i].Enabled = false;
                }
                if (setCellsB?[i] != null)
                {
                    setCellsB[i].Value = 0;
                    setCellsB[i].Enabled = false;
                }
            }
            
            // Reset win displays
            if (nudWinsGridA != null) nudWinsGridA.Value = 0;
            if (nudWinsGridB != null) nudWinsGridB.Value = 0;
            if (nudTopWinsA != null) nudTopWinsA.Value = 0;
            if (nudTopWinsB != null) nudTopWinsB.Value = 0;
            suppressValueChanged = false;
            
            ResetTeamPanelColors();
            UpdateUI();
            UpdateCurrentSetDisplay();
            UpdateMatchStatus("Siap untuk memulai");
        }

        private void SwapTeams()
        {
            if (matchStarted)
            {
                var result = MessageBox.Show(
                    "Anda yakin ingin menukar posisi tim?\n" +
                    "Semua skor dan statistik akan ditukar.",
                    "Konfirmasi Tukar Tim",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes) return;
            }
            
            // Swap names
            string tempName = txtTeamA.Text;
            txtTeamA.Text = txtTeamB.Text;
            txtTeamB.Text = tempName;
            
            // Swap logos
            Image tempImage = picA.Image;
            picA.Image = picB.Image;
            picB.Image = tempImage;

            if (matchStarted)
            {
                // Swap current scores
                int tempPoints = pointsA;
                pointsA = pointsB;
                pointsB = tempPoints;

                // Swap set wins
                int tempWins = setWinsA;
                setWinsA = setWinsB;
                setWinsB = tempWins;

                // Swap set scores
                suppressValueChanged = true;
                for (int i = 0; i < setScores.Length; i++)
                {
                    var tempScore = setScores[i];
                    setScores[i] = (tempScore.b, tempScore.a);

                    if (setCellsA?[i] != null && setCellsB?[i] != null)
                    {
                        var tempCellValue = setCellsA[i].Value;
                        setCellsA[i].Value = setCellsB[i].Value;
                        setCellsB[i].Value = tempCellValue;
                    }
                }

                // Swap serve indicator
                if (serving == 'A') 
                    SetServe('B');
                else if (serving == 'B')
                    SetServe('A');

                // Update displays
                if (nudTopWinsA != null) nudTopWinsA.Value = setWinsA;
                if (nudTopWinsB != null) nudTopWinsB.Value = setWinsB;
                if (nudWinsGridA != null) nudWinsGridA.Value = setWinsA;
                if (nudWinsGridB != null) nudWinsGridB.Value = setWinsB;
                suppressValueChanged = false;

                UpdateUI();
                UpdateCurrentSetDisplay();
                UpdateProgressBars();
                UpdateMatchStatus("Posisi tim berhasil ditukar!");
            }
            else
            {
                UpdateMatchStatus("Tim berhasil ditukar");
            }
        }

        private void UpdateUI()
        {
            // Update central scores with improved width handling
            if (lblScoreA != null) 
            {
                lblScoreA.Text = pointsA.ToString();
                ScaleFontToFit(lblScoreA, 120f);
            }
            if (lblScoreB != null) 
            {
                lblScoreB.Text = pointsB.ToString();
                ScaleFontToFit(lblScoreB, 120f);
            }
            
            // Update team panel scores
            if (lblTeamScoreA != null)
            {
                lblTeamScoreA.Text = pointsA.ToString();
                ScaleFontToFit(lblTeamScoreA, 80f);
            }
            
            if (lblTeamScoreB != null)
            {
                lblTeamScoreB.Text = pointsB.ToString();
                ScaleFontToFit(lblTeamScoreB, 80f);
            }
            
            // Sync all win displays
            suppressValueChanged = true;
            if (nudTopWinsA != null) nudTopWinsA.Value = Math.Min(nudTopWinsA.Maximum, setWinsA);
            if (nudTopWinsB != null) nudTopWinsB.Value = Math.Min(nudTopWinsB.Maximum, setWinsB);
            if (nudWinsGridA != null) nudWinsGridA.Value = Math.Min(nudWinsGridA.Maximum, setWinsA);
            if (nudWinsGridB != null) nudWinsGridB.Value = Math.Min(nudWinsGridB.Maximum, setWinsB);
            suppressValueChanged = false;

            // Visual indicator for potential set win
            int target = GetCurrentTargetPoints();
            int minLead = 2;
            
            if (pointsA >= target && pointsA - pointsB >= minLead)
            {
                if (lblScoreA != null) lblScoreA.BackColor = ChangeColorBrightness(TeamAColor, 0.3f);
                if (lblTeamScoreA != null) lblTeamScoreA.BackColor = ChangeColorBrightness(TeamAColor, 0.3f);
            }
            else if (pointsB >= target && pointsB - pointsA >= minLead)
            {
                if (lblScoreB != null) lblScoreB.BackColor = ChangeColorBrightness(TeamBColor, 0.3f);
                if (lblTeamScoreB != null) lblTeamScoreB.BackColor = ChangeColorBrightness(TeamBColor, 0.3f);
            }
            else
            {
                if (lblScoreA != null) lblScoreA.BackColor = Color.FromArgb(35, 47, 71);
                if (lblScoreB != null) lblScoreB.BackColor = Color.FromArgb(35, 47, 71);
                if (lblTeamScoreA != null) lblTeamScoreA.BackColor = Color.FromArgb(35, 47, 71);
                if (lblTeamScoreB != null) lblTeamScoreB.BackColor = Color.FromArgb(35, 47, 71);
            }
            
            UpdateServeIndicators();
            UpdateProgressBars();
        }

        private void ScaleFontToFit(Label label, float maxSize)
        {
            if (label == null || string.IsNullOrEmpty(label.Text) || label.ClientSize.Width <= 0 || label.ClientSize.Height <= 0) 
                return;
            
            try
            {
                using (var g = label.CreateGraphics())
                {
                    // Start with maximum size and work down
                    for (float size = maxSize; size >= 12; size -= 2f)
                    {
                        using (var testFont = new Font(label.Font.FontFamily, size, label.Font.Style))
                        {
                            var textSize = g.MeasureString(label.Text, testFont);
                            
                            // Allow more generous margins to prevent clipping
                            if (textSize.Width <= label.ClientSize.Width - 20 && 
                                textSize.Height <= label.ClientSize.Height - 10)
                            {
                                if (Math.Abs(label.Font.Size - size) > 1f)
                                    label.Font = new Font(label.Font.FontFamily, size, label.Font.Style);
                                return;
                            }
                        }
                    }
                    
                    // Fallback to minimum size
                    if (Math.Abs(label.Font.Size - 12f) > 1f)
                        label.Font = new Font(label.Font.FontFamily, 12f, label.Font.Style);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Font scaling error: {ex.Message}");
            }
        }

        private void ConfirmSetWin(char winner)
        {
            if (matchOver || !matchStarted) return;

            int target = GetCurrentTargetPoints();
            int minLead = 2;
            int winnerScore = winner == 'A' ? pointsA : pointsB;
            int loserScore = winner == 'A' ? pointsB : pointsA;
            string winnerName = winner == 'A' ? txtTeamA.Text : txtTeamB.Text;

            if (winnerScore < target)
            {
                MessageBox.Show(
                    $"Tim belum mencapai target poin ({target})!\n" +
                    $"Skor saat ini: {pointsA} - {pointsB}",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (winnerScore - loserScore < minLead)
            {
                MessageBox.Show(
                    "Selisih poin harus minimal 2 poin untuk memenangkan set!",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Konfirmasi {winnerName} memenangkan Set {currentSet}?\n\n" +
                $"Skor: {pointsA} - {pointsB}",
                "Konfirmasi Set",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                ProcessSetWin(winner);
        }

        private void ProcessSetWin(char winner)
        {
            // Record set score for current set
            setScores[currentSet - 1] = (pointsA, pointsB);
            
            suppressValueChanged = true;
            if (setCellsA?[currentSet - 1] != null) setCellsA[currentSet - 1].Value = pointsA;
            if (setCellsB?[currentSet - 1] != null) setCellsB[currentSet - 1].Value = pointsB;
            suppressValueChanged = false;
            
            // Update set wins based on winner
            if (winner == 'A')
                setWinsA++;
            else
                setWinsB++;
            
            // Update displays
            suppressValueChanged = true;
            if (nudTopWinsA != null) nudTopWinsA.Value = setWinsA;
            if (nudTopWinsB != null) nudTopWinsB.Value = setWinsB;
            if (nudWinsGridA != null) nudWinsGridA.Value = setWinsA;
            if (nudWinsGridB != null) nudWinsGridB.Value = setWinsB;
            suppressValueChanged = false;

            string teamName = winner == 'A' ? txtTeamA.Text : txtTeamB.Text;
            
            // Check match end conditions
            if (setWinsA == targetWins)
            {
                EndMatch('A');
                return;
            }
            else if (setWinsB == targetWins)
            {
                EndMatch('B');
                return;
            }
            
            // Continue to next set
            currentSet++;
            pointsA = pointsB = 0;
            SetServe(winner == 'A' ? 'B' : 'A');
            
            UpdateUI();
            UpdateCurrentSetDisplay();
            UpdateProgressBars();
            UpdateMatchStatus($"{teamName} memenangkan set!Lanjut Set-{currentSet}");
        }

        private void EndMatch(char winner)
        {
            if (matchOver) return;

            if ((winner == 'A' && setWinsA != targetWins) ||
                (winner == 'B' && setWinsB != targetWins))
            {
                return;
            }

            matchOver = true;
            string winnerName = winner == 'A' ? txtTeamA.Text : txtTeamB.Text;

            UpdateCurrentSetDisplay();
            UpdateMatchStatus($"{winnerName} menang dengan skor {setWinsA}-{setWinsB}!");

            if (winner == 'A')
            {
                HighlightWinner(panelA, TeamAColor);
                DimLoser(panelB);
            }
            else
            {
                HighlightWinner(panelB, TeamBColor);
                DimLoser(panelA);
            }

            MessageBox.Show(
                $"🏆 SELAMAT! 🏆\n\n{winnerName} memenangkan pertandingan!\n\n" +
                $"Skor akhir: {setWinsA}-{setWinsB}",
                "Pemenang!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void HighlightWinner(Panel panel, Color teamColor)
        {
            if (panel != null)
                panel.BackColor = ChangeColorBrightness(teamColor, 0.3f);
        }

        private void DimLoser(Panel panel)
        {
            if (panel != null)
                panel.BackColor = Color.FromArgb(40, 40, 40);
        }

        private void ResetTeamPanelColors()
        {
            if (panelA != null) panelA.BackColor = SecondaryColor;
            if (panelB != null) panelB.BackColor = SecondaryColor;
        }

        private void CheckMatchEndByWins()
        {
            if (!matchStarted || matchOver) return;

            if (setWinsA == targetWins)
            {
                EndMatch('A');
            }
            else if (setWinsB == targetWins)
            {
                EndMatch('B');
            }

            UpdateProgressBars();
        }
        
        private void ShowManualSetWinsDialog()
        {
            var form = new Form
            {
                Text = "Atur Set Manual",
                Size = new Size(450, 280),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = PrimaryColor,
                ForeColor = TextColor,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Padding = new Padding(20)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                RowStyles = {
                    new RowStyle(SizeType.Absolute, 60),
                    new RowStyle(SizeType.Absolute, 60),
                    new RowStyle(SizeType.Absolute, 80)
                }
            };

            // Label dan NumericUpDown untuk Tim A
            var lblA = new Label
            {
                Text = $"Set {txtTeamA.Text}:",
                ForeColor = TeamAColor,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                MinimumSize = new Size(120, 30)
            };

            var numA = CreateNumericUpDown(setWinsA, 5, TeamAColor);
            numA.Size = new Size(150, 40);
            numA.Margin = new Padding(8);

            // Label dan NumericUpDown untuk Tim B
            var lblB = new Label
            {
                Text = $"Set {txtTeamB.Text}:",
                ForeColor = TeamBColor,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                MinimumSize = new Size(120, 30)
            };

            var numB = CreateNumericUpDown(setWinsB, 5, TeamBColor);
            numB.Size = new Size(150, 40);
            numB.Margin = new Padding(8);

            // Button panel
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(0, 20, 0, 0)
            };

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var btnOK = CreateStyledButton("OK", 12, TeamAColor);
            var btnCancel = CreateStyledButton("Batal", 12, TeamBColor);

            SetButtonMinSize(btnOK, 100, 40);
            SetButtonMinSize(btnCancel, 100, 40);

            btnOK.Margin = new Padding(25, 0, 12, 0);
            btnCancel.Margin = new Padding(12, 0, 25, 0);

            btnOK.Click += (s, e) =>
            {
                setWinsA = (int)numA.Value;
                setWinsB = (int)numB.Value;
                UpdateUI();
                CheckMatchEndByWins();
                form.Close();
            };

            btnCancel.Click += (s, e) => form.Close();

            // Add controls to layout
            layout.Controls.Add(lblA, 0, 0);
            layout.Controls.Add(numA, 1, 0);
            layout.Controls.Add(lblB, 0, 1);
            layout.Controls.Add(numB, 1, 1);

            buttonPanel.Controls.Add(btnOK, 0, 0);
            buttonPanel.Controls.Add(btnCancel, 1, 0);
            
            layout.Controls.Add(buttonPanel, 0, 2);
            layout.SetColumnSpan(buttonPanel, 2);

            form.Controls.Add(layout);
            form.ShowDialog(this);
        }

        private void ApplySetsFromGrid()
        {
            suppressValueChanged = true;
            
            // Reset wins
            setWinsA = 0;
            setWinsB = 0;
            
            // Recalculate from grid
            for (int i = 0; i < Math.Min(5, totalSets); i++)
            {
                if (setCellsA?[i] != null && setCellsB?[i] != null)
                {
                    int scoreA = (int)setCellsA[i].Value;
                    int scoreB = (int)setCellsB[i].Value;
                    
                    setScores[i] = (scoreA, scoreB);
                    
                    // Check if this set has a winner
                    int target = i + 1 == totalSets ? 15 : 25;
                    int minLead = 2;
                    
                    if (scoreA > scoreB && scoreA >= target && (scoreA - scoreB) >= minLead)
                        setWinsA++;
                    else if (scoreB > scoreA && scoreB >= target && (scoreB - scoreA) >= minLead)
                        setWinsB++;
                }
            }

            // Update displays
            if (nudTopWinsA != null) nudTopWinsA.Value = setWinsA;
            if (nudTopWinsB != null) nudTopWinsB.Value = setWinsB;
            if (nudWinsGridA != null) nudWinsGridA.Value = setWinsA;
            if (nudWinsGridB != null) nudWinsGridB.Value = setWinsB;
            
            suppressValueChanged = false;
            
            UpdateUI();
            UpdateProgressBars();
            CheckMatchEndByWins();
            
            MessageBox.Show("Perubahan set berhasil diterapkan!", "Sukses", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}