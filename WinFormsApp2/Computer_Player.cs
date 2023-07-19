using Accessibility;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace WinFormsApp2
{
    public class Point
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Point(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public static bool operator ==(Point left, Point right)
        {
            if (left.Row == right.Row && left.Col == right.Col)
                return true;
            else
                return false;
        }

        public static bool operator !=(Point left, Point right)
        {
            if (left.Row != right.Row || left.Col != right.Col)
                return true;
            else
                return false;
        }
    }

    // this class simulates a computer player logic according to three difficulty levels: Simple, Medium, Hard
    // an instance of this class can be considered as a 2nd part of the model in the MVP pattern
    internal class Computer_Player // Simple (stupid) level
    {
        private IModel m; // interface reference to the model object
        private GameEventArgs ep; // event parameter
        internal event EventHandler<GameEventArgs> move_done; // event delegate
        private List<Point> Computer_point_list, Player_point_list;

        public Computer_Player(IModel model)
        {
            m = model;
            ep = new GameEventArgs();
            Computer_point_list = new List<Point>();
            Player_point_list = new List<Point>();
        }
        
        public void computers_move() // calls Computer's move subroutine according to the game difficulty level
        {
            int _row = -1;
            int _col = -1;
            if (m.Level == Model_Helper.Difficulty.Simple)
                computers_move_simple(out _row, out _col);
            else if (m.Level == Model_Helper.Difficulty.Medium)
                computers_move_medium(out _row, out _col);
            else if (m.Level == Model_Helper.Difficulty.Hard)
                computers_move_hard(out _row, out _col);
            ep.event_parameter = "Computers move: " + _row + " " + _col;
            move_done.Invoke(this, ep);
        }
        #region Simple
        // count the non-occupied fields (a field contains a value of 2);
        // the occupied fields contain values 0 - by Player or 1 - by Computer
        private int count_empty_fields()
        {
            int empty_fields = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (m[row, col] == 2)
                        empty_fields++;
                }
            }
            return empty_fields;
        }

        // randomly select one of the non-occupied field to move there
        private void computers_move_simple(out int _row, out int _col)
        {
            _row = -1;
            _col = -1;
            Random rnd = new Random();
            int random = rnd.Next(count_empty_fields());
            int counter = -1;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (m[row, col] == 2)
                    {
                        counter++;
                        if (counter == random)
                        {
                            _row = row;
                            _col = col;
                            break;
                        }
                    }
                }
                if (counter == random)
                    break;
            }

        }
        #endregion Simple

        #region Medium
        // returns coordinates of a first field occupied by Computer (1) or Player (0) in the out parameters or -1, -1 if not found
        private void find_first_field_occupied_by(int player, int start_row, int start_col, out int _row, out int _col)
        {
            _row = -1;
            _col = -1;
            for (int row = start_row; row < 3; row++)
            {
                if (row > start_row)
                    start_col = 0;
                for (int col = start_col; col < 3; col++)
                { // 0 - occupied by Player, 1 - by Computer, 2 - empty
                    if (m[row, col] == player)
                    {
                        _row = row;
                        _col = col;
                        break;
                    }
                }
                if (_row != -1)
                    break;
            }
        }

        // checks the row occupied by 1 sign of Computer, if the other two fields are empty
        private bool are_other_two_fields_of_row_empty(int row, int col)
        {
            bool result = false;
            int count = 0;
            for (int i = 0; i < 3; i++) // i is the number of a column, it counts along the row
            {
                if (i != col && m[row, i] == 2)
                    count++;
            }
            if (count == 2)
                result = true;
            return result;
        }
        // checks the vertical line occupied by 1 sign of Computer, if the other two fields are empty
        private bool are_other_two_fields_of_column_empty(int row, int col)
        {
            bool result = false;
            int count = 0;
            for (int i = 0; i < 3; i++) // i is a number of a row, it counts along a column
            {
                if (i != row && m[i, col] == 2)
                    count++;
            }
            if (count == 2)
                result = true;
            return result;
        }
        // checks, if a field lies on the primary diagonal
        private bool lies_on_primary_diagonal(int row, int col)
        {
            bool result = false;
            if (row == col)
                result = true;
            return result;
        }
        // checks, if a field lies on a secondary diagonal
        private bool lies_on_secondary_diagonal(int row, int col)
        {
            bool result = false;
            if ((row == 0 && col == 2) || (row == 1 && col == 1) || (row == 2 && col == 0))
                result = true;
            return result;
        }
        // checks, if the primary diagonal containing a field occupied by Computer has other two fields empty
        private bool are_other_two_fields_on_primary_diagonal_empty(int row) // row == col for the primary diagonal
        {
            bool result = false;
            int counter = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i != row && m[i, i] == 2)
                    counter++;
            }
            if (counter == 2)
                result = true;
            return result;
        }
        // checks, if the secondary diagonal containing a fiald occupied by Computer has other two fields empty
        private bool are_other_two_fields_on_secondary_diagonal_empty(int row) // col = 2 - row for the secondary diagonal
        {
            bool result = false;
            int counter = 0;
            for (int i = 0; i < 3; i++) // i is the row number
            {
                if (i != row && m[i, 2 - i] == 2)
                    counter++;
            }
            if (counter == 2)
                result = true;
            return result;
        }
        private bool are_the_two_points_on_the_same_line(Point p1, Point p2)
        {
            bool result = false;
            if (p1.Row == p2.Row || p1.Col == p2.Col ||
                (lies_on_primary_diagonal(p1.Row, p1.Col) && lies_on_primary_diagonal(p2.Row, p2.Col)) ||
                (lies_on_secondary_diagonal(p1.Row, p1.Col) && lies_on_secondary_diagonal(p2.Row, p2.Col)))
            {
                result = true;
            }
            return result;
        }
        private bool is_Point_at_center(Point p)
        {
            if (p.Row == 1 && p.Col == 1)
                return true;
            else
                return false;
        }
        private bool is_Point_at_corner(Point p)
        {
            if (p.Row == 0 && p.Col == 0 ||
                p.Row == 2 && p.Col == 2 ||
                p.Row == 0 && p.Col == 2 ||
                p.Row == 2 && p.Col == 0)
                return true;
            else return false;
        }
        private bool is_Point_at_edge_middle(Point p)
        {
            if (p.Row == 0 && p.Col == 1 ||
                p.Row == 1 && p.Col == 2 ||
                p.Row == 2 && p.Col == 1 ||
                p.Row == 1 && p.Col == 0)
                return true;
            else return false;
        }

        // if there is an empty field on the row, the method returns its column number, otherwise it returns -1
        private int empty_field_in_row(int row)
        {
            int col = -1;
            for (int i = 0; i < 3; i++)
            {
                if (m[row, i] == 2)
                {
                    col = i;
                    break;
                }
            }
            return col;
        }
        // if there is an empty field on the column, the method returns its row number, otherwise it returns -1
        private int empty_field_in_col(int col)
        {
            int row = -1;
            for (int i = 0; i < 3; i++)
            {
                if (m[i, col] == 2)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        // if there is an empty field on the primary diagonal, the method returns its row number, otherwise it returns -1
        private int empty_field_in_prim_diag()
        {
            int row = -1;
            for (int i = 0; i < 3; i++)
            {
                if (m[i, i] == 2)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        // if there is an empty field on the secondary diagonal, the method returns its row number, otherwise it returns -1
        private int empty_field_in_sec_diag()
        {
            int row = -1;
            for (int i = 0; i < 3; i++)
            {
                if (m[i, 2 - i] == 2)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        // returns list of points as an out parameter which are intersections for two lists of lines
        private void add_intersections_to_list(List<List<Point>> ll1, List<List<Point>> ll2, out List<Point> result)
        {
            List<Point> list = new List<Point>();
            foreach (var l1 in ll1)
            {
                foreach (var l2 in ll2)
                {
                    foreach (Point l1_point in l1)
                    {
                        foreach (Point l2_point in l2)
                        {
                            if (l1_point.Row == l2_point.Row && l1_point.Col == l2_point.Col) // if the two points match
                            {
                                list.Add(l1_point);
                                break; // exiting from the most inner cycle going to the next l1_point in l1
                            }
                        }
                    }
                }
            }
            result = list;
        }

        // returns the third Point on a line (row, column, primary or secondary diagonal) when the 1st and the 2nd Points are given;
        // if the given Points do not lie on the same line, it returns (-1, -1)
        private Point third_Point_on_the_line(Point p1, Point p2)
        {
            Point result = new Point(-1, -1);

            if (p1.Row == p2.Row) // the two Points lie on the same row
            {
                result.Row = p1.Row;
                for (int i = 0; i < 3; i++)
                {
                    if (i != p1.Col && i != p2.Col)
                    {
                        result.Col = i;
                        break;
                    }
                }
            }
            else if (p1.Col == p2.Col) // the two Points lie on the same column
            {
                result.Col = p1.Col;
                for (int i = 0; i < 3; i++)
                {
                    if (i != p1.Row && i != p2.Row)
                    {
                        result.Row = i;
                        break;
                    }
                }
            }
            else if (lies_on_primary_diagonal(p1.Row, p1.Col) == true && lies_on_primary_diagonal(p2.Row, p2.Col) == true)
            // the two Points lie on the primary diagonal
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != p1.Row && i != p2.Row)
                    {
                        result.Row = i;
                        result.Col = i;
                        break;
                    }
                }
            }
            else if (lies_on_secondary_diagonal(p1.Row, p1.Col) == true && lies_on_secondary_diagonal(p2.Row, p2.Col) == true)
            // the two Points lie on the secondary diagonal
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != p1.Row && i != p2.Row)
                    {
                        result.Row = i;
                        result.Col = 2 - i;
                        break;
                    }
                }
            }

            return result;
        }
        // returns the first Point from List<Point> which equals to the provided value
        // returns (-1, -1), if no match found
        private Point first_field_from_List_equal_to_value(List<Point> points, int value)
        {
            Point p = new Point(-1, -1);
            foreach (var point in points)
            {
                if (m[point.Row, point.Col] == value)
                {
                    p.Row = point.Row;
                    p.Col = point.Col;
                    break;
                }
            }
            return p;
        }
        // Checks all possible point pairs in the point list, if any two of them are on the same row, col or diagonal and
        // if yes, then return the coordinates of the first empty point lying on any of those lines;
        // if not, then return (-1, -1).
        // The list must contain at least 2 members for the correct work of the method
        private Point first_empty_field_on_any_line_between_any_fields_in_list(List<Point> fields)
        {
            Point p = new Point(-1, -1);
            for (int i = 0; i < fields.Count; i++)
            {
                if (i < fields.Count)
                {
                    for (int j = i + 1; j < fields.Count; j++)
                    {
                        if (are_the_two_points_on_the_same_line(fields[i], fields[j]) == true)
                        {
                            p = third_Point_on_the_line(fields[i], fields[j]);
                            if (m[p.Row, p.Col] == 2) // if the 3d Point on the common line is NOT occupied, stop searching
                                break;
                            else // if the field is occupied then reset the p coordinates to the default values
                            {
                                p.Row = -1;
                                p.Col = -1;
                            }
                        }
                    }
                    if (m[p.Row, p.Col] == 2)
                    {
                        break;
                    } // if the 3d Point on the common line is NOT occupied, stop searching
                }
            }
            return p;
        }
        // checks, if two Points lie on a common row, column, primary or secondary diagonal and the third point is not occupied
        // returns null or a list of the third non-occupied points
        private List<Point> third_field_empty(List<Point> p)
        {
            List<Point> result = new List<Point>();
            for (int i = 0; i < p.Count; i++)
            {
                for (int j = i + 1; j < p.Count; j++)
                {
                    if (p[i].Row == p[j].Row) // if i-th and j-th Points are on one row
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (m[p[i].Row, c] == 2) // looking for an empty field on the row
                            {
                                result.Add(new Point(p[i].Row, c));
                                break;
                            }
                        }
                    }
                    else if (p[i].Col == p[j].Col) // if i-th and j-th Points are on one column
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (m[c, p[i].Col] == 2) // looking for an empty field on the column
                            {
                                result.Add(new Point(c, p[i].Col));
                                break;
                            }
                        }
                    }
                    // if i-th and j-th Points are on the primary digonal
                    else if (lies_on_primary_diagonal(p[i].Row, p[i].Col) && lies_on_primary_diagonal(p[j].Row, p[j].Col))
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (m[c, c] == 2)
                            {
                                result.Add(new Point(c, c)); // looking for an empty field on the primary diagonal
                                break;
                            }
                        }
                    }
                    // if i-th and j-th Points are on the secondary digonal
                    else if (lies_on_secondary_diagonal(p[i].Row, p[i].Col) && lies_on_secondary_diagonal(p[j].Row, p[j].Col))
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (m[2 - c, c] == 2)
                            {
                                result.Add(new Point(2 - c, c)); // looking for an empty field on the secondary diagonal
                                break;
                            }
                        }
                    }

                }
            }
            return result;
        }
        // returns List<Point> of all empty fields on lines in which only one of three fields is occupied by Player_or_Computer and other two fields are emtpy
        private List<Point> empty_fields_in_lines_where_only_1_field_occupied_by(int Player_or_Computer)
        {
            List<Point> result = new List<Point>();
            List<Point> empty_fields_in_line = new List<Point>();
            bool is_there_match = false;
            int counter_PoC = 0, counter_empty = 0, counter_another = 0;
            // Check rows
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (m[r, c] == Player_or_Computer) // 0 - for Player, 1 - for Computer
                        counter_PoC++;
                    else if (m[r, c] == 2) // 2 - for empty
                    {
                        counter_empty++;
                        empty_fields_in_line.Add(new Point(r, c));
                    }
                    else // (m[r, c] != 2 && m[r, c] != Player_or_Computer)
                        counter_another++;
                }
                if (counter_PoC == 1 & counter_empty == 2)
                {
                    foreach (Point pr in empty_fields_in_line)
                    {
                        foreach (Point pe in result)
                        {
                            if (pe.Row == pr.Row && pe.Col == pr.Col)
                            {
                                is_there_match = true;
                                break;
                            }
                        }
                        if (is_there_match == true)
                        {
                            is_there_match = false;
                            continue;
                        }
                        else // (is_there_match == false)
                            result.Add(pr);
                    }
                }
                empty_fields_in_line.Clear();
                counter_PoC = 0;
                counter_empty = 0;
                counter_another = 0;
            }
            // check columns
            for (int c = 0; c < 3; c++)
            {
                for (int r = 0; r < 3; r++)
                {
                    if (m[r, c] == Player_or_Computer) // 0 - for Player, 1 - for Computer
                        counter_PoC++;
                    else if (m[r, c] == 2) // 2 - for empty
                    {
                        counter_empty++;
                        empty_fields_in_line.Add(new Point(r, c));
                    }
                    else // (m[r, c] != 2 && m[r, c] != Player_or_Computer)
                        counter_another++;
                }
                if (counter_PoC == 1 & counter_empty == 2)
                {
                    foreach (Point pr in empty_fields_in_line)
                    {
                        foreach (Point pe in result)
                        {
                            if (pe.Row == pr.Row && pe.Col == pr.Col)
                            {
                                is_there_match = true;
                                break;
                            }
                        }
                        if (is_there_match == true)
                        {
                            is_there_match = false;
                            continue;
                        }
                        else // (is_there_match == false)
                            result.Add(pr);
                    }
                }
                empty_fields_in_line.Clear();
                counter_PoC = 0;
                counter_empty = 0;
                counter_another = 0;
            }
            // check primary diagonal
            for (int c = 0; c < 3; c++)
            {
                if (m[c, c] == Player_or_Computer) // 0 - for Player, 1 - for Computer
                    counter_PoC++;
                else if (m[c, c] == 2) // 2 - for empty
                {
                    counter_empty++;
                    empty_fields_in_line.Add(new Point(c, c));
                }
                else // (m[c, c] != 2 && m[c, c] != Player_or_Computer)
                    counter_another++;
            }
            if (counter_PoC == 1 & counter_empty == 2)
            {
                foreach (Point pr in empty_fields_in_line)
                {
                    foreach (Point pe in result)
                    {
                        if (pe == pr)
                        {
                            is_there_match = true;
                            break;
                        }
                    }
                    if (is_there_match == true)
                    {
                        is_there_match = false;
                        continue;
                    }
                    else // (is_there_match == false)
                        result.Add(pr);
                }
            }
            empty_fields_in_line.Clear();
            counter_PoC = 0;
            counter_empty = 0;
            counter_another = 0;
            // check secondary diagonal
            for (int c = 0; c < 3; c++)
            {
                if (m[2 - c, c] == Player_or_Computer) // 0 - for Player, 1 - for Computer
                    counter_PoC++;
                else if (m[2 - c, c] == 2) // 2 - for empty
                {
                    counter_empty++;
                    empty_fields_in_line.Add(new Point(2 - c, c));
                }
                else // (m[2 - c, c] != 2 && m[2 - c, c] != Player_or_Computer)
                    counter_another++;
            }
            if (counter_PoC == 1 & counter_empty == 2)
            {
                foreach (Point pr in empty_fields_in_line)
                {
                    foreach (Point pe in result)
                    {
                        if (pe.Row == pr.Row && pe.Col == pr.Col)
                        {
                            is_there_match = true;
                            break;
                        }
                    }
                    if (is_there_match == true)
                    {
                        is_there_match = false;
                        continue;
                    }
                    else // (is_there_match == false)
                        result.Add(pr);
                }
            }
            empty_fields_in_line.Clear();
            counter_PoC = 0;
            counter_empty = 0;
            counter_another = 0;

            return result;
        }
        // returns List<Point> of all empty fields
        private List<Point> all_empty_fields()
        {
            List<Point> result = new List<Point>();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (m[r, c] == 2)
                        result.Add(new Point(r, c));
                }
            }
            return result;
        }
        // returns List<Point> of the playground fields occupied by Player_or_Computer (0 for Player, 1 for Computer, 2 for non-occupied)
        private List<Point> fields_occupied_by(int Player_or_Computer)
        {
            List<Point> result = new List<Point>();
            int start_row = 0, start_col = 0;
            while (true) // adds fields occupied by Player_or_Computer to the List<Point> result
            {
                find_first_field_occupied_by(Player_or_Computer, start_row, start_col, out int i_row, out int i_col);
                if (i_row == -1 && i_col == -1)
                    break;
                else if (i_row != -1 && i_col != -1)
                {
                    result.Add(new Point(i_row, i_col));
                    start_row = i_row;
                    start_col = i_col;
                }
                if (start_col < 2)
                    start_col = i_col + 1;
                else if (start_col == 2 && start_row < 2)
                {
                    start_row++;
                    start_col = 0;
                }
                else // (start_col == 2 && start_row == 2)
                    break;
            }
            return result;
        }
        // removes dublicated points from the list
        private List<Point> remove_dublicates(List<Point> point_list)
        {
            bool to_remove = false;
            for (int i = point_list.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (point_list[i].Row == point_list[j].Row && point_list[i].Col == point_list[j].Col)
                    {
                        to_remove = true;
                        break;
                    }
                }
                if (to_remove == true)
                {
                    point_list.RemoveAt(i);
                    to_remove = false;
                }
            }
            return point_list;
        }
        // returns a list of lines List<List<Point>> for the given Point in the playground
        private List<List<Point>>? line_list(Point p)
        {
            if (p.Row < 0 || p.Row > 2 || p.Col < 0 || p.Col > 2)
                return null;
            else
            {
                List<List<Point>> result = new List<List<Point>>();
                // always addd the row and the column to the line list
                List<Point> rowl = new List<Point>();
                for (int i = 0; i < 3; i++)
                    rowl.Add(new Point(p.Row, i));
                result.Add(rowl);
                List<Point> coll = new List<Point>();
                for (int i = 0; i < 3; i++)
                    coll.Add(new Point(i, p.Col));
                result.Add(coll);

                if (is_Point_at_corner(p) == true) // add a diagonal to the line list
                {
                    if (lies_on_primary_diagonal(p.Row, p.Col) == true)
                    {
                        List<Point> p_diag = new List<Point>();
                        for (int i = 0; i < 3; i++)
                            p_diag.Add(new Point(i, i));
                        result.Add(p_diag);
                    }
                    else // (lies_on_secondary_diagonal(int row, int col) == true)
                    {
                        List<Point> s_diag = new List<Point>();
                        for (int i = 0; i < 3; i++)
                            s_diag.Add(new Point(i, 2 - i));
                        result.Add(s_diag);
                    }
                }
                else if (is_Point_at_center(p) == true) // add two diagonals to the line list
                {
                    List<Point> p_diag = new List<Point>();
                    for (int i = 0; i < 3; i++)
                        p_diag.Add(new Point(i, i));
                    result.Add(p_diag);

                    List<Point> s_diag = new List<Point>();
                    for (int i = 0; i < 3; i++)
                        s_diag.Add(new Point(i, 2 - i));
                    result.Add(s_diag);
                }
                return result;
            }
        }
        // deletes lines containing Points occupied by Player (0) or Computer (1) and returns an updated list of lines List<List<Point>>
        private List<List<Point>>? line_list_with_no_Player_or_Computer(List<List<Point>>? llist, int Player_or_Computer)
        {
            if (llist == null)
                return null;
            else
            {
                bool found_PoC_sign = false;
                for (int i = llist.Count - 1; i >= 0; i--) // i is the line number
                {
                    for (int j = 0; j < llist[i].Count; j++) // j is the Point number in the line
                    {
                        if (m[llist[i][j].Row, llist[i][j].Col] == Player_or_Computer)
                        {
                            found_PoC_sign = true;
                            break;
                        }
                    }
                    if (found_PoC_sign == true)
                    {
                        llist.RemoveAt(i);
                        found_PoC_sign = false;
                    }
                }
                return llist;
            }
        }
        // returns a List<Point> of all intersections of 3-member-lines (each containing a field occupied by Player but NOT by Computer)
        // or returns null, if:
        // the number of Points in the list is 0, 1, more than 3, or
        // at least two of the points lie on the same line, or
        // or return an empty list with Count == 0, if the intersections are useless, since the intersected lines contain Player's sign
        private List<Point>? intersections(List<Point> point_list)
        {
            List<Point> result = new List<Point>();
            // if the given points are located on the same line or the number of Points in the list is more than 3 => return null
            if (point_list == null || point_list.Count == 0 || point_list.Count == 1 || point_list.Count > 3) // point_list.Count may be equal to 2 or 3 only
                return null;
            /*
            else if (are_the_two_points_on_the_same_line(point_list[0], point_list[1]) || are_the_two_points_on_the_same_line(point_list[0], point_list[2]) ||
                are_the_two_points_on_the_same_line(point_list[1], point_list[2]))
            {
                // if true, there are still possible intersections
            }
            */
            else
            {
                // create a list of lines for each given Point
                List<List<Point>>? ll1 = line_list(point_list[0]);
                List<List<Point>>? ll2 = line_list(point_list[1]);
                if (ll1 == null || ll2 == null)
                    throw new ApplicationException("Incorrect result of the line_list method applied to point_list[0] and or point_list[1]");
                List<List<Point>>? ll3 = null;
                if (point_list.Count == 3)
                {
                    ll3 = line_list(point_list[2]);
                    if (ll3 == null)
                        throw new ApplicationException("Incorrect result of the line_list method applied to point_list[2]");
                }
                // idetification of the sign of the competitor (Player_or_Computer)
                int Player_or_Computer = -1;
                if (m[point_list[0].Row, point_list[0].Col] == 0) // 0 - Player
                    Player_or_Computer = 1; // 1 - Computer
                else if (m[point_list[0].Row, point_list[0].Col] == 1) // 1 - Computer
                    Player_or_Computer = 0; // 0 - Player

                // remove lines containing Player_or_Computer sign from the lists
                ll1 = line_list_with_no_Player_or_Computer(ll1, Player_or_Computer);
                ll2 = line_list_with_no_Player_or_Computer(ll2, Player_or_Computer);

                if (point_list.Count == 3)
                    ll3 = line_list_with_no_Player_or_Computer(ll3, Player_or_Computer);
                // compare each line from one list with each line of the other list and find common fields, add this fields to the result list
                if ((ll1.Count == 0 && ll2.Count == 0 && ll3 == null) ||
                    (ll1.Count == 0 && ll2.Count == 0) || (ll1.Count == 0 && ll3.Count == 0) ||
                    (ll2.Count == 0 && ll3.Count == 0))
                    return result; // if two of the lists ll1-ll3 do not have members, then return list with Count == 0
                else
                {
                    if (ll1.Count > 0 && ll2.Count > 0)
                        add_intersections_to_list(ll1, ll2, out result);
                    if (ll3 != null)
                    {
                        if (ll1.Count > 0 && ll3.Count > 0)
                            add_intersections_to_list(ll1, ll3, out result);
                        if (ll2.Count > 0 && ll3.Count > 0)
                            add_intersections_to_list(ll2, ll3, out result);
                    }
                    result = remove_dublicates(result);
                }

            }
            return result;
        }
        // deletes all points occupied by Player and Computer and returns an updated List<Point>
        private List<Point>? remove_occupied_fields(List<Point>? p_list)
        {
            if (p_list == null || p_list.Count == 0) return p_list;
            for (int i = p_list.Count - 1; i >= 0; i--)
            {
                if (m[p_list[i].Row, p_list[i].Col] != 2) // if field is occupied
                {
                    p_list.Remove(p_list[i]);
                }
            }
            return p_list;
        }
        // general strategy for the 2nd move, if Computer moves second.
        // It moves to the remaining free field on a 3-field-line (columns, rows, diagonals) occupied by Player,
        // if applicable; if the signs of Player are not located on a common line (which is possible for maximum of
        // 3 signs), it identifies all non-occupied intersections
        // of the 3-field-lines containing signs of Player and moves randomly to one of them;
        // if all intersections are occupied, it moves to a free field on a line containing its own sign and not occupied
        // by Player; if it is not possible, it moves to any free field.
        private void general_medium_strategy_C2nd_2move(out int _row, out int _col)
        {
            Random rnd = new Random();
            int random = -1;
            _row = -1; _col = -1;
            
            try
            {
                Player_point_list.Clear();
                Player_point_list = fields_occupied_by(0); // get List of fields occupied by Player (0)
                Point p = first_empty_field_on_any_line_between_any_fields_in_list(Player_point_list);
                if (p.Row != -1 && p.Col != -1)
                {
                    _row = p.Row;
                    _col = p.Col;
                }
                else // (p.Row == -1 || p.Col == -1)
                     // other fields on the lines containing the listed fields are occupied (by Computer) or
                     // the signs of Player are not located on a common line (which is possible for maximum of
                     //           3 signs), Computer identifies all non-occupied intersections
                     //           of the 3-field-lines containing signs of Player and moves randomly to one of them
                {
                    List<Point>? intersections_list = intersections(Player_point_list);
                    
                    // removes occupied fields from the intersections list
                    if (intersections_list != null && intersections_list.Count > 0)
                        intersections_list = remove_occupied_fields(intersections_list);
                    if (intersections_list != null && intersections_list.Count > 0)
                    {
                        random = rnd.Next(intersections_list.Count);
                        _row = intersections_list[random].Row;
                        _col = intersections_list[random].Col;
                    }
                    else if (intersections_list == null || intersections_list.Count == 0)
                    // if all intersections are occupied, it moves to a free field on a line containing its own sign and not occupied
                    //           by Player; if it is not possible, it moves to any free field.
                    {
                        // try to find fields in lines where only one field is occupied by Computer and two others are empty
                        List<Point>? List_empty_fields_to_go = empty_fields_in_lines_where_only_1_field_occupied_by(1); // 1 for Computer
                        /*
                        if (intersections_list == null) // it means that the input fields number > 3
                        {
                            // throw new ApplicationException("intersection_list == null, this should never happen.");
                        } */
                        if (List_empty_fields_to_go != null && List_empty_fields_to_go.Count > 0)
                        {
                            random = rnd.Next(List_empty_fields_to_go.Count);
                            _row = List_empty_fields_to_go[random].Row;
                            _col = List_empty_fields_to_go[random].Col;
                        }
                        else //  (List_empty_fields_to_go == null || List_empty_fields_to_go.Count == 0)
                        {
                            List_empty_fields_to_go = all_empty_fields();
                            if (List_empty_fields_to_go != null && List_empty_fields_to_go.Count > 0)
                            {
                                random = rnd.Next(List_empty_fields_to_go.Count);
                                _row = List_empty_fields_to_go[random].Row;
                                _col = List_empty_fields_to_go[random].Col;
                            }
                            else // all fields are occupied, game finished, this case should not happen
                                throw new ApplicationException("Error: all fields seem to be occupied, although the number of empty fields is " + count_empty_fields());
                        }
                    }
                }
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        // Medium level.
        // I. If Computer moves first.
        // 1st move. It moves randomly (as at the simple level).
        // 2nd move. It moves to one of the empty fields in the 3-field-lines (columns, rows, diagonals)
        //           containing its sign from the 1st move and two non-occupied fields.
        // 3d and later moves.  If there exist a line containing two sings of Computer and a free third field, it goes there to win;
        //           otherwise it acts as in section II, 2nd move.
        // II. If Computer moves 2nd.
        // 1st move. It randomly moves to a free field.
        // 2nd move. It moves to the remaining free field on a 3-field-line (columns, rows, diagonals) occupied by Player,
        //           if applicable; if the signs of Player are not located on a common line (which is possible for maximum of
        //           3 signs), it identifies all non-occupied intersections
        //           of the 3-field-lines containing signs of Player and moves randomly to one of them;
        //           if all intersections are occupied, it moves to a free field on a line containing its own sign and not occupied
        //           by Player; if it is not possible, it moves to any free field.
        // 3d and later moves.  It moves to a free field on a line containing its own sings, if applicable;
        //           otherwise it repeats the strategy of the 2nd move.
        private void computers_move_medium(out int _row, out int _col)
        {
            _row = -1;
            _col = -1;
            Random rnd = new Random();
            int random = -1;
            // I.If Computer moves first.
            if (m.Goes_First == Model_Helper.Players.Computer)
            {
                // 1st move. Computer moves randomly (as at the simple level).
                if (count_empty_fields() == 9)
                {
                    computers_move_simple(out _row, out _col);
                }
                // 2nd move. Computer moves to one of the fields in the 3-field-lines (verticals, horizontals, diagonals)
                //           containing its sign from the 1st move which are still free from the sign of Player.
                else if (count_empty_fields() == 7)
                {
                    int c_row = -1, c_col = -1; // to store coordinates of the field occupied by Computer
                    find_first_field_occupied_by(1, 0, 0, out c_row, out c_col);
                    // check the row containing the sign of Computer, if the other two fields are empty
                    // then move randomly to one of them
                    if (are_other_two_fields_of_row_empty(c_row, c_col) == true)
                    {
                        random = rnd.Next(2);
                        int counter = -1;
                        for (int i = 0; i<3; i++)
                        {
                            if (m[c_row, i] == 2)
                            {
                                counter++;
                                if (counter == random)
                                {
                                    _row = c_row;
                                    _col = i;
                                    break;
                                }
                            }
                        }
                    }
                    // check the column containing the sign of Computer, if other two fields are empty
                    // then move randomly to one of them
                    else if (are_other_two_fields_of_column_empty(c_row, c_col))
                    {
                        random = rnd.Next(2);
                        int counter = -1;
                        for (int i = 0; i < 3; i++)
                        {
                            if (m[i, c_col] == 2)
                            {
                                counter++;
                                if (counter == random)
                                {
                                    _row = i;
                                    _col = c_col;
                                    break;
                                }
                            }
                        }
                    }
                    // if the sign of Computer lies on the primary or secondary diagonal, check, if other two fields are empty
                    // then move randomly to one of them
                    else
                    {
                        if (lies_on_primary_diagonal(c_row, c_col) == true)
                        {
                            random = rnd.Next(2);
                            int counter = -1;
                            for (int i = 0; i < 3; i++)
                            {
                                if (m[i, i] == 2)
                                {
                                    counter++;
                                    if (counter == random)
                                    {
                                        _row = i;
                                        _col = i;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (lies_on_secondary_diagonal(c_row, c_col))
                        {
                            random = rnd.Next(2);
                            int counter = -1;
                            for (int i = 0; i < 3; i++)
                            {
                                if (m[i, 2 - i] == 2)
                                {
                                    counter++;
                                    if (counter == random)
                                    {
                                        _row = i;
                                        _col = 2 - i;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }
                // 3d and later moves.  If there exists a line containing two sings of Computer and a free third field,
                //           it goes there to win; otherwise it acts as in section II, 2nd move.
                //           Since Computer moved first, it can make maximum 5 moves before the game finishes.
                //           The 5th move is not an issue, since it occurs to the last empty field. There can be from 2 to 4
                //           Computer signs on the playground.
                else if (count_empty_fields() <= 5)
                {
                    Computer_point_list.Clear();
                    Computer_point_list = fields_occupied_by(1); // get List of fields occupied by Computer (1)

                    // checking all possible point pairs in the Computer_point_list, if any two of them are on the same row, col or diagonal and,
                    // if yes, then if the third point on that line is empty, save its coordinates to p
                    // otherwise return (-1, -1)
                    Point p = first_empty_field_on_any_line_between_any_fields_in_list(Computer_point_list);
                    
                    if (p.Row != -1 && p.Col != -1) // If there exists a line containing two sings of Computer and a free third field,
                                                    // Computer moves there to win
                    {
                        _row = p.Row;
                        _col = p.Col;
                    }
                    else // otherwise Computer acts as in section II, 2nd move.
                        general_medium_strategy_C2nd_2move(out _row, out _col);
                }

            }
            // II. If Computer moves 2nd.
            else if (m.Goes_First == Model_Helper.Players.Player)
            {
                // 1st move. It randomly moves to a free field
                if (count_empty_fields() == 8)
                {
                    computers_move_simple(out _row, out _col);
                }
                // 2nd move. It moves to the remaining free field on a 3-field-line (columns, rows, diagonals) occupied by Player,
                //           if applicable; if the signs of Player are not located on a common line (which is possible for maximum of
                //           3 signs), it identifies all non-occupied intersections
                //           of the 3-field-lines containing signs of Player and moves randomly to one of them;
                //           if all intersections are occupied, it moves to a free field on a line containing its own sign and not occupied
                //           by Player; if it is not possible, it moves to any free field.
                if (count_empty_fields() <= 6)
                {
                    general_medium_strategy_C2nd_2move(out _row, out _col);
                }
            }
        }
        #endregion Medium
        #region Hard
        // I. If Computer moves first.
        // 1st move. It moves randomly to a corner or the center.
        // 2nd move. a) If the 1st move was to a corner, and Player moved to the opposite corner, Computer moves to one of the empty corners
        //              and at the 3d move to the last free corner.
        //           b) Else if the 1st move was to a corner, and Player moved to an adjacent corner, Computer moves to the edge middle field 
        //              next to Computer sign from the 1st move and not on the same line with Player's sign.
        //              The 3d move of the Computer is to the center.
        //           c) Else if the 1st move was to a corner, and Player moved to the center, Computer moves to the opposite corner.
        //              The 3d move: if Player moves to a corner, Computer moves to the last free corner;
        //              If Player moves to an edge middle, Computer moves to the corner at the far side from Player's sign.
        //           d) If the 1st move was to the center
        private void computers_move_hard(out int _row, out int _col)
        {
            _row = -1;
            _col = -1;

        }
        #endregion Hard
    }


}
