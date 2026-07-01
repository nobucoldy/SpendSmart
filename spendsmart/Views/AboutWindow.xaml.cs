using System.Collections.ObjectModel;
using System.Windows;

namespace spendsmart.Views;

public partial class AboutWindow : Window
{
    public ObservableCollection<SinhVien> DanhSachSinhVien { get; } = new();

    public AboutWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        DanhSachSinhVien.Clear();
        DanhSachSinhVien.Add(new SinhVien { MaSV = "23010574", Ten = "Nguyễn Quế Bắc", Email = "23010574@st.phenikaa-uni.edu.vn" });
        DanhSachSinhVien.Add(new SinhVien { MaSV = "23010147", Ten = "Nguyễn Thị Quỳnh Anh", Email = "23010147@st.phenikaa-uni.edu.vn" });
        DanhSachSinhVien.Add(new SinhVien { MaSV = "22010188", Ten = "Lê Việt Anh", Email = "22010188@st.phenikaa-uni.edu.vn" });

        dgMembers.ItemsSource = DanhSachSinhVien;
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        myMedia.Play();
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        myMedia.Pause();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        myMedia.Stop();
    }
}

public sealed class SinhVien
{
    public string MaSV { get; set; } = string.Empty;

    public string Ten { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
