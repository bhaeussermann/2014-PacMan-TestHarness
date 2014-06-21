using System;
using System.Drawing;
using PacManDuel.Models;
using PacManDuel.Shared;
using PacManDuel.Helpers;

namespace PacManPerftGenerator
{
    class MainClass
    {
        private const int MazeWidth = 19;
        private static readonly Size[] MoveDirections = new Size[] { new Size(-1, 0), new Size(1, 0), new Size(0, -1), new Size(0, 1) };
        private static readonly bool[] DropPoisonPillValues = new bool[] { false, true };

        public static void Main (string[] args)
        {
            Player playerA = new Player("botA", string.Empty, string.Empty, 'A'), playerB = new Player("botB", string.Empty, string.Empty, 'B');
            Maze maze = new Maze();
            playerA.SetCurrentPosition(maze.FindCoordinateOf(Symbols.SYMBOL_PLAYER_A));
            playerB.SetCurrentPosition(maze.FindCoordinateOf(Symbols.SYMBOL_PLAYER_B));
            for (int i=1; i<=20; i++)
                Console.WriteLine(i+": "+Perft(maze, playerA, playerB, i));
        }

        private static long Perft(Maze currentMaze, Player currentPlayer, Player nextPlayer, int depth)
        {
            if (depth == 0) 
                return 1;

            long count = 0;
            foreach (Size moveDirection in MoveDirections)
                foreach (bool dropPoisonPill in DropPoisonPillValues)
                {
                    Point move = currentPlayer.GetCurrentPosition() + moveDirection;
                    // Y is used for the column (opposed to X as by convention); confusing :/....
                    if (move.Y < 0)
                        move.Y += MazeWidth;
                    else if (move.Y >= MazeWidth)
                        move.Y -= MazeWidth;

                    Maze updatedMaze = new Maze(currentMaze);
                    Player updatedCurrentPlayer = new Player(currentPlayer);
                    Player updatedNextPlayer = new Player(nextPlayer);

                    // Move current player and drop poison pill.
                    ApplyMoveToMaze(updatedMaze, currentPlayer.GetCurrentPosition(), move, dropPoisonPill);
                    // Check if valid move.
                    if (MazeValidator.ValidateMaze(updatedMaze, currentMaze) == Enums.MazeValidationOutcome.ValidMaze) 
                    {
                        // Check if valid move and update "updatedCurrentPlayer".
                        if (TurnMarshaller.ProcessMove(updatedMaze, currentMaze, move, currentPlayer.GetCurrentPosition(), updatedNextPlayer.GetCurrentPosition(), updatedCurrentPlayer) != Enums.TurnOutcome.MoveMadeAndDroppedPoisonPillIllegally) 
                        {
                            updatedCurrentPlayer.SetCurrentPosition(updatedMaze.FindCoordinateOf(Symbols.SYMBOL_PLAYER_A));
                            updatedNextPlayer.SetCurrentPosition(updatedMaze.FindCoordinateOf(Symbols.SYMBOL_PLAYER_B));
                            if (Game.RegenerateOpponentIfDead(updatedNextPlayer.GetCurrentPosition(), updatedMaze))
                                updatedNextPlayer.SetCurrentPosition(updatedMaze.FindCoordinateOf(Symbols.SYMBOL_PLAYER_B));
                            updatedMaze.SwapPlayerSymbols();

                            count += Perft(updatedMaze, updatedNextPlayer, updatedCurrentPlayer, depth - 1);
                        }
                    }
                }

            if (count == 0) 
            {
                // This is bad; log it.
                currentMaze.Print();
            }
            return count;
        }

        private static void ApplyMoveToMaze(Maze maze, Point currentPosition, Point newPosition, bool doDropPoisonPill)
        {
            maze.SetSymbol(currentPosition.X, currentPosition.Y, doDropPoisonPill ? Symbols.SYMBOL_POISON_PILL : Symbols.SYMBOL_EMPTY);
            maze.SetSymbol(newPosition.X, newPosition.Y, Symbols.SYMBOL_PLAYER_A);
        }
    }
}
