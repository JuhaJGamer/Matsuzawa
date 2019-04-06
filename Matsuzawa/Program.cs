using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Matsuzawa
{
    class Program
    {
        // static readonly = const. at least I can't see a difference. Except const wont accept { "","" } for some reason.
        /// <summary>
        /// menu[num], the string list for the menu options.
        /// </summary>
        static readonly string[] menu1 = { "3-numeral", "9-numeral", "9-numeral timed at 0.5 s", "Advanced", "Quit" }; // Menu string for the main menu
        static readonly string[] menu2 = { "Numeral count: {0}", "Timing: {0}" }; // Menu string
        /// <summary>
        /// xxColor, colors for the application
        /// </summary>
        const ConsoleColor bgColor = ConsoleColor.DarkBlue;
        const ConsoleColor fgColor = ConsoleColor.White;
        const ConsoleColor erColor = ConsoleColor.Black;
        /// <summary>
        /// Version numbering
        /// </summary>
        const int version = 1;
        const int subversion = 1;

        /// <summary>
        /// The main function. Starts the program and contains the main logic "path" of the program
        /// </summary>
        /// <param name="args">The system cmd arguments</param>
        static void Main(string[] args)
        {
            Console.Title = "Matsuzawa"; // Set window title (for taskmanager and stuff)
            while (true)
            {
                try // Just for catching exceptions
                {
                    int option = 0;
                    switch (option = Menu())
                    {
                        case 0: //3-numeral
                            NumTest(3, 0); // Start 3-numeral untimed test
                            break;
                        case 1:
                            NumTest(9, 0); // Start 9-numereal untimed test
                            break;
                        case 2:
                            NumTest(9, 0.5f); // Start 9-numeral timed test
                            break;
                        case 3:
                            CustomTest();
                            break;
                        case 4:
                            return;
                        default: // Something's REALLY wrong. Probably a rare CPU or RAM error
                            ErrorMessage("ERROR: Unexpected value", "Menu() returned a bad value\n\nExtra info:\nValue: " + option.ToString()); // Show error message
                            return;
                    }
                }
                catch (Exception e) // catch exceptions
                {
                    string[] words = e.StackTrace.Split(' '); // Split error message into words
                    string output = ""; // Assign clear output and line buffers
                    string line = ""; // Ditto

                    foreach (string word in words) // For each word
                    {
                        if ((line + word).Length > Console.WindowWidth - 2) // Check if it fits on screen
                        {
                            output += line + "\n"; // If not, append the line,
                            line = ""; // And clear the line buffer
                        }

                        line += word + " "; // Add the word and a space to the line
                    }

                    ErrorMessage("ERROR: " + e.GetType().ToString(), output); // Error message
                }
            }
        }

        /// <summary>
        /// A function to show a test customization menu, and then initiate a test woth the chosen parameters.
        /// </summary>
        private static void CustomTest()
        {
            int option = 0;
            int numerals = 3;
            float seconds = 0;

            while(true)
            {
                SetupBackground("Matsuzawa - Advanced Test");
                for(int i = 0; i < menu2.Length; i++)
                {
                    MenuChosenColor(option, i);
                    Console.SetCursorPosition(3, 3 + i);
                    Console.Write(String.Format(menu2[i], i == 0 ? numerals : seconds));

                    // Console interation
                    ConsoleKey key = Console.ReadKey().Key; // Read key
                    if (key == ConsoleKey.UpArrow) // If up arrow
                    {
                        if (option != 0) // Unless option is the first one
                        {
                            option = (option - 1); // Go up
                        }
                    }
                    else if (key == ConsoleKey.DownArrow)
                    {
                        if (option != menu2.Length - 1) // Unless option is the last one
                        {
                            option = (option + 1); // Go down
                        }
                    }
                    else if (key == ConsoleKey.Enter) // If enter
                    {
                        break; // Break
                    }
                    else if (key == ConsoleKey.RightArrow) // If right arrow
                    {
                        if(option == 0)
                        {
                            numerals = (numerals % 9) + 1; // Increment numerals (while staying in bounds 1-9).
                            // Quick explanation:
                            // % is the modulo operator
                            // 1 mod 9 is 1. 1 + 1 is 2. It incremented by one. same goes for all numbers 1-8
                            // 9 mod 9 is 0. 0+1 is one. It loops back around to 1 if the option ias already nine.
                        }
                        else
                        {
                            seconds += 0.1f; // add 0.1f to the value
                        }
                    }
                    else if (key == ConsoleKey.LeftArrow) // If left arrow
                    {
                        if (option == 0)
                        {
                            numerals = (numerals + 8) % 9; // Increment numerals (while staying in bounds 1-9).
                            // Quick explanation:
                            // % is the modulo operator
                            // 1 mod 9 is 1. 1 + 1 is 2. It incremented by one. same goes for all numbers 1-8
                            // 9 mod 9 is 0. 0+1 is one. It loops back around to 1 if the option ias already nine.
                            numerals = numerals == 0 ? 9 : numerals; //Loop around
                        }
                        else
                        {
                            seconds += 0.1f; // add 0.1f to the value
                        }
                    }
                }
            }
            throw new NotImplementedException();
        }

         /// <summary>
         /// Start the number-box test with specified numerals and time
         /// </summary>
         /// <param name="numerals">The amount of numerals used in the test</param>
         /// <param name="seconds">The memorization time in seconds. 0 for until user decides they are ready</param>
        static void NumTest(int numerals, float seconds)
        {
            /*
             * Pre-part explanation:
             * When setting the startx and starty, the other side of the calculation looks like ((x*2)+2), 
             * where x is either 8 or 6.
             * This is made up of x (8 or 6), which is the grid size, times two to get double the size so
             * they can be spaced 1 block apart, plus one to add a free column,
             * and two to account for the window borders. 
             */
            int[,] grid = FillGrid(numerals); // Generate grid with 9 numerals
            SetupBackground(string.Format("Matsuzawa - {0}-numeral {1}timed", numerals, seconds == 0 ? "un" : (seconds.ToString()) + "-")); // Setup blank background
            int startx = (Console.WindowWidth / 2) - ((8 * 2) + 1 + 2) / 2; // Set window start x as a centered one for a 8 by 6 grid
            int starty = (Console.WindowHeight / 2) - ((6 * 2) + 1 + 2) / 2; // Set window start y as a centered one for a 8 by 6 grid
            ShowWindow(startx, starty, (8 * 2) + 1 + 2, (6 * 2) + 1 + 2); // Print that window

            Stopwatch watch = new Stopwatch(); // Timing stopwatch
            TimeSpan midMs; // Middle time

            // Print grid
            for (int i = 0; i < 8; i++) // Every column starting at
            {
                for (int j = 0; j < 6; j++) // Every row startign at
                {
                    Console.SetCursorPosition(startx + 2 + i * 2, starty + 2 + j * 2); // Set cursor position
                    Console.Write(grid[i, j] == 0 ? " " : grid[i, j].ToString()); // Print either nothing or the numeral
                }
            }

            watch.Start(); // Start watch

            if (seconds == 0.0f) // If seconds aren't set
            {
                Console.ReadKey(); // Use user input
            }
            else
            {
                Thread.Sleep((int)(seconds * 1000)); // Otherwise, sleep the required amount (in ms, so 1000 times the original seconds)
            }

            midMs = watch.Elapsed; // Take the midtime (only used with user inout, but taken here)

            int optionx = 0; // Selected option coordinates
            int optiony = 0; // Ditto
            int found = 0;
            int notDone = 0; // Is the game still going? (1) And has it encountered an error? (-1)

            while (notDone == 0) // Until completed or failed (break)
            {
                for (int i = 0; i < 8; i++) // Every column
                {
                    for (int j = 0; j < 6; j++) // Every row
                    {
                        if (optionx == i && optiony == j) // If selected
                        {
                            Console.ForegroundColor = bgColor; // Set correct colors
                            Console.BackgroundColor = fgColor; // Ditto
                        }
                        else // If not
                        {
                            Console.BackgroundColor = bgColor; // Set the other colors
                            Console.ForegroundColor = fgColor; // Ditto
                        }

                        Console.SetCursorPosition(startx + 2 + i * 2, starty + 2 + j * 2); // Cursor pos just like in the other one
                        Console.Write(grid[i, j] <= found && grid[i, j] != 0 ? grid[i, j].ToString() : "X"); // Print it, if it's not empty or not found
                    }
                }

                switch (Console.ReadKey().Key) // Read key
                {
                    case ConsoleKey.UpArrow: // If up arrow
                        optiony = optiony == 0 ? 0 : optiony - 1; // Move up if possible
                        break;
                    case ConsoleKey.DownArrow: // If down arrow
                        optiony = optiony == 5 ? 5 : optiony + 1; // Move down if possible
                        break;
                    case ConsoleKey.LeftArrow: // If left arrow
                        optionx = optionx == 0 ? 0 : optionx - 1; // Move left if possible
                        break;
                    case ConsoleKey.RightArrow: // If right arrow
                        optionx = optionx == 7 ? 7 : optionx + 1; // Move right if possible
                        break;
                    case ConsoleKey.Enter: // If enter
                        if (grid[optionx, optiony] == found + 1) // Check if it's the correct one
                        {
                            found++; // Increment found counter
                            if (found == numerals) // If everythign was found
                            {
                                notDone = 1; // Success
                            }
                        }
                        else // If it's a miss
                        {
                            notDone = -1; // Set flag to failure
                        }
                        break;
                    default:
                        break;
                }
            }
            watch.Stop(); // Stop watch
            SetupBackground(string.Format("Matsuzawa - {0}-numeral {1}timed | FINISHED", numerals, seconds == 0 ? "un" : (seconds.ToString()) + "-")); // Setup blank background

            // Check for which message should be printed
            if (notDone == -1)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - "FAILED".Length / 2, Console.WindowHeight / 2 - 2); // Center message
                Console.Write("FAILED"); // Print message
            }
            else
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - "SUCCEEDED".Length / 2, Console.WindowHeight / 2 - 2); // Ditto
                Console.Write("SUCCEEDED"); // Ditto
            }

            // Check for which viewing time should be shown
            if (seconds == 0.0f)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - ("Viewing time: " + (midMs.TotalSeconds * 1.0).ToString() + " s").Length / 2, Console.WindowHeight / 2); // See above
                Console.Write("Viewing time: " + (midMs.TotalSeconds * 1.0).ToString() + " s");
            }
            else
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - ("Viewing time: " + seconds.ToString()).Length / 2, Console.WindowHeight / 2); // See above
                Console.Write("Viewing time: " + seconds.ToString() + " s");
            }

            // Print playing time (total time - viewing time)
            Console.SetCursorPosition(Console.WindowWidth / 2 - ("Playing time: " + ((watch.Elapsed - midMs).TotalSeconds * 1.0).ToString() + " s").Length / 2, Console.WindowHeight / 2 + 1); // See above
            Console.Write("Playing time: " + (watch.Elapsed - midMs).TotalSeconds.ToString() + " s");

            // Print total time, as seen above
            Console.SetCursorPosition(Console.WindowWidth / 2 - ("Total time: " + (watch.Elapsed.TotalSeconds * 1.0).ToString() + " s").Length / 2, Console.WindowHeight / 2 + 2); // See above
            Console.Write("Total time: " + watch.Elapsed.TotalSeconds.ToString() + " s");

            Console.ReadKey(); // Wait until key is pressed to exit
        }

         /// <summary>
         /// Draw a window on the console
         /// </summary>
         /// <param name="x">Top left X coordinate</param>
         /// <param name="y">Top rleft Y coordinate</param>
         /// <param name="width">Width of the window (border included)</param>
         /// <param name="height">Height of the window (border included)</param>
        static void ShowWindow(int x, int y, int width, int height)
        {
            Console.ForegroundColor = fgColor; // Color pick
            Console.BackgroundColor = bgColor; // Color pick

            // Here we cerate the border bars with block-drawing characters

            // Vertical
            for (int i = 0; i < height; i++) // Left
            {
                Console.SetCursorPosition(x, y + i); // Set position
                Console.Write("║"); // Print character
            }
            for (int i = 0; i < height; i++) // Right
            {
                Console.SetCursorPosition(x + width - 1, y + i); // Set position
                Console.Write("║"); // Print character
            }

            // Horizontal
            Console.SetCursorPosition(x, y);
            for (int i = 0; i < width; i++) // Bottom
            {
                Console.Write("═");
            }
            Console.SetCursorPosition(x, y + height - 1);
            for (int i = 0; i < width; i++) // Bottom
            {
                Console.Write("═");
            }

            // Corners
            Console.SetCursorPosition(x, y); // Top-left
            Console.Write("╔");
            Console.SetCursorPosition(x + width - 1, y); // Top-right
            Console.Write("╗");
            Console.SetCursorPosition(x, y + height - 1); // Bottom-left
            Console.Write("╚");
            Console.SetCursorPosition(x + width - 1, y + height - 1); // Bottom-right
            Console.Write("╝");

            Console.SetCursorPosition(0, 0); // Return to 0,0
        }

         /// <summary>
         /// Shows an error message on the screen
         /// </summary>
         /// <param name="title">Title of the message</param>
         /// <param name="message">The main body of the message</param>
        static void ErrorMessage(string title, string message)
        {
            int starty = Console.WindowHeight / 2; // Set starting y value as half of screen
            starty -= 5; // add3 to that y

            // Set colours
            Console.ForegroundColor = fgColor;
            Console.BackgroundColor = erColor;
            for (int i = 0; i < 5 * 2; i++)
            {
                Console.SetCursorPosition(0, starty + i);
                for (int j = 0; j < Console.WindowWidth; j++)
                {
                    Console.Write(" ");
                }
            }
            int titlex = Console.WindowWidth / 2; // Set title's starting x to half console width
            titlex -= title.Length / 2; // Remove half of tiutle fro mthat, centering it
            Console.SetCursorPosition(titlex, starty + 0); // Set cursor to appropriate line and position
            Console.Write(title); // Print title

            string[] lines = message.Split('\n');

            int messagex = 0;
            for (int i = 0; i < Math.Min(lines.Length, 6); i++)
            {
                messagex = Console.WindowWidth / 2; // Set message's starting x to half of console width
                messagex -= lines[i].Length / 2; // Set message's starting x back by half of it's length, centering it when printed.
                Console.SetCursorPosition(messagex, starty + 2 + i);
                Console.Write(lines[i]);
            }

            Console.ReadKey(); // Wait until dismissal
            return;
        }

         /// <summary>
         /// Sets up a background with borders
         /// </summary>
         /// <param name="title">title to be placed at the top of the window</param>
        static void SetupBackground(string title = "")
        {
            Console.CursorVisible = false; // Set cursor invisible
            Console.ForegroundColor = fgColor; // Color pick
            Console.BackgroundColor = bgColor; // Color pick
            Console.Clear(); // Blank menu

            // Vertical
            for (int i = 0; i < Console.WindowHeight; i++) // Left
            {
                Console.SetCursorPosition(0, i); // Set position
                Console.Write("║"); // Print character
            }
            for (int i = 0; i < Console.WindowHeight; i++) // Right
            {
                Console.SetCursorPosition(Console.WindowWidth - 1, i); // Set position
                Console.Write("║"); // Print character
            }

            // Horizontal
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 8; i++) // First 8 of the Top
            {
                Console.Write("═");
            }
            Console.Write(title); // Print the title
            for (int i = 8 + title.Length; i < Console.WindowWidth; i++) // The rest of the top
            {
                Console.Write("═");
            }
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            for (int i = 0; i < Console.WindowWidth - 1; i++) // Bottom
            {
                Console.Write("═");
            }

            // Corners
            Console.SetCursorPosition(0, 0); // Top-left
            Console.Write("╔");
            Console.SetCursorPosition(Console.WindowWidth - 1, 0); // Top-right
            Console.Write("╗");
            Console.SetCursorPosition(0, Console.WindowHeight - 1); // Bottom-left
            Console.Write("╚");
            Console.SetCursorPosition(Console.WindowWidth - 1, Console.WindowHeight - 1); // Bottom-right
            Console.Write("╝");

            Console.SetCursorPosition(0, 0); // Return to 0,0
        }

        /// <summary>
        /// Function for choosing the color of a menu option.
        /// </summary>
        /// <param name="option">The currently selected option number</param>
        /// <param name="i">The current menu option</param>
        /// <returns>void</returns>
        static void MenuChosenColor(int option, int i)
        {
            Console.ForegroundColor = option == i ? bgColor : fgColor; // If option is selected, set foreground blue. Otherwise set foreground white.
            Console.BackgroundColor = option == i ? fgColor : bgColor; // If option isn't selected (note difference to above), set background white. Otherwise set background blue.
        }

         /// <summary>
         /// Displays a menu to the user
         /// </summary>
         /// <returns>The chosen option</returns>
        static int Menu()
        {
            while (true) // Until menu closes (every break)
            {
                int option = 0; // Option buffer

                Console.CursorVisible = false; // Set cursor invisible

                SetupBackground("Matsuzawa v"+version+"."+subversion); // Set up background

                while (true) // Until option chosen (next turn)
                {
                    for (int i = 0; i < menu1.Length; i++) // For each menu option
                    {
                        MenuChosenColor(option, i);
                        Console.SetCursorPosition(3, 3 + i); // Set position in column
                        Console.Write(menu1[i]); // Print option

                        /*
                         * ATTENTION!
                         * If the next part seems stupid, it is. 
                         * I just didn't want to spend 30 minutes fixing the little problem
                         * with the third option, that causes a white bar to appear after the
                         * 4th option.
                         */
                        Console.BackgroundColor = bgColor; // Set bg color
                        Console.SetCursorPosition(3 + 4, 3 + 4); // Set position to 5th of 4th option
                        Console.Write(" "); // Clear that one
                    }

                    ConsoleKey key = Console.ReadKey().Key; // Read key
                    if (key == ConsoleKey.UpArrow) // If up arrow
                    {
                        if (option != 0) // Unless option is the first one
                        {
                            option = (option - 1); // Go up
                        }
                    }
                    else if (key == ConsoleKey.DownArrow)
                    {
                        if (option != menu1.Length - 1) // Unless option is the last one
                        {
                            option = (option + 1); // Go down
                        }
                    }
                    else if (key == ConsoleKey.Enter) // If enter
                    {
                        break; // Break
                    }
                }

                return option; // Return the option
            }
        }

        /*
         * function: FillGrid(intNumerals)
         * Fills a grid with a specified amount of numerals
         * 
         * Parameters:
         *  - int numerals
         *      Integer value showing maximum number of numerals
         *      
         */
         /// <summary>
         /// Fills a grid with the specified amount of numerals
         /// /// </summary>
         /// <param name="numerals">Integer value showing maximum number of numerals. Range 1-9</param>
         /// <returns>The grid array</returns>
        static int[,] FillGrid(int numerals)
        {
            if(numerals > 1 || numerals < 1)
            {
                return new int[0,0];
            }
            int[,] grid = new int[8, 6]; // grid to be filled
            // Fill up
            {
                for (int i = 0; i < 8; i++) // x
                {
                    for (int j = 0; j < 6; j++) // y
                    {
                        grid[i, j] = 0; // zero it
                    }
                }
            }

            // Randomization
            {
                int counter = 1; // counter for filled-in numerals
                Random r = new Random(); // random
                int x, y; // random point storage
                while (counter < numerals + 1) // while not all numerals filled
                {
                    x = r.Next(8); // select random point
                    y = r.Next(6); // see above
                    if (grid[x, y] == 0) // if it isn't already a numeral (no collisions)
                    {
                        grid[x, y] = counter++; // set point to numeral and increment counter
                    }
                }
            }

            return grid; // return the grid
        }
    }
}
