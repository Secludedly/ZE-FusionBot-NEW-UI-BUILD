using FontAwesome.Sharp;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.WinForms.Properties;
using SysBot.Pokemon.Z3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    public sealed partial class Main : Form
    {
        // Main form properties
        private Form activeForm = null; // Active child form in the main panel

        // Running environment and configuration
        private IPokeBotRunner RunningEnvironment { get; set; } // Bot runner based on game mode

        // Program configuration
        private ProgramConfig Config { get; set; }

        // Static properties for update state
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // Do not serialize in the designer
        public static bool IsUpdating { get; set; } = false;

        // Program has update flag
        internal bool hasUpdate = false;

        // Flag to indicate if the form is still loading
        private bool _isFormLoading = true;

        // Left‑side button styling
        private IconButton currentBtn;                             // Currently active button
        private Panel leftBorderBtn;                               // Left border panel for the active button
        private Dictionary<IconButton, Timer> hoverTimers = new(); // Dictionary to hold hover timers for buttons

        // BotsForm & runner to help manage the bots
        private readonly List<PokeBotState> Bots = new(); // List of bots created in the program
        private BotsForm _botsForm;                       // BotsForm instance to manage bot controls
        private IPokeBotRunner _runner;                   // Runner instance to manage bot operations

        // Place GameMode BG Images into panelLeftSide
        public Panel PanelLeftSide => panelLeftSide;

        // LogsForm loading
        private LogsForm _logsForm;

        // HubForm loading
        private HubForm _hubForm;

        // Main Constructor
        public Main()
        {
            Task.Run(BotMonitor);      // Start the bot monitor in a separate task
            InitializeComponent();     // Initialize all the form components before program
            InitializeLeftSideImage(); // Initialize the left side image in panelLeftSide

            // Wait for the form crap to load before initializing
            this.Load += async (s, e) => await InitializeAsync();

            // Load custom fonts before initializing
            FontManager.LoadFonts(
                "bahnschrift.ttf",
                "Bobbleboddy_light.ttf",
                "gadugi.ttf",
                "gadugib.ttf",
                "segoeui.ttf",
                "segoeuib.ttf",
                "segoeuii.ttf",
                "segoeuil.ttf",
                "UbuntuMono-R.ttf",
                "UbuntuMono-B.ttf",
                "UbuntuMono-BI.ttf",
                "UbuntuMono-RI.ttf",
                "segoeuisl.ttf",
                "segoeuiz.ttf",
                "seguibl.ttf",
                "seguibli.ttf",
                "seguili.ttf",
                "seguisb.ttf",
                "seguisbi.ttf",
                "seguisli.ttf",
                "SegUIVar.ttf"
                );

            // Set up left‑panel buttons & effects
            ApplyButtonEffects();
            SetupHoverAnimation(iconButton1);                     // Bots button
            SetupHoverAnimation(iconButton2);                     // Hub button
            SetupHoverAnimation(iconButton3);                     // Logs button
            leftBorderBtn = new Panel { Size = new Size(7, 60) }; // Left border for active button
            panelLeftSide.Controls.Add(leftBorderBtn);            // Add left border to the panel
            panelTitleBar.MouseDown += panelTitleBar_MouseDown;   // Allow dragging the window from the title bar

            // Title‑bar controls
            this.Text = ""; // Set the form title to empty

            this.ControlBox = false;                                           // Disable the default Minimize/Maximize/Close
            this.FormBorderStyle = FormBorderStyle.None;                       // Remove the default form border
            this.DoubleBuffered = true;                                        // Enable double buffering to reduce flickering
            this.SetStyle(ControlStyles.ResizeRedraw, true);                   // Redraw on resize
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea; // Set the maximized bounds to the working area of the screen

            // Handlers for the Close/Maximize/Minimize buttons
            btnClose.Click += BtnClose_Click;       // Close button
            btnMaximize.Click += BtnMaximize_Click; // Maximize button
            btnMinimize.Click += BtnMinimize_Click; // Minimize button

            // Set up hover animations for Close/Maximize/Minimize buttons
            SetupTopButtonHoverAnimation(btnClose, Color.FromArgb(232, 17, 35));    // Color is red
            SetupTopButtonHoverAnimation(btnMaximize, Color.FromArgb(0, 120, 215)); // Color is blue
            SetupTopButtonHoverAnimation(btnMinimize, Color.FromArgb(255, 185, 0)); // Color is yellow
        }

        // Runs once when Main form first loads
        private async Task InitializeAsync()
        {
            if (IsUpdating)
                return;

            PokeTradeBotSWSH.SeedChecker = new Z3SeedSearchHandler<PK8>();

            try
            {
                var (updateAvailable, _, _) = await UpdateChecker.CheckForUpdatesAsync();
                hasUpdate = updateAvailable;
            }
            catch { }
            _botsForm = new BotsForm();
            _botsForm.GameModeChanged += (_, newMode) =>
            {
                Config.Mode = newMode;
                SaveCurrentConfig();
                RunningEnvironment = GetRunner(Config);
                InitUtil.InitializeStubs(newMode);
                UpdateBackgroundImage(newMode);
                UpdateRunnerAndUI();
            };

            _logsForm = new LogsForm();
            LogUtil.Forwarders.Add(new UIRichTextBoxForwarder(_logsForm.LogsBox));
            _logsForm.LogsBox.MaxLength = 32767;

            if (File.Exists(Program.ConfigPath))
            {
                var lines = File.ReadAllText(Program.ConfigPath);
                Config = JsonSerializer.Deserialize<ProgramConfig>(lines) ?? new ProgramConfig();
                LogConfig.MaxArchiveFiles = Config.Hub.MaxArchiveFiles;
                LogConfig.LoggingEnabled = Config.Hub.LoggingEnabled;

                // Clean up any invalid bot entries
                Config.Bots = Config.Bots
                    .Where(b => b != null && b.IsValid() && !string.IsNullOrWhiteSpace(b.Connection?.IP))
                    .GroupBy(b => $"{b.Connection.IP}:{b.Connection.Port}")
                    .Select(g => g.First())
                    .ToArray();

                _botsForm.GameModeBox.SelectedValue = (int)Config.Mode;
                RunningEnvironment = GetRunner(Config);

                foreach (var bot in Config.Bots)
                {
                    if (!Bots.Any(b => b.Connection.Equals(bot.Connection)))
                    {
                        if (string.IsNullOrWhiteSpace(bot.Connection?.IP) || bot.Connection.Port <= 0)
                        {
                            Console.WriteLine("Skipping invalid bot with empty IP or port.");
                            continue;
                        }
                        bot.Initialize();
                        AddBot(bot);
                    }
                }
            }
            else
            {
                Config = new ProgramConfig();
                RunningEnvironment = GetRunner(Config);
                Config.Hub.Folder.CreateDefaults(Program.WorkingDirectory);
            }

            LoadControls();
            Text = $"{(string.IsNullOrEmpty(Config.Hub.BotName) ? "ZE FusionBot |" : Config.Hub.BotName)} {TradeBot.Version} | Mode: {Config.Mode}";
            InitUtil.InitializeStubs(Config.Mode);
            _isFormLoading = false;
            UpdateBackgroundImage(Config.Mode);
            ActivateButton(iconButton1, RGBColors.color4);
            OpenChildForm(_botsForm);
            SaveCurrentConfig();

            _botsForm.StartButton.Click += B_Start_Click;
            _botsForm.StopButton.Click += B_Stop_Click;
            _botsForm.RebootStopButton.Click += B_RebootStop_Click;
            _botsForm.UpdateButton.Click += Updater_Click;
            _botsForm.AddBotButton.Click += B_New_Click;

            lblTitle.Text = Text;
        }

        private void UpdateRunnerAndUI()
        {
            Text = $"{(string.IsNullOrEmpty(Config.Hub.BotName) ? "ZE FusionBot |" : Config.Hub.BotName)} {TradeBot.Version} | Mode: {Config.Mode}";
            lblTitle.Text = Text;
        }


        // Save the current config to the file
        private static IPokeBotRunner GetRunner(ProgramConfig cfg) => cfg.Mode switch       // Get the correct runner based on the game mode
        {
            ProgramMode.SWSH => new PokeBotRunnerImpl<PK8>(cfg.Hub, new BotFactory8SWSH()), // SWSH mode
            ProgramMode.BDSP => new PokeBotRunnerImpl<PB8>(cfg.Hub, new BotFactory8BS()),   // BDSP mode
            ProgramMode.LA => new PokeBotRunnerImpl<PA8>(cfg.Hub, new BotFactory8LA()),     // PLA mode
            ProgramMode.SV => new PokeBotRunnerImpl<PK9>(cfg.Hub, new BotFactory9SV()),     // SV mode
            ProgramMode.LGPE => new PokeBotRunnerImpl<PB7>(cfg.Hub, new BotFactory7LGPE()), // LGPE mode
            _ => throw new IndexOutOfRangeException("Unsupported mode."),                   // Invalid mode
        };

        // Save the current config to the file again
        private async Task BotMonitor() // Monitor the bots and update their state
        {
            while (!Disposing)          // Main form is kept alive
            {
                try
                {
                    foreach (var c in _botsForm.BotPanel.Controls.OfType<BotController>()) // Iterate through each BotController in the BotsForm
                        c.ReadState();                                                     // Read the state of the bot controller
                }
                catch
                {
                    // Updating the collection by adding/removing bots will change the iterator
                }
                await Task.Delay(2_000).ConfigureAwait(false); // Add a delay of 2 seconds before the next iteration
            }
        }

        // Load the controls for the BotsForm
        private void LoadControls()
        {
            // Establish global minimum size for the BotsForm
            MinimumSize = Size;

            // Routine Selection
            var routines = ((PokeRoutineType[])Enum.GetValues(typeof(PokeRoutineType))).Where(z => RunningEnvironment.SupportsRoutine(z)) // Get all routine types
                .Select(z => new { Text = z.ToString(), Value = (int)z }).ToList(); // Create a list of routine types with their text and value
            _botsForm.RoutineBox.DisplayMember = "Text";                            // Set the display text for the RoutineBox
            _botsForm.RoutineBox.ValueMember = "Value";                             // Set the value number for the RoutineBox (Flextrade, etc.)
            _botsForm.RoutineBox.DataSource = routines;                             // Bind the RoutineBox to the list of routine types (Dropdown list)
            _botsForm.RoutineBox.SelectedValue = (int)PokeRoutineType.FlexTrade;    // Set the default to FlexTrade in RoutineBox

            // Protocol Selection
            var protocols = ((SwitchProtocol[])Enum.GetValues(typeof(SwitchProtocol))) // Get all switch protocols
                .Select(z => new { Text = z.ToString(), Value = (int)z }).ToList();    // Create a list of protocols with their text and value
            _botsForm.ProtocolBox.DisplayMember = "Text";                              // Set the display text for the ProtocolBox
            _botsForm.ProtocolBox.ValueMember = "Value";                               // Set the value number for the ProtocolBox (WiFi/USB)
            _botsForm.ProtocolBox.DataSource = protocols;                              // Bind the ProtocolBox to the list of protocols (Dropdown list)
            _botsForm.ProtocolBox.SelectedValue = (int)SwitchProtocol.WiFi;            // Set the default to WiFi in ProtocolBox

            // GameMode Selection
            var gameModes = Enum.GetValues(typeof(ProgramMode))                    // Get all program modes
                .Cast<ProgramMode>()                                               // Cast to ProgramMode
                .Where(m => m != ProgramMode.None)                                 // Exclude Invalid mode from list (Dropdown)
                .Select(mode => new { Text = mode.ToString(), Value = (int)mode }) // Create a list of game modes with their text and value
                .ToList();                                                         // Convert list to Text and Value properties
            _botsForm.GameModeBox.DisplayMember = "Text";                          // Set the display text for GameModeBox
            _botsForm.GameModeBox.ValueMember = "Value";                           // Set the value number for GameModeBox (SV, SWSH, BDSP, etc.)
            _botsForm.GameModeBox.DataSource = gameModes;                          // Bind the GameModeBox to the list of game modes (Dropdown list)
            _botsForm.GameModeBox.SelectedValue = (int)Config.Mode;                // Set the default to current/last used mode in GameModeBox
            SaveCurrentConfig();                                                   // Save the current config for BotsForm data
        }

        // Start the bot with the current config
        private void B_Start_Click(object sender, EventArgs e) // Start all bots on Start button click
        {
            SaveCurrentConfig();                               // Save the current config before starting the bot

            LogUtil.LogInfo("Starting all bots...", "Form");   // Log the start action
            RunningEnvironment.InitializeStart();              // Initialize the bot runner
            SendAll(BotControlCommand.Start);                  // Send the Start command to all bots present in the controller
            _logsForm.LogsBox.Select();                        // Select the logs box in the LogsForm to write to

            if (Bots.Count == 0)
                WinFormsUtil.Alert("No bots configured, but all supporting services have been started.");
        }

        // Restart the bot and stop all consoles with current config
        private void B_RebootStop_Click(object sender, EventArgs e) // Restart all bots and reboot the game on console
        {
            B_Stop_Click(sender, e); // Stop all bots first

            // Log the reboot and stop action
            Task.Run(async () =>
            {
                await Task.Delay(3_500).ConfigureAwait(false);             // Add 3.5 second delay before rebooting
                SaveCurrentConfig();                                       // Save the current config before rebooting
                LogUtil.LogInfo("Restarting all the consoles...", "Form"); // Log the restart bots action
                RunningEnvironment.InitializeStart();                      // Start up the bot runner again
                SendAll(BotControlCommand.RebootAndStop);                  // Send the RebootAndStop command to all bots
                await Task.Delay(5_000).ConfigureAwait(false);             // Add a 5 second delay before restarting the bots
                SendAll(BotControlCommand.Start);                          // Start the bot after the delay
                _logsForm.LogsBox.Select();                                // Select the logs box in the LogsForm to write to

                if (Bots.Count == 0)
                    WinFormsUtil.Alert("No bots configured, but all supporting services have been issued the reboot command.");
            });
        }

        // Sends command to all bots
        private void SendAll(BotControlCommand cmd)                                // Send command to all bots in the BotsForm
        {
            foreach (var c in _botsForm.BotPanel.Controls.OfType<BotController>()) // Iterate through each BotController in the BotsForm
                c.SendCommand(cmd);                                                // Send the command to the bot controller
        }

        // Stop or Idle/Resume all bots
        private void B_Stop_Click(object sender, EventArgs e)     // Stop all bots on Stop button click
        {
            var env = RunningEnvironment;                         // Get the current running environment
            if (!_botsForm.BotPanel.Controls.OfType<BotController>().Any(c => c.IsRunning()) && (ModifierKeys & Keys.Alt) == 0)
            // If not running and no Alt key pressed
            {
                WinFormsUtil.Alert("Nothing is currently running.");
                return;
            }

            var cmd = BotControlCommand.Stop; // Default command to stop all bots

            if ((ModifierKeys & Keys.Control) != 0 || (ModifierKeys & Keys.Shift) != 0) // If Control or Shift key is pressed
            {
                if (env.IsRunning)
                {
                    WinFormsUtil.Alert("Commanding all bots to Idle.", "Press Stop (without a modifier key) to hard-stop and unlock control, or press Stop with the modifier key again to resume.");
                    cmd = BotControlCommand.Idle;
                }
                else
                {
                    WinFormsUtil.Alert("Commanding all bots to resume their original task.", "Press Stop (without a modifier key) to hard-stop and unlock control.");
                    cmd = BotControlCommand.Resume;
                }
            }
            else
            {
                env.StopAll();
            }
            SendAll(cmd);
        }

        // Add a new bot with the current config
        private void B_New_Click(object sender, EventArgs e) // Add a new bot on Add button click
        {
            var cfg = CreateNewBotConfig(); // Create a new bot config based on current settings in BotsForm

            // If the config is null or invalid, show an alert and return
            if (cfg == null)
                return;
            if (!AddBot(cfg))
            {
                WinFormsUtil.Alert("Unable to add bot; ensure details are valid and not duplicate with an already existing bot.");
                return;
            }
            System.Media.SystemSounds.Asterisk.Play(); // Play a sound to indicate the bot was added successfully
        }

        // Update handling
        private async void Updater_Click(object sender, EventArgs e)
        {
            await UpdateChecker.CheckForUpdatesAsync(forceShow: true); // <-- this will auto-handle the UpdateForm
        }


        // Add a new bot to the environment and UI
        private bool AddBot(PokeBotState? cfg)
        {
            if (cfg == null || !cfg.IsValid())
                return false;

            if (Bots.Any(z => z.Connection.Equals(cfg.Connection)) ||
                RunningEnvironment.Bots.Any(z => z.Bot.Config.Connection.Equals(cfg.Connection)) ||
                _botsForm.BotPanel.Controls.OfType<BotController>().Any(c => c.State.Connection.Equals(cfg.Connection)))
            {
                Console.WriteLine("Duplicate bot detected.");
                return false;
            }

            if (!RunningEnvironment.SupportsRoutine(cfg.InitialRoutine))
            {
                Console.WriteLine($"Mode {Config.Mode} doesn't support routine {cfg.InitialRoutine}.");
                return false;
            }

            try
            {
                var newBot = RunningEnvironment.CreateBotFromConfig(cfg);
                RunningEnvironment.Add(newBot);
                AddBotControl(cfg);
                Bots.Add(cfg);
                Config.Bots = Bots.ToArray();
                SaveCurrentConfig();
                Console.WriteLine($"Added bot: {cfg.Connection.IP}:{cfg.Connection.Port}");
                return true;
            }
            catch (Exception ex)
            {
                WinFormsUtil.Error($"Failed to create bot: {ex.Message}");
                return false;
            }
        }

        private void AddBotControl(PokeBotState cfg)
        {
            var row = new BotController { Width = _botsForm.BotPanel.Width };
            row.Initialize(RunningEnvironment, cfg);
            _botsForm.BotPanel.Controls.Add(row);
            _botsForm.BotPanel.SetFlowBreak(row, true);

            row.Click += (s, e) =>
            {
                var details = cfg.Connection;
                _botsForm.IPBox.Text = details.IP;
                _botsForm.PortBox.Value = details.Port;
                _botsForm.ProtocolBox.SelectedIndex = (int)details.Protocol;
                _botsForm.RoutineBox.SelectedValue = (int)cfg.InitialRoutine;
            };

            row.Remove += (s, e) =>
            {
                Bots.RemoveAll(b => b.Connection.Equals(row.State.Connection));
                RunningEnvironment.Remove(row.State, !RunningEnvironment.Config.SkipConsoleBotCreation);
                _botsForm.BotPanel.Controls.Remove(row);
                Config.Bots = Bots.ToArray();
                SaveCurrentConfig();
            };
        }


        // Creates a new bot config based on current settings in BotsForm class
        private PokeBotState CreateNewBotConfig() // Create a new bot configuration based on the current settings in the BotsForm
        {
            var ip = _botsForm.IPBox.Text.Trim();    // Get the IP address from the IPBox and trim any whitespace
            var port = (int)_botsForm.PortBox.Value; // Get the port number from the PortBox
            if (string.IsNullOrWhiteSpace(ip))       // Check if the IP address is empty or whitespace
            {
                WinFormsUtil.Error("IP address cannot be empty.");
                return null;
            }
            if (!System.Net.IPAddress.TryParse(ip, out _))
            {
                WinFormsUtil.Error($"Invalid IP address: {ip}");
                return null;
            }
            if (_botsForm.ProtocolBox.SelectedValue == null)
            {
                WinFormsUtil.Error("Please select a protocol.");
                return null;
            }
            if (_botsForm.RoutineBox.SelectedValue == null)
            {
                WinFormsUtil.Error("Please select a routine.");
                return null;
            }

            // Create a new SwitchConnectionConfig based on the IP and port
            var cfg = BotConfigUtil.GetConfig<SwitchConnectionConfig>(ip, port); // Get the connection config based on the IP and port
            cfg.Protocol = (SwitchProtocol)_botsForm.ProtocolBox.SelectedValue;  // Set the protocol from the ProtocolBox
            var pk = new PokeBotState { Connection = cfg };                      // Create a new PokeBotState with the connection config
            var type = (PokeRoutineType)_botsForm.RoutineBox.SelectedValue;      // Set the routine type from the RoutineBox
            pk.Initialize(type);                                                 // Initialize the PokeBotState with the selected routine type
            return pk;                                                           // Return the new PokeBotState configuration
        }

        // Initialize the method for the left side image in the panelLeftSide
        private PictureBox leftSideImage;

        // Initialize the meat and potatoes for the left side image in the panelLeftSide
        private void InitializeLeftSideImage()
        {
            leftSideImage = new PictureBox
            {
                Size = new Size(200, 35),             // Put actual image dimensions here, or add custom to resize
                Location = new Point(99, 685),        // Fixed position for the image using XY
                SizeMode = PictureBoxSizeMode.Normal, // Makes sure the image is not stretched or resized
                BackColor = Color.Transparent,        // Makes sure the image has no background
                BorderStyle = BorderStyle.None        // Makes sure there's no vague borders and shit
            };

            panelLeftSide.Controls.Add(leftSideImage);                 // Add the left side image to the panelLeftSide
            panelLeftSide.Resize += (s, e) => PositionLeftSideImage(); // Reposition the image when the panel is resized
            PositionLeftSideImage();                                   // Position the image initially
        }

        // Position the left side image in the panelLeftSide
        private void PositionLeftSideImage()
        {
            if (leftSideImage == null || panelLeftSide == null) // If the left side image or panel is null, do nothing
                return;

            // Center horizontally
            int x = (panelLeftSide.Width - leftSideImage.Width) / 2; // Calculate the X position to center the image in the panel

            // Fixed vertical offset
            int y = 360;

            leftSideImage.Location = new Point(x, y);
        }

        // Update the background image based on the current game mode
        private void UpdateBackgroundImage(ProgramMode mode)
        {
            if (leftSideImage == null) return;

            switch (mode)
            {
                case ProgramMode.SV:
                    leftSideImage.Image = Resources.sv_mode_image;   // Set the image for SV mode
                    break;
                case ProgramMode.SWSH:
                    leftSideImage.Image = Resources.swsh_mode_image; // Set the image for SWSH mode
                    break;
                case ProgramMode.BDSP:
                    leftSideImage.Image = Resources.bdsp_mode_image; // Set the image for BDSP mode
                    break;
                case ProgramMode.LA: 
                    leftSideImage.Image = Resources.pla_mode_image;  // Set the image for PLA mode
                    break;
                case ProgramMode.LGPE:
                    leftSideImage.Image = Resources.lgpe_mode_image; // Set the image for LGPE mode
                    break;
                default:
                    leftSideImage.Image = null;
                    break;
            }
        }

        // Resize the BotController controls when the panel is resized, focused on width
        private void FLP_Bots_Resize(object sender, EventArgs e)
        {
            // Resize all BotController controls in the BotPanel to match the width of the panel
            foreach (var c in _botsForm.BotPanel.Controls.OfType<BotController>()) // Iterate through each BotController in the BotPanel
                c.Width = _botsForm.BotPanel.Width;                                // Set the width of the BotController to the width of the BotPanel
        }

        // Protocol and IP selection handling
        private void CB_Protocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            _botsForm.IPBox.Visible = _botsForm.ProtocolBox.SelectedIndex == 0; // Show the IPBox only if the selected protocol is WiFi
        }

        // Drag the window from the titlebar
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")] // Release the mouse capture
        private extern static void ReleaseCapture();             // Release the mouse capture to allow dragging the window
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]    // Send a message to the window
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam); // Send a message to the window to allow dragging
        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)                         // Allow dragging the window from the title bar
        {
            ReleaseCapture();                           // Release the mouse capture
            SendMessage(this.Handle, 0x112, 0xf012, 0); // Send a message to the window to allow dragging
        }

        // Method to activate Bots button and change its color, loading BotsForm class
        private void Bots_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4); // Change the color of the active Bots button
            OpenChildForm(_botsForm);                 // Load the BotsForm in the main panel
        }

        // Method to activate Hub button and change its color, loading HubForm class
        private void Hub_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color5); // Change the color of the active Hub button
            currentBtn.Refresh();                     // Refresh the current button to apply the new color
            leftBorderBtn.Refresh();                  // Refresh the left border to match the active button

            // If the HubForm is not initialized, create a new instance
            if (_hubForm == null)
                _hubForm = new HubForm(RunningEnvironment.Config);

            OpenChildForm(_hubForm); // Load the HubForm in the main panel
        }

        // Method to activate Logs button and change its color, loading LogsForm class
        private void Logs_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, Color.FromArgb(0, 255, 255)); // Change the color of the active Logs button
            currentBtn.Refresh();                                // Refresh the current button to apply the new color
            leftBorderBtn.Refresh();                             // Refresh the left border to match the active button
            OpenChildForm(_logsForm);                            // Load the LogsForm in the main panel
        }

        // Close button
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Exit program on Close button click
        }

        // Maximize and Restore button
        private void BtnMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)   // If the window is in normal state, then...
                WindowState = FormWindowState.Maximized; // ...Maximize the window
            else
                WindowState = FormWindowState.Normal; // Restore the window to normal state if Maximized
        }

        // Minimize button
        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized; // Minimize the window on Minimize button click
        }

        // Method and logic to open the child form(Bots, Hub, or Logs) in panelMain
        private async void OpenChildForm(Form childForm)
        {
            // If the active form is already open, hide it
            if (activeForm != null) activeForm.Hide();

            activeForm = childForm;                           // Set the active form to the new child form
            childForm.TopLevel = false;                       // Set the child form to be a non-top-level form
            childForm.FormBorderStyle = FormBorderStyle.None; // Remove the border style of the child form
            panelMain.Controls.Add(childForm);                // Add the child form to the main panel
            SlideFadeInForm(childForm);                       // Activate the SlideFadeInForm method to the child form
            childForm.Size = panelMain.ClientSize;                         // Set the size of the child form to match the panelMain size
            childForm.Location = new Point(panelMain.ClientSize.Width, 0); // Set the initial location of the child form to the right edge of the panelMain
            childForm.Opacity = 0;                                         // Set the initial opacity of the child form to 0 (invisible)
            childForm.Show();                                              // Show the child form

            // SlideFadeInForm utilization in the child form
            while (childForm.Left > 0 || childForm.Opacity < 1.0) // While the child form is not fully visible
            {
                // Slide the child form to the left and increase its opacity
                await Task.Delay(10);                                        // Wait for 10 milliseconds for smoother animation
                childForm.Left = Math.Max(childForm.Left - 40, 0);           // Move the child form left by 40 pixels, but not less than 0
                childForm.Opacity = Math.Min(childForm.Opacity + 0.05, 1.0); // Increase the opacity of the child form by 0.05, but not more than 1.0 (fully visible)
            }
            childForm.Dock = DockStyle.Fill; // Set the child form to fill the entire panelMain
            childForm.BringToFront();        // Bring the child form to the front of the panelMain controls
        }

        // RGB Color dictionary
        private struct RGBColors
        {
            public static Color color1 = Color.FromArgb(172, 126, 241); // Vibrant Purple
            public static Color color2 = Color.FromArgb(249, 118, 176); // Dark Crimson Pink
            public static Color color3 = Color.FromArgb(253, 138, 114); // Light Orange
            public static Color color4 = Color.FromArgb(95, 77, 221);   // Dark Purple
            public static Color color5 = Color.FromArgb(249, 88, 155);  // Light Pink
            public static Color color6 = Color.FromArgb(24, 161, 251);  // Light Blue
        }

        // Method to slide and fade in the child forms (Bots, Hub, Logs) when it is opened
        private async void SlideFadeInForm(Form form)
        {
            form.Dock = DockStyle.None;                               // Remove any docking style from the form
            form.Size = panelMain.ClientSize;                         // Set the size of the form to match the panelMain size to make a seamless transition
            form.Location = new Point(panelMain.ClientSize.Width, 0); // Set the initial location of the form to the right edge of the panelMain
            form.Opacity = 0;                                         // Set the initial opacity of the form to 0 (invisible)
            form.Show();                                              // Show the form in all its glory

            // Slide the form to the left and increase its opacity
            while (form.Left > 0 || form.Opacity < 1.0) // While the form is not fully visible
            {
                await Task.Delay(10);                              // Wait for 10 milliseconds for smoother animation
                form.Left = Math.Max(form.Left - 40, 0);           // Move the form left by 40 pixels, but not less than 0
                form.Opacity = Math.Min(form.Opacity + 0.05, 1.0); // Increase the opacity of the form by 0.05, but not more than 1.0 (fully visible)
            }
            form.Dock = DockStyle.Fill; // Set the form to fill the entire panelMain like it should
            form.BringToFront();        // Bring the form to the front of panelMain
        }

        // Animation method for the Bots, Hub, and Logs buttons
        private void ApplyButtonEffects()
        {
            foreach (Control control in panelLeftSide.Controls)     // Go through each control in the left side panel
            {
                if (control is FontAwesome.Sharp.IconButton button) // Check if the control is an IconButton 
                {
                    button.MouseEnter += (s, e) => AnimateButtonHover(button, true);  // Start hover animation
                    button.MouseLeave += (s, e) => AnimateButtonHover(button, false); // Stop hover animation
                }
            }
        }

        // Animation method for Bots, Hub, and Logs button hover effect
        private async void AnimateButtonHover(FontAwesome.Sharp.IconButton button, bool hover)
        {
            float targetScale = hover ? 1.1f : 1.0f;                       // Target scale for hover effect
            float step = 0.02f * (hover ? 1 : -1);                         // Step size for scaling
            float currentScale = button.Tag is float scale ? scale : 1.0f; // Get current scale from Tag or default to 1.0f

            while ((hover && currentScale < targetScale) || (!hover && currentScale > targetScale)) // While scaling towards target
            {
                currentScale = Math.Clamp(currentScale + step, 1.0f, 1.1f);                         // Clamp the scale between 1.0 and 1.1
                button.Tag = currentScale;                                                          // Store the current scale in the button's Tag property
                button.Font = new Font(button.Font.FontFamily, 11F * currentScale, FontStyle.Bold); // Adjust font size based on scale
                await Task.Delay(10);                                                               // Delay for 10 milliseconds for smoother animation
            }
        }

        // Method to set up hover animation for Bots, Hub, and Logs button
        private void SetupHoverAnimation(IconButton button)
        {
            Timer fadeTimer = new Timer();                  // Create a new timer for the hover animation
            fadeTimer.Interval = 15;                        // Lower value = smoother effect
            Color baseColor = Color.FromArgb(31, 30, 68);   // Default color for the buttons
            Color hoverColor = Color.FromArgb(60, 40, 100); // Color of the buttons when hovered over
            float t = 0f;                                   // Current interpolation value (0 to 1)
            float speed = 0.03f;                            // Lower value = slower fade
            bool hovering = false;                          // Whether the mouse is hovering over the button

            // Start the fade timer when the button is hovered over or not
            fadeTimer.Tick += (s, e) =>
            {
                if (hovering && t < 1f)       // If hovering, increase the interpolation value
                    t += speed;
                else if (!hovering && t > 0f) // If not hovering, decrease the interpolation value
                    t -= speed;

                t = Math.Clamp(t, 0f, 1f); // Ensure t(interpolation) is between 0 and 1

                // Interpolate the button's background color based on the interpolation value
                int r = (int)(baseColor.R + (hoverColor.R - baseColor.R) * t);
                int g = (int)(baseColor.G + (hoverColor.G - baseColor.G) * t);
                int b = (int)(baseColor.B + (hoverColor.B - baseColor.B) * t);
                button.BackColor = Color.FromArgb(r, g, b);

                if ((hovering && t >= 1f) || (!hovering && t <= 0f)) // Wait for animation timer to complete
                    fadeTimer.Stop();                                // Stop the timer when complete
            };

            // Assign the hover state to the button
            button.MouseEnter += (s, e) => StartColorFade(button, Color.FromArgb(60, 40, 100)); // Hover color
            button.MouseLeave += (s, e) => StartColorFade(button, Color.FromArgb(31, 30, 68));  // Default color
            button.UseVisualStyleBackColor = false;                                             // Set UseVisualStyleBackColor to false to allow custom colors
            button.BackColor = baseColor;                                                       // Set the initial background color of the button
        }

        // Method to set up hover animation for the top buttons (Close, Minimize, Maximize)
        private void SetupTopButtonHoverAnimation(IconPictureBox button, Color hoverColor)
        {
            Color baseColor = button.BackColor;            // Default color for the buttons before hover
            float fadeSpeed = 0.02f;                       // Speed of the fade animation, lower value = slower fade
            Timer fadeTimer = new Timer { Interval = 15 }; // Timer for the fade animation, lower value = smoother effect
            float step = 0f;                               // Current step in the fade animation, higher values = slower fade
            bool hovering = false;                         // Whether the mouse is hovering over the button

            // Start the fade timer when the button is hovered over or not
            fadeTimer.Tick += (s, e) =>
            {
                if (hovering && step < 1f) // If hovering, increase the step value of 1 (fade in) according to timer
                    step += fadeSpeed;
                else if (!hovering && step > 0f) // If not hovering, decrease the step value of 0 (fade out) according to timer
                    step -= fadeSpeed;

                // Clamp the step value between 0 and 1
                step = Math.Clamp(step, 0f, 1f);                                      // Ensure step is between 0 and 1
                button.BackColor = LerpColor(baseColor, hoverColor, EaseInOut(step)); // Interpolate color based on step value

                if ((hovering && step >= 1f) || (!hovering && step <= 0f)) // Wait for animation timer to complete
                    fadeTimer.Stop();                                      // Stop the timer when complete
            };

            button.MouseEnter += (s, e) => { hovering = true; fadeTimer.Start(); };  // Start the fade timer when the mouse enters the button
            button.MouseLeave += (s, e) => { hovering = false; fadeTimer.Start(); }; // Stop the fade timer when the mouse leaves the button
        }

        // Method to start the color fade animation on the Bots, Hub, and Logs buttons
        private void StartColorFade(IconButton btn, Color endColor)
        {
            if (hoverTimers.ContainsKey(btn)) // If the button already has a hover timer, stop and dispose of it
            {
                hoverTimers[btn].Stop();    // Stop the existing timer
                hoverTimers[btn].Dispose(); // Dispose of the existing timer
            }

            Timer t = new Timer();            // Create a new timer for the hover animation
            Color startColor = btn.BackColor; // Current color of the button
            float fadeSpeed = 0.02f;          // 0.02 = 750ms fade, higher values = slower fade
            float step = 0.0f;                // Current step in the fade animation, higher values = slower fade
            t.Interval = 15;                  // Around 60fps, lower value = smoother effect

            // Set up the timer tick event for the hover animation
            t.Tick += (s, e) =>
            {
                step += fadeSpeed;
                if (step >= 1.0f) // Fade speed step reached 100%
                {
                    btn.BackColor = endColor; // Set the button's background color to the end color
                    t.Stop();                 // Stop the timer when the fade is complete
                    t.Dispose();              // Dispose of the timer to free resources
                    hoverTimers.Remove(btn);  // Remove the button from the hover timers dictionary
                    return;
                }

                btn.BackColor = LerpColor(startColor, endColor, EaseInOut(step)); // Interpolate color
            };

            hoverTimers[btn] = t; // Store the timer in the hover timers dictionary
            t.Start();            // Start the timer to begin the hover animation
        }

        // Linear interpolation for colors on Bots, Hub, and Logs button hover effect
        private float EaseInOut(float t) => t < 0.5 ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2;

        // Lerp color method for the Bots, Hub, and Logs button hover effect
        private Color LerpColor(Color start, Color end, float t) // Linearly interpolate between two colors
        {
            t = Math.Clamp(t, 0f, 1f); // ensure 0 ≤ t ≤ 1 for good interpolation

            // Calculate the interpolated color components
            int r = (int)Math.Clamp(start.R + (end.R - start.R) * t, 0, 255);
            int g = (int)Math.Clamp(start.G + (end.G - start.G) * t, 0, 255);
            int b = (int)Math.Clamp(start.B + (end.B - start.B) * t, 0, 255);
            return Color.FromArgb(r, g, b);
        }

        // Method to activate the buttons and set the left border
        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn != null) // Check if the sender is not null
            {
                DisableButton();
                // Cast the sender to Bots, Hub, and Logs button
                currentBtn = (IconButton)senderBtn;
                currentBtn.BackColor = Color.FromArgb(37, 36, 81);                // Darker shade for active button
                currentBtn.ForeColor = color;                                     // Set the text color of the active button
                currentBtn.TextAlign = ContentAlignment.MiddleCenter;             // Center the text in the active button
                currentBtn.IconColor = color;                                     // Set the icon color of the active button
                currentBtn.TextImageRelation = TextImageRelation.TextBeforeImage; // Set the text and image relation of the active button
                currentBtn.ImageAlign = ContentAlignment.MiddleRight;             // Align the image to the right of the text in the active button

                // Set the left border button properties
                leftBorderBtn.BackColor = color;                              // Set the left border color
                leftBorderBtn.Location = new Point(0, currentBtn.Location.Y); // Set the left border position to the active button's position
                leftBorderBtn.Visible = true;                                 // Make the left border visible
                leftBorderBtn.BringToFront();                                 // Bring the left border to the front
                childFormIcon.IconChar = currentBtn.IconChar;                 // Set the icon of the current child form to the active button's icon
                childFormIcon.IconColor = color;                              // Set the icon color of the current child form to the active button's color
                lblTitleChildForm.Text = ((IconButton)senderBtn).Text;        // Set the title of the child form to the active button's text
            }
        }

        // Method to disable the current button and reset its style to default
        private void DisableButton()
        {
            if (currentBtn != null)
            {
                currentBtn.BackColor = Color.FromArgb(31, 30, 68);                // Default background color
                currentBtn.ForeColor = Color.Gainsboro;                           // Default text color
                currentBtn.TextAlign = ContentAlignment.MiddleLeft;               // Center the text in the button
                currentBtn.IconColor = Color.Gainsboro;                           // Default icon color
                currentBtn.TextImageRelation = TextImageRelation.ImageBeforeText; // Set the text and image relation to default
                currentBtn.ImageAlign = ContentAlignment.MiddleLeft;              // Align the image to the left of the text in the button
            }
        }

        // Config save method
        private void SaveCurrentConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions // Serialize the current config to json
                {
                    WriteIndented = true                     // Format the json with indentation for readability
                });
                File.WriteAllText(Program.ConfigPath, json); // Save the serialized json to the config file
            }
            catch (Exception ex)
            {
                WinFormsUtil.Error($"Failed to save configuration:\n{ex.Message}");
            }
        }
    }
}
