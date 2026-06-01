using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using TurnosDesk.Operator.Models;
using TurnosDesk.Operator.Services;

namespace TurnosDesk.Operator;

public sealed partial class MainWindow : Window
{
    private const string ApiBaseUrl = "https://localhost:7001";

    private readonly TurnosDeskApiClient _apiClient;
    private readonly TurnosDeskRealtimeClient _realtimeClient;
    private readonly ObservableCollection<OperatorEventItem> _events = new();

    private QueueTicketResponse? _currentTicket;

    public MainWindow()
    {
        InitializeComponent();

        _apiClient = new TurnosDeskApiClient(ApiBaseUrl);
        _realtimeClient = new TurnosDeskRealtimeClient(ApiBaseUrl);

        EventsListView.ItemsSource = _events;

        ConfigureRealtime();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await SafeRunAsync(async () =>
        {
            await LoadInitialDataAsync();
            await _realtimeClient.StartAsync();

            RealtimeStatusText.Text = "SignalR conectado";
            AddEvent("Conexión establecida", "La app del asesor está conectada al Hub de TurnosDesk.");
        });
    }

    private void ConfigureRealtime()
    {
        _realtimeClient.TicketCalled += ticket =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                AddEvent("Turno llamado", $"{ticket.Folio} enviado a {ticket.ServiceModuleName ?? "módulo no asignado"}.");
            });
        };

        _realtimeClient.TicketStarted += ticket =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                AddEvent("Atención iniciada", $"{ticket.Folio} está en atención.");
            });
        };

        _realtimeClient.TicketCompleted += ticket =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                AddEvent("Atención finalizada", $"{ticket.Folio} fue finalizado correctamente.");
            });
        };

        _realtimeClient.TicketNoShow += ticket =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                AddEvent("No presentado", $"{ticket.Folio} fue marcado como no presentado.");
            });
        };

        _realtimeClient.TicketCancelled += ticket =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                AddEvent("Turno cancelado", $"{ticket.Folio} fue cancelado.");
            });
        };

        _realtimeClient.ConnectionClosed += () =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RealtimeStatusText.Text = "SignalR desconectado";
                AddEvent("Conexión cerrada", "Se perdió la conexión en tiempo real.");
            });
        };
    }

    private async Task LoadInitialDataAsync()
    {
        StatusMessageText.Text = "Cargando datos del sistema...";

        var branches = await _apiClient.GetBranchesAsync();
        BranchComboBox.ItemsSource = branches;

        if (branches.Count > 0)
        {
            BranchComboBox.SelectedIndex = 0;
        }

        var services = await _apiClient.GetServiceTypesAsync();
        ServiceTypeComboBox.ItemsSource = services;

        StatusMessageText.Text = "Datos cargados correctamente.";
    }

    private async Task LoadModulesForSelectedBranchAsync()
    {
        if (BranchComboBox.SelectedItem is not BranchResponse branch)
        {
            ModuleComboBox.ItemsSource = Array.Empty<ServiceModuleResponse>();
            return;
        }

        var modules = await _apiClient.GetModulesAsync(branch.Id);
        ModuleComboBox.ItemsSource = modules;

        if (modules.Count > 0)
        {
            ModuleComboBox.SelectedIndex = 0;
        }
    }

    private async void BranchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await SafeRunAsync(LoadModulesForSelectedBranchAsync);
    }

    private async void LoadDataButton_Click(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(LoadInitialDataAsync);
    }

    private async void CallNextButton_Click(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            var branch = GetSelectedBranch();
            var module = GetSelectedModule();
            var serviceType = ServiceTypeComboBox.SelectedItem as ServiceTypeResponse;

            var ticket = await _apiClient.CallNextAsync(
                branch.Id,
                module.Id,
                serviceType?.Id,
                GetOperatorName()
            );

            SetCurrentTicket(ticket);
            AddEvent("Turno llamado", $"{ticket.Folio} fue llamado correctamente.");
        });
    }

    private async void StartServiceButton_Click(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            var ticket = EnsureCurrentTicket();

            var updatedTicket = await _apiClient.StartServiceAsync(
                ticket.Id,
                GetOperatorName()
            );

            SetCurrentTicket(updatedTicket);
            AddEvent("Atención iniciada", $"Se inició la atención de {updatedTicket.Folio}.");
        });
    }

    private async void CompleteServiceButton_Click(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            var ticket = EnsureCurrentTicket();

            var updatedTicket = await _apiClient.CompleteServiceAsync(
                ticket.Id,
                "Atención finalizada desde la app de asesor.",
                GetOperatorName()
            );

            SetCurrentTicket(updatedTicket);
            AddEvent("Atención finalizada", $"Se finalizó la atención de {updatedTicket.Folio}.");
        });
    }

    private async void NoShowButton_Click(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            var ticket = EnsureCurrentTicket();

            var updatedTicket = await _apiClient.MarkNoShowAsync(
                ticket.Id,
                "El cliente no acudió al módulo.",
                GetOperatorName()
            );

            SetCurrentTicket(updatedTicket);
            AddEvent("No presentado", $"{updatedTicket.Folio} fue marcado como no presentado.");
        });
    }

    private async void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        await SafeRunAsync(async () =>
        {
            var ticket = EnsureCurrentTicket();

            var updatedTicket = await _apiClient.CancelAsync(
                ticket.Id,
                "Cancelado desde la app de asesor.",
                GetOperatorName()
            );

            SetCurrentTicket(updatedTicket);
            AddEvent("Turno cancelado", $"{updatedTicket.Folio} fue cancelado correctamente.");
        });
    }

    private BranchResponse GetSelectedBranch()
    {
        if (BranchComboBox.SelectedItem is BranchResponse branch)
        {
            return branch;
        }

        throw new InvalidOperationException("Selecciona una sucursal antes de continuar.");
    }

    private ServiceModuleResponse GetSelectedModule()
    {
        if (ModuleComboBox.SelectedItem is ServiceModuleResponse module)
        {
            return module;
        }

        throw new InvalidOperationException("Selecciona un módulo de atención antes de continuar.");
    }

    private QueueTicketResponse EnsureCurrentTicket()
    {
        if (_currentTicket is not null)
        {
            return _currentTicket;
        }

        throw new InvalidOperationException("Primero debes llamar un turno.");
    }

    private string GetOperatorName()
    {
        return string.IsNullOrWhiteSpace(OperatorNameTextBox.Text)
            ? "Asesor"
            : OperatorNameTextBox.Text.Trim();
    }

    private void SetCurrentTicket(QueueTicketResponse ticket)
    {
        _currentTicket = ticket;

        CurrentTicketFolioText.Text = ticket.Folio;
        CurrentTicketDetailText.Text = $"{ticket.ServiceTypeName} · {ticket.ServiceModuleName ?? "Módulo no asignado"} · {GetStatusLabel(ticket.Status)}";
    }

    private static string GetStatusLabel(QueueTicketStatus status)
    {
        return status switch
        {
            QueueTicketStatus.Pending => "Pendiente",
            QueueTicketStatus.Called => "Llamado",
            QueueTicketStatus.InService => "En atención",
            QueueTicketStatus.Completed => "Finalizado",
            QueueTicketStatus.Cancelled => "Cancelado",
            QueueTicketStatus.NoShow => "No presentado",
            _ => status.ToString()
        };
    }

    private void AddEvent(string title, string description)
    {
        _events.Insert(0, new OperatorEventItem(
            Title: title,
            Description: $"{DateTime.Now:HH:mm} · {description}"
        ));

        while (_events.Count > 20)
        {
            _events.RemoveAt(_events.Count - 1);
        }
    }

    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            StatusMessageText.Text = exception.Message;
            AddEvent("Operación no completada", exception.Message);
        }
    }

    private sealed record OperatorEventItem(string Title, string Description);
}
