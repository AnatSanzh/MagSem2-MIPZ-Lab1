using System;

namespace MagSem2_MIPZ_Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputSets = InputOutputUtils.ParseInput(Console.In);

            var alghorithmResults = inputSets.ConvertAll(inputSet => Alghorithms.SimulateEurodiffusion(inputSet));

            Console.Out.WriteLine();
            InputOutputUtils.OutputResults(Console.Out, inputSets, alghorithmResults);
        }
    }
}
