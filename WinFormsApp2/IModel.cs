using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp2
{
    internal interface IModel
    {
        // Difficulty level: Easy, Medium (by Default), Hard
        Model_Helper.Difficulty Level { get; set; }
        // Who goes first
        Model_Helper.Players Goes_First { get; set; }
        // Player plays with: Cross or Zero. Computer plays with the other sign.
        Model_Helper.Signs Players_Sign {  get; set;}
        // Computer plays with: another sign than the Player
        Model_Helper.Signs Computers_Sign { get; set; }

        // Чий зараз хід
        Model_Helper.Players Current_Move { get; set; }
        // Гру не розпочато, розпочато, або закінчено виграшем Гравця, виграшем Компьютера, нічиєю
        Model_Helper.GameState Game_State { get; set; }


        int this[int row, int col] // gets or sets a value from the array int_field
        {
            get;
            set;
        }

        /*
        abstract int Get_field(int row, int col); // get a value from the array int_field

        abstract void Set_field(int row, int col, int value); // set a value into the array int_field
        */
        abstract bool All_Fields_Occupied(); // checks, if all playgroundfields are occupied and returns true, if yes, otherwise returns false

        abstract bool Is_There_Winner(); // checks, if the player who has done a recent move is a winner
        abstract void Clear_Playground(); // sets the playground to the initial state

    }
}
