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
            
            string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=PhoneBookDB_grohotovde_2307g;Integrated Security=True;TrustServerCertificate=True";
            services.AddDbContext<PhoneBookContext>(options =>
                options.UseSqlServer(connectionString));

           
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<MainViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
    }
}