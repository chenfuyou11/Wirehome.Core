using System;
using System.Reflection;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Console = Colorful.Console;
using System.Drawing;

namespace HA4Iot.Movement.Test
{
    public class TestRunner
    {
        static void Main(string[] args)
        {
            Console.WriteLine("------------------------------------------- BEGIN -----------------------------------------------------", Color.Orange);
            Console.WriteLine("");

            RunTestsForProject();

            Console.WriteLine("-------------------------------------------- END ------------------------------------------------------", Color.Orange);
            Console.Read();

        }

        public static void RunTestsForProject()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            foreach (Type testType in myAssembly.GetTypes().Where(x => x.GetTypeInfo().CustomAttributes.Any(y => y.AttributeType == typeof(TestClassAttribute))))
            {
                var instance = Activator.CreateInstance(testType);

                foreach (var method in testType.GetMethods().Where(x => x.GetCustomAttributes<TestMethodAttribute>().Any()))
                {
                    RunTestMethos(instance, method);
                }
            }
        }

        private static void RunTestMethos(object instance, MethodInfo method)
        {
            var timer = new Stopwatch();
            Exception testException = null;

            try
            {
                Console.WriteLine($"[START] {method.Name}", Color.Green);
                timer.Start();
                method.Invoke(instance, null);
                timer.Stop();

            }
            catch (Exception e)
            {
                timer.Stop();
                testException = e;
            }
            finally
            {
                var time = $"{timer.Elapsed.Seconds}:{timer.Elapsed.Milliseconds}";

                if (testException == null)
                {
                    Console.WriteLine($"[{time}] {method.Name}: OK", Color.Green);
                }
                else
                {
                    Console.WriteLine($"[{time}] {method.Name} : EXCEPTION: {testException.GetInnerMostException().Message}", Color.Red);
                }

                Console.WriteLine("");
            }
        }

    }

    public static class ExceptionHelper
    {
        public static Exception GetInnerMostException(this Exception e)
        {
            if (e == null)
                return null;

            while (e.InnerException != null)
                e = e.InnerException;

            return e;
        }
    }
}
