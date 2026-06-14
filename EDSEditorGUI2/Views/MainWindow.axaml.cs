using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using EDSEditorGUI2.Mapper;
using EDSEditorGUI2.ViewModels;
using libEDSsharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace EDSEditorGUI2.Views;

public partial class MainWindow : Window
{
    readonly FilePickerFileType xpd = new("CANopen XPD 1.1")
    {
        Patterns = ["*.xpd"]
    };
    readonly FilePickerFileType xdd = new("CANopen XDD 1.1")
    {
        Patterns = ["*.xdd"]
    };
    readonly FilePickerFileType xdc = new("CANopen XDC 1.1")
    {
        Patterns = ["*.xdc"]
    };

    public MainWindow()
    {
        InitializeComponent();
        LoadProfileList();
    }
    private void LoadProfileList()
    {
        // load default profiles from the install directory
        // load user profiles from the My Documents\.edseditor\profiles\ folder
        // Personal is my documents in windows and ~ in mono

        try
        {
            List<string> profilelist = [.. Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Profiles"))];
            string homepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".edseditor");
            homepath = Path.Combine(homepath, "profiles");

            if (Directory.Exists(homepath))
            {
                profilelist.AddRange(Directory.GetFiles(homepath));
            }

            List<MenuItem> newMenuItems = [];

            newMenuItems.Add(new MenuItem { Header = "Open Profile File...", Tag = "opendialog" });
            foreach (string file in profilelist)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (ext == ".xpd" || ext == ".xdd")
                {
                    newMenuItems.Add(new MenuItem { Header = Path.GetFileName(file), Tag = file });
                }
            }

            foreach (var i in newMenuItems)
            {
                i.Click += OnProfileMenuClick;
                profileMenu.Items.Add(i);
            }

        }
        catch (Exception e)
        {
            Debug.WriteLine($"Loading profiles has failed for the following reason : {e}");
        }
    }
    /// <summary>
    /// Combines different filepicker entrie into one
    /// </summary>
    /// <param name="name">the name of the new filepicker</param>
    /// <param name="types">list of filepicker</param>
    /// <returns>a combination of all filepicker types</returns>
    private static FilePickerFileType CombineFilePickerType(string name, List<FilePickerFileType> types)
    {
        List<string> patterns = [];

        foreach (var t in types)
        {
            if (t.Patterns is not null)
            {
                foreach (var p in t.Patterns)
                {
                    patterns.Add(p);
                }
            }
        }

        return new FilePickerFileType(name) { Patterns = patterns };
    }

    /// <summary>
    /// Eventhandler for any of the profile submenues
    /// </summary>
    /// <param name="sender">event trigger object</param>
    /// <param name="args">event arguments</param>
    /// <exception cref="Exception">On logical errors that sould not happend</exception>
    private async void OnProfileMenuClick(object? sender, RoutedEventArgs args)
    {
        var s = (MenuItem)sender!;
        string filePath;

        if (s.Tag is string fileSource)
        {
            if (fileSource == "opendialog")
            {
                // Get top level from the current control. Alternatively, you can use Window reference instead.
                var topLevel = TopLevel.GetTopLevel(this) ?? throw new Exception("Internal GUI error");

                // Start async operation to open the dialog.
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Text File",
                    AllowMultiple = false,
                    FileTypeFilter = [CombineFilePickerType("All supported files", [xpd, xdd, xdc]), xpd, xdd, xdc]
                });

                if (files.Count >= 1)
                {
                    filePath = files[0].Path.ToString();
                }
                else
                {
                    return;
                }
            }
            else
            {
                filePath = fileSource;
            }
            CanOpenXDD_1_1 coxml_1_1 = new();
            var eds = coxml_1_1.ReadXML(filePath);
            var proto = MappingEDS.MapToProtobuffer(eds);
            var viewModel = ProtobufferViewModelMapper.MapFromProtobuffer(proto);

            if (DataContext is MainWindowViewModel dc && dc.SelectedDevice != null)
            {
                var selectedObjects = dc.SelectedDevice.Objects;
                dc.InitMergeStatus(viewModel, [0]);
                await DialogHost.Show(Resources["InsertObjectsDialog"]!, "RootDialogHost", OnDialogClosing);
            }
        }
    }
    /// <summary>
    /// Event handler for the offset textbox in the profile import dialog
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnOffsetTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel dc && null != InsertObjects_Offsets.Text)
        {
            // look for "words" containing numbers
            string pattern = @"\b\d+\b";
            List<int> offsets = [];

            foreach (Match match in Regex.Matches(InsertObjects_Offsets.Text, pattern,
                                    RegexOptions.None,
                                    TimeSpan.FromSeconds(1)))
            {
                _ = int.TryParse(match.Value, out int result);
                offsets.Add(result);
            }

            dc.UpdateMergeStatus(offsets);

            int columnsNeeded = 2 + offsets.Count;
            while (grid.Columns.Count != columnsNeeded)
            {
                // need to add or remove columns
                if (grid.Columns.Count > columnsNeeded)
                {
                    grid.Columns.RemoveAt(grid.Columns.Count - 1);
                }
                else
                {
                    int offset = offsets[grid.Columns.Count - 2];
                    int index = grid.Columns.Count - 2;
                    var cellTemplate = new FuncDataTemplate<ODIndexMergeStatus>((item, scope) =>
                    {
                        var textBlock = new TextBlock
                        {
                            [!TextBlock.TextProperty] = new Binding($"Offsets[{index}].Index") { StringFormat = @"0x{0:x}" },
                            [!TextBlock.ForegroundProperty] = new Binding($"Offsets[{index}].Collision") { Converter = new Converter.BrushConverter() },
                        };
                        return textBlock;
                    });
                    DataGridTemplateColumn colOffset = new()
                    {
                        CellTemplate = cellTemplate,
                        Header = $"Offset {offset}",
                        IsReadOnly = true,
                    };
                    grid.Columns.Add(colOffset);
                }
            }
            // Update column headers
            for (var i = 0; i < offsets.Count; i++)
            {
                int offset = offsets[i];
                if (grid.Columns[2 + i].Header.ToString() != $"Offset {offset}")
                {
                    grid.Columns[2 + i].Header = $"Offset {offset}";
                    grid.Columns[2 + i].Width = DataGridLength.Auto;
                }
            }
        }
    }
    /// <summary>
    /// Called when insert objects dialog is closed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDialogClosing(object? sender, DialogClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel dc)
        {
            if (e.Parameter is not null && (string)e.Parameter == "insert" && dc.SelectedDevice != null)
            {
                //Merging MergeStatus into SelectedDevice.Objects.Data
                foreach (var insertObj in dc.MergeStatus)
                {
                    if (insertObj.Insert)
                    {
                        bool matched = false;
                        foreach (var orgObj in dc.SelectedDevice.Objects)
                        {
                            var indexAsInteger = orgObj.Key.ToInteger();
                            if (indexAsInteger == (insertObj.OriginalIndex + dc.InsertObjectsOffset))
                            {
                                dc.SelectedDevice.Objects[orgObj.Key] = insertObj.Object;
                                matched = true;
                            }
                        }
                        if (!matched)
                        {
                            foreach (var offset in insertObj.Offsets)
                            {
                                if (offset.Collision == false)
                                {
                                    string strIndex = offset.Index.ToString("X2");
                                    dc.SelectedDevice.Objects[strIndex] = insertObj.Object;
                                }
                            }
                        }
                    }
                }
            }
            dc.MergeStatus.Clear();
            InsertObjects_Offsets.Text = "0";
        }
    }
}