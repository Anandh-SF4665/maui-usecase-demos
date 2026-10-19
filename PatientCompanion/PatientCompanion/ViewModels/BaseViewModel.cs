using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

namespace PatientCompanion.ViewModels;

public partial class BaseViewModel : ObservableObject, IDisposable
{
    private bool disposed;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
            OnDispose();

        disposed = true;
    }

    protected virtual void OnDispose()
    {
    }
}