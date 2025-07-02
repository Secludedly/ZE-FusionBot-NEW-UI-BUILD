using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SysBot.Base;

namespace SysBot.Pokemon.WinForms
{
    public partial class BotsForm : Form
    {
        public event EventHandler<ProgramMode>? GameModeChanged;

        public PictureBox ImageOverlay;
        public FlowLayoutPanel BotPanel => _FLP_Bots;

        public Button StartButton => _B_Start;
        public Button StopButton => _B_Stop;
        public Button RebootStopButton => _B_RebootStop;
        public Button UpdateButton => _updater;
        public Button AddBotButton => _B_New;

        public TextBox IPBox => _TB_IP;
        public NumericUpDown PortBox => _NUD_Port;

        public ComboBox ProtocolBox => _CB_Protocol;
        public ComboBox RoutineBox => _CB_Routine;
        public ComboBox GameModeBox => _comboBox1;

        private readonly List<BotController> BotControls = new();

        private Button _B_Start;
        private Button _B_Stop;
        private Button _B_RebootStop;
        private Button _updater;
        private Button _B_New;

        private TextBox _TB_IP;
        private NumericUpDown _NUD_Port;

        private ComboBox _CB_Protocol;
        private ComboBox _CB_Routine;
        private ComboBox _comboBox1;

        private FlowLayoutPanel _FLP_Bots;
        private PictureBox _pictureBox1;



        public BotsForm()
        {
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Buttons
            _B_Start = new FancyButton { Text = "Start", Location = new Point(10, 7), Size = new Size(100, 40) };
            _B_Stop = new FancyButton { Text = "Stop", Location = new Point(120, 7), Size = new Size(100, 40) };
            _B_RebootStop = new FancyButton { Text = "Restart", Location = new Point(230, 7), Size = new Size(100, 40) };
            _updater = new FancyButton { Text = "Update", Location = new Point(340, 7), Size = new Size(100, 40) };
            _B_New = new FancyButton { Text = "Add", Location = new Point(424, 55), Size = new Size(54, 30) };

            // Colors for boxes and controls
            Color darkBG = Color.FromArgb(20, 19, 57);
            Color whiteText = Color.White;

            // Controls
            _TB_IP = new TextBox { Location = new Point(12, 57), Width = 120, BackColor = darkBG, ForeColor = whiteText };
            _NUD_Port = new NumericUpDown { Location = new Point(144, 57), Width = 65, Maximum = 65535, Minimum = 0, Value = 6000, BackColor = darkBG, ForeColor = whiteText };

            _CB_Protocol = new ComboBox { Location = new Point(221, 57), Width = 60, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = darkBG, ForeColor = whiteText };
            var protocols = ((SwitchProtocol[])Enum.GetValues(typeof(SwitchProtocol)))
                .Select(z => new { Text = z.ToString(), Value = (int)z }).ToArray();
            _CB_Protocol.DisplayMember = "Text";
            _CB_Protocol.ValueMember = "Value";
            _CB_Protocol.DataSource = protocols;
            _CB_Protocol.SelectedValue = (int)SwitchProtocol.WiFi;

            _CB_Routine = new ComboBox { Location = new Point(292, 57), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = darkBG, ForeColor = whiteText };
            var routines = ((PokeRoutineType[])Enum.GetValues(typeof(PokeRoutineType)))
                .Select(z => new { Text = z.ToString(), Value = (int)z }).ToArray();
            _CB_Routine.DisplayMember = "Text";
            _CB_Routine.ValueMember = "Value";
            _CB_Routine.DataSource = routines;
            _CB_Routine.SelectedValue = (int)PokeRoutineType.FlexTrade;

            _comboBox1 = new ComboBox { Location = new Point(672, 6), Width = 68, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = darkBG, ForeColor = whiteText };
            var modes = ((ProgramMode[])Enum.GetValues(typeof(ProgramMode)))
                .Select(m => new { Text = m.ToString(), Value = (int)m }).ToArray();
            _comboBox1.DisplayMember = "Text";
            _comboBox1.ValueMember = "Value";
            _comboBox1.DataSource = modes;
            _comboBox1.SelectedIndexChanged += (s, e) =>
            {
                if (_comboBox1.SelectedValue is int val && Enum.IsDefined(typeof(ProgramMode), val))
                    GameModeChanged?.Invoke(this, (ProgramMode)val);
            };


            _FLP_Bots = new FlowLayoutPanel
            {
                Location = new Point(9, 89),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Size = new Size(ClientSize.Width - 24, ClientSize.Height - 100),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.FromArgb(28, 27, 65)
            };

            this.BackColor = Color.FromArgb(28, 27, 65);
            this.Resize += (s, e) => {
                _FLP_Bots.BackColor = Color.FromArgb(23, 22, 60);

                _FLP_Bots.Size = new Size(ClientSize.Width - 24, ClientSize.Height - 100);
                ResizeBots();
            };


            Controls.AddRange(new Control[] {
                _B_Start, _B_Stop, _B_RebootStop, _updater, _B_New,
                _TB_IP, _NUD_Port, _CB_Protocol, _CB_Routine, _comboBox1,
                _FLP_Bots
            });

            Text = "Bots Controller";
            Size = new Size(757, 53);
        }

