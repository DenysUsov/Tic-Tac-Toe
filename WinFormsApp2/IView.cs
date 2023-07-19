using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp2
{
    internal interface IView
    {

        // declaring events
        event EventHandler<GameEventArgs> difficulty_simple; // зміна рівня складності гри (можливо до початку нової гри)
        event EventHandler<GameEventArgs> difficulty_medium;
        event EventHandler<GameEventArgs> difficulty_hard;

        event EventHandler<GameEventArgs> players_move_first; // зміна черговості ходів (можливо до початку нової гри)
        event EventHandler<GameEventArgs> computers_move_first;

        event EventHandler<GameEventArgs> player_plays_cross; // вибір знаку, яким гратиме гравець (можливо до початку нової гри)
        event EventHandler<GameEventArgs> player_plays_zero;

        event EventHandler<GameEventArgs> game_started; // почато нову гру (була натиснута кнопка "Почати нову гру")
        event EventHandler<GameEventArgs> move_done; // гравець натиснув на кнопку на ігровому полі

        // abstract void Add(System.Windows.Forms.Button b); // adds a button control in the form to a list of Button objects
        abstract void GameStarted(); // sets a radiobutton or a text message notifying that the game was started/is running
        abstract void GameFinished(string message); // displays a message notifying that the game was finished
        abstract void Block_Start_new_game_Button(); // blocks the button Start_new_game
        abstract void Enable_Start_new_game_Button(); // enables the button Start_new_game
        abstract void ClearField(); // clears the playground
        abstract void GameNotYetStarted(); // sets a radiobutton or a text message notifying that the game is not running, settings can be changed
        abstract void LockTabSettings(); // Locks game settings (e.g. after the game started)
        abstract void ReleaseTabSettings(); // Releases settings (e.g. after the game finished, before the game started)
        abstract void SetButtonText(int button_number, string text); // sets Text property of the playground buttons
        abstract void MarkWhoIsOnMove(string who_is_on_move); // sets a radiobutton or a text message notifying who is currently on move
        abstract void Block_Playground(); // blocks the buttons of the playground to prevent the Player going there during Computers move
        abstract void Release_Playground(); // releases the playground buttons to make Player able to do a move
        abstract void Winner(string s); // displays the winner
    }
}
