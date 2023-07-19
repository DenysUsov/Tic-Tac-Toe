using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinFormsApp2
{
    internal class Presenter
    {
        private IView v;
        private IModel m;
        private Computer_Player cp;

        public Presenter(IView _v, IModel _m, Computer_Player _cp)
        {
            this.v = _v;
            this.m = _m;
            this.cp = _cp;
            // identifying the method treating the events in the class Form1
            v.difficulty_simple += this.OnDifficultyLevelSet;
            v.difficulty_medium += this.OnDifficultyLevelSet;
            v.difficulty_hard += this.OnDifficultyLevelSet;

            v.players_move_first += this.OnSelectionWhoseMoveFirst;
            v.computers_move_first += this.OnSelectionWhoseMoveFirst;

            v.player_plays_cross += this.OnSignSelection;
            v.player_plays_zero += this.OnSignSelection;

            v.game_started += this.OnGameStart;

            v.move_done += this.OnMoveDone;
            cp.move_done += this.OnMoveDone;
        }

        #region Before_Game_Start
        public void OnDifficultyLevelSet(object? sender, GameEventArgs ep)
        {
            if (ep.event_parameter == "simple")
                m.Level = Model_Helper.Difficulty.Simple;
            else if (ep.event_parameter == "medium")
                m.Level = Model_Helper.Difficulty.Medium;
            else if (ep.event_parameter == "hard")
                m.Level = Model_Helper.Difficulty.Hard;
        }

        public void OnSelectionWhoseMoveFirst(object? sender, GameEventArgs ep)
        {
            if (ep.event_parameter == "Players move first")
                m.Goes_First = Model_Helper.Players.Player;
            else if (ep.event_parameter == "Computers move first")
                m.Goes_First = Model_Helper.Players.Computer;
        }

        public void OnSignSelection(object? sender, GameEventArgs ep)
        {
            if (ep.event_parameter == "Player plays with Cross")
            {
                m.Players_Sign = Model_Helper.Signs.X;
                m.Computers_Sign = Model_Helper.Signs.O;
            }
            else if (ep.event_parameter == "Player plays with Zero")
            {
                m.Players_Sign = Model_Helper.Signs.O;
                m.Computers_Sign = Model_Helper.Signs.X;
            }
        }
        #endregion Before_Game_Start
        
        public void OnGameStart(object? sender, GameEventArgs ep)
        {
            if (m.Game_State == Model_Helper.GameState.Not_started)
            {
                // freezing controls for game settings and the button Start_new_game until the end of the game
                v.LockTabSettings();
                // updating the model
                m.Game_State = Model_Helper.GameState.Started;
                // Changing the text on the button Start_new_game
                v.GameStarted();
                // updating the model who is on move
                if (m.Goes_First == Model_Helper.Players.Player)
                    m.Current_Move = Model_Helper.Players.Player;
                else
                    m.Current_Move = Model_Helper.Players.Computer;
                // Let the Player or Computer (depending on who is on move now) to do the move
                // enable the radiobuttons for the current move. Check, who makes the first move and set the respective radio button checked
                if (m.Current_Move == Model_Helper.Players.Player)
                {
                    // check the respective radiobutton and do nothing; after the players move done, a respective event should be invoked
                    v.MarkWhoIsOnMove("Player");
                    // release the playground buttons
                    v.Release_Playground();
                }
                else // ((m.Current_Move == Model_Helper.Players.Computer)
                {
                    // check the respective radio button
                    v.MarkWhoIsOnMove("Computer");
                    // block the playground buttons to prevent Player to press them during the Computers move
                    v.Block_Playground();
                    // initiating a move by Computer
                    cp.computers_move(); // after the computers move done, a respective event should be invoked
                }
            }
            else if (m.Game_State == Model_Helper.GameState.Started)
            {
                // this case should never happen
                Console.WriteLine("The Start_new_game button is still enabled during the game!");
                throw new Exception("The Start_new_game button is still enabled during the game!");
            }
            else if (m.Game_State == Model_Helper.GameState.PlayerWon || m.Game_State == Model_Helper.GameState.ComputerWon 
                || m.Game_State == Model_Helper.GameState.Draw)
            {   
                // this is necessary for the next case (m.Game_State == Model_Helper.GameState.GameFinished) to work
                m.Game_State = Model_Helper.GameState.GameFinished;
                v.GameFinished("Гру закінчено");
            }
            else if (m.Game_State == Model_Helper.GameState.GameFinished)
            {
                v.ClearField(); // clearing the playground button texts
                m.Clear_Playground(); // clearing the playground array of the model
                m.Game_State = Model_Helper.GameState.Not_started;
                v.GameNotYetStarted();
                v.ReleaseTabSettings();
            }
        }

        public void OnMoveDone(object? sender, GameEventArgs ep)
        {
            // update the model
            string[] words;
            char[] to_trim = { 'b', 'u', 't', 't', 'o', 'n' };
            int row, col, value, button_number;
            string text;
            if (sender is Computer_Player)
            {
                words = ep.event_parameter.Split(' ');
                row = int.Parse(words[2]);
                col = int.Parse(words[3]);
                value = 1; // Convention: Player = 0, Computer = 1, empty = 2
                try
                {
                    if (m[row, col] == 2)
                    { 
                        m[row, col] = value;
                        // update the field appearance
                        button_number = convert_row_col_to_button_number(row, col);
                        text = m.Computers_Sign.ToString();
                        v.SetButtonText(button_number, text);
                    }
                    else
                        throw new ApplicationException("Computer is trying to move to an occupied field " + row + " " + col);
                }
                catch (ApplicationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (sender is Button)
            {
                button_number = int.Parse(ep.event_parameter.Trim(to_trim));
                value = 0; // Convention: Player = 0, Computer = 1, empty = 2
                convert_button_number_to_row_col(button_number, out row, out col);
                try
                {
                    if (m[row, col] == 2)
                    {
                        // update the model
                        m[row, col] = value;
                        // update the field appearance
                        text = m.Players_Sign.ToString();
                        v.SetButtonText(button_number, text);
                    }
                    else
                        throw new ApplicationException("Player is trying to move to an occupied field " + row + " " + col);
                }
                catch (ApplicationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            // check, if the game has been won at the current move
            // if there is a winner, report the winner and stop the game
            if (m.Is_There_Winner() == true)
            {
                v.Block_Playground();
                if (m.Current_Move == Model_Helper.Players.Player)
                {
                    m.Game_State = Model_Helper.GameState.PlayerWon;
                    v.Winner("Player");  
                }
                else if (m.Current_Move == Model_Helper.Players.Computer)
                {
                    m.Game_State = Model_Helper.GameState.ComputerWon;
                    v.Winner("Computer");
                }
                v.Enable_Start_new_game_Button();
                // v.GameFinished();
            }
            else if (m.All_Fields_Occupied() == true) // m.Is_There_Winner() == false
                // if there is no winner, check, if the game has finished (if all fields are occupied)
            {
                // do not initiate the next move, report a draw and stop the game
                v.Block_Playground();
                m.Game_State = Model_Helper.GameState.Draw;
                v.Winner("Nobody");
                v.Enable_Start_new_game_Button();
                // v.GameFinished();
            }
            else // (m.Is_There_Winner() == false && m.All_Fields_Occupied() == false) : game not finished
            {
                // initiate the next move
                if (m.Current_Move == Model_Helper.Players.Player)
                {
                    m.Current_Move = Model_Helper.Players.Computer;
                    // check the respective radio button
                    v.MarkWhoIsOnMove("Computer");
                    // block the playground buttons to prevent Player to press them during the Computers move
                    v.Block_Playground();
                    // initiating a move by Computer
                    cp.computers_move(); // after the computers move done, a respective event should be invoked
                }
                else // m.Current_Move == Model_Helper.Players.Computer
                {
                    m.Current_Move = Model_Helper.Players.Player;
                    // check the respective radiobutton and do nothing; after the players move done, a respective event should be invoked
                    v.MarkWhoIsOnMove("Player");
                    // release the playground buttons
                    v.Release_Playground();
                }
            }

        }

        private void convert_button_number_to_row_col(int button_number, out int _row, out int _col)
        {
            int number = -1;
            _row = -1;
            _col = -1;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    number++;
                    if (number == button_number)
                    {
                        _row = row;
                        _col = col;
                        break;
                    }
                }
                if (number == button_number)
                    break;
            }
        }

        private int convert_row_col_to_button_number(int _row, int _col)
        {
            int result = -1;
            bool br = false;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    result++;
                    if (row == _row && col == _col)
                    {
                        br = true;
                        break;
                    }
                    else
                        continue;
                }
                if (br)
                    break;
            }
            return result;
        }

    }
}
