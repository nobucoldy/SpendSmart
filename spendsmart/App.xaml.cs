using System.Windows;
using Microsoft.EntityFrameworkCore;
using spendsmart.Data;

namespace spendsmart
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                using var dbContext = new AppDbContext();
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Khong the cap nhat co so du lieu: {ex.Message}",
                    "SpendSmart",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
