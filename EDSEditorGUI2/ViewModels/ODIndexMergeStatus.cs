using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace EDSEditorGUI2.ViewModels;

/// <summary>
/// Viewmodel for merging one index
/// </summary>
public partial class ODIndexMergeOffsetStatus : ObservableObject
{
    public ODIndexMergeOffsetStatus(int index, bool collision)
    {
        Index = index;
        Collision = collision;
    }
    [ObservableProperty]
    private int _index;

    [ObservableProperty]
    private bool _collision;
}

/// <summary>
/// Used as a view model when merging ODs or inserting profiles
/// </summary>
public partial class ODIndexMergeStatus : ObservableObject
{
    [ObservableProperty]
    private bool _insert;

    [ObservableProperty]
    private string _originalObject = string.Empty;

    [ObservableProperty]
    private List<ODIndexMergeOffsetStatus> _offsets = [];

    [ObservableProperty]
    private bool _indexCollision;

    [ObservableProperty]
    private int _originalIndex;

    [ObservableProperty]
    public required OdObject _object;

    [ObservableProperty]
    public IBrush _textBrush = new SolidColorBrush(Colors.Black);
}
