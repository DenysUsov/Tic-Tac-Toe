using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    internal class Model_Helper
    {
        public enum Difficulty { Simple = 0, Medium, Hard };
        public enum Players { Player = 0, Computer = 1 };
        public enum Signs { O = 0, X = 1 };
        public enum GameState { Not_started = 0, Started = 1, PlayerWon = 2, ComputerWon = 3, Draw = 4, GameFinished = 5};
    }

    internal class Model: IModel
    {
        private Model_Helper.Signs players_sign = Model_Helper.Signs.X;
        private Model_Helper.Signs computers_sign = Model_Helper.Signs.O;

        // Difficulty level: Easy, Medium (by Default), Hard
        public Model_Helper.Difficulty Level { get; set; } = Model_Helper.Difficulty.Medium;
        // Who goes first
        public Model_Helper.Players Goes_First { get; set; } = Model_Helper.Players.Player;
        // Player plays with: Cross or Zero. Computer plays with the other sign.
        public Model_Helper.Signs Players_Sign
        {
            get { return players_sign; }
            set { players_sign = value; }
        }
        // Computer plays with: another sign than the Player
        public Model_Helper.Signs Computers_Sign
        {
            get { return computers_sign; }

            set
            {
                if (players_sign == Model_Helper.Signs.X)
                    computers_sign = Model_Helper.Signs.O;
                else if (players_sign == Model_Helper.Signs.O)
                    computers_sign = Model_Helper.Signs.X;
            }
        }

        // Чий зараз хід
        public Model_Helper.Players Current_Move { get; set; } = Model_Helper.Players.Player;
        // Гру не розпочато, розпочато, або закінчено виграшем Гравця, виграшем Компьютера, нічиєю
        public Model_Helper.GameState Game_State { get; set; } = Model_Helper.GameState.Not_started;

        // Array for the playground 9x9, possible states of each position: occupied by Player = 0, occupied by Computer = 1, empty = 2
        private int[,] int_field = new int[,] { { 2, 2, 2 }, {2, 2, 2 }, { 2, 2, 2} };

        public int this[int row, int col] // gets or sets a value from the array int_field
        {
            get
            {
                try
                {
                    if (row >= 0 && row <= 2 && col >= 0 && col <= 2)
                    {
                        return int_field[row, col];
                    }
                    else
                        throw new ApplicationException("\nIncorrect row or/and column value [0;2] in a get reguest: row = " + row + " col = " + col);
                }
                catch(ApplicationException ex)
                {
                    Console.WriteLine(ex.Message);
                    return -1;
                }
            }
            set
            {
                try
                {
                    if (row >= 0 && row <= 2 && col >= 0 && col <= 2)
                    {
                        int_field[row, col] = value;
                    }
                    else
                        throw new ApplicationException("\nIncorrect row or/and column value [0;2] in a set request: row = " + row + " col = " + col);
                }
                catch (ApplicationException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }
       /*
        public int Get_field(int row, int col) // get a value from the array int_field
        {
            return int_field[row, col];
        }

        public void Set_field(int row, int col, int value) // set a value into the array int_field
        {
            int_field[row, col] = value;
        }
    */

        public bool All_Fields_Occupied() // checks, if all playgroundfields are occupied (0 or 1) and returns true, if yes, otherwise (2) returns false
        {
            bool result = true;
            for (int col = 0; col < 3; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    if (int_field[row, col] == 2)
                    {
                        result = false;
                        break;
                    }
                    else
                        continue;
                }
                if (result == false)
                    break;
            }
            return result;
        }

        public bool Is_There_Winner() // checks, if the player who has done a recent move is a winner
        {
            int int_sign;
            // check the sign of the player on move
            if (Current_Move == Model_Helper.Players.Player)
                int_sign = 0;
            else // (Current_Move == Model_Helper.Players.Computer)
                int_sign = 1;
            // check each row, each column and each diagonal for being fully occupied with the sign of the player on move
            for (int col = 0; col < 3; col++) // checking columns
            {
                if (this[0, col] == int_sign && this[1, col] == int_sign && this[2, col] == int_sign)
                    return true;
            }
            for (int row = 0; row < 3; row++) // checking rows
            {
                if (this[row, 0] == int_sign && this[row, 1] == int_sign && this[row, 2] == int_sign)
                    return true;
            }
            if (this[0, 0] == int_sign && this[1, 1] == int_sign && this[2, 2] == int_sign) // check the main diagonal
                return true;
            if (this[2, 0] == int_sign && this[1, 1] == int_sign && this[0, 2] == int_sign) // check the 2nd diagonal
                return true;
            return false;
        }

        public void Clear_Playground() // sets the playground to the initial state
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int_field[row, col] = 2; // occupied by Player = 0, occupied by Computer = 1, empty = 2
                }
            }
        }
        
    }
}
