using Microsoft.Extensions.DependencyInjection;
using PhoneBook.ViewModels;
using PhoneBookWpf.Data;
using PhoneBookWpf.Services;
using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace PhoneBookWpf
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 1. Регистрация контекста базы данных
            // ОБЯЗАТЕЛЬНО ПРОВЕРЬТЕ, что имя сервера и базы данных здесь ваши!
            string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=PhoneBookDB_grohotovde_2307g;Integrated Security=True;TrustServerCertificate=True";
            services.AddDbContext<PhoneBookContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Регистрация сервисов и ViewModel
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<MainViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 3. Создание и отображение главного окна
            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
    }
}