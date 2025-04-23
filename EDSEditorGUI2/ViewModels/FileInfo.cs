using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace EDSEditorGUI2.ViewModels;

public partial class FileInfo : ObservableObject
{
    [ObservableProperty]
    private string _fileVersion = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _creationTime;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    [ObservableProperty]
    private DateTime _modificationTime;

    [ObservableProperty]
    private string _modifiedBy = string.Empty;
}
