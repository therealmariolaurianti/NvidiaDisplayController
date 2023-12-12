﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WindowsDisplaySample
{
    internal class ConsoleNavigation
    {
        public static void PrintNavigation(Dictionary<object, Action> menuItems, string title, string message)
        {
            // ReSharper disable once RedundantTypeArgumentsOfMethod
            PrintObject<object>(menuItems.Keys.ToArray(), index => { menuItems[index](); }, title, message);
        }

        public static void PrintObject(object obj, string title, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine();
                Console.WriteLine(title);
            }
            WriteObject(obj);
            Console.Write(string.IsNullOrWhiteSpace(message)
                ? "Press enter to go back"
                : $"{message} (Press enter to go back)");
            Console.ReadLine();
        }

        public static void PrintObject<T>(T obj, Action action, string title, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine();
                Console.WriteLine(title);
            }
            WriteObject(obj);
            Console.Write(string.IsNullOrWhiteSpace(message)
                ? "Press enter to continue"
                : $"{message} (Press enter to continue)");
            Console.ReadLine();
            action();
        }

        public static void PrintObject<T>(T[] objects, Action<T> action, string title, string message)
        {
            while (true)
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    Console.WriteLine();
                    Console.WriteLine(title);
                }
                WriteObject(objects);
                Console.Write($"{message} (Press enter to go back): ");
                var userInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(userInput))
                    return;
                int pathIndex;
                if (int.TryParse(userInput, out pathIndex) &&
                    (pathIndex >= objects.GetLowerBound(0)) &&
                    (pathIndex <= objects.GetUpperBound(0)))
                    try
                    {
                        action(objects[pathIndex]);
                    }
                    catch (Exception ex)
                    {
                        WriteException(ex);
                    }
            }
        }

        public static void WriteException(Exception ex)
        {
            Console.WriteLine("{0} - Error: {1}", ex.GetType().Name, ex.Message);
        }

        private static void WriteObject(object obj, int padding = 0)
        {
            try
            {
                if (padding == 0)
                    Console.WriteLine(new string('_', Console.BufferWidth));
                if (obj.GetType().IsValueType || obj is string)
                {
                    Console.WriteLine(new string(' ', padding*3) + obj);
                }
                else if (obj is IEnumerable)
                {
                    var i = 0;
                    foreach (var arrayItem in (IEnumerable) obj)
                    {
                        Console.WriteLine("[{0}]: {{", i);
                        WriteObject(arrayItem, padding + 1);
                        Console.WriteLine("},");
                        i++;
                    }
                }
                else
                {
                    foreach (var propertyInfo in obj.GetType().GetProperties().OrderBy(info => info.Name))
                        if (propertyInfo.CanRead)
                        {
                            string value;
                            try
                            {
                                value = propertyInfo.GetValue(obj).ToString();
                            }
                            catch (TargetInvocationException ex)
                            {
                                value = ex.InnerException?.GetType().ToString();
                            }
                            catch (Exception ex)
                            {
                                value = ex.GetType().ToString();
                            }
                            Console.WriteLine($"{new string(' ', padding*3)}{propertyInfo.Name}: {value ?? "[NULL]"}");
                        }
                }
                if (padding == 0)
                    Console.WriteLine(new string('_', Console.BufferWidth));
            }
            catch (Exception ex)
            {
                WriteException(ex);
            }
        }
    }
}