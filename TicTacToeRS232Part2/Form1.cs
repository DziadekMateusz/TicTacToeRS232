using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TicTacToeRS232Part2
{
    public partial class Form1 : Form
    {
        // A delegate used to safely call a method in a GUI thread
        delegate void SetTextCallback(string text);
        string InputData = String.Empty;
        private System.Windows.Forms.Button[,] TTTbuttons;
        private bool CheckWin = false;

        public Form1()
        {
            InitializeComponent();
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1);
            Board(); // Initializing the game board
        }

        // Handling data reception from a serial port
        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            InputData = serialPort1.ReadExisting(); // read data
            if (InputData != String.Empty)
            {
                // Call the SetText method 
                this.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }

        // Initializing the game board
        private void Board()
        {
            TTTbuttons = new System.Windows.Forms.Button[3, 3]
            {
                { button1_1, button1_2, button1_3 },
                { button2_1, button2_2, button2_3 },
                { button3_1, button3_2, button3_3 }
            };

            // Handling click events
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    TTTbuttons[i, j].Click += TTTbuttons_Click;
                }
            }
        }

        // Click the game button (Player O's move)
        private void TTTbuttons_Click(object sender, EventArgs e)
        {
            if (CheckWin)
                return;

            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            if (btn != null && string.IsNullOrEmpty(btn.Text) && serialPort1.IsOpen)
            {
                string symbol = "O";
                btn.Text = symbol;
                CheckWinCondition();
                serialPort1.Write("TTT:" + btn.Name + ":" + symbol);
            }
        }

        private void CheckWinCondition()
        {
            string[,] board = new string[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board[i, j] = TTTbuttons[i, j].Text;
                }
            }

            // Line checking
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] != "" &&
                    board[i, 0] == board[i, 1] &&
                    board[i, 1] == board[i, 2])
                {
                    GameEnd(board[i, 0]);
                    return;
                }
            }

            // Column checking
            for (int j = 0; j < 3; j++)
            {
                if (board[0, j] != "" &&
                    board[0, j] == board[1, j] &&
                    board[1, j] == board[2, j])
                {
                    GameEnd(board[0, j]);
                    return;
                }
            }

            // Diagonal 1
            if (board[0, 0] != "" &&
                board[0, 0] == board[1, 1] &&
                board[1, 1] == board[2, 2])
            {
                GameEnd(board[0, 0]);
                return;
            }

            // Diagonal 2
            if (board[0, 2] != "" &&
                board[0, 2] == board[1, 1] &&
                board[1, 1] == board[2, 0])
            {
                GameEnd(board[0, 2]);
                return;
            }
        }

        private void GameEnd(string zwyciezca)
        {
            CheckWin = true;

            foreach (System.Windows.Forms.Button b in TTTbuttons)
            {
                b.Enabled = false;
            }

            MessageBox.Show("Player won: " + zwyciezca);
        }

        // Displaying received data and handling messages
        private void SetText(string text)
        {
            // Responses to simple text messages
            if (text == "BTN1")
                this.textBox1.Text = "Button 1 was pressed";
            else if (text == "BTN2")
                this.textBox1.Text = "Button 2 was pressed";
            else if (text == "BTN3")
                this.textBox1.Text = "Button 3 was pressed";
            else if (text.StartsWith("TRACKBAR:"))
            {
                // Reading trackbar values from data
                string valueStr = text.Substring(9);
                int valueT;
                if (int.TryParse(valueStr, out valueT))
                {
                    trackBar1.Value = valueT;
                }
                this.textBox1.Text = "TrackBar: " + valueStr;
            }
            else
                this.textBox1.Text = text;

            // Handling the Tic-Tac-Toe game message
            if (text.StartsWith("TTT:"))
            {
                string[] parts = text.Split(':');
                if (parts.Length == 3)
                {
                    string buttonName = parts[1];
                    string symbol = parts[2];
                    foreach (System.Windows.Forms.Button b in TTTbuttons)
                    {
                        if (b.Name == buttonName && string.IsNullOrEmpty(b.Text))
                        {
                            b.Text = symbol;
                            CheckWinCondition();
                            break;
                        }
                    }
                }
                return;
            }

            this.textBox1.Text = text;

            // If a number is received, move the slider
            if (int.TryParse(text.Trim(), out int value))
            {
                if (value >= trackBar1.Minimum && value <= trackBar1.Maximum)
                {
                    trackBar1.Value = value;
                }
            }
        }

        // Refresh the list of COM ports
        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);

            if (ports.Length > 0)
            {
                foreach (string port in ports)
                {
                    comboBox1.Items.Add(port);
                }

                comboBox1.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("No COM ports were found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Opening/closing the serial port
        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Open")
            {
                // Port settings
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                serialPort1.DataBits = Convert.ToInt16(comboBoxDataBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), comboBoxParity.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBoxStopBits.Text);
                serialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), comboBoxHandshake.Text);

                try
                {
                    serialPort1.Open();
                    button2.Text = "Close";
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (button2.Text == "Close")
            {
                try
                {
                    serialPort1.Close();
                    button2.Text = "Open";
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Form startup settings (initialization of the list of ports and parameters)
        private void Form1_Load(object sender, EventArgs e)
        {
            // List of standard baud rates
            comboBox2.Items.Add(300);
            comboBox2.Items.Add(600);
            comboBox2.Items.Add(1200);
            comboBox2.Items.Add(2400);
            comboBox2.Items.Add(9600);
            comboBox2.Items.Add(14400);
            comboBox2.Items.Add(19200);
            comboBox2.Items.Add(38400);
            comboBox2.Items.Add(57600);
            comboBox2.Items.Add(115200);
            comboBox2.Items.ToString();
            comboBox2.Text = comboBox2.Items[0].ToString();

            // Number of data bits
            comboBoxDataBits.Items.Add(5);
            comboBoxDataBits.Items.Add(6);
            comboBoxDataBits.Items.Add(7);
            comboBoxDataBits.Items.Add(8);
            comboBoxDataBits.Text = "8"; // default

            // Parity
            comboBoxParity.Items.AddRange(Enum.GetNames(typeof(Parity)));
            comboBoxParity.Text = "None"; // default

            // Number of stop bits
            comboBoxStopBits.Items.AddRange(Enum.GetNames(typeof(StopBits)));
            comboBoxStopBits.Text = "One"; // default

            // Handshake constrol
            comboBoxHandshake.Items.AddRange(Enum.GetNames(typeof(Handshake)));
            comboBoxHandshake.Text = "None"; // default

            // Detecting available COM ports
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length > 0)
            {
                do
                {
                    index += 1;
                    comboBox1.Items.Add(ArrayComPortsNames[index]);
                }
                while (!((ArrayComPortsNames[index] == ComPortName) || (index == ArrayComPortsNames.GetUpperBound(0))));
                Array.Sort(ArrayComPortsNames);
            }
        }
    }
}