        public void AddNewBot(IPokeBotRunner runner, PokeBotState cfg)
        {
            if (cfg == null)
                return;
            // Now create the controller for the same config
            var controller = new BotController();
            controller.Initialize(runner, cfg);
            controller.Margin = new Padding(0, 1, 0, 1);
            controller.Remove += (s, e) => RemoveBot(controller);
            controller.Click += (s, e) => LoadBotSettingsToUI(cfg);
            _FLP_Bots.Controls.Add(controller);
            _FLP_Bots.SetFlowBreak(controller, true);
            BotControls.Add(controller);
            _FLP_Bots.PerformLayout();
            _FLP_Bots.Update();
            if (_FLP_Bots.Controls.Count > 0 && _FLP_Bots.Controls[0] is BotController first)
            {
                controller.Width = first.Width;
            }
            else
            {
                // Fallback just in case no bots exist yet
                controller.Width = _FLP_Bots.ClientSize.Width - 5;
            }
        }

        private void RemoveBot(BotController controller)
        {
            _FLP_Bots.Controls.Remove(controller);
            BotControls.Remove(controller);
        }

        private void ResizeBots()
        {
            int safeWidth = _FLP_Bots.ClientSize.Width - 5;

            foreach (var ctrl in BotControls)
        {
            ctrl.Width = safeWidth;
        }
    }

        public void ReadAllBotStates()
        {
            foreach (var bot in BotControls)
                bot.ReadState();
        }

        private void LoadBotSettingsToUI(PokeBotState cfg)
        {
            var details = cfg.Connection;
            _TB_IP.Text = details.IP;
            _NUD_Port.Value = details.Port;
            _CB_Protocol.SelectedValue = (int)details.Protocol;
            _CB_Routine.SelectedValue = (int)cfg.InitialRoutine;
        }

