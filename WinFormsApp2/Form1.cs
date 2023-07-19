using System.Runtime.CompilerServices;

namespace WinFormsApp2
{
    public partial class Form1 : Form, IView
    {

        // List of 9 playground buttons as Button object links
        public List<System.Windows.Forms.Button> Buttons = new List<System.Windows.Forms.Button>();

        // creating events. Methods treating the events are added to the event delegates in the class Program, the method Main()
        public event EventHandler<GameEventArgs> difficulty_simple; // зміна рівня складності гри (можливо до початку нової гри)
        public event EventHandler<GameEventArgs> difficulty_medium;
        public event EventHandler<GameEventArgs> difficulty_hard;

        public event EventHandler<GameEventArgs> players_move_first; // зміна черговості ходів (можливо до початку нової гри)
        public event EventHandler<GameEventArgs> computers_move_first;

        public event EventHandler<GameEventArgs> player_plays_cross; // вибір знаку, яким гратиме гравець (можливо до початку нової гри)
        public event EventHandler<GameEventArgs> player_plays_zero;

        public event EventHandler<GameEventArgs> game_started; // почато нову гру (була натиснута кнопка "Почати нову гру")
        public event EventHandler<GameEventArgs> move_done; // гравець натиснув на кнопку на ігровому полі

        GameEventArgs ep;
        public Form1()
        {
            // InitializeComponent is a special method used by a designer
            // (a code generating utility as a result of form editing in the graphical window)
            InitializeComponent(); 

            ep = new GameEventArgs();

            // adding the 9 playground buttons to the List buttons
            Buttons.Add(button0);
            Buttons.Add(button1);
            Buttons.Add(button2);
            Buttons.Add(button3);
            Buttons.Add(button4);
            Buttons.Add(button5);
            Buttons.Add(button6);
            Buttons.Add(button7);
            Buttons.Add(button8);

            Block_Playground(); // the playground buttons are blocked before the game start
        }
        #region IView Implementation
       
        public void GameStarted() // sets a radiobutton or a text message notifying that the game was started/is running
        {
            Block_Start_new_game_Button();
            Start_new_game.Text = "Гру розпочато";
        }
        public void GameFinished(string message) // sets a radiobutton or a text message notifying that the game was finished
        {
            // Enable_Start_new_game_Button();
            Start_new_game.Text = message;
        }

        public void Block_Start_new_game_Button() // blocks the button Start_new_game
        {
            Start_new_game.Enabled = false;
        }
        public void Enable_Start_new_game_Button() // enables the button Start_new_game
        {
            Start_new_game.Enabled = true;
        }
        public void ClearField() // clears the playground
        {
            for (int i = 0; i < 9; i++)
            {
                SetButtonText(i, "");
            }
        }
        public void GameNotYetStarted() // sets a radiobutton or a text message notifying that the game is not running, settings can be changed
        {
            Start_new_game.Text = "Почати нову гру";
        }
        public void LockTabSettings()
        {
            this.tabSettings.Enabled = false;
            this.Start_new_game.Enabled = false;
        }
        public void ReleaseTabSettings()
        {
            this.tabSettings.Enabled = true;
            this.Start_new_game.Enabled = true;
        }
        public void SetButtonText(int button_number, string text) // sets Text property of the playground buttons
        {
           if (button_number == 0)
                button0.Text = text;
           else if (button_number == 1)
                button1.Text = text;
           else if (button_number == 2)
                button2.Text = text;
           else if (button_number == 3)
                button3.Text = text;
           else if (button_number == 4)
                button4.Text = text;
           else if (button_number == 5)
                button5.Text = text;
           else if (button_number == 6)
                button6.Text = text;
           else if (button_number == 7)
                button7.Text = text;
           else if (button_number == 8)
                button8.Text = text;
        }
        public void MarkWhoIsOnMove(string who_is_on_move) // depending on who is on move ("Computer" or "Player"), checks the respective radio button
        {
            if (who_is_on_move == "Computer")
                ComputersMove.Checked = true;
            else if (who_is_on_move == "Player")
                PlayersMove.Checked = true;
            else
                throw new Exception("Parameter who_is_on_move has a wrong value!");
        }
        public void Block_Playground() // blocks the buttons of the playground to prevent the Player going there during Computers move
        {
            for (int i = 0; i < 9; i++)
            {
                Buttons[i].Enabled = false;
            }
        }
        public void Release_Playground() // releases the playground buttons to make Player able to do a move
        {
            for (int i = 0; i < 9; i++)
            {
                Buttons[i].Enabled = true;
            }
        }
        public void Winner(string s) // displays the winner
        {
            Start_new_game.Text = s + " won!";
        }

        #endregion IView Implementation

        // Button in the playground pressed, the button number is extracted from the sender parameter
        public void Playground_Click(object? sender, EventArgs e)
        {
            Button b = (Button)sender;
            ep.event_parameter = b.Name;
            ep.Control = sender;
            if (b.Text.Length == 0) // no text means that the field is not occupied yet
            {
                move_done.Invoke(sender, ep); // 1st parameter: sender, i.e. a Button object
            }
            // else (b.Text.Length != 0) => ingnore since a move was already done here earlier
        } 

        private void DifficultyRadioButtonClick(object? sender, EventArgs e) // Difficulty level change
        {
            RadioButton r = (RadioButton)sender;
            r.Checked = true;

            if (Simple.Checked == true)
            {
                ep.event_parameter = "simple";
                difficulty_simple.Invoke(this, ep);
            }
            else if (Medium.Checked == true)
            {
                ep.event_parameter = "medium";
                difficulty_medium.Invoke(this, ep);
            }
            else if (Hard.Checked == true)
            {
                ep.event_parameter = "hard";
                difficulty_hard.Invoke(this, ep);
            }
        }
        private void CheckBoxClick(object? sender, EventArgs e) // Who goes first
        {
            CheckBox chb = (CheckBox)sender;
            // chb.Checked = !(chb.Checked);
            
            if (Player_goes_first.Checked == true)
            {
                ep.event_parameter = "Players move first";
                players_move_first.Invoke(this, ep);
            }
            else if (Player_goes_first.Checked == false)
            {
                ep.event_parameter = "Computers move first";
                computers_move_first.Invoke(this, ep);
            }
        }
        private void PlayerSignRadioButtonClick(object? sender, EventArgs e) // Sign of the player
        {
            RadioButton r = (RadioButton)sender;
            r.Checked = true;
            if (Cross.Checked == true)
            {
                ep.event_parameter = "Player plays with Cross";
                player_plays_cross.Invoke(this, ep);
            }
            else if (Zero.Checked == true)
            {
                ep.event_parameter = "Player plays with Zero";
                player_plays_zero.Invoke(this, ep);
            }
        }

        private void GameStartClick(object? sender, EventArgs e) // Game start
        {
            ep.event_parameter = this.Start_new_game.Text.ToString();
            game_started.Invoke(sender, ep); // 1st parameter: sender
        }
        
    }
}