using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HashGenerator {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      SetToolTipToControls();
      ofd.Title = "Pick file to generate hash...";
      ofd.Filter = "All Files (*.*)|*.*";
      selectedRadio = rbSHA1;
      threadWatcher.Tick += new EventHandler(threadWatcher_Tick);
      threadWatcher.Interval = new TimeSpan(1000000);
      tssActivity.Content = "Select input file to compute hash.";
      threadWatcher.Start();
    }

    /// <summary> Sets tooltip to controls </summary>
    private void SetToolTipToControls() {
      this.ToolTip = "Hash Generator - By Amresh";
      btnBrowse.ToolTip = "Browse input file.";
      txtSource.ToolTip = "Click to browser input file if not available.";
      rbMD5.ToolTip = "Check to generate MD5 hash.";
      rbSHA1.ToolTip = "Check to generate SHA-1 hash.";
      rbSHA256.ToolTip = "Check to generate SHA-256 hash.";
      rbSHA384.ToolTip = "Check to generate SHA-384 hash.";
      rbSHA512.ToolTip = "Check to generate SHA-512 hash.";
      txtHash.ToolTip = "Generated hash displayed here.";
      statusStrip.ToolTip = "Status messages displayed here.";
      btnCompare.ToolTip = "Compare generated hash with specified hash.";
      txtCompare.ToolTip = "Enter hash value to compare with generated hash.";
      btnHelp.ToolTip = "Click to display help content.";
      btnStop.ToolTip = "Click to stop current hash computation.";
    }

    /// <summary> Updates tooltip of some controls based on selected hash </summary>
    /// <param name="strHashLabelContent"> hash label content </param>
    private void UpdateToolTipToControls(string strHashLabelContent) {
      string contentToUpdate = strHashLabelContent.ToLower();
      btnCompare.ToolTip = "Compare generated hash with specified " + contentToUpdate + ".";
      txtCompare.ToolTip = "Enter " + contentToUpdate + " value to compare with generated hash.";
      txtHash.ToolTip = "Generated " + contentToUpdate + " value.";
    }

    /// <summary> Source property to set file source in startup </summary>
    public string Source {
      get { return strArg; }
      set { strArg = value; }
    }

    /// <summary> Type property to set hash type in startup </summary>
    public string Type {
      get { return strType; }
      set { strType = value; }
    }

    /// <summary> Returns type based on strType set </summary>
    /// <returns> hash type </returns>
    private void SetType() {
      strType = strType.ToLower().Trim();
      switch (strType) {
        case "md5":
          hashType = HashType.MD5;
          selectedRadio = rbMD5;
          break;
        case "sha1":
        case "sha-1":
          hashType = HashType.SHA1;
          selectedRadio = rbSHA1;
          break;
        case "sha256":
        case "sha-256":
          hashType = HashType.SHA256;
          selectedRadio = rbSHA256;
          break;
        case "sha384":
        case "sha-384":
          hashType = HashType.SHA384;
          selectedRadio = rbSHA384;
          break;
        case "sha512":
        case "sha-512":
          hashType = HashType.SHA512;
          selectedRadio = rbSHA512;
          break;
      }
      selectedRadio.IsChecked = true;
    }

    /// <summary> Event handler for form load </summary>
    public void InitArgs() {
      if (strArg.Length == 0 || strArg == strSource)
        return;

      if (!File.Exists(strArg)) {
        DisplayStatusMessage("Invalid input.", true);
        return;
      }

      strSource = strArg;
      txtSource.Text = strSource;
      hash.Reset();
      FileInfo file = new FileInfo(strSource);
      this.Title = file.Name;
      SetType();
      OnHashTypeSelection(selectedRadio, null);
    }

    /// <summary> Event handler for drag enter </summary>
    private void OnDragEnter(object sender, DragEventArgs de) {
      if (de.Data.GetDataPresent(DataFormats.Text) ||
         de.Data.GetDataPresent(DataFormats.FileDrop)) {
        de.Effects = DragDropEffects.Copy;
      } else de.Effects = DragDropEffects.None;
    }

    /// <summary> 
    /// This method is implemented to display the contents of a file after
    /// it has been dragged and dropped onto the window.
    /// </summary>
    private void OnDragDrop(object sender, DragEventArgs de) {
      try {
        if (de.Data.GetDataPresent(DataFormats.FileDrop)) {
          string[] fileNames = (string[])de.Data.GetData(DataFormats.FileDrop);
          if (fileNames.Length > 1) {
            DisplayStatusMessage("Multiple files drop not supported.", true);
            return;
          }

          if (!File.Exists(fileNames[0]) || strSource == fileNames[0])
            return;

          strSource = fileNames[0];
          txtSource.Text = strSource;
          hash.Reset();
          FileInfo file = new FileInfo(strSource);
          this.Title = file.Name;
          selectedRadio.IsChecked = true;
          OnHashTypeSelection(selectedRadio, null);
        } else
          DisplayStatusMessage("Failed to load input file.", true);
      } catch (Exception ex) {
        DisplayStatusMessage(ex.Message, true);
      }
    }

    /// <summary> returns current generated hash, get is public and set is private </summary>
    public string Hash {
      get {
        switch (hashType) {
          case HashType.MD5:
            return hash.MD5;
          case HashType.SHA1:
            return hash.SHA1;
          case HashType.SHA256:
            return hash.SHA256;
          case HashType.SHA384:
            return hash.SHA384;
          case HashType.SHA512:
            return hash.SHA512;
        }
        return "";
      }

      private set {
        switch (hashType) {
          case HashType.MD5:
            hash.MD5 = value;
            break;
          case HashType.SHA1:
            hash.SHA1 = value;
            break;
          case HashType.SHA256:
            hash.SHA256 = value;
            break;
          case HashType.SHA384:
            hash.SHA384 = value;
            break;
          case HashType.SHA512:
            hash.SHA512 = value;
            break;
        }
      }
    }

    /// <summary> Input text box click event handler, opens file selection dialog if empty </summary>
    private void OnInputClick(object sender, EventArgs e) {
      if (txtSource.Text.Length == 0)
        OnBrowseSource(null, null);
    }

    /// <summary> Click event handler for file browse button </summary>
    private void OnBrowseSource(object sender, EventArgs e) {
      Nullable<bool> result = ofd.ShowDialog();
      if (result == true) {
        if (strSource == ofd.FileName)
          return;

        strSource = ofd.FileName;
        txtSource.Text = strSource;
        hash.Reset();
        FileInfo file = new FileInfo(strSource);
        this.Title = file.Name;
        selectedRadio.IsChecked = true;
        OnHashTypeSelection(selectedRadio, null);
      }
    }

    /// <summary> Check change event handler for radios, generate hash based on selection </summary>
    private void OnHashTypeSelection(object sender, EventArgs e) {
      if (!IsLoaded)
        return;

      RadioButton rbSrc = (RadioButton)sender;
      if (!rbSrc.IsChecked == true || runStatus == RunStatus.COMPUTING)
        return;

      txtSource.Text = strSource;
      switch (rbSrc.Name) {
        case "rbMD5":
          selectedRadio = rbMD5;
          GenerateHash(HashType.MD5);
          break;
        case "rbSHA1":
          selectedRadio = rbSHA1;
          GenerateHash(HashType.SHA1);
          break;
        case "rbSHA256":
          selectedRadio = rbSHA256;
          GenerateHash(HashType.SHA256);
          break;
        case "rbSHA384":
          selectedRadio = rbSHA384;
          GenerateHash(HashType.SHA384);
          break;
        case "rbSHA512":
          selectedRadio = rbSHA512;
          GenerateHash(HashType.SHA512);
          break;
      }
    }

    /// <summary> Generates hash based on specified hash type </summary>
    /// <param name="hsType"> type of hash to generate </param>
    private void GenerateHash(HashType hsType) {
      hashType = hsType;
      txtHash.Text = "";
      strCurrentHashType = GetCurrentHashTypeString();
      lblHash.Content = strCurrentHashType + " Hash";
      UpdateToolTipToControls(lblHash.Content.ToString());
      if (!File.Exists(strSource))
        return;

      try {
        DisplayStatusMessage("Computing " + strCurrentHashType + " hash ...", false);
        hashComputer = new Thread(new ThreadStart(ComputeHash));
        hashComputer.Priority = ThreadPriority.AboveNormal;
        hashComputer.Start();
        hashComputer.Join(100);
        //this.Dispatcher.Invoke(new HashComputerDelegate(ComputeHash), DispatcherPriority.Background);
      } catch (Exception) {
        EnableControls(true);
        DisplayStatusMessage("Failed to compute hash.", true);
      }
    }

    /// <summary> Dsiplays message in status bar </summary>
    /// <param name="strMessage"> Message to display </param>
    /// <param name="isError"> displayed in red if true, else default control text color </param>
    public void DisplayStatusMessage(string strMessage, bool isError) {
      Color textColor = (isError) ? Colors.Red : Colors.Black;
      tssActivity.Foreground = (isError) ? Brushes.Red : Brushes.Black;
      tssActivity.Content = strMessage;
    }

    /// <summary> Enables and dispales control </summary>
    /// <param name="iEnable">Controls enabled if true else false </param>
    private void EnableControls(bool iEnable) {
      this.AllowDrop = iEnable;
      tssImage.Visibility = (iEnable) ? Visibility.Hidden : Visibility.Visible;
      tssImage.Width = (iEnable) ? 0 : statusStrip.Height - 4;
      btnCompare.IsEnabled = txtCompare.IsEnabled = (Hash.Length != 0) ? iEnable : false;
      gbInput.IsEnabled = iEnable;
      btnBrowse.IsEnabled = iEnable;
      btnStop.IsEnabled = (runStatus == RunStatus.COMPUTING);
    }

    /// <summary> Computes hash for input file (thread proc) </summary>
    private void ComputeHash() {
      runStatus = RunStatus.COMPUTING;
      if (Hash.Length == 0) {
        hashFile = new FileStream(strSource, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] retVal = null;
        switch (hashType) {
          case HashType.MD5:
            MD5 md5 = new MD5CryptoServiceProvider();
            retVal = md5.ComputeHash(hashFile);
            break;
          case HashType.SHA1:
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            retVal = sha1.ComputeHash(hashFile);
            break;
          case HashType.SHA256:
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            retVal = sha256.ComputeHash(hashFile);
            break;
          case HashType.SHA384:
            SHA384 sha384 = new SHA384CryptoServiceProvider();
            retVal = sha384.ComputeHash(hashFile);
            break;
          case HashType.SHA512:
            SHA512 sha512 = new SHA512CryptoServiceProvider();
            retVal = sha512.ComputeHash(hashFile);
            break;
          default:
            break;
        }
        hashFile.Close();

        if (retVal == null) {
          runStatus = RunStatus.OTHER;
          Hash = "";
          return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (byte i in retVal)
          sb.Append(i.ToString("x2").ToUpper());
        Hash = sb.ToString();
      }

      if (Hash.Length != 0)
        runStatus = RunStatus.COMPUTED;
      else
        runStatus = RunStatus.OTHER;
    }

    /// <summary> Converts hashtype enum into string </summary>
    /// <returns> Formatted hash string </returns>
    private string GetCurrentHashTypeString() {
      string strHashType = hashType.ToString();
      if (strHashType.Contains("SHA"))
        strHashType = strHashType.Insert(3, "-");

      return strHashType;
    }

    /// <summary> Click event handler for compare button </summary>
    private void OnHitCompare(object sender, EventArgs e) {
      string suppliedHash = txtCompare.Text;
      if (suppliedHash.Length == 0 || Hash.Length == 0)
        return;

      if (suppliedHash == Hash)
        DisplayStatusMessage("Hash values are equal.", false);
      else
        DisplayStatusMessage("Hash values are not equal.", false);
    }

    /// <summary> Tick event handler for timer </summary>
    private void threadWatcher_Tick(object state, EventArgs e) {
      if (!this.IsActive)
        return;

      EnableControls(runStatus != RunStatus.COMPUTING);
      txtHash.Text = Hash;
      Clipboard.SetDataObject(txtHash.Text);
      if (runStatus == RunStatus.COMPUTED) {
        DisplayStatusMessage(strCurrentHashType + " hash computed successfully, copied to clipboard.", false);
        runStatus = RunStatus.OTHER;
      }
    }

    /// <summary> Help buttong click handler </summary>
    private void OnHelpClick(object sender, EventArgs e) {
      string strHelp = "Drag and drop supported for single file.\n";
      strHelp += "Command line parameters are supported as mentioned below.\n";
      strHelp += "  1. HashGenerator.exe <file> MD5/md5 - For MD5 hash.\n";
      strHelp += "  2. HashGenerator.exe <file> SHA?/sha?/SHA-?/sha-? - For SHA? hash.\n\n";
      strHelp += "Notes:\n";
      strHelp += "  1. Character ? above can be replaced by 1, 256, 384 and 512.\n";
      strHelp += "  2. If 2nd parameter not specified, SHA-1 hash generated by default.\n";
      strHelp += "  3. If 2nd parameter not correct, SHA-1 hash generated by default.\n";
      strHelp += "  4. Hash computation can be stopped anytime when in progress.\n";
      strHelp += "___________________________________________________________________________\n";
      strHelp += "Hash Generator - By Amresh";
      MessageBox.Show(this, strHelp, "Hash Generator Help", MessageBoxButton.OK, MessageBoxImage.Question);
    }

    /// <summary> Stop button click handler </summary>
    private void OnStopClick(object sender, EventArgs e) {
      if (runStatus != RunStatus.COMPUTING || hashComputer == null)
        return;

      runStatus = RunStatus.OTHER;
      hashComputer.Abort();
      DisplayStatusMessage("Aborted by user.", true);
      if (hashFile != null)
        hashFile.Close();
    }

    private void OnExpanded(object sender, RoutedEventArgs e) {
      Expander exp = (Expander)sender;
      string currentHeader = exp.Header.ToString();
      exp.Header = currentHeader.Replace(strHeaderPart, "");
      exp.Foreground = Brushes.Black;
    }

    private void OnCollapsed(object sender, RoutedEventArgs e) {
      Expander exp = (Expander)sender;
      exp.Header += strHeaderPart;
      exp.Foreground = Brushes.IndianRed;
    }

    public delegate void HashComputerDelegate();
    private enum HashType { MD5, SHA1, SHA256, SHA384, SHA512, NONE };
    private enum RunStatus { COMPUTING, COMPUTED, OTHER };
    private string strSource;
    private OpenFileDialog ofd = new OpenFileDialog();
    private RadioButton selectedRadio = null;
    private Hash hash = new Hash();
    private HashType hashType = HashType.SHA1;
    private RunStatus runStatus = RunStatus.OTHER;
    private Thread hashComputer = null;
    private FileStream hashFile = null;
    private string strArg = "";
    private string strType = "";
    private DispatcherTimer threadWatcher = new DispatcherTimer();
    private string strCurrentHashType = "";
    private string strHeaderPart = " controls hidden, click button to view.";
  }
}