        private void InitializeComponent()
        {
            _B_Start = new Button();
            _B_Stop = new Button();
            _B_RebootStop = new Button();
            _updater = new Button();
            _B_New = new Button();
            _TB_IP = new TextBox();
            _NUD_Port = new NumericUpDown();
            _CB_Protocol = new ComboBox();
            _CB_Routine = new ComboBox();
            _comboBox1 = new ComboBox();
            _pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)_NUD_Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // B_Start
            // 
            _B_Start.Location = new Point(12, 3);
            _B_Start.Name = "B_Start";
            _B_Start.Size = new Size(94, 29);
            _B_Start.TabIndex = 0;
            _B_Start.Text = "Start";
            _B_Start.UseVisualStyleBackColor = true;
            // 
            // B_Stop
            // 
            _B_Stop.Location = new Point(112, 3);
            _B_Stop.Name = "B_Stop";
            _B_Stop.Size = new Size(94, 29);
            _B_Stop.TabIndex = 1;
            _B_Stop.Text = "Stop";
            _B_Stop.UseVisualStyleBackColor = true;
            // 
            // B_RebootStop
            // 
            _B_RebootStop.Location = new Point(212, 3);
            _B_RebootStop.Name = "B_RebootStop";
            _B_RebootStop.Size = new Size(94, 29);
            _B_RebootStop.TabIndex = 2;
            _B_RebootStop.Text = "Reboot";
            _B_RebootStop.UseVisualStyleBackColor = true;
            // 
            // updater
            // 
            _updater.Location = new Point(312, 3);
            _updater.Name = "updater";
            _updater.Size = new Size(94, 29);
            _updater.TabIndex = 3;
            _updater.Text = "Update";
            _updater.UseVisualStyleBackColor = true;
            // 
            // B_New
            // 
            _B_New.Location = new Point(574, 55);
            _B_New.Name = "B_New";
            _B_New.Size = new Size(94, 29);
            _B_New.TabIndex = 4;
            _B_New.Text = "Add";
            _B_New.UseVisualStyleBackColor = true;
            // 
            // TB_IP
            // 
            _TB_IP.Location = new Point(12, 50);
            _TB_IP.Name = "TB_IP";
            _TB_IP.Size = new Size(146, 27);
            _TB_IP.TabIndex = 5;
            // 
            // NUD_Port
            // 
            _NUD_Port.Location = new Point(164, 50);
            _NUD_Port.Name = "NUD_Port";
            _NUD_Port.Size = new Size(63, 27);
            _NUD_Port.TabIndex = 6;
            // 
            // CB_Protocol
            // 
            _CB_Protocol.FormattingEnabled = true;
            _CB_Protocol.Location = new Point(233, 49);
            _CB_Protocol.Name = "CB_Protocol";
            _CB_Protocol.Size = new Size(73, 28);
            _CB_Protocol.TabIndex = 7;
            // 
            // CB_Routine
            // 
            _CB_Routine.FormattingEnabled = true;
            _CB_Routine.Location = new Point(312, 50);
            _CB_Routine.Name = "CB_Routine";
            _CB_Routine.Size = new Size(102, 28);
            _CB_Routine.TabIndex = 8;
            // 
            // comboBox1
            // 
            _comboBox1.FormattingEnabled = true;
            _comboBox1.Location = new Point(654, 12);
            _comboBox1.Name = "comboBox1";
            _comboBox1.Size = new Size(73, 28);
            _comboBox1.TabIndex = 9;
            // 
            // pictureBox1
            // 
            _pictureBox1.Location = new Point(132, 182);
            _pictureBox1.Name = "pictureBox1";
            _pictureBox1.Size = new Size(125, 62);
            _pictureBox1.TabIndex = 10;
            _pictureBox1.TabStop = false;
            // 
            // BotsForm
            // 
            ClientSize = new Size(739, 258);
            Controls.Add(_pictureBox1);
            Controls.Add(_comboBox1);
            Controls.Add(_CB_Routine);
            Controls.Add(_CB_Protocol);
            Controls.Add(_NUD_Port);
            Controls.Add(_TB_IP);
            Controls.Add(_B_New);
            Controls.Add(_updater);
            Controls.Add(_B_RebootStop);
            Controls.Add(_B_Stop);
            Controls.Add(_B_Start);
            Name = "BotsForm";
            ((System.ComponentModel.ISupportInitialize)_NUD_Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)_pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        private PokeBotState? CreateNewBotConfig()
        {
            var ip = _TB_IP.Text.Trim();
            var port = (int)_NUD_Port.Value;
            // Validate IP address
            if (string.IsNullOrWhiteSpace(ip))
            {
                WinFormsUtil.Error("IP address cannot be empty.");
                return null;
            }
            if (!System.Net.IPAddress.TryParse(ip, out _))
            {
                WinFormsUtil.Error($"Invalid IP address: {ip}");
                return null;
            }
            var cfg = BotConfigUtil.GetConfig<SwitchConnectionConfig>(ip, port);
            cfg.Protocol = (SwitchProtocol)_CB_Protocol.SelectedValue;
            var state = new PokeBotState { Connection = cfg };
            // Use SelectedValue for routine (should be int)
            var routineValue = _CB_Routine.SelectedValue;
            if (routineValue == null)
            {
                WinFormsUtil.Error("Please select a valid routine.");
                return null;
            }
            var routine = (PokeRoutineType)routineValue;
            state.Initialize(routine);
            return state;
        }

        
    }
}
