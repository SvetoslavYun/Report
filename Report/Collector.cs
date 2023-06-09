﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Protocols;

namespace Report
{
    public class Collector
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Gun { get; set; }
        public String Automaton_serial { get; set; }
        public String Automaton { get; set; }
        public String Permission { get; set; }
        public String Meaning { get; set; }
        public String Certificate { get; set; }
        public String Token { get; set; }
        public String Power { get; set; }



        static SQLiteConnection connection;

        public Collector()
        {
            // Получение строки подключения из файла конфигурации
            var connString = ConfigurationManager.ConnectionStrings["DemoConnection"].ConnectionString;
            // Создание объекта подключения
            connection = new SQLiteConnection(connString);
        }

        static Collector()
        {
            // Получение строки подключения из файла конфигурации
            var connString = ConfigurationManager.ConnectionStrings["DemoConnection"].ConnectionString;
            // Создание объекта подключения
            connection = new SQLiteConnection(connString);
        }
        public static IEnumerable<Collector> GetAllCollector()
        {
            var commandString = "SELECT * FROM Collectors";
            SQLiteCommand getAllCommand = new SQLiteCommand(commandString, connection);
            connection.Open();
            var reader = getAllCommand.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var id= reader.GetInt32(0);
                    var name = reader.GetString(1);
                    var gun = reader.GetString(2);
                    var automaton_serial = reader.GetString(3);
                    var automaton = reader.GetString(4);
                    var permission = reader.GetString(5);
                    var meaning = reader.GetString(6);
                    var certificate = reader.GetString(7);
                    var token = reader.GetString(8);
                    var power = reader.GetString(9);
                    var collector = new Collector
                    {
                        Id= id,
                        Name = name,
                        Gun= gun,
                        Automaton_serial=automaton_serial,
                        Automaton=automaton,
                        Permission= permission,
                        Meaning=meaning,
                        Certificate=certificate,
                        Token=token,
                        Power=power                        
                    };
                    yield return collector;
                }
            };
            connection.Close();
        }

        public void Insert()
        {
            var commandString = "INSERT INTO Collectors (Name, Gun, Automaton_serial, Automaton, Permission, Meaning, Certificate, Token, Power)" + "VALUES (@name, @gun, @automaton_serial, @automaton, @permission, @meaning, @certificate, @token, @power)";
            SQLiteCommand insertCommand = new SQLiteCommand(commandString, connection);
            insertCommand.Parameters.AddRange(new SQLiteParameter[] {
               new SQLiteParameter("name", Name),
               new SQLiteParameter("gun", Gun),
               new SQLiteParameter("automaton_serial", Automaton_serial),
               new SQLiteParameter("automaton", Automaton),
               new SQLiteParameter("permission", Permission),
               new SQLiteParameter("meaning", Meaning),
               new SQLiteParameter("certificate", Certificate),
               new SQLiteParameter("token", Token),
               new SQLiteParameter("power", Power)
        });

            connection.Open();
            insertCommand.ExecuteNonQuery();
            connection.Close();
        }

        public void Update()
        {
            var commandString = "UPDATE Collectors SET Name=@name, Gun=@gun, Automaton_serial=@automaton_serial, Automaton=@automaton, Permission=@permission, Meaning=@meaning, Certificate=@certificate, Token=@token, Power=@power WHERE(Id = @id)";
            SQLiteCommand updateCommand = new SQLiteCommand(commandString, connection);
            updateCommand.Parameters.AddRange(new SQLiteParameter[] {
           new SQLiteParameter("name", Name),
           new SQLiteParameter("gun", Gun),
           new SQLiteParameter("automaton_serial", Automaton_serial),
           new SQLiteParameter("automaton", Automaton),
           new SQLiteParameter("permission", Permission),
           new SQLiteParameter("meaning", Meaning),
           new SQLiteParameter("certificate", Certificate),
           new SQLiteParameter("token", Token),
           new SQLiteParameter("power", Power),
           new SQLiteParameter("id", Id)
    });
            connection.Open();
            updateCommand.ExecuteNonQuery();
            connection.Close();
        }

    }
}



