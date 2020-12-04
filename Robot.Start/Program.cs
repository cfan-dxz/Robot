using Robot.Driver;
using System;

namespace Robot.Start
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Begin");

            //new AmzAutoTest().Do();
            new AmzAutoTest().TimingJob();

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
