/*
 * TurnOffDisplay
 * 
 * Copyright (C) 2014 Diogo Soares
 * 
 * This file is part of TurnOffDisplay.
 * 
 * TurnOffDisplay is free software: you can redistribute it and/or modify it under the terms of the
 * GNU General Public License as published by the Free Software Foundation, either version 2 of the
 * License, or (at your option) any later version.
 * 
 * TurnOffDisplay is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * 
 * See the GNU General Public License for more details. You should have received a copy of the GNU
 * General Public License along with PhotoFrame. If not, see <http://www.gnu.org/licenses/>.
 * 
 * Authors: Diogo Soares
 */

using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.IO;
using System.Management;

namespace TurnOffDisplay
{
    class Program
    {
        private const int HWND_BROADCAST = 0xFFFF;
        private const int SC_MONITORPOWER = 0xF170;
        private const int WM_SYSCOMMAND = 0x112;

        private const int MONITOR_ON = -1;
        private const int MONITOR_OFF = 2;
        private const int MONITOR_STANBY = 1;

        private const int COUNT_CLEAR_DISPLAY = 20;

        private static int m_CurrentMonitorStatus = int.MaxValue;

        private static bool m_IsValuesSet = false;
        private static DateTime m_TurnOn = Convert.ToDateTime("07:00:00 AM");
        private static DateTime m_TurnOff = Convert.ToDateTime("08:00:00 PM");
        private static int m_waiting = 5;
        private static bool m_minimizeAllWindows = true;
        private static int m_countToClear = 0;
        private static String fileName = "Settings.ini";

        [DllImport("user32.dll")]
        static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        static void Main(string[] args)
        {
            while (true)
            {
                ReadIniFile();

                DateTime DateNow = Convert.ToDateTime(DateTime.Now);

                bool on = m_TurnOn <= DateNow;
                bool off = DateNow <= m_TurnOff;

                if (m_TurnOn <= DateNow && DateNow <= m_TurnOff)
                {
                    ChangeMonitorStatus(MONITOR_ON);
                }
                else
                {
                    ChangeMonitorStatus(MONITOR_OFF);
                }

                if (m_countToClear == COUNT_CLEAR_DISPLAY)
                {
                    m_countToClear = 0;
                    Console.Clear();
                }
                else
                {
                    m_countToClear++;
                }

                if (m_minimizeAllWindows)
                {
                    Shell32.Shell objShel = new Shell32.Shell();

                    //// Show the desktop
                    ((Shell32.IShellDispatch4)objShel).ToggleDesktop();
                    //// Restore the desktop
                    //((Shell32.IShellDispatch4)objShel).ToggleDesktop();

                    objShel = null;
                }

                Thread.Sleep(m_waiting * 60 * 1000);
            }

            //Console.WriteLine("----------------------");
            //WriteMessage("Press Enter to close", 1);
            //Console.ReadLine();
        }

        static void ReadIniFile()
        {
            String path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            String file = path + "\\" + fileName;

            if (File.Exists(file))
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(file);

                    DateTime TurnOn = Convert.ToDateTime(sr.ReadLine());
                    DateTime TurnOff = Convert.ToDateTime(sr.ReadLine());
                    int Waiting = Int16.Parse(sr.ReadLine());

                    if (TurnOn > TurnOff)
                    {
                        TurnOff = TurnOff.AddDays(1);
                    }

                    if (m_TurnOn != TurnOn | m_TurnOff != TurnOff | m_waiting != Waiting)
                    {
                        m_TurnOn = TurnOn;
                        m_TurnOff = TurnOff;
                        m_waiting = Waiting;
                    }

                    m_IsValuesSet = true;
                }
                catch
                {
                    WriteMessage("Invalid file.");
                    m_IsValuesSet = false;
                }
                finally
                {
                    if (sr != null)
                        sr.Close();
                }
            }
            else
            {
                if (!m_IsValuesSet)
                {
                    WriteMessage("Time.ini not found.");
                    WriteMessage("Default values loaded.");
                }
            }
        }

        static void ChangeMonitorStatus(int monitorStatus)
        {
            if (monitorStatus != m_CurrentMonitorStatus)
            {
                m_CurrentMonitorStatus = monitorStatus;

                if (m_CurrentMonitorStatus == MONITOR_ON)
                {
                    WriteMessage("Monitor turned On");
                }
                else if (m_CurrentMonitorStatus == MONITOR_OFF)
                {
                    WriteMessage("Monitor turned Off");
                }
                else if (m_CurrentMonitorStatus == MONITOR_STANBY)
                {
                    WriteMessage("Monitor turned StandBy");
                }
                else
                {
                    WriteMessage("Error");
                }

                SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, m_CurrentMonitorStatus);
            }
        }

        static void WriteMessage(String message, int numWhiteLine = 0)
        {
            for (int i = 0; i < numWhiteLine; i++)
            {
                Console.WriteLine();
            }

            Console.WriteLine(DateTime.Now.ToString() + ": " + message);
        }
    }
}
