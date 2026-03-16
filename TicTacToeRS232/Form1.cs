using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace TicTacToeRS232
{
    public partial class Form1 : Form
    {
        // A two-dimensional button panel (for playing tic-tac-toe)
        private System.Windows.Forms.Button[,] TTTbuttons;
        private bool gameEnd = false;

        public Form1()
        {
            InitializeComponent();
            Board(); // Initializing the game board
            // Connecting a data reception event from the serial port
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort1_DataReceived);
        }

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

            // Handshake control
            comboBoxHandshake.Items.AddRange(Enum.GetNames(typeof(Handshake)));
            comboBoxHandshake.Text = "None"; // default

            // Detecting available COM ports
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames(); // Retrieve the list of COM ports
            if (ArrayComPortsNames.Length > 0)
            {
                do
                {
                    index += 1;
                    comboBox1.Items.Add(ArrayComPortsNames[index]); // Add a port to the list
                }
                while (!((ArrayComPortsNames[index] == ComPortName) || (index == ArrayComPortsNames.GetUpperBound(0))));
                Array.Sort(ArrayComPortsNames); // Sorting port names
            }
        }

        private void CheckWin()
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
            gameEnd = true;

            foreach (System.Windows.Forms.Button b in TTTbuttons)
            {
                b.Enabled = false;
            }

            MessageBox.Show("Player won: " + zwyciezca);
        }

        // Handling data reception from a serial port
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string InputData = serialPort1.ReadExisting(); // read data
            {
                // Calling the SetText method in a GUI thread
                this.BeginInvoke(new Action<string>(SetText), InputData);
            }
        }

        // Initializing the buttons for the tic-tac-toe game
        private void Board()
        {
            TTTbuttons = new System.Windows.Forms.Button[3, 3]
            {
                { button1_1, button1_2, button1_3 },
                { button2_1, button2_2, button2_3 },
                { button3_1, button3_2, button3_3 }
            };

            // Add click handling for each button
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    TTTbuttons[i, j].Click += TTTbuttons_Click;
                }
            }
        }

        // Handling a button click (Player X's movement)
        private void TTTbuttons_Click(object sender, EventArgs e)
        {
            if (gameEnd)
                return;

            System.Windows.Forms.Button btn = sender as System.Windows.Forms.Button;
            if (btn != null && string.IsNullOrEmpty(btn.Text) && serialPort1.IsOpen)
            {
                string symbol = "X"; // Player symbol
                btn.Text = symbol;
                CheckWin();
                // Sending information to the receiver (button name and symbol)
                serialPort1.Write("TTT:" + btn.Name + ":" + symbol);
            }
        }

        // Text configuration after receiving data from the port
        private void SetText(string text)
        {
            if (text.StartsWith("TTT:"))
            {
                // Processing a move message in the game
                string[] parts = text.Split(':');
                if (parts.Length == 3)
                {
                    string buttonName = parts[1];
                    string symbol = parts[2];
                    foreach (System.Windows.Forms.Button b in TTTbuttons)
                    {
                        if (b.Name == buttonName && string.IsNullOrEmpty(b.Text))
                        {
                            b.Text = symbol; // Inserting the opponent's symbol
                            CheckWin();
                            break;
                        }
                    }
                }
                return;
            }
        }

        // Button to open or close the COM port
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
                    serialPort1.Open(); // Open port
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
                    serialPort1.Close(); // Close port
                    button2.Text = "Open";
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // Sending text from a textbox
        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textBox1.Text);
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

        // Sending pre-written messages
        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Text 1";
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textBox1.Text);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Text 2";
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textBox1.Text);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Text 3";
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(textBox1.Text);
            }
        }

        // Sending the TrackBar value
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("TRACKBAR:" + trackBar1.Value.ToString());
            }
        }
    }
}
