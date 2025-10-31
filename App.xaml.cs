using System;
using System.Threading;
using System.Windows;

namespace ThreeInstanceApp
{
    public partial class App : Application
    {
        private static Semaphore semaphore;
        private const string SemaphoreName = "Global\\ThreeInstanceApp_Semaphore";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool createdNew;
            semaphore = new Semaphore(3, 3, SemaphoreName, out createdNew);

            if (!semaphore.WaitOne(0))
            {
                MessageBox.Show(
                    "Вже запущено 3 копії програми.\nЗакрий одну з них, щоб відкрити нову.",
                    "Обмеження запуску",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                Shutdown();
                return;
            }

            Exit += (s, args) =>
            {
                semaphore.Release();
                semaphore.Dispose();
            };
        }
    }
}