using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace _3._4.Options.Exercise.ConsoleApp
{
    public class CalculatorOptions
    {
        public string DefaultOperation { get; set; } = "Add";
    }

    public interface ICalculatorService
    {
        double Calculate(double a, double b, string? operation = null);
    }

    public class CalculatorService : ICalculatorService
    {
        private readonly IOptionsMonitor<CalculatorOptions> _options;

        public CalculatorService(IOptionsMonitor<CalculatorOptions> options)
        {
            _options = options;

            _options.OnChange(opt =>
            {
                Console.WriteLine($"\n[OnChange] Операція за замовчуванням змінена на: {opt.DefaultOperation}");
            });
        }

        public double Calculate(double a, double b, string? operation = null)
        {
            string op = operation ?? _options.CurrentValue.DefaultOperation;

            return op.ToLower() switch
            {
                "add" or "+" => a + b,
                "subtract" or "-" => a - b,
                "multiply" or "*" => a * b,
                "divide" or "/" => b == 0 ? throw new DivideByZeroException("Ділення на нуль!") : a / b,
                _ => throw new ArgumentException($"Невідома операція: {op}")
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.Configure<CalculatorOptions>(configuration.GetSection("Calculator"));
            services.AddSingleton<ICalculatorService, CalculatorService>();

            var provider = services.BuildServiceProvider();
            var calculator = provider.GetRequiredService<ICalculatorService>();
            var options = provider.GetRequiredService<IOptionsMonitor<CalculatorOptions>>();

            Console.WriteLine("=== Options Pattern + CalculatorService ===\n");
            Console.WriteLine($"Операція за замовчуванням: {options.CurrentValue.DefaultOperation}\n");

            try
            {
                Console.WriteLine($"10 + 5 = {calculator.Calculate(10, 5)}");
                Console.WriteLine($"10 - 5 = {calculator.Calculate(10, 5, "Subtract")}");
                Console.WriteLine($"10 * 5 = {calculator.Calculate(10, 5, "Multiply")}");
                Console.WriteLine($"10 / 5 = {calculator.Calculate(10, 5, "Divide")}");

                Console.WriteLine("\nСпроба ділення на нуль:");
                Console.WriteLine($"10 / 0 = {calculator.Calculate(10, 0, "Divide")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            Console.WriteLine("\n=== Готово ===");
        }
    }
}
