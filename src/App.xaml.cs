using System;
using System.Windows;

namespace HashGenerator {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    private void OnAppLoad(object sender, StartupEventArgs e) {
      try {
        MainWindow wnd = new MainWindow();
        if (e.Args.Length == 1)
          wnd.Source = e.Args[0];
        else if (e.Args.Length > 1) {
          wnd.Source = e.Args[0];
          wnd.Type = e.Args[1];
        }
        wnd.Show();
        wnd.InitArgs();
      } catch (Exception ex) {
        MessageBox.Show(ex.Message, "Error Occurred!", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}
