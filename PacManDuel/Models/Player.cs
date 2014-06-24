using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using PacManDuel.Exceptions;

namespace PacManDuel.Models
{
    public class Player
    {
        private readonly String _playerName;
        private readonly String _workingPath;
        private readonly String _executableFileName;
        private int _score;
        private int _numberOfPoisonPills;
        private Point _currentPosition;
        private char _symbol;

        public Player(String playerName, String workingPath, String executableFileName, char symbol)
        {
            _playerName = playerName;
            _workingPath = workingPath;
            _executableFileName = executableFileName;
            _score = 0;
            _numberOfPoisonPills = 1;
            _symbol = symbol;
        }

        public Player(Player player) : this(player._playerName, player._workingPath, player._executableFileName, player._symbol)
        {
            _score = player._score;
            _numberOfPoisonPills = player._numberOfPoisonPills;
            _currentPosition = player._currentPosition;
        }

        public Maze GetMove(Maze maze, String outputFilePath, StreamWriter logFile)
        {
            var playerOutputFilePath = _workingPath + System.IO.Path.DirectorySeparatorChar + Properties.Settings.Default.SettingBotOutputFileName;
            File.Delete(playerOutputFilePath);
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = _workingPath,
                    FileName = _workingPath + System.IO.Path.DirectorySeparatorChar + _executableFileName,
                    Arguments = "\"" + outputFilePath + "\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            System.Diagnostics.DataReceivedEventHandler h = (sender, args) => {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(_workingPath + System.IO.Path.DirectorySeparatorChar + "botlogs_capture.txt", true))
                {
                    if (!String.IsNullOrEmpty(args.Data)) 
                        file.WriteLine(args.Data);
                }
            };
            p.OutputDataReceived  += h;
            p.ErrorDataReceived  += h;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            bool didExit = p.WaitForExit(Properties.Settings.Default.SettingBotOutputTimeoutSeconds * 1000);
            if (!didExit)
                p.Kill();

            if (!File.Exists(playerOutputFilePath)) 
            {
                logFile.WriteLine("[GAME] : Timeout from player " + _playerName);
                return null;
            }
            try
            {
                var mazeFromPlayer = new Maze(playerOutputFilePath);
                return mazeFromPlayer;
            }
            catch (UnreadableMazeException e)
            {
                Console.WriteLine(e.ToString());
                logFile.WriteLine("[GAME] : Unreadable maze from player: " + _playerName);
            }
            return null;
        }

        public void AddToScore(int score)
        {
            _score += score;
        }

        public int GetScore()
        {
            return _score;
        }

        public bool IsAllowedPoisonPillDrop()
        {
            return _numberOfPoisonPills > 0;
        }

        public void UsePoisonPill()
        {
            _numberOfPoisonPills--;
        }

        public String GetPlayerName()
        {
            return _playerName;
        }

        public String GetPlayerPath()
        {
            return _workingPath;
        }

        public Point GetCurrentPosition()
        {
            return _currentPosition;
        }

        public void SetCurrentPosition(Point coordinate)
        {
            _currentPosition = coordinate;
        }

        public char GetSymbol()
        {
            return _symbol;
        }

    }
}
