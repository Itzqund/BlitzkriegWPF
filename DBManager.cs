using System;
using System.Collections.Generic;
using System.Text;

namespace BlitzkriegWPF
{
    static class DBManager
    {
        static string connectionString = "";
        public static Blitzkrieg GetBlitzkriegs(int completionId)
        {
            // запрос из бд
            string[] 
                runs = null,
                notes = null;
            bool[] checks = null;
            int[] attempts = null;

            return new Blitzkrieg(runs, checks, attempts, notes);
        }
        
        public static void AddBlitzkeig(Blitzkrieg blitzkrieg)
        {
            // запрос на добавление в бд
            // обновление таблиц на форме, что-то типа MainWindow.RefreshTables()
        }

        public static void ChangeBlitzkeig(Blitzkrieg blitzkrieg)
        {
            // запрос на изменение в бд
            // обновление таблиц на форме, что-то типа MainWindow.RefreshTables()
        }

        public static void DeleteBlitzkeig(int completionId)
        {
            // запрос на удаление в бд
            // обновление таблиц на форме, что-то типа MainWindow.RefreshTables()
        }

        public static void LogAction(string logMessage, int idLogType)
        {

        }
    }
}