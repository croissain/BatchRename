using Microsoft.Win32;
using RenameRuleLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace BatchRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<IRenameRule> rules = new List<IRenameRule>(); //Danh sách các quy tắc đổi tên được nạp từ file .dll
        BindingList<IRenameRule> actions = new BindingList<IRenameRule>(); //Danh sách các quy tắc đổi tên được áp dụng
        BindingList<IRenameRuleParser> parsers = new BindingList<IRenameRuleParser>(); //Các parser sẽ tiến hành parse các string thành các quy tắc đổi tên
        Dictionary<string, IRenameRuleParser> parserPrototypes = new Dictionary<string, IRenameRuleParser>();
        List<string> listPreset = new List<string>();
        private bool _isBatched = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Get Preset
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "//Preset");
            LoadAllPreset();

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var f in fis)
            {
                var assembly = Assembly.LoadFile(f.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && typeof(IRenameRuleParser).IsAssignableFrom(type))
                    {
                        parsers.Add(Activator.CreateInstance(type) as IRenameRuleParser);
                    }
                    else if (type.IsClass && typeof(IRenameRule).IsAssignableFrom(type))
                    {
                        rules.Add(Activator.CreateInstance(type) as IRenameRule);
                    }
                }
            }
            foreach (var parser in parsers)
            {
                parserPrototypes.Add(parser.MagicWord, parser);
            }

            ActionMethodBox.ItemsSource = rules;
            presetBox.ItemsSource = listPreset;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            FileTab.Items.Refresh();
            FolderTab.Items.Refresh();
            listPreset.Clear();
            LoadAllPreset();
        }

        private void AddMethodButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ActionMethodBox.SelectedItem as IRenameRule;
            actions.Add(item.Clone());
            ActionListBox.ItemsSource = actions;
        }

        private void DeleteRuleButton_Click(object sender, RoutedEventArgs e)
        {
            actions.Clear();
            ActionListBox.ItemsSource = actions;
            FileTab.ItemsSource = null;
            FileTab.Items.Clear();
            FolderTab.ItemsSource = null;
            FolderTab.Items.Clear();
            _isBatched = false;
        }

        private void DeleteDirButton_Click(object sender, RoutedEventArgs e)
        {
            FileTab.ItemsSource = null;
            FileTab.Items.Clear();
            FolderTab.ItemsSource = null;
            FolderTab.Items.Clear();
        }

        private void AddFileButtons_Click(object sender, RoutedEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog();
            screen.Multiselect = true;
            if (screen.ShowDialog() == true)
            {
                foreach (var file in screen.FileNames)
                {
                    FileTab.Items.Add(new File()
                    {
                        Filename = System.IO.Path.GetFileName(file),
                        Path = file
                    });
                }
            }
        }

        private void AddFileFromFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog screen = new System.Windows.Forms.FolderBrowserDialog();
            if (screen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // get all filenames in path
                string path = screen.SelectedPath + "\\";
                string[] files = Directory.GetFiles(path);

                // add all to filenameList
                foreach (var file in files)
                {
                    FileTab.Items.Add(new File()
                    {
                        Filename = System.IO.Path.GetFileName(file),
                        Path = file
                    });
                }
            }
        }

        private void AddFolderButtons_Click(object sender, RoutedEventArgs e)
        {
            string directory;
            var screen = new FolderBrowserDialog();
            if (screen.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directory = screen.SelectedPath;

                FolderTab.Items.Add(new Folder()
                {
                    Foldername = System.IO.Path.GetFileName(directory),
                    Path = directory
                });

            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ActionListBox.SelectedItem as IRenameRule;
            var index = ActionListBox.SelectedIndex;
            string line = item.Config(item); //Nhận string trả về từ config dialog

            if (line != "")
            {
                IRenameRuleParser parser = parserPrototypes[item.MagicWord];
                actions[index] = parser.Parse(line); //Tiến hành parse line thành rename rule
            }
            ActionListBox.ItemsSource = actions;
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var index = ActionListBox.SelectedIndex;
            actions.RemoveAt(index);
            ActionListBox.ItemsSource = actions;
        }

        private bool DuplicateFile(File file)
        {
            foreach(File f in FileTab.Items)
            {
                if (f.Filename == file.Filename && f != file)
                    return true;
            }
            return false;
        }

        private bool DuplicateFolder(Folder folder)
        {
            foreach (Folder f in FolderTab.Items)
            {
                if (f.Foldername == folder.Foldername && f != folder)
                    return true;
            }
            return false;
        }

        private void CopyAll(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                System.Windows.MessageBox.Show("Source Directory does not exist or could not be found !");
            }

            if (!Directory.Exists(destDirName))
            {
                DirectoryInfo tempFolder = Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    CopyAll(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void RemoveDirectory(string sourcePath)
        {
            DirectoryInfo src = new DirectoryInfo(sourcePath);

            foreach (var file in src.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in src.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void StartBacthBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isFailed = false;
            //check input from users;
            if (ActionListBox.Items.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Add Method Before Batching!", "Erro Detected in Input", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else if (FileTab.Items.Count == 0 && FolderTab.Items.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("You haven't added Choose File Or Folder yet!", "Erro Detected in Input", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else if (_isBatched)
            {
                System.Windows.Forms.MessageBox.Show("Duplicate Error!\n You must clear all file or folder to rebatch.", "Duplicate Handle", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else
            {
                ObservableCollection<File> FileList = new ObservableCollection<File>();
                ObservableCollection<Folder> FolderList = new ObservableCollection<Folder>();
                //file process
                foreach (File file in FileTab.Items)
                {
                    string result = file.Filename;
                    foreach (var rule in actions)
                    {
                        result = rule.Rename(result);
                    }

                    var path = Path.GetDirectoryName(file.Path);
                    try
                    {
                        var tempfile = new FileInfo(file.Path);
                        tempfile.MoveTo(path + "\\" + result);
                        file.Newfilename = result;
                        if (DuplicateFile(file))
                        {
                            file.Status = "Duplicate";
                            System.Windows.MessageBox.Show("!!!Warning Duplicate File!!!\n Some file in list is duplicated, be careful before batching, your changes will be lost.", "Duplicate  Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                            file.Status = "Ok";
                    }
                    catch (Exception ex)
                    {
                        isFailed = true;
                        file.Newfilename = result;
                        file.Status = ex.GetType().Name;
                        FileList.Add(file);
                    }
                }
                //folder process
                int count = 0;
                foreach (Folder folder in FolderTab.Items)
                {
                    string result = folder.Foldername;
                    foreach (var rule in actions)
                    {
                        result = rule.Rename(result);
                    }

                    string newfolderpath = Path.GetDirectoryName(folder.Path) + "\\" + result;
                    string tempFolderName = "\\TempFolderBatchRename";
                    string tempFolderPath = Path.GetDirectoryName(folder.Path) + tempFolderName;

                    CopyAll(folder.Path, tempFolderPath, true);

                    if (folder.Path.Equals(newfolderpath) == false)
                    {
                        RemoveDirectory(folder.Path);
                        Directory.Delete(folder.Path);
                        try
                        {
                            Directory.Move(tempFolderPath, newfolderpath);
                            folder.Newfolder = result;
                            if (DuplicateFolder(folder))
                            {
                                folder.Status = "Duplicate";
                                System.Windows.MessageBox.Show("!!!Warning Duplicate Folder!!!\n Some folder in list is duplicated, be careful before batching, your changes will be lost.", "Duplicate  Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                                folder.Status = "Ok";
                        }
                        catch (Exception ex)
                        {
                            isFailed = true;
                            string duplicatestore = Path.GetDirectoryName(folder.Path) + "\\Store" + $"{++count}";
                            CopyAll(tempFolderPath, duplicatestore, true);
                            RemoveDirectory(tempFolderPath);
                            Directory.Delete(tempFolderPath);
                            folder.Newfolder = result;
                            folder.Status = ex.GetType().Name;
                            FolderList.Add(folder);
                        }
                    }
                    else
                    {
                        RemoveDirectory(tempFolderPath);
                        Directory.Delete(tempFolderPath);
                    }
                }

                if (isFailed == false)
                {
                    System.Windows.MessageBox.Show("Rename Success! Check your file or folder again!");
                    _isBatched = true;
                }

                FolderTab.Items.Refresh();
                FileTab.Items.Refresh();
            }
        }

        private void PreviewBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ActionListBox.Items.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("You haven't added any methods yet!", "Erro Detected in Input", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else if (FileTab.Items.Count == 0 && FolderTab.Items.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("You haven't added Choose File Or Folder yet!", "Erro Detected in Input", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else if (_isBatched)
            {
                System.Windows.Forms.MessageBox.Show("Duplicate Error!\n You must clear all file or folder to rebatch.", "Duplicate Handle", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
            }
            else
            {
                ObservableCollection<File> FileListPreview = new ObservableCollection<File>();
                ObservableCollection<Folder> FolderListPreview = new ObservableCollection<Folder>();

                //file process
                foreach (File file in FileTab.Items)
                {
                    var tempFile = file;
                    string result = tempFile.Filename;
                    foreach (var rule in actions)
                    {
                        result = rule.Rename(result);
                    }
                    try
                    {
                        tempFile.Newfilename = result;
                        if (DuplicateFile(file))
                        {
                            tempFile.Status = "Duplicate";
                            System.Windows.MessageBox.Show("!!!Warning Duplicate File!!!\n Some file in list is duplicated, be careful before batching, your changes will be lost.", "Duplicate  Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                            tempFile.Status = "Ok";
                        
                    }
                    catch (Exception ex)
                    {
                        tempFile.Newfilename = result;
                        tempFile.Status = ex.GetType().Name;
                        FileListPreview.Add(tempFile);
                    }
                }
                //folder process
                foreach (Folder folder in FolderTab.Items)
                {
                    var tempFolder = folder;
                    string result = tempFolder.Foldername;
                    foreach (var rule in actions)
                    {
                        result = rule.Rename(result);
                    }

                    try
                    {
                        tempFolder.Newfolder = result;
                        if (DuplicateFolder(folder))
                        {
                            tempFolder.Status = "Duplicate";
                            System.Windows.MessageBox.Show("!!!Warning Duplicate Folder!!!\n Some folder in list is duplicated, be careful before batching, your changes will be lost.", "Duplicate  Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                            tempFolder.Status = "Ok";
                    }
                    catch (Exception ex)
                    {
                        tempFolder.Newfolder = result;
                        tempFolder.Status = ex.GetType().Name;
                        FolderListPreview.Add(tempFolder);
                    }
                }

                FolderTab.Items.Refresh();
                FileTab.Items.Refresh();
            }
        }

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            if (ActionListBox.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("You need add method before press save preset!");
            }
            else
            {
                var screen = new PresetControl(); //Hiển thị dialog để nhập tên file preset.
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                string folderPresetName = path + "Preset"; //Thư mục lưu trữ các preset
                if (screen.ShowDialog() == true)
                {
                    string presetName = PresetControl.presetName + ".txt";

                    //Create file
                    string filename = folderPresetName + "\\" + presetName;
                    FileStream file = System.IO.File.Create(filename);
                    file.Close();

                    using (StreamWriter writer = new StreamWriter(filename))
                    {
                        foreach (IRenameRule action in ActionListBox.Items)
                        {
                            writer.WriteLine(action.ToString());
                        }
                    }
                    System.Windows.MessageBox.Show("Save success!");
                }
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            actions.Clear();
            var dlg = new Microsoft.Win32.OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                string presetfilename = dlg.FileName;

                using (StreamReader reader = new StreamReader(presetfilename))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var firstSpaceIndex = line.IndexOf(" ");
                        var magicword = line.Substring(0, firstSpaceIndex);
                        IRenameRuleParser parser = parserPrototypes[magicword];
                        actions.Add(parser.Parse(line));
                        ActionListBox.ItemsSource = actions;
                    }
                }
            }
        }

        private void LoadAllPreset()
        {
            string pathPreset = AppDomain.CurrentDomain.BaseDirectory + "//Preset";
            string[] presetFiles = Directory.GetFiles(pathPreset);
            foreach (var filename in presetFiles)
            {
                string temp = filename.Replace(pathPreset, "");
                temp = temp.Replace("\\", "").Replace(".txt", "");
                listPreset.Add(temp);
            }
        }

        private void presetBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            actions.Clear();
            string presetName = presetBox.SelectedItem as string;
            string presetFileName = AppDomain.CurrentDomain.BaseDirectory + "//Preset//" + presetName + ".txt";
            using (StreamReader reader = new StreamReader(presetFileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var firstSpaceIndex = line.IndexOf(" ");
                    var magicword = line.Substring(0, firstSpaceIndex);
                    IRenameRuleParser parser = parserPrototypes[magicword];
                    actions.Add(parser.Parse(line));
                    ActionListBox.ItemsSource = actions;
                }
            }
        }

        private void FileTabStack_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var file in files)
                {
                    string filename = System.IO.Path.GetFileName(file);

                    FileTab.Items.Add(new File()
                    {
                        Filename = filename,
                        Path = file
                    });
                }
            }
        }

        private void FoldertabStack_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] folders = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var folder in folders)
                {
                    string foldername = System.IO.Path.GetFileName(folder);

                    FolderTab.Items.Add(new Folder()
                    {
                        Foldername = foldername,
                        Path = folder
                    });
                }
            }
        }

    }
}
